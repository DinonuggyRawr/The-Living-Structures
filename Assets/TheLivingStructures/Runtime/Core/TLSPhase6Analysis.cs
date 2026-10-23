using System;
using System.Collections.Generic;

namespace TLS.Core
{
    public enum TLSComplexityAlgorithm { ArrayAccess, LinearSearchMissing, BinarySearchMissing, MergeSortReverse, InsertionSortReverse }
    public sealed class TLSOperationCounts
    {
        public long Comparisons, Reads, Writes, Visits, Allocations;
        public long Total { get { return Comparisons + Reads + Writes + Visits + Allocations; } }
        public TLSOperationCounts Clone() { return (TLSOperationCounts)MemberwiseClone(); }
        public override string ToString() { return "Comparisons=" + Comparisons + "  reads=" + Reads + "  writes=" + Writes + "  visits=" + Visits + "  allocations=" + Allocations; }
    }
    public sealed class TLSAnalysisFrame
    {
        public readonly string Explanation;
        public readonly int Focus;
        public readonly TLSOperationCounts Counts;
        public readonly int[] Snapshot;
        public TLSAnalysisFrame(string why, int focus, TLSOperationCounts counts, int[] array)
        { Explanation = why; Focus = focus; Counts = counts.Clone(); Snapshot = (int[])array.Clone(); }
    }
    public sealed class TLSAnalysisTrace
    {
        public TLSComplexityAlgorithm Algorithm;
        public int Size;
        public readonly List<TLSAnalysisFrame> Frames = new List<TLSAnalysisFrame>();
        public TLSOperationCounts End { get { return Frames[Frames.Count - 1].Counts; } }
        public string GrowthClass { get { return TLSAnalysisEngine.Complexity(Algorithm); } }
        public string Scenario { get { return TLSAnalysisEngine.Scenario(Algorithm); } }
    }
    /// <summary>Deterministic model counters for explicitly documented C-like operations; never elapsed-time claims.</summary>
    public static class TLSAnalysisEngine
    {
        public static string Name(TLSComplexityAlgorithm a)
        {
            switch (a)
            {
                case TLSComplexityAlgorithm.ArrayAccess: return "ARRAY INDEX ACCESS";
                case TLSComplexityAlgorithm.LinearSearchMissing: return "LINEAR SEARCH (MISS)";
                case TLSComplexityAlgorithm.BinarySearchMissing: return "BINARY SEARCH (MISS)";
                case TLSComplexityAlgorithm.MergeSortReverse: return "MERGE SORT (REVERSE)";
                case TLSComplexityAlgorithm.InsertionSortReverse: return "INSERTION SORT (REVERSE)";
                default: throw new ArgumentOutOfRangeException("a");
            }
        }
        public static string Complexity(TLSComplexityAlgorithm a)
        {
            switch (a)
            {
                case TLSComplexityAlgorithm.ArrayAccess: return "O(1)";
                case TLSComplexityAlgorithm.LinearSearchMissing: return "O(n)";
                case TLSComplexityAlgorithm.BinarySearchMissing: return "O(log n)";
                case TLSComplexityAlgorithm.MergeSortReverse: return "O(n log n)";
                case TLSComplexityAlgorithm.InsertionSortReverse: return "O(n^2)";
                default: throw new ArgumentOutOfRangeException("a");
            }
        }
        public static string CSketch(TLSComplexityAlgorithm a)
        {
            switch (a)
            {
                case TLSComplexityAlgorithm.ArrayAccess: return "int item = a[n / 2];";
                case TLSComplexityAlgorithm.LinearSearchMissing: return "for (size_t i = 0; i < n; ++i) if (a[i] == target) return (int)i;";
                case TLSComplexityAlgorithm.BinarySearchMissing: return "mid = low + (high - low) / 2; /* requires sorted a[] */";
                case TLSComplexityAlgorithm.MergeSortReverse: return "merge_sort(a, 0, n); /* divide, merge, copy back */";
                case TLSComplexityAlgorithm.InsertionSortReverse: return "while (j > 0 && a[j-1] > key) { a[j] = a[j-1]; --j; }";
                default: throw new ArgumentOutOfRangeException("a");
            }
        }
        public static string SpaceComplexity(TLSComplexityAlgorithm a)
        { return a == TLSComplexityAlgorithm.MergeSortReverse ? "O(n) temp array" : "O(1) extra array space"; }
        public static string Scenario(TLSComplexityAlgorithm a)
        {
            switch (a)
            {
                case TLSComplexityAlgorithm.ArrayAccess: return "Read a[n/2] in bounds; array cells count as one read.";
                case TLSComplexityAlgorithm.LinearSearchMissing: return "Scan n distinct ascending values for absent n; comparisons and reads count.";
                case TLSComplexityAlgorithm.BinarySearchMissing: return "Search sorted ascending n values for absent n; midpoint checks count.";
                case TLSComplexityAlgorithm.MergeSortReverse: return "Merge-sort n descending values; compare, read and write elements.";
                case TLSComplexityAlgorithm.InsertionSortReverse: return "Insertion-sort n descending values; compare, read and shift elements.";
                default: throw new ArgumentOutOfRangeException("a");
            }
        }
        private static void Add(TLSAnalysisTrace trace, TLSOperationCounts c, int[] a, int focus, string text)
        {
            trace.Frames.Add(new TLSAnalysisFrame(text, focus, c, a));
        }
        public static TLSAnalysisTrace Build(TLSComplexityAlgorithm kind, int n)
        {
            if ((int)kind < 0 || (int)kind > 4) throw new ArgumentOutOfRangeException("kind");
            if (n < 2 || n > 64) throw new ArgumentOutOfRangeException("n", "Allowed 2 through 64 cells.");
            TLSAnalysisTrace t = new TLSAnalysisTrace { Algorithm = kind, Size = n };
            int[] a = new int[n];
            for (int i = 0; i < n; i++) a[i] = kind == TLSComplexityAlgorithm.MergeSortReverse || kind == TLSComplexityAlgorithm.InsertionSortReverse ? n - i : i;
            TLSOperationCounts c = new TLSOperationCounts();
            Add(t, c, a, -1, "Start: only defined array operations count. Setup is excluded from the counted operation.");
            if (kind == TLSComplexityAlgorithm.ArrayAccess)
            {
                c.Reads++; c.Visits++;
                Add(t, c, a, n/2, "Read a[n/2] by index; one element visit and one array read regardless of n.");
            }
            else if (kind == TLSComplexityAlgorithm.LinearSearchMissing)
            {
                for (int i = 0; i < n; i++)
                { c.Visits++; c.Reads++; c.Comparisons++; Add(t, c, a, i, "Compare a["+i+"] to absent target "+n+". Continue scanning."); }
                Add(t, c, a, -1, "Target absent after n comparisons. This is a worst-case linear search.");
            }
            else if (kind == TLSComplexityAlgorithm.BinarySearchMissing)
            {
                int low = 0, high = n-1;
                while (low <= high)
                {
                    int mid = low + (high-low)/2;
                    c.Visits++;c.Reads++;c.Comparisons++;
                    Add(t,c,a,mid,"Inspect sorted midpoint "+mid+"; "+a[mid]+" is less than absent target "+n+".");
                    low = mid+1;
                }
                Add(t,c,a,-1,"Target absent. Sorted input is an essential binary-search precondition.");
            }
            else if (kind == TLSComplexityAlgorithm.InsertionSortReverse)
            {
                for (int i = 1; i < n; i++)
                {
                    int key=a[i];c.Reads++; int j=i-1;
                    while (j>=0)
                    {
                        c.Reads++;c.Comparisons++;
                        if (a[j]<=key) break;
                        a[j+1]=a[j];c.Writes++;j--;
                    }
                    a[j+1]=key;c.Writes++;c.Visits++;
                    Add(t,c,a,i,"Insert original element "+i+" into sorted prefix; shifts/write costs accumulate.");
                }
                Add(t,c,a,-1,"Reverse input forces quadratic shifting in insertion sort.");
            }
            else
            {
                int[] temp=new int[n]; c.Allocations++; // One modeled temporary array allocation, not n heap allocations.
                Merge(a,temp,0,n,c,t);
                Add(t,c,a,-1,"Merging sorted runs produces O(n log n) comparisons and writes.");
            }
            return t;
        }
        private static void Merge(int[] a,int[] temp,int begin,int end,TLSOperationCounts c,TLSAnalysisTrace t)
        {
            if (end-begin<=1)return;
            int mid=begin+(end-begin)/2;
            Merge(a,temp,begin,mid,c,t);Merge(a,temp,mid,end,c,t);
            int i=begin,j=mid,k=begin;
            while(i<mid&&j<end)
            {
                c.Reads+=2;c.Comparisons++;
                temp[k++]=a[i]<=a[j]?a[i++]:a[j++];c.Writes++;
            }
            while(i<mid){temp[k++]=a[i++];c.Reads++;c.Writes++;}
            while(j<end){temp[k++]=a[j++];c.Reads++;c.Writes++;}
            for(k=begin;k<end;k++){a[k]=temp[k];c.Reads++;c.Writes++;}
            c.Visits++;
            Add(t,c,a,begin,"Merge ["+begin+","+mid+") and ["+mid+","+end+"). Each merge writes back to the array.");
        }
    }
    public sealed class TLSAnalysisSession
    {
        public TLSComplexityAlgorithm Selected { get; private set; } = TLSComplexityAlgorithm.LinearSearchMissing;
        public int Size { get; private set; } = 8;
        public TLSAnalysisTrace Trace { get; private set; }
        public int Step { get; private set; }
        public string Feedback { get; private set; } = "Choose an algorithm, then BUILD TRACE. Counters are not milliseconds.";
        public int? PredictedComparisons { get; private set; }
        public bool OutcomeSeen { get; private set; }
        public string PredictionFeedback { get; private set; } = "Prediction is formative and session-only.";
        public TLSAnalysisFrame Current { get { return Trace == null ? null : Trace.Frames[Step]; } }
        public void Select(TLSComplexityAlgorithm a) { if ((int)a<0||(int)a>4)throw new ArgumentOutOfRangeException("a");Selected=a;Trace=null;Step=0;PredictedComparisons=null;OutcomeSeen=false;Feedback="Algorithm selected. Build a new trace."; }
        public void SetSize(int n) { if(n!=4&&n!=8&&n!=16&&n!=32&&n!=64)throw new ArgumentOutOfRangeException("n");Size=n;Trace=null;Step=0;PredictedComparisons=null;OutcomeSeen=false;Feedback="Input size changed. Build a new trace."; }
        public void Build() { Trace=TLSAnalysisEngine.Build(Selected,Size);Step=0;PredictedComparisons=null;OutcomeSeen=false;PredictionFeedback="Make a comparison-count prediction before FINISH.";Feedback="Trace constructed from an explicit algorithm and input scenario."; }
        public bool Predict(int value)
        {
            if(Trace==null||Step!=0||OutcomeSeen||PredictedComparisons.HasValue||value<0||value>5000)return false;
            PredictedComparisons=value;PredictionFeedback="Prediction recorded. Execute before seeing the final comparison count.";return true;
        }
        private void Report()
        {
            if(Trace==null||Step!=Trace.Frames.Count-1)return;
            OutcomeSeen=true;
            PredictionFeedback=PredictedComparisons.HasValue?
                "Predicted "+PredictedComparisons.Value+" comparisons; observed "+Trace.End.Comparisons+" (model-defined comparisons)." :
                "No prediction submitted; observed comparisons: "+Trace.End.Comparisons+".";
            Feedback="Completed deterministic operation trace; not a benchmark of elapsed time.";
        }
        public void Next(){if(Trace!=null&&Step<Trace.Frames.Count-1){Step++;Report();}}
        public void Previous(){if(Trace!=null&&Step>0){Step--;Feedback="Viewing an earlier snapshot; authoritative array state is not mutated.";}}
        public void Finish(){if(Trace!=null){Step=Trace.Frames.Count-1;Report();}}
        public void Reset(){Trace=null;Step=0;PredictedComparisons=null;OutcomeSeen=false;Feedback="Trace reset. Nothing saved.";PredictionFeedback="Prediction is formative and session-only.";}
    }
}
