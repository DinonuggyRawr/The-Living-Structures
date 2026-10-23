using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TLS.Core
{
    /// <summary>
    /// Phase 1.5 WebAssembly backend. Compiles the learner's C with a locally installed
    /// Zig/Clang wasm32-wasi compiler, then invokes only the resulting .wasm module in
    /// Wasmtime with fuel, per-module memory/stack, epoch time and host process limits.
    /// Neither student source nor their WASM module is launched as a native executable.
    /// Zig and Wasmtime themselves are host-native trusted tools: NOT a hardened public
    /// submission service. All work is session-only under a random temporary directory.
    /// </summary>
    public static class StackWasmRunner
    {
        public const int SourceLimit = 5000;
        public const int CasesPerFunction = 8;
        public const int CompileTimeoutMs = 75000;
        public const int CaseTimeoutMs = 6000;
        public const int MaxDiagnostics = 2800;
        public const string Folder = "TLS_Toolchain_Windows";
        private const string Prelude = "#include <stdbool.h>\n#include <stddef.h>\n#include <limits.h>\n#define CAPACITY 5\ntypedef struct { int data[CAPACITY]; size_t count; } Stack;\n";

        public sealed class Result
        {
            public bool CompilerRan;
            public bool Compiled;
            public bool Completed;
            public int Passed;
            public int Total = CasesPerFunction;
            public string Status = "Not run.";
            public string Diagnostic = "";
            public readonly bool[] Cases = new bool[CasesPerFunction];
            public readonly string[] CaseNotes = new string[CasesPerFunction];
            public bool AllPassed { get { return CompilerRan && Compiled && Completed && Passed == Total; } }
        }

        private sealed class CommandResult
        {
            public int ExitCode = -1;
            public bool TimedOut;
            public bool Cancelled;
            public string Output = "";
            public string Error = "";
        }

        public static readonly string[] PushCases = {
            "empty stack", "one element", "partially full", "last free slot", "full stack",
            "INT_MIN value", "INT_MAX value", "NULL stack"};
        public static readonly string[] ReadCases = {
            "empty stack", "one element", "two elements", "full stack", "inactive cell preserved",
            "NULL stack", "NULL output", "both pointers NULL"};

        public static string CaseName(StackFunctionEvaluator.Function function, int index)
        {
            if (index < 0 || index >= CasesPerFunction) throw new ArgumentOutOfRangeException("index");
            return (function == StackFunctionEvaluator.Function.Push ? PushCases : ReadCases)[index];
        }

        public static string BuildSubmission(string student)
        {
            if (string.IsNullOrWhiteSpace(student) || student.Length > SourceLimit || student.IndexOf('\0') >= 0)
                throw new ArgumentException("Complete C function required (1-5000 characters; no NUL bytes).", "student");
            return Prelude + "\n#line 1 \"student.c\"\n" + student + "\n";
        }

        public static string ToolchainRoot(string unityDataPath)
        {
            return TLSBundledToolchain.RootFromUnityDataPath(unityDataPath);
        }
        public static string CompilerPath(string root) { return TLSBundledToolchain.ZigPath(root); }
        public static string RuntimePath(string root) { return TLSBundledToolchain.WasmtimePath(root); }
        public static string ToolchainStatus(string root)
        {
            return TLSBundledToolchain.Check(root, !UnityEngine.Application.isEditor).Message;
        }
        private static string Quote(string value) { return "\"" + value.Replace("\"", "\\\"") + "\""; }
        public static string BuildCompileArguments(string studentFile, string harnessFile, string outputFile,
            StackFunctionEvaluator.Function function)
        {
            int selected = (int)function;
            if (selected < 0 || selected > 2) throw new ArgumentOutOfRangeException("function");
            // All paths are generated inside a private temporary folder, NEVER from a learner input.
            return "cc -target wasm32-wasi -std=c11 -O0 -Wall -Wextra -Werror -pedantic " +
                "-Wl,-z,stack-size=262144 -Wl,--max-memory=16777216 " +
                "-DTLS_SELECTED=" + selected.ToString(CultureInfo.InvariantCulture) + " " +
                Quote(studentFile) + " " + Quote(harnessFile) + " -o " + Quote(outputFile);
        }
        public static string BuildRunArguments(string wasmFile, int caseIndex)
        {
            if (caseIndex < 0 || caseIndex >= CasesPerFunction) throw new ArgumentOutOfRangeException("caseIndex");
            // No --dir, --env, network, socket or filesystem permissions granted. CLI flags precede the module.
            // Wasmtime v50 supports these -W options; failures are never interpreted as a passing test.
            return "run -W fuel=20000000,max-memory-size=16777216,max-wasm-stack=262144,timeout=2s " +
                Quote(wasmFile) + " " + caseIndex.ToString(CultureInfo.InvariantCulture);
        }
        public static bool IsWasmFile(string file)
        {
            if (!File.Exists(file) || new FileInfo(file).Length < 8 || new FileInfo(file).Length > 16 * 1024 * 1024) return false;
            using (FileStream s = File.OpenRead(file))
                return s.ReadByte() == 0 && s.ReadByte() == 0x61 && s.ReadByte() == 0x73 && s.ReadByte() == 0x6d &&
                    s.ReadByte() == 1 && s.ReadByte() == 0 && s.ReadByte() == 0 && s.ReadByte() == 0;
        }
        public static string ExplainExitCode(int code, bool timedOut)
        {
            if (timedOut) return "time limit reached";
            if (code == 2) return "unexpected bool return";
            if (code == 3) return "count mismatch";
            if (code == 4) return "backing array mismatch";
            if (code == 5) return "output parameter mismatch";
            return "WASM trap, nonzero exit, or runtime error (" + code.ToString(CultureInfo.InvariantCulture) + ")";
        }
        private static string Cap(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length <= max ? s : s.Substring(0, max) + " [truncated]";
        }
        private static async Task<string> DrainLimited(System.IO.StreamReader reader, int limit)
        {
            char[] buffer = new char[1024];
            StringBuilder text = new StringBuilder();
            bool truncated = false;
            int count;
            while ((count = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                int available = limit - text.Length;
                if (available > 0) text.Append(buffer, 0, Math.Min(count, available));
                if (count > available) truncated = true;
            }
            if (truncated) text.Append(" [truncated]");
            return text.ToString();
        }
        private static CommandResult Exec(string program, string arguments, string workingDirectory,
            int timeoutMs, CancellationToken cancellation)
        {
            CommandResult ret = new CommandResult();
            var info = new ProcessStartInfo(program, arguments);
            info.WorkingDirectory = workingDirectory;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            using (Process p = new Process())
            {
                p.StartInfo = info;
                cancellation.ThrowIfCancellationRequested();
                p.Start();
                // Read both pipes immediately: a compiler can emit large diagnostics.
                Task<string> stdout = DrainLimited(p.StandardOutput, MaxDiagnostics);
                Task<string> stderr = DrainLimited(p.StandardError, MaxDiagnostics);
                p.StandardInput.Close();
                int elapsed = 0;
                while (!p.WaitForExit(100))
                {
                    elapsed += 100;
                    if (cancellation.IsCancellationRequested || elapsed >= timeoutMs)
                    {
                        ret.Cancelled = cancellation.IsCancellationRequested;
                        ret.TimedOut = !ret.Cancelled;
                        try { p.Kill(); } catch { }
                        break;
                    }
                }
                // Wait briefly after kill to release handles before temp-folder cleanup.
                try { if (!p.HasExited) p.WaitForExit(1500); } catch { }
                if (!ret.Cancelled && !ret.TimedOut) ret.ExitCode = p.ExitCode;
                // Avoid hanging waiting for pipes if a malicious process spawned child inheriting handles.
                try { if (Task.WaitAll(new Task[] { stdout, stderr }, 1500))
                    {ret.Output = Cap(stdout.Result, MaxDiagnostics);ret.Error = Cap(stderr.Result, MaxDiagnostics);} }
                catch { }
            }
            return ret;
        }
        public static Result Run(StackFunctionEvaluator.Function function, string source,
            string toolRoot, CancellationToken cancellation)
        {
            Result result = new Result();
            string submission;
            try { submission = BuildSubmission(source); }
            catch (Exception e) { result.Status = e.Message; return result; }
            TLSBundledToolchain.CheckResult check = TLSBundledToolchain.Check(toolRoot, !UnityEngine.Application.isEditor);
            if (!check.Ready)
            {
                result.Status = "C11-to-WebAssembly tools unavailable; modeled tests are still ready.";
                result.Diagnostic = ToolchainStatus(toolRoot ?? "");
                return result;
            }
            string folder = Path.Combine(Path.GetTempPath(), "tls-wasm-" + Guid.NewGuid().ToString("N"));
            try
            {
                Directory.CreateDirectory(folder);
                string studentFile = Path.Combine(folder, "student.c");
                string harnessFile = Path.Combine(folder, "harness.c");
                string wasmFile = Path.Combine(folder, "stack_test.wasm");
                File.WriteAllText(studentFile, submission, new UTF8Encoding(false));
                File.WriteAllText(harnessFile, StackWasmHarness.Source, new UTF8Encoding(false));
                cancellation.ThrowIfCancellationRequested();
                CommandResult compile = Exec(CompilerPath(toolRoot),
                    BuildCompileArguments(studentFile, harnessFile, wasmFile, function), folder,
                    CompileTimeoutMs, cancellation);
                if (compile.Cancelled) {result.Status = "Compilation cancelled. No credit recorded.";return result;}
                if (compile.TimedOut) {result.Status = "C compiler exceeded the time limit. No tests credited.";return result;}
                result.CompilerRan = true;
                result.Diagnostic = Cap(compile.Error + "\n" + compile.Output, MaxDiagnostics);
                if (compile.ExitCode != 0)
                {
                    result.Status = "C11 compilation failed (exit " + compile.ExitCode + "). Check compiler diagnostics.";
                    return result;
                }
                if (!IsWasmFile(wasmFile))
                {
                    result.Status = "Compiler did not produce a valid bounded-size WebAssembly module.";
                    return result;
                }
                result.Compiled = true;
                // Eight separate Wasmtime processes prevent one case contaminating another's global state.
                for (int i = 0; i < CasesPerFunction; i++)
                {
                    cancellation.ThrowIfCancellationRequested();
                    CommandResult run = Exec(RuntimePath(toolRoot), BuildRunArguments(wasmFile, i), folder,
                        CaseTimeoutMs, cancellation);
                    if (run.Cancelled || run.TimedOut)
                    {
                        result.Status = "WASM run interrupted at case " + (i+1) + ". No complete score awarded.";
                        result.CaseNotes[i] = ExplainExitCode(-1, run.TimedOut);
                        return result;
                    }
                    if (run.ExitCode != 0 &&
                        (run.Error.IndexOf("unexpected argument", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         run.Error.IndexOf("unknown option", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         run.Error.IndexOf("unrecognized option", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        result.Status = "Wasmtime rejected the required runtime-limiting options; no score awarded.";
                        result.Diagnostic = Cap(run.Error, MaxDiagnostics);
                        return result;
                    }
                    result.Cases[i] = run.ExitCode == 0;
                    if (result.Cases[i]) result.Passed++;
                    else
                    {
                        result.CaseNotes[i] = ExplainExitCode(run.ExitCode, false);
                        if (string.IsNullOrWhiteSpace(result.Diagnostic))
                            result.Diagnostic = Cap("Case " + (i+1) + ": " + run.Error + " " + run.Output, MaxDiagnostics);
                    }
                }
                result.Completed = true;
                result.Status = result.AllPassed ?
                    "C11 compiled to WASM: 8/8 isolated tests passed (not comprehensive correctness)." :
                    "C11 compiled to WASM: " + result.Passed + "/8 tests passed. " + FirstFailureDescription(result);
                return result;
            }
            catch (OperationCanceledException) {result.Status = "Run cancelled. No complete score awarded.";return result;}
            catch (Exception e)
            {
                result.Status = "Unable to run local Zig/Wasmtime toolchain. No complete score awarded.";
                result.Diagnostic = Cap(e.GetType().Name + ": " + e.Message, 600);
                return result;
            }
            finally
            {
                try { if (Directory.Exists(folder)) Directory.Delete(folder, true); }
                catch { /* Never delete outside our own randomized temp folder. */ }
            }
        }
        public static string FirstFailureDescription(Result r)
        {
            if (r == null || !r.Completed) return "No complete WASM test result.";
            for (int i = 0; i < CasesPerFunction; i++)
                if (!r.Cases[i]) return "First failed case: " + (i+1) + " (" + (r.CaseNotes[i] ?? "unknown") + ").";
            return "All cases passed.";
        }
    }
}
