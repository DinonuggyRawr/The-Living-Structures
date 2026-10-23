using System;
using System.Collections.Generic;
using System.Linq;

namespace TLS.Core
{
    public sealed class TLSPhase5Frame<T>
    {
        public readonly T State;
        public readonly string Why;
        public readonly string Frontier;
        public readonly int Focus;
        public readonly int Line;
        public TLSPhase5Frame(T state, string why, int focus = -1, int line = 0, string frontier = "")
        { State = state; Why = why; Focus = focus; Line = line; Frontier = frontier; }
    }
    public sealed class TLSPhase5Trace<T>
    {
        public readonly string Name;
        public readonly string[] Code;
        public readonly List<TLSPhase5Frame<T>> Frames = new List<TLSPhase5Frame<T>>();
        public bool Success;
        public string Output = "";
        public TLSPhase5Trace(string name, params string[] code) { Name = name; Code = code; }
    }
    public abstract class TLSPhase5Session<T>
    {
        protected T live;
        public TLSPhase5Trace<T> Trace { get; protected set; }
        public int Step { get; private set; }
        public string Diagnostic { get; protected set; } = "Choose an operation to see its C algorithm and state changes.";
        public string Prediction { get; private set; } = "Before running an operation, predict whether the element count changes.";
        public bool Predicted { get; private set; }
        private int predictedChange;
        protected abstract T Copy(T source);
        protected abstract int Count(T source);
        public T Current { get { return live; } }
        public T Visible { get { return Trace == null ? live : Trace.Frames[Step].State; } }
        public TLSPhase5Frame<T> Frame { get { return Trace == null ? null : Trace.Frames[Step]; } }
        protected TLSPhase5Session(T initial) { live = initial; }
        protected TLSPhase5Trace<T> Start(string name, params string[] code)
        {
            TLSPhase5Trace<T> t = new TLSPhase5Trace<T>(name, code);
            t.Frames.Add(new TLSPhase5Frame<T>(Copy(live), "Beginning state: no edits committed yet."));
            Trace = t; Step = 0; Predicted = false;
            Prediction = "Before stepping, predict MORE, FEWER or SAME elements.";
            Diagnostic = name + " — trace ready.";
            return t;
        }
        protected void Add(TLSPhase5Trace<T> t, T state, string why, int focus = -1, int line = 0, string frontier = "")
        { t.Frames.Add(new TLSPhase5Frame<T>(Copy(state), why, focus, line, frontier)); }
        protected void Done(TLSPhase5Trace<T> t, bool success, string result)
        {
            t.Success = success; t.Output = result;
            if (t.Frames.Count == 1) Add(t, live, result);
            Diagnostic = result;
        }
        public void Reject(string message) { Diagnostic = message; }
        public void ClearCompletedTrace() { if (Trace != null && Step == Trace.Frames.Count - 1) { Trace = null; Step = 0; Predicted = false; } }
        public void Next()
        {
            if (Trace == null || Step >= Trace.Frames.Count - 1) return;
            ++Step;
            if (Step == Trace.Frames.Count - 1)
            {
                if (Trace.Success) live = Copy(Trace.Frames[Step].State);
                if (Predicted)
                {
                    int actual = Math.Sign(Count(live) - Count(Trace.Frames[0].State));
                    Prediction = (predictedChange == actual ? "Prediction matched." : "Prediction differed.") +
                        " Actual: " + (actual > 0 ? "MORE" : actual < 0 ? "FEWER" : "SAME") +
                        ". Check the guard, count update, and memory trace.";
                }
            }
        }
        public void Previous()
        {
            if (Trace == null || Step == 0) return;
            if (Step == Trace.Frames.Count - 1 && Trace.Success) live = Copy(Trace.Frames[0].State);
            --Step;
        }
        public void Finish() { if (Trace != null) while (Step < Trace.Frames.Count - 1) Next(); }
        public void ResetTrace() { if (Trace != null && Trace.Success && Step == Trace.Frames.Count - 1) live = Copy(Trace.Frames[0].State); Trace = null; Step = 0; Predicted = false; }
        public void Predict(int countDelta)
        {
            if (Trace == null || Step != 0 || Predicted || countDelta < -1 || countDelta > 1) return;
            Predicted = true;
            predictedChange = countDelta;
            Prediction = "Prediction recorded. Execute the full trace to reveal causal feedback.";
        }
    }
    public enum TLSHeapKind { Min, Max }
    public enum TLSHeapAction { Insert, Extract, BuildExample, Reset }
    public sealed class TLSHeapState
    {
        public readonly List<int> Data = new List<int>();
        public TLSHeapKind Kind;
        public const int Capacity = 15;
        public TLSHeapState Copy() { TLSHeapState c = new TLSHeapState(); c.Kind = Kind; c.Data.AddRange(Data); return c; }
        public bool Valid()
        {
            if (Data.Count > Capacity) return false;
            for (int i=1;i<Data.Count;i++) if (Higher(Data[i],Data[(i-1)/2])) return false;
            return true;
        }
        public bool Higher(int a,int b) { return Kind == TLSHeapKind.Min ? a < b : a > b; }
    }
    public sealed class TLSHeapSession : TLSPhase5Session<TLSHeapState>
    {
        public TLSHeapSession() : base(new TLSHeapState()) { }
        protected override TLSHeapState Copy(TLSHeapState s) { return s.Copy(); }
        protected override int Count(TLSHeapState s) { return s.Data.Count; }
        public void Select(TLSHeapKind kind)
        {
            ResetTrace(); live = new TLSHeapState { Kind = kind };
            Diagnostic = "Switched heap order and reset nodes. No C memory is allocated.";
        }
        public void Begin(TLSHeapAction op,int value = 0)
        {
            if (!live.Valid()) throw new InvalidOperationException("Heap invariant was broken before the operation.");
            TLSHeapState work = live.Copy();
            TLSPhase5Trace<TLSHeapState> t = Start(op.ToString(),
                "// Array-backed binary heap (modeled C11 operations)",
                "if (size == capacity) return false; // guard",
                "data[size++] = value; // or replace the root",
                "while (child outranks parent) swap(child, parent);",
                "while (parent outranked by best child) swap(parent, best);",
                "// Finished heap satisfies parent-priority invariant");
            if (op == TLSHeapAction.Reset) { work.Data.Clear(); Add(t,work,"Reset the heap's logical size to zero.", -1,5); Done(t,true,"Heap reset."); return; }
            if (op == TLSHeapAction.BuildExample)
            {
                work.Data.Clear(); work.Data.AddRange(new[]{9,4,7,1,3,6,2});
                Add(t,work,"Load unsorted sample into contiguous storage; not a heap yet.",-1,2);
                for(int i=work.Data.Count/2-1;i>=0;i--) SiftDown(work,i,t);
                Done(t,true,"Bottom-up heapify completes in O(n) time.");return;
            }
            if (op == TLSHeapAction.Insert)
            {
                if(work.Data.Count==TLSHeapState.Capacity){Add(t,work,"Heap full: reject without writing outside the array.",-1,1);Done(t,false,"Insertion rejected: capacity reached.");return;}
                work.Data.Add(value);Add(t,work,"Append at the next free array position.",work.Data.Count-1,2);
                int i=work.Data.Count-1;
                while(i>0)
                {
                    int p=(i-1)/2;if(!work.Higher(work.Data[i],work.Data[p]))break;
                    int tmp=work.Data[i];work.Data[i]=work.Data[p];work.Data[p]=tmp;
                    Add(t,work,"Swap with parent because the child has higher heap priority.",p,3);i=p;
                }
                Done(t,true,"Heap insertion complete: O(log n) comparisons/swaps.");return;
            }
            if(work.Data.Count==0){Add(t,work,"Empty heap: extraction has no value to return.",-1,1);Done(t,false,"Extraction rejected: empty heap.");return;}
            int extracted=work.Data[0];int last=work.Data.Count-1;
            work.Data[0]=work.Data[last];work.Data.RemoveAt(last);
            Add(t,work,"Remove highest-priority root and move last array element to index 0.",work.Data.Count>0?0:-1,2);
            if(work.Data.Count>0)SiftDown(work,0,t);
            Done(t,true,"Extracted "+extracted+"; heap order restored in O(log n).");
        }
        private static void SiftDown(TLSHeapState s,int start,TLSPhase5Trace<TLSHeapState> t)
        {
            int i=start;
            while(2*i+1<s.Data.Count)
            {
                int best=2*i+1, other=best+1;
                if(other<s.Data.Count&&s.Higher(s.Data[other],s.Data[best]))best=other;
                if(!s.Higher(s.Data[best],s.Data[i]))break;
                int swap=s.Data[i];s.Data[i]=s.Data[best];s.Data[best]=swap;
                // A tiny copy is intentional: each frame remains inspectable even after later swaps.
                t.Frames.Add(new TLSPhase5Frame<TLSHeapState>(s.Copy(),"Swap parent with its highest-priority child.",best,4));i=best;
            }
        }
    }
    public enum TLSGraphAction { AddVertex, AddEdge, RemoveEdge, BFS, DFS, Example, Clear }
    public sealed class TLSGraphState
    {
        public const int Capacity=9;
        public int Vertices;
        public bool Directed;
        public readonly bool[,] Edges=new bool[Capacity,Capacity];
        public TLSGraphState Copy()
        {
            TLSGraphState r=new TLSGraphState{Vertices=Vertices,Directed=Directed};
            Array.Copy(Edges,r.Edges,Edges.Length);return r;
        }
        public int EdgeCount
        {
            get {int n=0;for(int i=0;i<Vertices;i++)for(int j=0;j<Vertices;j++)if(Edges[i,j])n++;return Directed?n:n/2;}
        }
    }
    public sealed class TLSGraphSession : TLSPhase5Session<TLSGraphState>
    {
        public TLSGraphSession():base(new TLSGraphState()) { Begin(TLSGraphAction.Example);Finish();ClearCompletedTrace(); }
        protected override TLSGraphState Copy(TLSGraphState s){return s.Copy();}
        protected override int Count(TLSGraphState s){return s.Vertices;}
        public void SetDirected(bool directed)
        {
            ResetTrace();live=new TLSGraphState{Directed=directed};Diagnostic="Graph direction changed; old edges cleared rather than silently reinterpreted.";
        }
        public static int Id(string label)
        {
            if(string.IsNullOrWhiteSpace(label)||label.Trim().Length!=1)return -1;
            char c=char.ToUpperInvariant(label.Trim()[0]);return c>='A'&&c<='I'?c-'A':-1;
        }
        public void Begin(TLSGraphAction op,string a="A",string b="B")
        {
            TLSGraphState w=live.Copy();int from=Id(a),to=Id(b);
            TLSPhase5Trace<TLSGraphState> t=Start(op.ToString(),
                "// Graph uses a fixed adjacency matrix in this lesson",
                "if (vertex >= n) return false; // validate",
                "adj[u][v] = 1; // plus reverse for undirected",
                "visited[start] = true; push_frontier(start);",
                "while (frontier_not_empty) { u = take_next();",
                "    for each neighbor in ascending label order: visit / queue;",
                "} // O(V + E) for adjacency lists; matrix scan O(V^2)");
            if(op==TLSGraphAction.Clear) { w=new TLSGraphState{Directed=w.Directed};Add(t,w,"Remove all vertices and edges.",-1,0);Done(t,true,"Graph cleared.");return; }
            if(op==TLSGraphAction.Example)
            {
                w=new TLSGraphState{Vertices=6,Directed=w.Directed};
                int[,] connections={{0,1},{0,2},{1,3},{2,3},{3,4},{4,5}};
                for(int i=0;i<connections.GetLength(0);i++)Link(w,connections[i,0],connections[i,1],true);
                Add(t,w,"Load six-node example. All arrows follow directed/undirected setting.",-1,2);Done(t,true,"Graph example loaded.");return;
            }
            if(op==TLSGraphAction.AddVertex)
            {
                if(w.Vertices>=TLSGraphState.Capacity){Add(t,w,"Maximum of nine teaching vertices reached.",-1,1);Done(t,false,"No vertex added: graph full.");return;}
                int id=w.Vertices++;Add(t,w,"Create vertex "+(char)('A'+id)+" with no incident edges.",id,1);Done(t,true,"Vertex added.");return;
            }
            if(from<0||from>=w.Vertices||(op!=TLSGraphAction.BFS&&op!=TLSGraphAction.DFS&&(to<0||to>=w.Vertices||from==to)))
            {Add(t,w,"Invalid or missing vertex label; edges cannot be self-links in this beginner model.",-1,1);Done(t,false,"Select existing labels (A-I). No edits committed.");return;}
            if(op==TLSGraphAction.AddEdge||op==TLSGraphAction.RemoveEdge)
            {
                bool inserting=op==TLSGraphAction.AddEdge;
                if(w.Edges[from,to]==inserting){Add(t,w,"Graph edge already has that state.",from,2);Done(t,false,"No change: edge already "+(inserting?"present.":"absent."));return;}
                Link(w,from,to,inserting);Add(t,w,"Update adjacency "+a.ToUpperInvariant()+" to "+b.ToUpperInvariant()+
                    (w.Directed?" (directed).":" and the symmetric cell."),from,2);
                Done(t,true,"Graph edge updated.");return;
            }
            List<int> visited=new List<int>();
            bool[] discovered=new bool[w.Vertices];
            if(op==TLSGraphAction.BFS)
            {
                Queue<int> pending=new Queue<int>();pending.Enqueue(from);discovered[from]=true;
                Add(t,w,"BFS marks the start when enqueued to avoid duplicates.",from,3,Frontier(pending));
                while(pending.Count>0)
                {
                    int v=pending.Dequeue();visited.Add(v);
                    for(int j=0;j<w.Vertices;j++)if(w.Edges[v,j]&&!discovered[j]){discovered[j]=true;pending.Enqueue(j);}
                    Add(t,w,"Visit "+(char)('A'+v)+"; enqueue its undiscovered neighbors in label order.",v,5,Frontier(pending));
                }
            }
            else
            {
                Stack<int> pending=new Stack<int>();pending.Push(from);
                Add(t,w,"DFS uses a LIFO frontier. Reverse neighbor pushes give ascending visit order.",from,3,Frontier(pending));
                while(pending.Count>0)
                {
                    int v=pending.Pop();if(discovered[v])continue;
                    discovered[v]=true;visited.Add(v);
                    for(int j=w.Vertices-1;j>=0;j--)if(w.Edges[v,j]&&!discovered[j])pending.Push(j);
                    Add(t,w,"Visit "+(char)('A'+v)+"; push neighbors onto the DFS stack.",v,5,Frontier(pending));
                }
            }
            string output=string.Join(" -> ",visited.Select(i=>((char)('A'+i)).ToString()).ToArray());
            Done(t,true,op+" visit order: "+output+". Disconnected vertices stay unvisited.");
        }
        private static string Frontier(IEnumerable<int> pending){return "FRONTIER: "+string.Join(", ",pending.Select(i=>((char)('A'+i)).ToString()).ToArray());}
        private static void Link(TLSGraphState s,int a,int b,bool enabled){s.Edges[a,b]=enabled;if(!s.Directed)s.Edges[b,a]=enabled;}
    }
    public enum TLSHashKind { Chaining, LinearProbing }
    public enum TLSHashAction { Insert, Find, Remove, Reset, CollisionExample }
    public enum TLSHashSlotKind { Empty, Occupied, Deleted }
    public sealed class TLSHashState
    {
        public const int Capacity=7;
        public TLSHashKind Kind;
        public readonly List<int>[] Chains = new List<int>[Capacity];
        public readonly int[] Values = new int[Capacity];
        public readonly TLSHashSlotKind[] Kinds = new TLSHashSlotKind[Capacity];
        public TLSHashState(){for(int i=0;i<Capacity;i++)Chains[i]=new List<int>();}
        public TLSHashState Copy()
        {
            TLSHashState s=new TLSHashState{Kind=Kind};
            for(int i=0;i<Capacity;i++){s.Chains[i].AddRange(Chains[i]);s.Values[i]=Values[i];s.Kinds[i]=Kinds[i];}
            return s;
        }
        public int Count
        {
            get {int c=0;for(int i=0;i<Capacity;i++)c+=Kind==TLSHashKind.Chaining?Chains[i].Count:(Kinds[i]==TLSHashSlotKind.Occupied?1:0);return c;}
        }
        public static int Bucket(int key){int h=key%Capacity;return h<0?h+Capacity:h;}
    }
    public sealed class TLSHashSession:TLSPhase5Session<TLSHashState>
    {
        public TLSHashSession():base(new TLSHashState()){}
        protected override TLSHashState Copy(TLSHashState s){return s.Copy();}
        protected override int Count(TLSHashState s){return s.Count;}
        public void Select(TLSHashKind kind){ResetTrace();live=new TLSHashState{Kind=kind};Diagnostic="Collision strategy changed; table reset (old entries not converted).";}
        public void Begin(TLSHashAction action,int key=0)
        {
            TLSHashState w=live.Copy();int bucket=TLSHashState.Bucket(key);
            TLSPhase5Trace<TLSHashState> t=Start(action.ToString(),
                "// Integer-key set; hash(key) = normalized key % 7",
                "index = ((key % 7) + 7) % 7; // negative-safe",
                "if (key exists) reject duplicate;",
                "// chaining: traverse bucket list",
                "// probing: inspect slots, preserve tombstones",
                "// insert / find / delete; verify probe termination");
            if(action==TLSHashAction.Reset){w=new TLSHashState{Kind=w.Kind};Add(t,w,"All buckets become empty.",-1,0);Done(t,true,"Hash table reset.");return;}
            if(action==TLSHashAction.CollisionExample)
            {
                w=new TLSHashState{Kind=w.Kind};
                foreach(int k in new[]{1,8,15,-6}) Insert(w,k,t);
                Done(t,true,"Four keys hash to bucket 1; compare chaining versus probing.");return;
            }
            Add(t,w,"Compute home bucket "+bucket+" from normalized modulo.",bucket,1);
            if(w.Kind==TLSHashKind.Chaining)
            {
                List<int> chain=w.Chains[bucket];int pos=chain.IndexOf(key);
                Add(t,w,"Traverse bucket "+bucket+"; collisions are separate linked-chain entries.",bucket,3);
                if(action==TLSHashAction.Insert)
                {
                    if(pos>=0||w.Count>=21){Done(t,false,pos>=0?"Duplicate key rejected.":"Teaching chain-entry limit reached.");return;}
                    chain.Add(key);Add(t,w,"Append the key to this bucket's conceptual collision chain.",bucket,5);Done(t,true,"Inserted "+key+" in O(1) expected hashing, O(n) worst-case chain search.");return;
                }
                if(action==TLSHashAction.Find){Done(t,true,pos>=0?"FOUND "+key+" in bucket "+bucket+".":"NOT FOUND "+key+"; traversal ended.");return;}
                if(pos<0){Done(t,false,"NOT FOUND; no entry removed.");return;}
                chain.RemoveAt(pos);Add(t,w,"Unlink matching chain entry without changing other keys.",bucket,5);Done(t,true,"Removed key "+key+" from collision chain.");return;
            }
            int firstTombstone=-1,found=-1,empty=-1;
            for(int offset=0;offset<TLSHashState.Capacity;offset++)
            {
                int i=(bucket+offset)%TLSHashState.Capacity;
                Add(t,w,"Probe slot "+i+" (offset "+offset+").",i,4);
                if(w.Kinds[i]==TLSHashSlotKind.Occupied&&w.Values[i]==key){found=i;break;}
                if(w.Kinds[i]==TLSHashSlotKind.Deleted&&firstTombstone<0)firstTombstone=i;
                if(w.Kinds[i]==TLSHashSlotKind.Empty){empty=i;break;}
            }
            if(action==TLSHashAction.Find){Done(t,true,found>=0?"FOUND "+key+" at slot "+found+".":"NOT FOUND after probing.");return;}
            if(action==TLSHashAction.Remove)
            {
                if(found<0){Done(t,false,"NOT FOUND; cannot remove absent key.");return;}
                w.Kinds[found]=TLSHashSlotKind.Deleted;
                Add(t,w,"Mark slot DELETED; setting EMPTY would break later probe chains.",found,5);
                Done(t,true,"Removed key with a tombstone.");return;
            }
            if(found>=0){Done(t,false,"Duplicate key rejected; existing table unchanged.");return;}
            int chosen=firstTombstone>=0?firstTombstone:empty;
            if(chosen<0){Done(t,false,"Table full; no empty or deleted slot.");return;}
            w.Kinds[chosen]=TLSHashSlotKind.Occupied;w.Values[chosen]=key;
            Add(t,w,"Use "+(firstTombstone>=0?"reusable tombstone":"first empty slot")+" at index "+chosen+".",chosen,5);
            Done(t,true,"Inserted key "+key+" with linear probing.");
        }
        private static void Insert(TLSHashState w,int key,TLSPhase5Trace<TLSHashState> t)
        {
            int b=TLSHashState.Bucket(key);
            if(w.Kind==TLSHashKind.Chaining){w.Chains[b].Add(key);AddStatic(t,w,"Add key "+key+" to chain "+b+".",b);return;}
            for(int i=0;i<TLSHashState.Capacity;i++)
            {
                int k=(b+i)%TLSHashState.Capacity;
                if(w.Kinds[k]!=TLSHashSlotKind.Occupied){w.Kinds[k]=TLSHashSlotKind.Occupied;w.Values[k]=key;AddStatic(t,w,"Probe from bucket "+b+"; place "+key+" at "+k+".",k);return;}
            }
        }
        private static void AddStatic(TLSPhase5Trace<TLSHashState> t,TLSHashState s,string why,int focus)
        {t.Frames.Add(new TLSPhase5Frame<TLSHashState>(s.Copy(),why,focus,5));}
    }
}
