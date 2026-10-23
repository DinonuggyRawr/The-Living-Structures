using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TLS.Core
{
    /// <summary>
    /// Optional native C11 learning tests. Launches ONLY a fixed local Docker command.
    /// Student text is sent as UTF-8/base64 via stdin, never interpolated into any host
    /// command line or written to the host filesystem by this code. Container is nonroot,
    /// networkless, read-only, capability-dropped, CPU/memory/PID bounded, ephemeral.
    /// Docker is defense in depth, NOT a perfect security boundary; instructor controls
    /// whether this feature is enabled. Never counts as certified C mastery.
    /// </summary>
    public static class StackNativeDockerRunner
    {
        public const string Image = "gcc:14-bookworm";
        public const int SourceLimit = 5000;
        public const int HostTimeoutMilliseconds = 25000;
        public const int CasesPerFunction = 8;
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

        public static readonly string[] PushCases = {
            "empty stack", "one element", "partially full", "last free slot", "full stack",
            "INT_MIN value", "INT_MAX value", "NULL stack"};
        public static readonly string[] ReadCases = {
            "empty stack", "one element", "two elements", "full stack", "inactive cell preserved",
            "NULL stack", "NULL output", "both pointers NULL"};

        public static string CaseName(StackFunctionEvaluator.Function function, int index)
        {
            if(index < 0 || index >= CasesPerFunction) throw new ArgumentOutOfRangeException("index");
            return (function == StackFunctionEvaluator.Function.Push ? PushCases : ReadCases)[index];
        }
        public static string BuildSubmission(string student)
        {
            if(student == null || student.Length == 0 || student.Length > SourceLimit || student.IndexOf('\0') >= 0)
                throw new ArgumentException("Complete C function required (1-5000 characters, no NUL bytes).", "student");
            return Prelude + "\n#line 1 \"student.c\"\n" + student + "\n";
        }

        // Fixed shell script. There are no source-code, user-path, or student-provided
        // command-line arguments. Both C translation units arrive as base64 via stdin.
        public static string BuildContainerScript(StackFunctionEvaluator.Function function)
        {
            int selected = (int)function;
            if(selected < 0 || selected > 2) throw new ArgumentOutOfRangeException("function");
            return "set -u\n" +
                   "umask 077\n" +
                   "IFS= read -r student64 || exit 31\n" +
                   "IFS= read -r harness64 || exit 32\n" +
                   "printf '%s' \"$student64\" | base64 -d > /work/student.c || exit 33\n" +
                   "printf '%s' \"$harness64\" | base64 -d > /work/test_harness.c || exit 34\n" +
                   "gcc -std=c11 -O0 -Wall -Wextra -Werror -pedantic -fstack-protector-strong -fsanitize=undefined -fno-sanitize-recover=undefined -DTLS_SELECTED=" + selected.ToString(CultureInfo.InvariantCulture) +
                   " /work/student.c /work/test_harness.c -o /work/tls_test > /work/compile.log 2>&1\n" +
                   "rc=$?\n" +
                   "if [ \"$rc\" -ne 0 ]; then\n" +
                   "  echo 'TLS_STATUS|COMPILE_ERROR'\n" +
                   "  sed -n '1,18s/^/TLS_DIAG|/p' /work/compile.log | cut -c 1-250\n" +
                   "  exit 0\n" +
                   "fi\n" +
                   "echo 'TLS_STATUS|COMPILED'\n" +
                   "for i in 0 1 2 3 4 5 6 7; do\n" +
                   "  timeout 2s /work/tls_test \"$i\" >/dev/null 2>&1\n" +
                   "  rc=$?\n" +
                   "  if [ \"$rc\" -eq 0 ]; then echo \"TLS_CASE|$i|PASS\"; else echo \"TLS_CASE|$i|FAIL|$rc\"; fi\n" +
                   "done\n" +
                   "echo 'TLS_STATUS|DONE'\n";
        }
        private static string QuoteArg(string literal)
        {
            // Static literals only. Windows CreateProcess command-line quote escaping.
            return "\"" + literal.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }
        public static string BuildDockerArguments(string uniqueName, StackFunctionEvaluator.Function function)
        {
            if(string.IsNullOrEmpty(uniqueName) || !uniqueName.StartsWith("tls-native-", StringComparison.Ordinal))
                throw new ArgumentException("Unexpected temporary container ID.", "uniqueName");
            return "run --rm -i --pull=never --name " + uniqueName +
                " --network none --cap-drop ALL --security-opt no-new-privileges" +
                " --pids-limit 64 --memory 256m --memory-swap 256m --cpus 1" +
                " --read-only --user 65534:65534" +
                " --tmpfs /work:rw,exec,nosuid,nodev,size=32m,mode=1777" +
                " --tmpfs /tmp:rw,nosuid,nodev,size=16m,mode=1777" +
                " --workdir /work --entrypoint /bin/sh " + Image + " -c " + QuoteArg(BuildContainerScript(function));
        }
        public static Result ParseOutput(string output, int exitCode)
        {
            Result result = new Result();
            if(exitCode != 0)
            {
                result.Status = "Docker runner could not start/finish (exit " + exitCode + ").";
                result.Diagnostic = Cap(output, 1200);
                return result;
            }
            bool[] seen = new bool[CasesPerFunction];
            bool compiled=false, done=false, compileError=false;
            StringBuilder diag = new StringBuilder();
            string[] lines = (output ?? "").Split('\n');
            foreach(string raw in lines)
            {
                string line=raw.TrimEnd('\r');
                if(line=="TLS_STATUS|COMPILE_ERROR") compileError=true;
                else if(line=="TLS_STATUS|COMPILED") compiled=true;
                else if(line=="TLS_STATUS|DONE") done=true;
                else if(line.StartsWith("TLS_DIAG|", StringComparison.Ordinal) && diag.Length < 1500)
                    diag.AppendLine(line.Substring(9));
                else if(line.StartsWith("TLS_CASE|", StringComparison.Ordinal) && compiled)
                {
                    string[] parts=line.Split('|');int n;
                    if(parts.Length>=3 && int.TryParse(parts[1],out n) && n>=0 && n<CasesPerFunction && !seen[n])
                    {
                        seen[n]=true;
                        result.Cases[n]=parts[2]=="PASS";
                        if(result.Cases[n]) result.Passed++;
                        else
                        {
                            int reason=0;
                            if(parts.Length>=4) int.TryParse(parts[3],out reason);
                            result.CaseNotes[n]=reason==2?"unexpected bool return":
                                reason==3?"count mismatch":reason==4?"backing array mismatch":
                                reason==5?"output parameter mismatch":
                                reason==124?"time limit reached":
                                "runtime failure or sanitizer exit ("+reason+")";
                        }
                    }
                }
            }
            result.CompilerRan = compiled || compileError;
            result.Compiled = compiled;
            result.Diagnostic = Cap(diag.ToString(), 1500);
            if(compileError) result.Status = "C11 compiler rejected this source. Review the diagnostics.";
            else if(!compiled || !done || Array.Exists(seen, s => !s))
                result.Status = "Incomplete runner response. No verified test result; retry or inspect Docker.";
            else
            {
                result.Completed=true;
                result.Status = result.Passed==CasesPerFunction ?
                    "Compiled as C11: all 8 container tests passed (NOT certification or memory-safety proof)." :
                    "C11 compiled: " + result.Passed + "/8 tests passed. First failure: " + FirstFailureDescription(result);
            }
            return result;
        }
        public static string FirstFailureDescription(Result result)
        {
            if(result==null || !result.Completed) return "no complete native result";
            for(int i=0;i<CasesPerFunction;i++)
                if(!result.Cases[i]) return "case " +(i+1)+" ("+(result.CaseNotes[i]??"test failed")+")";
            return "none";
        }
        private static string Cap(string value, int max)
        { return string.IsNullOrEmpty(value)?"":value.Length<=max?value:value.Substring(0,max)+" [truncated]"; }

        private static ProcessStartInfo DockerProcess(string args)
        {
            ProcessStartInfo info = new ProcessStartInfo("docker",args);
            info.UseShellExecute=false; info.CreateNoWindow=true;
            info.RedirectStandardInput=true;info.RedirectStandardOutput=true;info.RedirectStandardError=true;
            return info;
        }
        private static void StopContainer(string name)
        {
            // Force-remove by generated internal name if the CLI was interrupted.
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("docker","rm -f " + name);
                psi.UseShellExecute=false;psi.CreateNoWindow=true;
                using(Process cleanup=Process.Start(psi)) if(cleanup!=null) { if(!cleanup.WaitForExit(4000)) cleanup.Kill(); }
            }
            catch { /* docker may be stopped; no host file or process is affected */ }
        }
        public static Result Run(StackFunctionEvaluator.Function function, string source, CancellationToken cancellation)
        {
            Result r=new Result();
            string submission;
            try { submission=BuildSubmission(source); }
            catch(Exception ex) {r.Status=ex.Message;return r;}
            string name = "tls-native-" + Guid.NewGuid().ToString("N").Substring(0,12);
            Process proc=null;
            try
            {
                cancellation.ThrowIfCancellationRequested();
                proc=Process.Start(DockerProcess(BuildDockerArguments(name,function)));
                if(proc==null) {r.Status="Docker did not start.";return r;}
                Task<string> stdOut=proc.StandardOutput.ReadToEndAsync();
                Task<string> stdErr=proc.StandardError.ReadToEndAsync();
                string payload=Convert.ToBase64String(Encoding.UTF8.GetBytes(submission))+"\n"+
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(StackNativeHarness.Source))+"\n";
                proc.StandardInput.Write(payload);
                proc.StandardInput.Close();
                int waited=0;
                while(!proc.WaitForExit(100))
                {
                    waited+=100;
                    if(cancellation.IsCancellationRequested || waited>=HostTimeoutMilliseconds)
                    {
                        try {proc.Kill();} catch {}
                        StopContainer(name);
                        r.Status=cancellation.IsCancellationRequested?"Run cancelled; container cleanup requested.":
                            "Runner exceeded 25 seconds; container cleanup requested. No result awarded.";
                        return r;
                    }
                }
                string outText=stdOut.GetAwaiter().GetResult();
                string errText=stdErr.GetAwaiter().GetResult();
                r=ParseOutput(outText,proc.ExitCode);
                if(!r.CompilerRan && !string.IsNullOrEmpty(errText)) r.Diagnostic=Cap(errText,1200);
                return r;
            }
            catch(OperationCanceledException) {r.Status="Run cancelled.";return r;}
            catch(Exception ex) {r.Status="Cannot launch Docker. Install/start Docker Desktop, use Linux containers, and pull "+Image+" first.";
                r.Diagnostic=Cap(ex.Message,350);StopContainer(name);return r;}
            finally {if(proc!=null) proc.Dispose();}
        }
    }
}
