using System;
namespace TLS.Core
{
    public enum TLSPhase6Function { LinearSearch, BinarySearch, InsertionSort }
    public static class TLSPhase6FunctionCatalog
    {
        public const int Count=3;
        public static bool Valid(TLSPhase6Function f) { return (int)f>=0&&(int)f<3; }
        private static string Get(string[] a,TLSPhase6Function f){if(!Valid(f))throw new ArgumentOutOfRangeException("f");return a[(int)f];}
        public static string Name(TLSPhase6Function f){return Get(Names,f);}
        public static string Goal(TLSPhase6Function f){return Get(Goals,f);}
        public static string Hint(TLSPhase6Function f){return Get(Hints,f);}
        public static string Starter(TLSPhase6Function f){return Get(Starters,f);}
        public static string Reference(TLSPhase6Function f){return Get(References,f);}
        private static readonly string[] Names={"LINEAR SEARCH","BINARY SEARCH","INSERTION SORT"};
        private static readonly string[] Goals={
            "Find the FIRST matching element, or -1. Handle NULL/empty, and do not modify the array. O(n) worst-case.",
            "Search an ascending SORTED array; return matching index or -1. Handle NULL/empty. O(log n) worst-case.",
            "Sort a mutable array ascending in place. Return false for NULL when count>0; do not overwrite outside count. O(n^2) worst-case."};
        private static readonly string[] Hints={
            "Check all active indices i < count. Return the first match, including a last-slot match.",
            "Use low <= high and mid = low + (high - low) / 2; advance bounds after examining mid. Sorted input required.",
            "Save the current key, shift strictly larger predecessors right, then write key at j+1. Guard NULL when count>0."};
        private static readonly string[] Starters={
            "int linear_search(const int *data, size_t count, int target) {\n    /* TODO: return first index or -1, including NULL/empty guards. */\n    return -1;\n}",
            "int binary_search(const int *data, size_t count, int target) {\n    /* TODO: sorted ascending array, return index or -1. */\n    return -1;\n}",
            "bool insertion_sort(int *data, size_t count) {\n    /* TODO: stable ascending insertion sort in-place. */\n    return false;\n}"};
        private static readonly string[] References={
            "int linear_search(const int *data, size_t count, int target) {\n    if (data == NULL) return -1;\n    for (size_t i = 0; i < count; ++i) {\n        if (data[i] == target) return (int)i;\n    }\n    return -1;\n}",
            "int binary_search(const int *data, size_t count, int target) {\n    if (data == NULL || count == 0) return -1;\n    size_t lo = 0, hi = count;\n    while (lo < hi) {\n        size_t mid = lo + (hi - lo) / 2;\n        if (data[mid] == target) return (int)mid;\n        if (data[mid] < target) lo = mid + 1;\n        else hi = mid;\n    }\n    return -1;\n}",
            "bool insertion_sort(int *data, size_t count) {\n    if (data == NULL) return count == 0;\n    for (size_t i = 1; i < count; ++i) {\n        int key = data[i];\n        size_t j = i;\n        while (j > 0 && data[j - 1] > key) {\n            data[j] = data[j - 1];\n            --j;\n        }\n        data[j] = key;\n    }\n    return true;\n}"};
    }
    public sealed class TLSPhase6FunctionSession
    {
        private readonly string[] drafts=new string[3];
        private readonly bool[] attempted=new bool[3],exposed=new bool[3],passing=new bool[3],first=new bool[3];
        public TLSPhase6Function Selected {get;private set;}=TLSPhase6Function.LinearSearch;
        public bool Guided {get;private set;}=true;
        public bool HintVisible {get;private set;}
        public bool ReferenceVisible {get;private set;}
        public TLSPhase6FunctionSession(){for(int i=0;i<3;i++)drafts[i]=TLSPhase6FunctionCatalog.Starter((TLSPhase6Function)i);}
        public string Draft {get{return drafts[(int)Selected];}set{drafts[(int)Selected]=value??"";}}
        public void Select(TLSPhase6Function f){if(!TLSPhase6FunctionCatalog.Valid(f))throw new ArgumentOutOfRangeException("f");Selected=f;HintVisible=false;ReferenceVisible=false;}
        public void SetGuided(bool value){Guided=value;HintVisible=false;ReferenceVisible=false;}
        public void Hint(){if(Guided){exposed[(int)Selected]=true;HintVisible=true;}}
        public void Reveal(){exposed[(int)Selected]=true;Draft=TLSPhase6FunctionCatalog.Reference(Selected);ReferenceVisible=true;HintVisible=false;}
        public void Restore(){Draft=TLSPhase6FunctionCatalog.Starter(Selected);ReferenceVisible=false;HintVisible=false;}
        public void Record(TLSPhase6Function f,bool evaluated,bool passed,bool independent)
        {
            if(!evaluated)return;int i=(int)f;
            if(!attempted[i]&&!exposed[i]&&independent&&passed)first[i]=true;
            attempted[i]=true;if(passed)passing[i]=true;
        }
        public int Passed {get{int n=0;foreach(bool b in passing)if(b)n++;return n;}}
        public int Independent {get{int n=0;foreach(bool b in first)if(b)n++;return n;}}
    }
}
