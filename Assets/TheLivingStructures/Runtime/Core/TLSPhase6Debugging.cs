using System;
using System.Collections.Generic;

namespace TLS.Core
{
    public enum TLSDebugFault { LinearOffByOne, BinarySkipMiddle, ShiftStopsAtOne, QueueMissingModulo, HashStopsAtTombstone }
    public sealed class TLSCaseComparison
    {
        public readonly string Label, Expected, Actual;
        public bool Pass { get { return Expected == Actual; } }
        public TLSCaseComparison(string label,string expected,string actual){Label=label;Expected=expected;Actual=actual;}
    }
    public sealed class TLSDebugScenario
    {
        public TLSDebugFault Fault;
        public string Title, BrokenC, CorrectC, Explanation, TestPrinciple;
        public readonly List<TLSCaseComparison> Cases = new List<TLSCaseComparison>();
        public int Failing { get { int n=0;foreach(TLSCaseComparison x in Cases)if(!x.Pass)n++;return n; } }
    }
    public static class TLSDebugCatalog
    {
        public static string Name(TLSDebugFault fault)
        {
            switch(fault)
            {
                case TLSDebugFault.LinearOffByOne:return "LINEAR LOOP BOUND";
                case TLSDebugFault.BinarySkipMiddle:return "BINARY SEARCH UPDATE";
                case TLSDebugFault.ShiftStopsAtOne:return "ARRAY SHIFT BOUND";
                case TLSDebugFault.QueueMissingModulo:return "QUEUE WRAPAROUND";
                case TLSDebugFault.HashStopsAtTombstone:return "HASH DELETION CHAIN";
                default:throw new ArgumentOutOfRangeException("fault");
            }
        }
        public static string CorrectivePrinciple(TLSDebugFault fault)
        {
            switch(fault)
            {
                case TLSDebugFault.LinearOffByOne:return "A: scan i < count, including the last cell";
                case TLSDebugFault.BinarySkipMiddle:return "B: after checking mid, use high = mid - 1";
                case TLSDebugFault.ShiftStopsAtOne:return "C: shift index zero when required";
                case TLSDebugFault.QueueMissingModulo:return "D: wrap physical tail with modulo capacity";
                case TLSDebugFault.HashStopsAtTombstone:return "E: continue probing through DELETED slots";
                default:throw new ArgumentOutOfRangeException("fault");
            }
        }
        private static int Linear(int[] a,int value,bool bug)
        {for(int i=0;i<a.Length-(bug?1:0);i++)if(a[i]==value)return i;return -1;}
        private static int Binary(int[] a,int value,bool bug)
        {
            int lo=0,hi=a.Length-1;
            while(lo<=hi)
            {
                int mid=lo+(hi-lo)/2;if(a[mid]==value)return mid;
                if(a[mid]<value)lo=mid+1;else hi=mid-(bug?2:1);
            }
            return -1;
        }
        private static string InsertAtFront(int[] a,bool bug)
        {
            // Input a[0..length-2] is active; insert 7 at index zero by shifting right.
            int n=a.Length-1;
            for(int j=n-1;j>=(bug?1:0);j--)a[j+1]=a[j];
            a[0]=7;return string.Join(",",a);
        }
        private static string Queue(int capacity,int head,int count,int value,bool bug)
        {
            int[] cells=new int[capacity];for(int i=0;i<count;i++)cells[(head+i)%capacity]=i+10;
            if(count==capacity)return "FULL";
            int tail=head+count;if(!bug)tail%=capacity;
            if(tail>=capacity)return "REJECTED_NO_WRAP";
            cells[tail]=value;
            // Print physical tail and logical FIFO sequence, not raw idle cells.
            string order="";for(int i=0;i<count+1;i++)order+=(i==0?"":",")+cells[(head+i)%capacity];
            return "tail="+tail+" FIFO="+order;
        }
        private static int HashFind(int[] keys,int[] kinds,int home,int key,bool bug)
        {
            for(int step=0;step<7;step++)
            {
                int index=(home+step)%7;
                if(kinds[index]==0)return -1;
                if(bug&&kinds[index]==2)return -1;
                if(kinds[index]==1&&keys[index]==key)return index;
            }
            return -1;
        }
        private static void Add(TLSDebugScenario s,string title,string correct,string wrong)
        {s.Cases.Add(new TLSCaseComparison(title,correct,wrong));}
        public static TLSDebugScenario Create(TLSDebugFault fault)
        {
            TLSDebugScenario s=new TLSDebugScenario{Fault=fault,Title=Name(fault)};
            if(fault==TLSDebugFault.LinearOffByOne)
            {
                s.BrokenC="for (size_t i = 0; i + 1 < count; ++i) if (a[i] == x) return i;";
                s.CorrectC="for (size_t i = 0; i < count; ++i) if (a[i] == x) return i;";
                s.Explanation="Off-by-one: the last logical cell is skipped. Empty arrays are guarded naturally by i < count.";
                s.TestPrinciple="Test empty, first, middle, last and missing values; include duplicates and negatives.";
                int[][] a={new int[0],new[]{3},new[]{3},new[]{3,5},new[]{3,5},new[]{3,5,8},new[]{-4,3,8},new[]{2,2,3}};
                int[] target={5,3,1,3,5,8,-4,3};
                for(int i=0;i<8;i++)Add(s,"case "+(i+1)+" target="+target[i],Linear(a[i],target[i],false).ToString(),Linear(a[i],target[i],true).ToString());
            }
            else if(fault==TLSDebugFault.BinarySkipMiddle)
            {
                s.BrokenC="if (a[mid] > x) high = mid - 2;";s.CorrectC="if (a[mid] > x) high = mid - 1;";
                s.Explanation="The incorrect upper bound can skip a valid candidate; the input must already be sorted.";
                s.TestPrinciple="Test first/last, unsuccessful lookups, short even/odd arrays, and negative values.";
                int[][] a={new int[0],new[]{1},new[]{1,2},new[]{1,2,3},new[]{1,2,3,4},new[]{1,2,3,4,5},new[]{-3,0,5,9,11,20},new[]{1,3,5,7,9,11,13}};
                int[] target={5,1,1,1,2,2,0,1};
                for(int i=0;i<8;i++)Add(s,"case "+(i+1)+" target="+target[i],Binary(a[i],target[i],false).ToString(),Binary(a[i],target[i],true).ToString());
            }
            else if(fault==TLSDebugFault.ShiftStopsAtOne)
            {
                s.BrokenC="for (int j = count - 1; j > 0; --j) a[j + 1] = a[j];";
                s.CorrectC="for (int j = count - 1; j >= 0; --j) a[j + 1] = a[j];";
                s.Explanation="When inserting at index zero, the former a[0] must move to a[1]. This variant intentionally models front insertion only.";
                s.TestPrinciple="Compare full raw-array state, not just count or the inserted first element.";
                int[][] a={new[]{0},new[]{2,0},new[]{2,4,0},new[]{2,4,6,0},new[]{-1,0},new[]{1,1,0},new[]{0,3,0},new[]{5,6,7,8,0}};
                for(int i=0;i<8;i++)Add(s,"front insert "+(i+1),InsertAtFront((int[])a[i].Clone(),false),InsertAtFront((int[])a[i].Clone(),true));
            }
            else if(fault==TLSDebugFault.QueueMissingModulo)
            {
                s.BrokenC="size_t tail = q->head + q->count; /* missing % CAPACITY */";
                s.CorrectC="size_t tail = (q->head + q->count) % CAPACITY;";
                s.Explanation="A circular queue reuses earlier physical slots after the head advances; raw tail may be outside the buffer.";
                s.TestPrinciple="Exercise wraparound and full/empty cases independently, not only queues starting at head zero.";
                int[] h={0,0,0,1,2,3,4,4};int[] counts={0,1,4,1,2,0,1,4};
                for(int i=0;i<8;i++)Add(s,"head="+h[i]+" count="+counts[i],Queue(5,h[i],counts[i],99,false),Queue(5,h[i],counts[i],99,true));
            }
            else if(fault==TLSDebugFault.HashStopsAtTombstone)
            {
                s.BrokenC="if (slot.state == DELETED) return false;";
                s.CorrectC="if (slot.state == DELETED) continue; /* probe until EMPTY */";
                s.Explanation="DELETED is not EMPTY; a later colliding key may still be reachable beyond this tombstone.";
                s.TestPrinciple="Test collision chains with tombstones at home, middle and after wraparound.";
                for(int i=0;i<8;i++)
                {
                    int[] k=new int[7];int[] state=new int[7];int home=i%7;int target=i*7+home;
                    if(i==0){state[home]=0;}
                    else if(i==1){state[home]=1;k[home]=target;}
                    else if(i==2){state[home]=2;state[(home+1)%7]=1;k[(home+1)%7]=target;}
                    else if(i==3){state[home]=1;k[home]=111;state[(home+1)%7]=2;state[(home+2)%7]=1;k[(home+2)%7]=target;}
                    else if(i==4){state[home]=2;state[(home+1)%7]=1;k[(home+1)%7]=target;}
                    else if(i==5){state[home]=1;k[home]=target;state[(home+1)%7]=2;}
                    else if(i==6){state[home]=2;state[(home+1)%7]=0;}
                    else {state[home]=1;k[home]=100;state[(home+1)%7]=2;state[(home+2)%7]=1;k[(home+2)%7]=target;}
                    Add(s,"probe chain "+(i+1),HashFind(k,state,home,target,false).ToString(),HashFind(k,state,home,target,true).ToString());
                }
            }
            else throw new ArgumentOutOfRangeException("fault");
            return s;
        }
    }
    public sealed class TLSDebugSession
    {
        public TLSDebugFault Selected {get;private set;}
        public TLSDebugScenario Scenario {get;private set;}=TLSDebugCatalog.Create(TLSDebugFault.LinearOffByOne);
        public bool Guided {get;private set;}=true;
        public bool HintsUsed {get;private set;}
        public bool Revealed {get;private set;}
        private readonly bool[] attempted=new bool[5],independent=new bool[5];
        public string Feedback {get;private set;}="Run cases to expose differences between the reference and the seeded bug.";
        public bool Ran {get;private set;}
        public int Passed {get {int n=0;foreach(bool b in independent)if(b)n++;return n;}}
        public void Select(TLSDebugFault x){if((int)x<0||(int)x>4)throw new ArgumentOutOfRangeException("x");Selected=x;Scenario=TLSDebugCatalog.Create(x);Ran=false;HintsUsed=false;Revealed=false;Feedback="New bug scenario. Run cases to investigate.";}
        public void SetGuided(bool b){Guided=b;Feedback=b?"Guided: a hint is available.":"Independent: hints unavailable; prior exposure remains recorded.";}
        public void Run(){Ran=true;Feedback=Scenario.Failing+" of "+Scenario.Cases.Count+" seeded-case results differ from the correct reference. No arbitrary C compiled.";}
        public void Hint(){if(Guided){HintsUsed=true;Feedback=Scenario.TestPrinciple;}}
        public void Reveal(){Revealed=true;Feedback="FIX: "+Scenario.CorrectC+"  WHY: "+Scenario.Explanation;}
        public bool Submit(string answer)
        {
            if(!Ran){Feedback="Run the tests first; inspect the failing output.";return false;}
            if(string.IsNullOrWhiteSpace(answer)){Feedback="Choose an explanation A–E.";return false;}
            string expected=((char)('A'+(int)Selected)).ToString();bool correct=string.Equals(answer.Trim(),expected,StringComparison.OrdinalIgnoreCase);
            if(!attempted[(int)Selected]&&!Guided&&!HintsUsed&&!Revealed&&correct)independent[(int)Selected]=true;
            attempted[(int)Selected]=true;
            Feedback=correct?"Correct diagnosis: "+Scenario.Explanation:"Not quite. Compare the first failing case to the reference, or ask for a guided hint.";
            return correct;
        }
    }
}
