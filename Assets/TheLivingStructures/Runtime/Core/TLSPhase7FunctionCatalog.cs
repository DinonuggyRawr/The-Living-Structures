using System;
namespace TLS.Core
{
    public enum TLSPhase7Function { ArrayRemove, RecursiveCount }
    public static class TLSPhase7FunctionCatalog
    {
        public const int Count=2;
        public static bool Valid(TLSPhase7Function value){return (int)value>=0&&(int)value<Count;}
        public static string Name(TLSPhase7Function value)
        {return value==TLSPhase7Function.ArrayRemove?"REMOVE BY INDEX":value==TLSPhase7Function.RecursiveCount?"RECURSIVE TARGET COUNT":throw new ArgumentOutOfRangeException("value");}
        public static string Topic(TLSPhase7Function value){return value==TLSPhase7Function.ArrayRemove?"array":"recursion";}
        public static string Objective(TLSPhase7Function value)
        {return value==TLSPhase7Function.ArrayRemove?"Remove a specific array index by shifting remaining cells left. Reject invalid inputs without changing the array; preserve inactive raw cells.":
            "Count occurrences of a target across an array. Handle NULL for non-empty inputs; empty NULL input returns 0. Use a proper base case.";}
        public static string Hint(TLSPhase7Function value)
        {return value==TLSPhase7Function.ArrayRemove?"Check count pointer, data pointer, index < *count, and capacity before writes; move i+1 into i.":
            "For count == 0 return 0; reject NULL otherwise; compare first item then recurse with data + 1 and count - 1.";}
        public static string Starter(TLSPhase7Function value)
        {return value==TLSPhase7Function.ArrayRemove?
            "bool shift_left_remove(int *data, size_t *count, size_t capacity, size_t index) {\n    /* TODO: protect bounds, shift active cells left, and decrease count. */\n    return false;\n}":
            "int count_target_recursive(const int *data, size_t count, int target) {\n    /* TODO: handle empty, invalid input, and count matching values. */\n    return -1;\n}";}
        public static string Reference(TLSPhase7Function value)
        {return value==TLSPhase7Function.ArrayRemove?
            "bool shift_left_remove(int *data, size_t *count, size_t capacity, size_t index) {\n    if (data == NULL || count == NULL || *count > capacity || index >= *count) return false;\n    for (size_t i = index; i + 1 < *count; ++i) data[i] = data[i + 1];\n    --*count;\n    return true;\n}":
            "int count_target_recursive(const int *data, size_t count, int target) {\n    if (count == 0) return 0;\n    if (data == NULL) return -1;\n    int rest = count_target_recursive(data + 1, count - 1, target);\n    if (rest < 0) return -1;\n    return (data[0] == target ? 1 : 0) + rest;\n}";}
    }
    public sealed class TLSPhase7FunctionSession
    {
        private readonly string[] drafts=new string[2];
        private readonly bool[] attempted=new bool[2],exposed=new bool[2],passed=new bool[2],independent=new bool[2];
        public TLSPhase7Function Selected {get;private set;}
        public bool Guided {get;private set;}=true;
        public bool HintVisible {get;private set;}
        public bool ReferenceVisible {get;private set;}
        public TLSPhase7FunctionSession(){for(int i=0;i<2;i++)drafts[i]=TLSPhase7FunctionCatalog.Starter((TLSPhase7Function)i);}
        public string Draft {get{return drafts[(int)Selected];}set{drafts[(int)Selected]=value??"";}}
        public bool AlreadyAttempted {get{return attempted[(int)Selected];}}
        public bool Exposed {get{return exposed[(int)Selected];}}
        public void Select(TLSPhase7Function f){if(!TLSPhase7FunctionCatalog.Valid(f))throw new ArgumentOutOfRangeException("f");Selected=f;HintVisible=false;ReferenceVisible=false;}
        public void SetGuided(bool value){Guided=value;HintVisible=false;ReferenceVisible=false;}
        public void Hint(){if(Guided){exposed[(int)Selected]=true;HintVisible=true;}}
        public void Reveal(){exposed[(int)Selected]=true;Draft=TLSPhase7FunctionCatalog.Reference(Selected);ReferenceVisible=true;HintVisible=false;}
        public void Restore(){Draft=TLSPhase7FunctionCatalog.Starter(Selected);ReferenceVisible=false;HintVisible=false;}
        public bool Record(TLSPhase7Function selected,bool evaluated,bool success,bool wasIndependent,bool wasExposed)
        {
            if(!evaluated)return false;
            int i=(int)selected;
            bool firstClean=!attempted[i]&&!exposed[i]&&!wasExposed&&wasIndependent&&success;
            attempted[i]=true;if(success)passed[i]=true;if(firstClean)independent[i]=true;
            return firstClean;
        }
        public int Passed{get{int c=0;foreach(bool b in passed)if(b)c++;return c;}}
        public int Independent{get{int c=0;foreach(bool b in independent)if(b)c++;return c;}}
    }
}
