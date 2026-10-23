using System;
namespace TLS.Core
{
    public sealed class TLSPhase5FunctionLabSession
    {
        private readonly string[] drafts = new string[TLSPhase5FunctionCatalog.Count];
        private readonly bool[] attempted=new bool[TLSPhase5FunctionCatalog.Count];
        private readonly bool[] exposed=new bool[TLSPhase5FunctionCatalog.Count];
        private readonly bool[] passing=new bool[TLSPhase5FunctionCatalog.Count];
        private readonly bool[] firstIndependent=new bool[TLSPhase5FunctionCatalog.Count];
        public TLSPhase5Function Selected {get;private set;}=TLSPhase5Function.HeapInsertMin;
        public bool Guided {get;private set;}=true;
        public bool HintVisible {get;private set;}
        public bool ReferenceVisible {get;private set;}
        public TLSPhase5FunctionLabSession(){for(int i=0;i<drafts.Length;i++)drafts[i]=TLSPhase5FunctionCatalog.Starter((TLSPhase5Function)i);}
        public string Draft {get{return drafts[(int)Selected];}set{drafts[(int)Selected]=value??"";}}
        public void Select(TLSPhase5Function f){if(!TLSPhase5FunctionCatalog.Valid(f))throw new ArgumentOutOfRangeException("f");Selected=f;HintVisible=false;ReferenceVisible=false;}
        public void SetGuided(bool yes){Guided=yes;HintVisible=false;ReferenceVisible=false;}
        public void Hint(){if(Guided){exposed[(int)Selected]=true;HintVisible=true;}}
        public void Reveal(){exposed[(int)Selected]=true;Draft=TLSPhase5FunctionCatalog.Reference(Selected);ReferenceVisible=true;HintVisible=false;}
        public void Restore(){Draft=TLSPhase5FunctionCatalog.Starter(Selected);ReferenceVisible=false;HintVisible=false;}
        public void Record(TLSPhase5Function f,bool evaluated,bool passed,bool independent)
        {
            if(!evaluated)return;
            int i=(int)f;
            if(!attempted[i] && !exposed[i] && independent && passed)firstIndependent[i]=true;
            attempted[i]=true;if(passed)passing[i]=true;
        }
        public int PassedFunctions {get{int n=0;foreach(bool b in passing)if(b)n++;return n;}}
        public int IndependentFunctions {get{int n=0;foreach(bool b in firstIndependent)if(b)n++;return n;}}
    }
}
