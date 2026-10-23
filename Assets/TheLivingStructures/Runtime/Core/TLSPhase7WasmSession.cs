using System;
using System.Threading;
using System.Threading.Tasks;
namespace TLS.Core
{
    /// <summary>Explicit opt-in. Learner C compiles via trusted native Zig but executes as constrained Wasmtime.</summary>
    public sealed class TLSPhase7WasmSession
    {
        public bool OptedIn {get;private set;}
        public bool Busy {get;private set;}
        public string Status {get;private set;}="Enable the local WASM runner to submit real C11 transfer functions.";
        public TLSPhase7WasmRunner.Result Latest {get;private set;}
        public TLSPhase7Function LastFunction {get;private set;}
        public string LastSource {get;private set;}="";
        public bool IndependentJustRecorded {get;private set;}
        private Task<TLSPhase7WasmRunner.Result> task;
        private CancellationTokenSource cancellation;
        private TLSPhase7FunctionSession submitted;
        private TLSPhase7ProgressBook book;
        private bool wasIndependent,wasExposed,previouslyAttempted;
        public void Enable(){if(!Busy){OptedIn=true;Status="WASM runner enabled: run your own trusted learning code only.";}}
        public void Disable(){if(Busy)Cancel();OptedIn=false;Status="WASM runner disabled.";}
        public bool Start(TLSPhase7FunctionSession practice,TLSPhase7ProgressBook progress,string dataPath)
        {
            if(practice==null||progress==null)throw new ArgumentNullException("practice");
            if(!OptedIn||Busy){Status=!OptedIn?"Enable WASM first.":"A run is already in progress.";return false;}
            if(string.IsNullOrWhiteSpace(practice.Draft)||practice.Draft.Length>TLSPhase7WasmRunner.SourceLimit)
            {Status="Enter 1–7500 characters of complete C11 source.";return false;}
            string root;
            try{root=TLSPhase7WasmRunner.ToolchainRoot(dataPath);}
            catch(Exception e){Status=e.Message;return false;}
            LastFunction=practice.Selected;LastSource=practice.Draft;
            wasIndependent=!practice.Guided;wasExposed=practice.Exposed;previouslyAttempted=practice.AlreadyAttempted;
            submitted=practice;book=progress;
            IndependentJustRecorded=false;Latest=null;Busy=true;
            Status="Compiling C11, then executing eight independent restricted WASM cases...";
            cancellation=new CancellationTokenSource();
            CancellationToken token=cancellation.Token;
            TLSPhase7Function selected=LastFunction;string source=LastSource;
            task=Task.Run(()=>TLSPhase7WasmRunner.Run(selected,source,root,token));
            return true;
        }
        public bool Poll()
        {
            IndependentJustRecorded=false;
            if(!Busy||task==null||!task.IsCompleted)return false;
            Busy=false;
            try{Latest=task.GetAwaiter().GetResult();}
            catch(Exception e){Latest=new TLSPhase7WasmRunner.Result{Status="Runner exception: "+e.GetType().Name};}
            Status=Latest.Status;
            bool evaluated=Latest.CompilerRan&&(Latest.Completed||!Latest.Compiled);
            bool draftUnchanged=submitted!=null&&submitted.Selected==LastFunction&&submitted.Draft==LastSource;
            if(draftUnchanged)
            {
                submitted.Record(LastFunction,evaluated,Latest.AllPassed,wasIndependent,wasExposed);
                if(Latest.AllPassed&&book!=null)
                    IndependentJustRecorded=book.RecordCompilerCase(LastFunction.ToString(),TLSPhase7FunctionCatalog.Topic(LastFunction),
                        Latest.Compiled,Latest.Completed,Latest.AllPassed,!wasIndependent||wasExposed,previouslyAttempted,DateTime.UtcNow);
            }
            submitted=null;book=null;task=null;
            if(cancellation!=null){cancellation.Dispose();cancellation=null;}
            return true;
        }
        public void Cancel()
        {if(cancellation!=null)cancellation.Cancel();Status="Cancelling; no credit for incomplete attempts.";}
    }
}
