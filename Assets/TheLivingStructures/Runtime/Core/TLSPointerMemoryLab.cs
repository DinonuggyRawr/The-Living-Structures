using System;
using System.Collections.Generic;

namespace TLS.Core
{
    // Phase 3.1 is a DETERMINISTIC TEACHING MODEL, not a C interpreter or an allocator.
    // Symbolic IDs/addresses must never be mistaken for actual process addresses.
    public enum TLSPointerLessonId
    {
        NullGuard, Allocation, Aliasing, LinkNodes, Dangling, SafeCleanup,
        LostAllocation, AllocationFailure, InvalidDereference
    }

    public enum TLSPointerEvent
    {
        Begin, Declare, Allocate, Check, WriteField, Link, CopyPointer,
        Release, ForgetPointer, AllocationRejected, InvalidAccessPrevented
    }

    public sealed class TLSHeapNode
    {
        public string Id;
        public string Address;
        public bool Released;
        public bool DataInitialized;
        public int Data;
        public bool NextInitialized;
        public string NextId; // null means NULL only when NextInitialized is true
        public TLSHeapNode Clone()
        {
            return (TLSHeapNode)MemberwiseClone();
        }
    }

    public sealed class TLSPointerMemoryState
    {
        private readonly Dictionary<string, string> pointers = new Dictionary<string, string>();
        private readonly List<TLSHeapNode> nodes = new List<TLSHeapNode>();
        public IDictionary<string, string> Pointers { get { return new Dictionary<string, string>(pointers); } }
        public IList<TLSHeapNode> Nodes
        {
            get
            {
                List<TLSHeapNode> result = new List<TLSHeapNode>();
                foreach (TLSHeapNode node in nodes) result.Add(node.Clone());
                return result.AsReadOnly();
            }
        }
        public string PointerId(string variable) { return pointers.ContainsKey(variable) ? pointers[variable] : null; }
        public bool HasPointer(string variable) { return pointers.ContainsKey(variable); }
        public TLSHeapNode Node(string id)
        {
            foreach (TLSHeapNode item in nodes) if (item.Id == id) return item.Clone();
            return null;
        }
        private TLSHeapNode MutableNode(string id)
        {
            foreach (TLSHeapNode item in nodes) if (item.Id == id) return item;
            throw new InvalidOperationException("Unknown simulated heap object.");
        }
        public void Declare(string name, string id)
        {
            if (pointers.ContainsKey(name)) throw new InvalidOperationException("Already declared.");
            if (id != null) RequireLive(id);
            pointers.Add(name, id);
        }
        public void Assign(string name, string id)
        {
            if (!pointers.ContainsKey(name)) throw new InvalidOperationException("Undeclared pointer variable.");
            // Assigning NULL is always allowed. A freed symbolic object is never used as a new pointer value.
            if (id != null) RequireLive(id);
            pointers[name] = id;
        }
        public string Allocate()
        {
            if (nodes.Count >= 2) throw new InvalidOperationException("Lesson heap is bounded to two objects.");
            string id = "object" + (nodes.Count + 1);
            nodes.Add(new TLSHeapNode { Id = id, Address = nodes.Count == 0 ? "0x1000" : "0x1020" });
            return id;
        }
        public void SetData(string id, int value) { TLSHeapNode node = RequireLive(id); node.Data = value; node.DataInitialized = true; }
        public void SetNext(string id, string next)
        {
            TLSHeapNode node = RequireLive(id);
            if (next != null) RequireLive(next);
            node.NextId = next;
            node.NextInitialized = true;
        }
        public void Release(string id) { RequireLive(id).Released = true; }
        private TLSHeapNode RequireLive(string id)
        {
            TLSHeapNode node = MutableNode(id);
            if (node.Released) throw new InvalidOperationException("A released object cannot be accessed in the model.");
            return node;
        }
        public string PointerStatus(string name)
        {
            if (!pointers.ContainsKey(name)) return "NOT DECLARED";
            if (pointers[name] == null) return "NULL";
            TLSHeapNode node = MutableNode(pointers[name]);
            return node.Released ? "DANGLING (conceptual)" : node.Address + "  /  LIVE";
        }
        public bool IsUnreachable(string id)
        {
            TLSHeapNode target = MutableNode(id);
            if (target.Released) return false;
            HashSet<string> reached = new HashSet<string>();
            Queue<string> visit = new Queue<string>();
            foreach (string root in pointers.Values) if (root != null) visit.Enqueue(root);
            while (visit.Count > 0)
            {
                string currentId = visit.Dequeue();
                if (!reached.Add(currentId)) continue;
                TLSHeapNode node = MutableNode(currentId);
                if (!node.Released && node.NextInitialized && node.NextId != null) visit.Enqueue(node.NextId);
            }
            return !reached.Contains(id);
        }
        public TLSPointerMemoryState Clone()
        {
            TLSPointerMemoryState copy = new TLSPointerMemoryState();
            foreach (KeyValuePair<string,string> entry in pointers) copy.pointers.Add(entry.Key, entry.Value);
            foreach (TLSHeapNode node in nodes) copy.nodes.Add(node.Clone());
            return copy;
        }
    }

