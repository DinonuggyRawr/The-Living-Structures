using System;
using System.Threading;
using System.Threading.Tasks;

namespace TLS.Core
{
    /// <summary>Opt-in, session-only native compilation; never touches Academy mastery/saves.</summary>
    public sealed class StackNativeLabSession
    {
        public bool OptedIn { get; private set; }
        public bool Busy { get; private set; }
        public string Status { get; private set; } = "Runner DISABLED. Requires Docker Desktop with Linux containers and the gcc:14-bookworm image.";
        public StackNativeDockerRunner.Result Latest { get; private set; }
        public StackFunctionEvaluator.Function LastFunction { get; private set; }
        public int CompletedRuns { get; private set; }
        private Task<StackNativeDockerRunner.Result> running;
        private CancellationTokenSource cancel;

        public void Enable()
        {
            if(Busy) return;
            OptedIn = true;
            Status = "Explicit opt-in enabled for this session. Docker is not a perfect sandbox; only test code you trust.";
        }
        public void Disable()
        {
            if(Busy) Cancel();
            OptedIn = false;
            Status="Native runner disabled. Your function drafts and modeled testing remain available.";
        }
        public bool Start(StackFunctionEvaluator.Function f,string code)
        {
            if(!OptedIn) {Status="Enable the Docker runner explicitly before testing.";return false;}
            if(Busy) {Status="A native test run is already in progress.";return false;}
            if(string.IsNullOrWhiteSpace(code) || code.Length>StackNativeDockerRunner.SourceLimit)
            {Status="Draft must contain 1-5000 characters of C source.";return false;}
            LastFunction=f;Latest=null;Busy=true;
            Status="Running compiler and eight independent checks inside an ephemeral Docker Linux container...";
            cancel = new CancellationTokenSource();
            CancellationToken token=cancel.Token;
            string snapshot=code;
            running=Task.Run(()=>StackNativeDockerRunner.Run(f,snapshot,token));
            return true;
        }
        public bool Poll()
        {
            if(!Busy || running==null || !running.IsCompleted) return false;
            Busy=false;
            try {Latest=running.GetAwaiter().GetResult();}
            catch(Exception e) {Latest=new StackNativeDockerRunner.Result {Status="Runner failed: "+e.GetType().Name};}
            CompletedRuns++;
            Status=Latest.Status;
            running=null;
            if(cancel!=null) {cancel.Dispose();cancel=null;}
            return true;
        }
        public void Cancel()
        {
            if(cancel!=null) cancel.Cancel();
            Status="Cancellation requested. Runner cleanup may take a few seconds.";
        }
    }
}
