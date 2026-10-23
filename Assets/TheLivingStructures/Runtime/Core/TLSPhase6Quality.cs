using System;
namespace TLS.Core
{
    public sealed class TLSQualityQuestion
    {
        public readonly string Topic, Goal, OptionA, OptionB, Explanation, Hint;
        public readonly string Correct;
        public TLSQualityQuestion(string topic,string goal,string a,string b,string correct,string why,string hint)
        {Topic=topic;Goal=goal;OptionA=a;OptionB=b;Correct=correct;Explanation=why;Hint=hint;}
    }
    public static class TLSQualityCatalog
    {
        public static readonly TLSQualityQuestion[] Questions = {
            new TLSQualityQuestion("Pointer validation","Which function avoids dereferencing a NULL pointer?",
                "A: int get(const int *p) { return *p; }","B: bool get(const int *p, int *out) { if (!p || !out) return false; *out=*p; return true; }","B",
                "A status return makes NULL handling explicit for caller and output pointer.","Consider both input and output pointers."),
            new TLSQualityQuestion("Allocation size","Which allocation tracks the actual object type?",
                "A: Node *p = malloc(sizeof *p);","B: Node *p = malloc(sizeof(Node *));","A",
                "sizeof *p requests storage for the Node, not a pointer-sized quantity.","A pointer is not the object it points to."),
            new TLSQualityQuestion("Const correctness","Which declaration promises not to modify read-only input?",
                "A: int find(int *values, size_t count);","B: int find(const int *values, size_t count);","B",
                "const communicates read-only input and enables const arrays.","The search doesn't need to modify values."),
            new TLSQualityQuestion("Full buffer","Which push checks capacity before writing?",
                "A: if (s->count == CAPACITY) return false; s->data[s->count++] = v;","B: s->data[s->count++] = v; if (s->count > CAPACITY) return false;","A",
                "Validate before any potential out-of-bounds access; s must also be checked separately.","When does the first array write occur?"),
            new TLSQualityQuestion("Output contract","Which pop leaves the output untouched on failure?",
                "A: *out=0; if (s->count == 0) return false;","B: if (!s || !out || s->count == 0) return false; *out=s->data[--s->count]; return true;","B",
                "Validating inputs before mutation makes failure atomic.","Avoid writing to out before all guards pass."),
            new TLSQualityQuestion("Free safely","Which statement ordering avoids dereferencing a freed node?",
                "A: Node *next=p->next; free(p); p=next;","B: free(p); Node *next=p->next; p=next;","A",
                "Capture the next pointer while the node is live; never dereference after free.","Once free is called, pointer dereference is invalid."),
            new TLSQualityQuestion("Documentation","Which comment explains the design contract rather than restating syntax?",
                "A: // i++ increments i","B: // Precondition: values sorted ascending; returns -1 when absent.","B",
                "Document preconditions, failure contracts and invariants instead of merely repeating statements.","What does a caller need to know?"),
            new TLSQualityQuestion("Test boundaries","Which test set is more useful for queue wraparound?",
                "A: enqueue once from head=0","B: fill, dequeue twice, enqueue twice, then check logical order and raw positions","B",
                "A wraparound-specific test forces head and physical tail across the capacity boundary.","An ordinary enqueue may never wrap."),
            new TLSQualityQuestion("Error propagation","Which malloc use handles allocation failure?",
                "A: Node *p=malloc(sizeof *p); p->next=NULL;","B: Node *p=malloc(sizeof *p); if (p==NULL) return false; p->next=NULL;","B",
                "malloc may fail; dereferencing NULL is undefined behavior.","Ask what malloc returns when allocation fails."),
            new TLSQualityQuestion("Complexity documentation","Which claim correctly qualifies a hash-table search?",
                "A: hash search is guaranteed O(1) for every input","B: expected O(1) under suitable hashing/load assumptions; worst-case O(n)","B",
                "Average/expected complexity depends on assumptions; worst-case collisions can degrade lookup.","A big-O guarantee should state relevant assumptions."),
            new TLSQualityQuestion("Assertion design","Which check is appropriate for invalid learner input in a user-facing API?",
                "A: return an error status without modifying data","B: assert(input != NULL); crash on user input","A",
                "Assertions support internal invariants; ordinary invalid API requests need defined error handling.","Who controls the invalid input?"),
            new TLSQualityQuestion("Measurement honesty","Which performance note avoids confusing Big-O with runtime?",
                "A: 50 counted comparisons means 50 milliseconds","B: 50 comparisons is an algorithmic count; timing depends on machine and environment","B",
                "Operation counts and asymptotic growth are not wall-clock measurements.","Think about CPU, compiler, cache, and input.")
        };
    }
    public sealed class TLSQualitySession
    {
        private readonly bool[] firstIndependent=new bool[TLSQualityCatalog.Questions.Length];
        private readonly bool[] attempted=new bool[TLSQualityCatalog.Questions.Length];
        private readonly bool[] exposed=new bool[TLSQualityCatalog.Questions.Length];
        public int Index {get;private set;}
        public bool Guided {get;private set;}=true;
        public bool HintVisible {get;private set;}
        public bool ReferenceVisible {get;private set;}
        public TLSQualityQuestion Current {get{return TLSQualityCatalog.Questions[Index];}}
        public string Feedback {get;private set;}="Compare both C choices. Correct style must also preserve actual behavior.";
        public int Independent {get{int x=0;foreach(bool b in firstIndependent)if(b)x++;return x;}}
        public void Select(int index){if(index<0||index>=TLSQualityCatalog.Questions.Length)throw new ArgumentOutOfRangeException("index");Index=index;HintVisible=false;ReferenceVisible=false;Feedback="Read the design contract before choosing.";}
        public void SetGuided(bool b){Guided=b;HintVisible=false;ReferenceVisible=false;Feedback=b?"Guided mode enabled.":"Independent mode enabled. Previous exposure cannot be undone.";}
        public void Hint(){if(Guided){exposed[Index]=true;HintVisible=true;Feedback=Current.Hint;}}
        public void Reveal(){exposed[Index]=true;ReferenceVisible=true;Feedback="ANSWER: "+Current.Correct+". "+Current.Explanation;}
        public bool Choose(string option)
        {
            if(option!="A"&&option!="B")throw new ArgumentOutOfRangeException("option");
            bool correct=option==Current.Correct;
            if(!attempted[Index]&&!exposed[Index]&&!Guided&&correct)firstIndependent[Index]=true;
            attempted[Index]=true;
            Feedback=correct?"Correct. "+Current.Explanation:"Try again. "+(Guided?Current.Hint:"Compare error handling and preconditions.");
            return correct;
        }
    }
}