    public sealed class TLSPointerMemoryFrame
    {
        public int CodeLine; // -1 before the first statement
        public string Explanation;
        public string FocusVariable;
        public string FocusNode;
        public TLSPointerEvent Event;
        public TLSPointerMemoryState State;
        public bool UnsafeAccessBlocked;
    }

    public sealed class TLSPointerMemoryLesson
    {
        public TLSPointerLessonId Id;
        public string Title;
        public string Goal;
        public string[] Code;
        public string PredictionQuestion;
        public string PredictionA;
        public string PredictionB;
        public int CorrectChoice; // 0 or 1
        public string PredictionWhy;
        public List<TLSPointerMemoryFrame> Frames = new List<TLSPointerMemoryFrame>();
    }

    public static class TLSPointerMemoryCatalog
    {
        private sealed class Builder
        {
            public TLSPointerMemoryLesson Lesson = new TLSPointerMemoryLesson();
            public TLSPointerMemoryState State = new TLSPointerMemoryState();
            public Builder(TLSPointerLessonId id, string title, string goal, string[] code,
                string question, string answerA, string answerB, int correct, string reason)
            {
                Lesson.Id = id; Lesson.Title = title; Lesson.Goal = goal; Lesson.Code = code;
                Lesson.PredictionQuestion = question; Lesson.PredictionA = answerA;
                Lesson.PredictionB = answerB; Lesson.CorrectChoice = correct; Lesson.PredictionWhy = reason;
                Add(-1, "Before execution: no local pointers or modeled heap objects exist.",
                    TLSPointerEvent.Begin, null, null);
            }
            public void Add(int line, string explanation, TLSPointerEvent kind, string variable, string node,
                bool blocked = false)
            {
                if (line >= Lesson.Code.Length || line < -1) throw new InvalidOperationException("Invalid teaching line.");
                Lesson.Frames.Add(new TLSPointerMemoryFrame { CodeLine = line, Explanation = explanation,
                    Event = kind, FocusVariable = variable, FocusNode = node, State = State.Clone(),
                    UnsafeAccessBlocked = blocked });
            }
        }

