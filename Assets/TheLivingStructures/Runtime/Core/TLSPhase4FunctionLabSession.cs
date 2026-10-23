using System;
namespace TLS.Core
{
    public sealed class TLSPhase4FunctionLabSession
    {
        private readonly string[] drafts = new string[TLSPhase4FunctionCatalog.Count];
        private readonly bool[] attempted=new bool[TLSPhase4FunctionCatalog.Count];
        private readonly bool[] exposed=new bool[TLSPhase4FunctionCatalog.Count];
        private readonly bool[] passing=new bool[TLSPhase4FunctionCatalog.Count];
        private readonly bool[] firstIndependent=new bool[TLSPhase4FunctionCatalog.Count];
        public TLSPhase4Function Selected {get;private set;}=TLSPhase4Function.Insert;
        public bool Guided {get;private set;}=true;
        public bool HintVisible {get;private set;}
        public bool ReferenceVisible {get;private set;}
        public TLSPhase4FunctionLabSession(){for(int i=0;i<drafts.Length;i++)drafts[i]=TLSPhase4FunctionCatalog.Starter((TLSPhase4Function)i);}
        public string Draft {get{return drafts[(int)Selected];}set{drafts[(int)Selected]=value??"";}}
        public void Select(TLSPhase4Function f){if(!TLSPhase4FunctionCatalog.Valid(f))throw new ArgumentOutOfRangeException("f");Selected=f;HintVisible=false;ReferenceVisible=false;}
        public void SetGuided(bool yes){Guided=yes;HintVisible=false;ReferenceVisible=false;}
        public void Hint(){if(Guided){exposed[(int)Selected]=true;HintVisible=true;}}
        public void Reveal(){exposed[(int)Selected]=true;Draft=TLSPhase4FunctionCatalog.Reference(Selected);ReferenceVisible=true;HintVisible=false;}
        public void Restore(){Draft=TLSPhase4FunctionCatalog.Starter(Selected);ReferenceVisible=false;HintVisible=false;}
        public void Record(TLSPhase4Function f,bool evaluated,bool passed,bool independent)
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
