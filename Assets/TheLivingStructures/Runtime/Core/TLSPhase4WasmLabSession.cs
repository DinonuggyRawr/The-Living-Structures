using System;
using System.Threading;
using System.Threading.Tasks;

namespace TLS.Core
{
    /// <summary>Explicit opt-in C11 WebAssembly tests, with no save/progress mutations.</summary>
    public sealed class TLSPhase4WasmLabSession
    {
        public bool OptedIn { get; private set; }
        public bool Busy { get; private set; }
        public string Status { get; private set; } = "Runner disabled for this session. Existing visual and structured lessons remain available.";
        public TLSPhase4WasmRunner.Result Latest { get; private set; }
        public TLSPhase4Function LastFunction { get; private set; }
        public string LastSource { get; private set; } = "";
        public int CompletedRuns { get; private set; }
        private Task<TLSPhase4WasmRunner.Result> running;
        private CancellationTokenSource cancel;
        private TLSPhase4FunctionLabSession runningPractice;
        private bool runningIndependent;

        public void Enable() { if (!Busy) { OptedIn = true; Status = "WASM runner enabled. Run only your own trusted learning code."; } }
        public void Disable() { if (Busy) Cancel(); OptedIn = false; Status = "WASM runner disabled."; }
        public bool Start(TLSPhase4FunctionLabSession practice, string unityDataPath)
        {
            if (practice == null) throw new ArgumentNullException("practice");
            if (!OptedIn) {Status = "Enable WASM testing first.";return false;}
            if (Busy) {Status = "A compiler run is in progress.";return false;}
            if (string.IsNullOrWhiteSpace(practice.Draft) || practice.Draft.Length > TLSPhase4WasmRunner.SourceLimit)
            {Status = "Enter a complete C11 function (maximum 7500 characters).";return false;}
            string root;
            try {root = TLSPhase4WasmRunner.ToolchainRoot(unityDataPath);}
            catch (Exception e) {Status = e.Message;return false;}
            LastFunction = practice.Selected;
            LastSource = practice.Draft;
            runningIndependent = !practice.Guided;
            runningPractice = practice;
            Latest = null;
            Busy = true;
            Status = "Compiling C11 to WebAssembly; running eight isolated test cases...";
            cancel = new CancellationTokenSource();
            CancellationToken token = cancel.Token;
            TLSPhase4Function frozenFunction = LastFunction;
            string frozenSource = LastSource;
            running = Task.Run(() => TLSPhase4WasmRunner.Run(frozenFunction, frozenSource, root, token));
            return true;
        }
        public bool Poll()
        {
            if (!Busy || running == null || !running.IsCompleted) return false;
            Busy = false;
            try {Latest = running.GetAwaiter().GetResult();}
            catch (Exception e) {Latest = new TLSPhase4WasmRunner.Result {Status = "Runner failed: " + e.GetType().Name};}
            CompletedRuns++;
            Status = Latest.Status;
            // A real compiler diagnostic or completed case suite is an attempted submission.
            // Cancellation, runtime-tool failure, or timeout after successful compilation
            // must not secretly consume an unseen first-attempt opportunity.
            bool evaluated = Latest.CompilerRan && (Latest.Completed || !Latest.Compiled);
            if (runningPractice != null) runningPractice.Record(LastFunction, evaluated, Latest.AllPassed, runningIndependent);
            runningPractice = null;
            running = null;
            if (cancel != null) {cancel.Dispose();cancel = null;}
            return true;
        }
        public void Cancel()
        {
            if (cancel != null) cancel.Cancel();
            Status = "Cancelling WASM run. No completed test result will be credited.";
        }
    }
}