        public static string ShortName(TLSPointerLessonId id)
        {
            switch (id)
            {
                case TLSPointerLessonId.NullGuard: return "NULL GUARD";
                case TLSPointerLessonId.Allocation: return "MALLOC + FIELDS";
                case TLSPointerLessonId.Aliasing: return "ALIASING";
                case TLSPointerLessonId.LinkNodes: return "LINK NODES";
                case TLSPointerLessonId.Dangling: return "DANGLING";
                case TLSPointerLessonId.SafeCleanup: return "SAFE CLEANUP";
                case TLSPointerLessonId.LostAllocation: return "LOST POINTER";
                case TLSPointerLessonId.AllocationFailure: return "MALLOC FAIL";
                default: return "INVALID ACCESS";
            }
        }
        public static TLSPointerMemoryLesson Build(TLSPointerLessonId id)
        {
            Builder b;
            string first, second;
            switch (id)
            {
                case TLSPointerLessonId.NullGuard:
                    b = new Builder(id, "NULL pointer & guard", "Learn why a NULL check must happen before dereference.",
                        new[] {"Node *head = NULL;", "if (head == NULL) return;"},
                        "Will the guard prevent an invalid dereference?", "Yes, the function returns", "No, the node is read", 0,
                        "A null check branches away from code that would dereference the pointer.");
                    b.State.Declare("head", null);
                    b.Add(0, "head is a pointer variable containing NULL. No heap node exists.", TLSPointerEvent.Declare,"head",null);
                    b.Add(1, "The guard is true, so execution returns without accessing a node.", TLSPointerEvent.Check,"head",null);
                    break;
                case TLSPointerLessonId.Allocation:
                    b = new Builder(id, "malloc & field initialization", "Inspect allocation, uninitialized struct fields and deliberate writes.",
                        new[] {"Node *head = NULL;", "Node *node = malloc(sizeof *node);", "if (node == NULL) return;", "node->data = 42;", "node->next = NULL;", "head = node;"},
                        "Immediately after malloc succeeds, are the fields initialized to zero?", "Yes, they are zero", "No, they are indeterminate", 1,
                        "malloc allocates storage but does not initialize its contents. Never read an unwritten field.");
                    b.State.Declare("head", null); b.Add(0, "head starts at NULL.", TLSPointerEvent.Declare,"head",null);
                    first = b.State.Allocate(); b.State.Declare("node", first);
                    b.Add(1, "malloc succeeds in this fictional trace and returns 0x1000. data/next are indeterminate, not zero.", TLSPointerEvent.Allocate,"node",first);
                    b.Add(2, "The allocation was successful. The NULL guard does not return.",TLSPointerEvent.Check,"node",first);
                    b.State.SetData(first,42); b.Add(3,"Writing data initializes only that field.", TLSPointerEvent.WriteField,"node",first);
                    b.State.SetNext(first,null); b.Add(4,"Setting next to NULL initializes the link field.",TLSPointerEvent.Link,"node",first);
                    b.State.Assign("head",first); b.Add(5,"head now aliases node; both point to the same heap allocation.",TLSPointerEvent.CopyPointer,"head",first);
                    break;
                case TLSPointerLessonId.Aliasing:
                    b = new Builder(id,"Pointer aliasing","Two pointer variables can name the same heap object.",
                        new[] {"Node *head = malloc(sizeof *head);", "if (head == NULL) return;", "head->data = 10;", "head->next = NULL;", "Node *alias = head;", "alias->data = 99;"},
                        "After alias->data = 99, what will head->data be?", "99 (same object)", "10 (a copied object)", 0,
                        "Assigning a pointer copies an address, not the object; both pointers refer to one allocation.");
                    first=b.State.Allocate(); b.State.Declare("head",first); b.Add(0,"head receives a symbolic live heap address.",TLSPointerEvent.Allocate,"head",first);
                    b.Add(1,"NULL guard is false; it is safe to initialize the allocation.",TLSPointerEvent.Check,"head",first);
                    b.State.SetData(first,10); b.Add(2,"Initialize the data field to 10.",TLSPointerEvent.WriteField,"head",first);
                    b.State.SetNext(first,null); b.Add(3,"Initialize next to NULL.",TLSPointerEvent.Link,"head",first);
                    b.State.Declare("alias",first); b.Add(4,"alias copies head's pointer value. No new node is allocated.",TLSPointerEvent.CopyPointer,"alias",first);
                    b.State.SetData(first,99); b.Add(5,"Updating through alias changes the SAME heap field observed through head.",TLSPointerEvent.WriteField,"alias",first);
                    break;
                case TLSPointerLessonId.LinkNodes:
                    b=new Builder(id,"Connect two nodes","Build a two-node chain by changing next and head.",
                        new[] {"Node *head = malloc(sizeof *head);", "if (head == NULL) return;", "head->data = 10;", "head->next = NULL;", "Node *second = malloc(sizeof *second);", "if (second == NULL) { free(head); return; }", "second->data = 20;", "second->next = head;", "head = second;"},
                        "After head = second, which value is in the head node?", "10", "20", 1,
                        "head now points at the second allocation, whose next field points to the first.");
                    first=b.State.Allocate(); b.State.Declare("head",first);b.Add(0,"The first allocation is at symbolic 0x1000.",TLSPointerEvent.Allocate,"head",first);
                    b.Add(1,"Check that the first allocation succeeded.",TLSPointerEvent.Check,"head",first);
                    b.State.SetData(first,10);b.Add(2,"Initialize the first node's value.",TLSPointerEvent.WriteField,"head",first);
                    b.State.SetNext(first,null);b.Add(3,"The first node initially terminates the chain.",TLSPointerEvent.Link,"head",first);
                    second=b.State.Allocate();b.State.Declare("second",second);b.Add(4,"Allocate a second independent node at symbolic 0x1020.",TLSPointerEvent.Allocate,"second",second);
                    b.Add(5,"Check the second allocation. The failure branch would free the first node before returning.",TLSPointerEvent.Check,"second",second);
                    b.State.SetData(second,20);b.Add(6,"Initialize the second node's data field.",TLSPointerEvent.WriteField,"second",second);
                    b.State.SetNext(second,first);b.Add(7,"second->next now references the first node; no node is copied.",TLSPointerEvent.Link,"second",second);
                    b.State.Assign("head",second);b.Add(8,"Move head to the second node. Chain: 20 -> 10 -> NULL.",TLSPointerEvent.CopyPointer,"head",second);
                    break;
                case TLSPointerLessonId.Dangling:
                case TLSPointerLessonId.SafeCleanup:
                    bool cleanup = id==TLSPointerLessonId.SafeCleanup;
                    b=new Builder(id,cleanup?"Release and clear aliases":"Why a dangling pointer appears",
                        "See why free releases a heap object's lifetime without clearing other pointers.",
                        cleanup ? new[] {"Node *head = malloc(sizeof *head);", "if (head == NULL) return;", "head->data = 7;", "head->next = NULL;", "Node *alias = head;", "free(head);", "head = NULL;", "alias = NULL;"} :
                                  new[] {"Node *head = malloc(sizeof *head);", "if (head == NULL) return;", "head->data = 7;", "head->next = NULL;", "Node *alias = head;", "free(head);", "head = NULL;"},
                        cleanup?"Does free(head) automatically clear alias?":"Can alias be dereferenced safely after free(head)?",
                        cleanup?"Yes, free resets aliases":"No, clear each alias separately",
                        cleanup?"Not automatically":"No, the object lifetime ended",
                        1, cleanup?"free ends the object's lifetime; pointer variables must be handled explicitly. Never dereference aliases after free.":
                        "An alias to a released allocation is not a valid dereference target. Its prior address is shown only conceptually.");
                    first=b.State.Allocate(); b.State.Declare("head",first); b.Add(0,"Allocate a live heap node.",TLSPointerEvent.Allocate,"head",first);
                    b.Add(1,"Check allocation success.",TLSPointerEvent.Check,"head",first);
                    b.State.SetData(first,7);b.Add(2,"Initialize data.",TLSPointerEvent.WriteField,"head",first);
                    b.State.SetNext(first,null);b.Add(3,"Initialize next.",TLSPointerEvent.Link,"head",first);
                    b.State.Declare("alias",first);b.Add(4,"head and alias refer to the same block.",TLSPointerEvent.CopyPointer,"alias",first);
                    b.State.Release(first);b.Add(5,"free ends the object's lifetime. Both pointer arrows are DANGEROUS teaching references, not readable C pointers.",TLSPointerEvent.Release,"head",first);
                    b.State.Assign("head",null);b.Add(6,"Clearing head does not fix alias. Accessing alias would still be invalid.",TLSPointerEvent.ForgetPointer,"head",first);
                    if(cleanup){b.State.Assign("alias",null);b.Add(7,"Explicitly clear alias too; the released block remains unavailable.",TLSPointerEvent.ForgetPointer,"alias",first);}
                    break;
                case TLSPointerLessonId.LostAllocation:
                    b=new Builder(id,"Losing the last pointer","Distinguish an unreachable allocated block from memory that was freed.",
                        new[]{"Node *head = malloc(sizeof *head);","if (head == NULL) return;","head->data = 7;","head->next = NULL;","head = NULL;"},
                        "After head = NULL without free, what happens to the block?", "It is automatically freed", "It remains allocated but unreachable", 1,
                        "Changing a pointer does not deallocate heap memory. The allocation is unreachable in this modeled scope.");
                    first=b.State.Allocate();b.State.Declare("head",first);b.Add(0,"Allocate a node.",TLSPointerEvent.Allocate,"head",first);
                    b.Add(1,"Check the allocation result.",TLSPointerEvent.Check,"head",first);
                    b.State.SetData(first,7);b.Add(2,"Initialize its value.",TLSPointerEvent.WriteField,"head",first);
                    b.State.SetNext(first,null);b.Add(3,"Initialize the next pointer.",TLSPointerEvent.Link,"head",first);
                    b.State.Assign("head",null);b.Add(4,"The node stays allocated, but no pointer in this modeled function reaches it: potential leak.",TLSPointerEvent.ForgetPointer,"head",first);
                    break;
                case TLSPointerLessonId.AllocationFailure:
                    b=new Builder(id,"A failed malloc","Observe NULL returns without inventing a heap allocation.",
                        new[]{"Node *node = malloc(sizeof *node);","if (node == NULL) return;"},
                        "When malloc fails, does the function dereference node?", "No, guard returns", "Yes, writes default fields", 0,
                        "The modeled allocation returned NULL and the branch prevents any dereference.");
                    b.State.Declare("node",null);b.Add(0,"Simulated allocator failure returns NULL, not a fabricated node at 0x1000.",TLSPointerEvent.AllocationRejected,"node",null);
                    b.Add(1,"The guard returns. A failed malloc does not allocate an object.",TLSPointerEvent.Check,"node",null);
                    break;
                case TLSPointerLessonId.InvalidDereference:
                    b=new Builder(id,"Invalid NULL access (blocked)","Inspect unsafe C code without executing undefined behavior.",
                        new[]{"Node *head = NULL;","head->data = 5;  /* UNSAFE: do not run */"},
                        "What would real C guarantee for head->data here?", "It writes to address zero", "Nothing; undefined behavior", 1,
                        "Dereferencing NULL is undefined behavior, NOT a predictable crash or zero write. The lab blocks this illustrative step.");
                    b.State.Declare("head",null);b.Add(0,"head holds NULL. There is no object to write.",TLSPointerEvent.Declare,"head",null);
                    b.Add(1,"UNSAFE C EXPRESSION BLOCKED BY THE MODEL. Real C has undefined behavior; no memory write occurred here.",TLSPointerEvent.InvalidAccessPrevented,"head",null,true);
                    break;
                default: throw new ArgumentOutOfRangeException("id");
            }
            return b.Lesson;
        }
    }

