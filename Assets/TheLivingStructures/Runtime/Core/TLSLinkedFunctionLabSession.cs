using System;
namespace TLS.Core
{
    public sealed class TLSLinkedFunctionLabSession
    {
        private readonly string[] drafts = new string[TLSLinkedFunctionCatalog.Count];
        private readonly bool[] attempted=new bool[TLSLinkedFunctionCatalog.Count];
        private readonly bool[] exposed=new bool[TLSLinkedFunctionCatalog.Count];
        private readonly bool[] passing=new bool[TLSLinkedFunctionCatalog.Count];
        private readonly bool[] firstIndependent=new bool[TLSLinkedFunctionCatalog.Count];
        public TLSLinkedFunction Selected {get;private set;}=TLSLinkedFunction.Prepend;
        public bool Guided {get;private set;}=true;
        public bool HintVisible {get;private set;}
        public bool ReferenceVisible {get;private set;}
        public TLSLinkedFunctionLabSession(){for(int i=0;i<drafts.Length;i++)drafts[i]=TLSLinkedFunctionCatalog.Starter((TLSLinkedFunction)i);}
        public string Draft {get{return drafts[(int)Selected];}set{drafts[(int)Selected]=value??"";}}
        public void Select(TLSLinkedFunction f){if(!TLSLinkedFunctionCatalog.IsValid(f))throw new ArgumentOutOfRangeException("f");Selected=f;HintVisible=false;ReferenceVisible=false;}
        public void SetGuided(bool yes){Guided=yes;HintVisible=false;ReferenceVisible=false;}
        public void Hint(){if(Guided){exposed[(int)Selected]=true;HintVisible=true;}}
        public void Reveal(){exposed[(int)Selected]=true;Draft=TLSLinkedFunctionCatalog.Reference(Selected);ReferenceVisible=true;HintVisible=false;}
        public void Restore(){Draft=TLSLinkedFunctionCatalog.Starter(Selected);ReferenceVisible=false;HintVisible=false;}
        public void Record(TLSLinkedFunction f,bool evaluated,bool passed,bool independent)
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
