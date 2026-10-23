using System;
using System.Text.RegularExpressions;

namespace TLS.Core
{
    public enum TLSPhase5Domain { Heap, Graph, Hash }
    public enum TLSPhase5QuestionType { Trace, Complete, Repair, Challenge }
    public sealed class TLSPhase5Question
    {
        public readonly TLSPhase5Domain Domain;
        public readonly TLSPhase5QuestionType Type;
        public readonly string Title, Prompt, Answer, Hint, Explanation;
        public TLSPhase5Question(TLSPhase5Domain domain,TLSPhase5QuestionType type,string title,string prompt,string answer,string hint,string explanation)
        {Domain=domain;Type=type;Title=title;Prompt=prompt;Answer=answer;Hint=hint;Explanation=explanation;}
    }
    public static class TLSPhase5PracticeCatalog
    {
        private static TLSPhase5Question Q(TLSPhase5Domain domain,TLSPhase5QuestionType kind,string title,string prompt,string answer,string hint,string why)
        {return new TLSPhase5Question(domain,kind,title,prompt,answer,hint,why);}
        public static readonly TLSPhase5Question[] All = {
            Q(TLSPhase5Domain.Heap,TLSPhase5QuestionType.Trace,"Min-heap parent","In a 0-based array heap, what is the parent index of child index 6?","2","Compute (i - 1) / 2 in integer arithmetic.","(6 - 1) / 2 truncates to index 2."),
            Q(TLSPhase5Domain.Heap,TLSPhase5QuestionType.Trace,"Min-heap insertion","Insert 2 into min-heap array [3, 5, 4]. Which value becomes the root?","2","New entry compares with its parent.","Sift-up places 2 above 5 and then 3."),
            Q(TLSPhase5Domain.Heap,TLSPhase5QuestionType.Complete,"Child index","Complete C: size_t left = ____;","2 * i + 1","0-based array representation.","A complete binary tree has left child 2*i+1."),
            Q(TLSPhase5Domain.Heap,TLSPhase5QuestionType.Complete,"Parent index","Complete C: size_t parent = ____;","(i - 1) / 2","Only use this for i > 0.","Integer truncation returns the parent index."),
            Q(TLSPhase5Domain.Heap,TLSPhase5QuestionType.Repair,"Empty extract guard","Repair: if (heap->size < 0) return false; for an unsigned size_t.","if (heap->size == 0) return false;","Unsigned size can never be negative.","The correct empty-heap guard prevents reading data[0]."),
            Q(TLSPhase5Domain.Heap,TLSPhase5QuestionType.Repair,"Incorrect priority","Repair min-heap sift-up: if (data[parent] <= data[child]) swap(...); Write proper early break guard.","if (data[parent] <= data[child]) break;","When parent already <= child, heap priority holds.","Swapping ordered pairs would break the min-heap invariant."),
            Q(TLSPhase5Domain.Heap,TLSPhase5QuestionType.Challenge,"Heap is not sorted","Is the min heap [1, 4, 2, 8, 6, 3] a valid min-heap? Answer 1 for yes / 0 for no.","1","Compare every child with its parent.","Every parent is <= children, though the array is not globally sorted."),
            Q(TLSPhase5Domain.Heap,TLSPhase5QuestionType.Challenge,"Heapify complexity","Is bottom-up heapify O(n log n) worst-case? Answer 1 for yes or 0 for no.","0","Most sift-down calls occur near leaves.","Bottom-up heap construction is O(n); repeated insertion is O(n log n)."),
            Q(TLSPhase5Domain.Graph,TLSPhase5QuestionType.Trace,"BFS order","Undirected edges A-B, A-C, B-D, C-D. BFS starting A with alphabetic neighbors: what is the FOURTH visited vertex?","D","Use FIFO queue; mark when enqueued.","Order A,B,C,D; D is not queued twice."),
            Q(TLSPhase5Domain.Graph,TLSPhase5QuestionType.Trace,"Directed reachability","Directed edges A->B and C->A. From A, how many vertices are reachable including A?","2","Reverse edges do not exist automatically.","A reaches B but cannot traverse C->A backward."),
            Q(TLSPhase5Domain.Graph,TLSPhase5QuestionType.Complete,"Mark visited","Complete C: visited[next] = ____;","true","Do this before enqueuing in BFS.","Marking on enqueue prevents duplicate frontier entries."),
            Q(TLSPhase5Domain.Graph,TLSPhase5QuestionType.Complete,"Queue operation","Complete BFS: queue[tail++] = ____;","start","Start with the source vertex.","The source enters the FIFO queue first."),
            Q(TLSPhase5Domain.Graph,TLSPhase5QuestionType.Repair,"Directed edge","Repair directed add-edge which erroneously adds the reverse: adj[v][u] = true; Write a C statement updating only u to v.","adj[u][v] = true;","Only specified direction should be present.","Directed graphs distinguish u->v from v->u."),
            Q(TLSPhase5Domain.Graph,TLSPhase5QuestionType.Repair,"Wrong BFS frontier","Repair BFS code: int u = stack[--top]; when FIFO was intended, write removal using head and queue.","int u = queue[head++];","BFS removes from the oldest queued position.","LIFO behavior would be DFS-like, not BFS."),
            Q(TLSPhase5Domain.Graph,TLSPhase5QuestionType.Challenge,"Disconnected graph","Graph vertices A,B,C,D, with only edge A-B. BFS from A visits how many vertices?","2","Only reachable vertices enter the frontier.","BFS cannot cross missing edges to C or D."),
            Q(TLSPhase5Domain.Graph,TLSPhase5QuestionType.Challenge,"Matrix traversal cost","With an adjacency matrix, a standard BFS that scans all V possible neighbors per visited vertex is O(V^2) worst-case. True=1 False=0.","1","The matrix has V columns per row.","The adjacency-list BFS O(V+E) bound must not be falsely assigned to matrix scans."),
            Q(TLSPhase5Domain.Hash,TLSPhase5QuestionType.Trace,"Collision bucket","Modulo-7 hash table: bucket for 15?","1","15 divided by 7 has remainder 1.","15 and 8 collide at bucket 1."),
            Q(TLSPhase5Domain.Hash,TLSPhase5QuestionType.Trace,"Negative key","Normalized modulo-7 bucket for -6?","1","C remainder of -6 % 7 is -6, so add 7.","Nonnegative bucket = ((key % 7) + 7) % 7."),
            Q(TLSPhase5Domain.Hash,TLSPhase5QuestionType.Complete,"Safe modulo","Complete C for index: int i = ____;","(key % 7 + 7) % 7","C can return negative remainders.","Normalize before indexing into the bucket array."),
            Q(TLSPhase5Domain.Hash,TLSPhase5QuestionType.Complete,"Linear probe","Complete C to advance index: i = ____;","(i + 1) % 7","Wrap around to zero after slot six.","Modular increment visits the next probe slot."),
            Q(TLSPhase5Domain.Hash,TLSPhase5QuestionType.Repair,"Tombstone deletion","Repair: slot.status = EMPTY; after removing a key from a linear-probe cluster.","slot.status = DELETED;","Keep searching past a removed slot.","An EMPTY marker could prematurely terminate searches for colliding keys."),
            Q(TLSPhase5Domain.Hash,TLSPhase5QuestionType.Repair,"Duplicate insertion","Complete a guard returning false on a duplicate: if (slot.key == key) ____;","return false;","A key-set insertion must reject repeated keys.","Duplicate keys are not inserted twice in this teaching hash set."),
            Q(TLSPhase5Domain.Hash,TLSPhase5QuestionType.Challenge,"Tombstone search","Linear probing: keys 1 and 8 occupy slots 1 and 2. Delete 1 (tombstone). Searching for 8 still succeeds? Yes=1 No=0.","1","Do not stop when encountering DELETED.","Probe passes tombstone at slot 1 and finds key 8 at slot 2."),
            Q(TLSPhase5Domain.Hash,TLSPhase5QuestionType.Challenge,"Worst-case hashing","Can poor hashing cause O(n) lookup in separate chaining? Yes=1 No=0.","1","Every key may land in one bucket.","A worst-case chain can contain all n keys.")
        };
        public static int Count(TLSPhase5Domain domain,TLSPhase5QuestionType type)
        {int n=0;foreach(TLSPhase5Question q in All)if(q.Domain==domain&&q.Type==type)n++;return n;}
        public static int Find(TLSPhase5Domain domain,TLSPhase5QuestionType type,int index)
        {
            int n=0;for(int i=0;i<All.Length;i++)if(All[i].Domain==domain&&All[i].Type==type){if(n==index)return i;n++;}
            throw new ArgumentOutOfRangeException("index");
        }
        public static bool Matches(TLSPhase5Question q,string submitted)
        {
            if(q==null||string.IsNullOrWhiteSpace(submitted)||submitted.Length>2500)return false;
            if(q.Type==TLSPhase5QuestionType.Trace||q.Type==TLSPhase5QuestionType.Challenge)
                return string.Equals(q.Answer.Trim(),submitted.Trim(),StringComparison.Ordinal);
            // Reference-token equality is structured practice, NOT full compiler equivalence.
            const string pattern=@"->|\+\+|--|==|!=|<=|>=|&&|\|\||[A-Za-z_]\w*|\d+|[^\s]";
            MatchCollection a=Regex.Matches(q.Answer,pattern),b=Regex.Matches(submitted,pattern);
            if(a.Count!=b.Count)return false;
            for(int i=0;i<a.Count;i++)if(a[i].Value!=b[i].Value)return false;
            return true;
        }
    }
    public sealed class TLSPhase5PracticeSession
    {
        private readonly bool[] attempted=new bool[TLSPhase5PracticeCatalog.All.Length];
        private readonly bool[] exposed=new bool[TLSPhase5PracticeCatalog.All.Length];
        private readonly bool[] solved=new bool[TLSPhase5PracticeCatalog.All.Length];
        private readonly bool[] unseen=new bool[TLSPhase5PracticeCatalog.All.Length];
        public TLSPhase5Domain Domain{get;private set;}=TLSPhase5Domain.Heap;
        public TLSPhase5QuestionType Type{get;private set;}=TLSPhase5QuestionType.Trace;
        public int Index{get;private set;}
        public bool Guided{get;private set;}=true;
        public bool HintVisible{get;private set;}
        public bool ReferenceVisible{get;private set;}
        public string Answer{get;set;}="";
        public string Feedback{get;private set;}="Try a new question; structured C answer checks do not compile code.";
        public int Id{get{return TLSPhase5PracticeCatalog.Find(Domain,Type,Index);}}
        public TLSPhase5Question Selected{get{return TLSPhase5PracticeCatalog.All[Id];}}
        public int Solved{get{int n=0;foreach(bool b in solved)if(b)n++;return n;}}
        public int Independent{get{int n=0;foreach(bool b in unseen)if(b)n++;return n;}}
        public void Select(TLSPhase5Domain domain,TLSPhase5QuestionType type,int index)
        {
            TLSPhase5PracticeCatalog.Find(domain,type,index);Domain=domain;Type=type;Index=index;
            Answer="";HintVisible=false;ReferenceVisible=false;Feedback="New question. Hint or reveal forfeits first-unseen-attempt credit.";
        }
        public void SetGuided(bool guided){Guided=guided;HintVisible=false;ReferenceVisible=false;}
        public void Hint(){if(Guided){exposed[Id]=true;HintVisible=true;}}
        public void Reveal(){exposed[Id]=true;ReferenceVisible=true;HintVisible=false;}
        public bool Check()
        {
            int id=Id;bool ok=TLSPhase5PracticeCatalog.Matches(Selected,Answer);
            if(!attempted[id]&&!exposed[id]&&!Guided&&ok)unseen[id]=true;
            attempted[id]=true;if(ok)solved[id]=true;
            Feedback=ok?"Correct. "+Selected.Explanation:Guided?"Not yet. "+Selected.Hint:"Not yet. Retry, but first-attempt credit is spent for this question.";
            return ok;
        }
    }
}