    // Session-only: visual predictions are formative and NEVER write StudyProgress.
    public sealed class TLSPointerMemorySession
    {
        public TLSPointerMemoryLesson Lesson { get; private set; }
        public int Step { get; private set; }
        public int? Prediction { get; private set; }
        public int Attempts { get; private set; }
        public int Correct { get; private set; }
        public TLSPointerMemorySession() { Select(TLSPointerLessonId.NullGuard); }
        public void Select(TLSPointerLessonId id)
        {
            Lesson = TLSPointerMemoryCatalog.Build(id);
            Step = 0;
            Prediction = null;
        }
        public TLSPointerMemoryFrame Frame { get { return Lesson.Frames[Step]; } }
        public bool IsFinished { get { return Step == Lesson.Frames.Count - 1; } }
        public void Next() { if (!IsFinished) Step++; }
        public void Previous() { if (Step > 0) Step--; }
        public void Finish() { Step = Lesson.Frames.Count - 1; }
        public void Reset() { Step = 0; Prediction = null; }
        public bool Choose(int choice)
        {
            if (choice != 0 && choice != 1) return false;
            if (Step != 0 || Prediction.HasValue) return false;
            Prediction = choice;
            Attempts++;
            if (choice == Lesson.CorrectChoice) Correct++;
            return true;
        }
        public string PredictionReview
        {
            get
            {
                if (!IsFinished) return "Predict at step 0; feedback appears only after FINISH.";
                if (!Prediction.HasValue) return "No prediction submitted. " + Lesson.PredictionWhy;
                return (Prediction.Value == Lesson.CorrectChoice ? "CORRECT. " : "REVISIT THIS. ") + Lesson.PredictionWhy;
            }
        }
    }
}
