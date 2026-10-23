using System;
using System.Threading;
using System.Threading.Tasks;

namespace TLS.Core
{
    /// <summary>Session-only optional WebAssembly C testing. Does not update Academy saves or mastery.</summary>
    public sealed class StackWasmLabSession
    {
        public bool OptedIn { get; private set; }
        public bool Busy { get; private set; }
        public string Status { get; private set; } =
            "Runner disabled. Install the local Zig C-to-WASM and Wasmtime tools, then enable for this session.";
        public StackWasmRunner.Result Latest { get; private set; }
        public StackFunctionEvaluator.Function LastFunction { get; private set; }
        public int CompletedRuns { get; private set; }
        public string LastSource { get; private set; }
        private Task<StackWasmRunner.Result> running;
        private CancellationTokenSource cancel;

        public void Enable()
        {
            if (Busy) return;
            OptedIn = true;
            Status = "WASM runner enabled for this session. Only use your own trusted practice code.";
        }
        public void Disable()
        {
            if (Busy) Cancel();
            OptedIn = false;
            Status = "WebAssembly runner disabled. Modeled tests and drafts remain available.";
        }
        public bool Start(StackFunctionEvaluator.Function selected, string draft, string unityDataPath)
        {
            if (!OptedIn) {Status = "Enable the WASM compiler runner before testing.";return false;}
            if (Busy) {Status = "One compiler run is already active.";return false;}
            if (string.IsNullOrWhiteSpace(draft) || draft.Length > StackWasmRunner.SourceLimit)
            {Status = "Provide 1-5000 characters of a complete C function.";return false;}
            // Resolve Unity's data path on the Unity main thread, not inside a Task.Run worker.
            string toolRoot;
            try { toolRoot = StackWasmRunner.ToolchainRoot(unityDataPath); }
            catch (Exception e) {Status = e.Message;return false;}
            LastFunction = selected;
            LastSource = draft;
            Latest = null;
            Busy = true;
            Status = "Compiling C11 to WebAssembly, then running eight isolated bounded checks...";
            cancel = new CancellationTokenSource();
            CancellationToken token = cancel.Token;
            string frozen = draft;
            running = Task.Run(() => StackWasmRunner.Run(selected, frozen, toolRoot, token));
            return true;
        }
        public bool Poll()
        {
            if (!Busy || running == null || !running.IsCompleted) return false;
            Busy = false;
            try { Latest = running.GetAwaiter().GetResult(); }
            catch (Exception e) {Latest = new StackWasmRunner.Result {Status = "Runner failed: " + e.GetType().Name};}
            CompletedRuns++;
            Status = Latest.Status;
            running = null;
            if (cancel != null) {cancel.Dispose();cancel=null;}
            return true;
        }
        public void Cancel()
        {
            if (cancel != null) cancel.Cancel();
            Status = "Cancelling compilation or the current WebAssembly test...";
        }
    }
}
