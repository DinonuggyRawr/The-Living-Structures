using System;
using System.Collections.Generic;

namespace TLS.Core
{
    // Phase 3 integrated visual model: invented heap IDs, never actual process addresses.
    public enum TLSListKind { Singly, Doubly, Circular }
    public enum TLSListAction { Prepend, Append, InsertAt, RemoveAt, Search, Reverse, RecursiveCount, RecursiveFind, Clear }

    public sealed class TLSListNode
    {
        public int Id;
        public int Value;
        public int Next = -1;
        public int Prev = -1;
        public bool Freed;
        public TLSListNode Copy() { return (TLSListNode)MemberwiseClone(); }
    }

    public sealed class TLSListState
    {
        public TLSListKind Kind;
        public int Head = -1;
        public int Tail = -1;
        public int NextId = 1;
        public readonly List<TLSListNode> Heap = new List<TLSListNode>();
        public TLSListState(TLSListKind kind) { Kind = kind; }
        public TLSListState Clone()
        {
            TLSListState copy = new TLSListState(Kind);
            copy.Head = Head; copy.Tail = Tail; copy.NextId = NextId;
            foreach (TLSListNode node in Heap) copy.Heap.Add(node.Copy());
            return copy;
        }
        public TLSListNode Find(int id)
        {
            foreach (TLSListNode node in Heap) if (node.Id == id) return node;
            return null;
        }
        public int[] Walk()
        {
            List<int> ids = new List<int>();
            HashSet<int> visited = new HashSet<int>();
            int id = Head;
            while (id != -1 && visited.Add(id))
            {
                TLSListNode n = Find(id);
                if (n == null || n.Freed) throw new InvalidOperationException("List traversal reached missing or freed memory.");
                ids.Add(id);
                if (ids.Count > 12) throw new InvalidOperationException("Teaching list size exceeded.");
                id = n.Next;
            }
            if (Kind == TLSListKind.Circular && Head != -1 && id != Head)
                throw new InvalidOperationException("Circular list must return to head.");
            if (Kind != TLSListKind.Circular && id != -1)
                throw new InvalidOperationException("Noncircular list contains a cycle.");
            return ids.ToArray();
        }
        public int Count { get { return Walk().Length; } }
        public int[] Values()
        {
            int[] ids = Walk(); int[] result = new int[ids.Length];
            for (int i = 0; i < ids.Length; i++) result[i] = Find(ids[i]).Value;
            return result;
        }
        public bool Validate()
        {
            int[] ids = Walk();
            if (ids.Length == 0) return Head == -1 && Tail == -1;
            if (ids[ids.Length - 1] != Tail) return false;
            for (int i = 0; i < ids.Length; i++)
            {
                TLSListNode n = Find(ids[i]);
                int next = i + 1 < ids.Length ? ids[i + 1] : (Kind == TLSListKind.Circular ? Head : -1);
                if (n.Next != next) return false;
                if (Kind == TLSListKind.Doubly && n.Prev != (i == 0 ? -1 : ids[i - 1])) return false;
                if (Kind != TLSListKind.Doubly && n.Prev != -1) return false;
            }
            return true;
        }
        public static string Address(int id) { return id < 0 ? "NULL" : "0x" + (0x1000 + (id - 1) * 0x20).ToString("X4"); }
    }

    public sealed class TLSListFrame
    {
        public TLSListState State;
        public int CodeLine;
        public int FocusId = -1;
        public string Explanation;
        public string CallStack = "";
        public TLSListFrame(TLSListState state, int line, string explanation, int focus = -1, string calls = "")
        { State = state.Clone(); CodeLine = line; Explanation = explanation; FocusId = focus; CallStack = calls; }
    }

    public sealed class TLSListTrace
    {
        public TLSListAction Action;
        public string[] Code;
        public bool Succeeded;
        public int FoundIndex = -1;
        public string Diagnostic;
        public readonly List<TLSListFrame> Frames = new List<TLSListFrame>();
        public TLSListState Final { get { return Frames[Frames.Count - 1].State.Clone(); } }
    }

    public static class TLSLinkedListEngine
    {
        public const int MaxNodes = 8;
        private static readonly string[] MutationLines = {
            "/* guard pointer and insertion index */",
            "Node *node = malloc(sizeof *node); /* guarded */",
            "node->data = value; node->next = NULL;",
            "/* link neighbors before publishing head/tail */",
            "/* update head / tail and relevant prev/next */",
            "/* return: list structure validated */"
        };
        public static TLSListTrace Make(TLSListState initial, TLSListAction action, int value = 0, int index = 0)
        {
            if (initial == null || !initial.Validate()) throw new ArgumentException("Initial list is invalid.");
            TLSListTrace trace = new TLSListTrace(); trace.Action = action;
            TLSListState work = initial.Clone();
            trace.Code = action == TLSListAction.RecursiveCount ? new[] {
                "size_t count(const Node *node) {", "    if (node == NULL) return 0;", "    return 1 + count(node->next);", "}"
            } : action == TLSListAction.RecursiveFind ? new[] {
                "bool contains(const Node *node, int target) {", "    if (node == NULL) return false;", "    if (node->data == target) return true;", "    return contains(node->next, target);", "}"
            } : action == TLSListAction.Search ? new[] {
                "const Node *cur = head;", "while (cur != NULL) {", "    if (cur->data == target) return true;", "    cur = cur->next;", "}", "return false;"
            } : action == TLSListAction.RemoveAt ? new[] {
                "if (index < 0 || index >= count) return false;",
                "Node *victim = node_at(index); /* guard */",
                "Node *after = victim->next;",
                "/* reconnect previous and next links */",
                "/* update head/tail; free(victim); */",
                "return true;"
            } : action == TLSListAction.Reverse ? new[] {
                "/* reverse pointers only on LINEAR lists */",
                "Node *prev = NULL, *cur = head;",
                "/* save cur->next before reassignment */",
                "/* cur->next = prev; advance both */",
                "/* return prev as the new head */",
                "return true;"
            } : action == TLSListAction.Clear ? new[] {
                "Node *current = head;",
                "/* advance after saving next pointer */",
                "/* free current node */",
                "/* repeat until list boundary */",
                "head = tail = NULL;",
                "return true;"
            } : MutationLines;
            if (action == TLSListAction.Search && initial.Kind == TLSListKind.Circular)
                trace.Code = new[] {"if (head == NULL) return false;", "const Node *cur = head; do {",
                    "    if (cur->data == target) return true;", "    cur = cur->next;", "} while (cur != head);", "return false;"};
            trace.Frames.Add(new TLSListFrame(work, -1, "Starting from a valid " + work.Kind + " list. Changes are modeled; addresses are symbolic."));
            int[] ids = work.Walk();
            if (action == TLSListAction.Search || action == TLSListAction.RecursiveCount || action == TLSListAction.RecursiveFind)
            {
                bool recursion = action != TLSListAction.Search;
                if (recursion && work.Kind == TLSListKind.Circular)
                {
                    trace.Diagnostic = "The shown NULL-terminated recursive C function is for linear lists. A circular list requires a stopping boundary.";
                    trace.Frames.Add(new TLSListFrame(work, 0, trace.Diagnostic)); return trace;
                }
                for (int i = 0; i < ids.Length; i++)
                {
                    TLSListNode node = work.Find(ids[i]);
                    string stack = recursion ? "count(node@" + TLSListState.Address(ids[i]) + ")  / depth " + (i + 1) : "";
                    trace.Frames.Add(new TLSListFrame(work, recursion ? 2 : 2,
                        recursion ? "Enter frame for node " + node.Value + "; a distinct call awaits its result." :
                        "Compare node value " + node.Value + " with " + value + ".", ids[i], stack));
                    if (action != TLSListAction.RecursiveCount && node.Value == value) { trace.FoundIndex = i; break; }
                }
                if (recursion)
                {
                    int depth = action == TLSListAction.RecursiveCount ? ids.Length : (trace.FoundIndex < 0 ? ids.Length : trace.FoundIndex + 1);
                    trace.Frames.Add(new TLSListFrame(work, 1, "Base case / match reached. Return values unwind without moving heap nodes.", -1,
                        "base case or match | depth " + depth));
                    for (int i = depth - 1; i >= 0; --i)
                        trace.Frames.Add(new TLSListFrame(work, 2, action == TLSListAction.RecursiveCount ?
                            "Frame returns " + (depth - i) + " after adding 1 to the recursive result." :
                            "Propagate the found/not-found result back to the caller.", ids[i], "RETURN from depth " + (i+1)));
                }
                trace.Succeeded = true;
                trace.Diagnostic = action == TLSListAction.RecursiveCount ? "Recursive count = " + ids.Length :
                    (trace.FoundIndex >= 0 ? "Found at logical index " + trace.FoundIndex : "Not found");
                trace.Frames.Add(new TLSListFrame(work, trace.Code.Length - 1, trace.Diagnostic));
                return trace;
            }
            if (action == TLSListAction.Clear)
            {
                for (int i = 0; i < ids.Length; i++)
                {
                    int id = ids[i];
                    int nextId = i + 1 < ids.Length ? ids[i + 1] : -1;
                    // Save the next pointer first, unlink from the live list, then free.
                    // Every displayed frame must have a valid traversable live chain.
                    work.Head = nextId;
                    if (nextId == -1) work.Tail = -1;
                    else if (work.Kind == TLSListKind.Doubly) work.Find(nextId).Prev = -1;
                    else if (work.Kind == TLSListKind.Circular) work.Find(work.Tail).Next = nextId;
                    TLSListNode victim = work.Find(id);
                    victim.Next = -1; victim.Prev = -1; victim.Freed = true;
                    trace.Frames.Add(new TLSListFrame(work, 4, "Saved next; unlinked node from the live chain; free() ends this node lifetime.", id));
                }
            }
            else if (action == TLSListAction.Reverse)
            {
                if (work.Kind == TLSListKind.Circular)
                {
                    trace.Diagnostic = "Reversal is offered only for singly and doubly linked linear lists.";
                    trace.Frames.Add(new TLSListFrame(work, 0, trace.Diagnostic)); return trace;
                }
                Array.Reverse(ids);
                Rewire(work, ids);
                trace.Frames.Add(new TLSListFrame(work, 4, "Links reversed; former tail is now head. No allocation or free occurs.", work.Head));
            }
            else if (action == TLSListAction.Prepend || action == TLSListAction.Append || action == TLSListAction.InsertAt)
            {
                int slot = action == TLSListAction.Prepend ? 0 : action == TLSListAction.Append ? ids.Length : index;
                if (slot < 0 || slot > ids.Length || ids.Length == MaxNodes)
                {
                    trace.Diagnostic = "Rejected: insertion index invalid or teaching capacity reached (8 nodes).";
                    trace.Frames.Add(new TLSListFrame(work, 0, trace.Diagnostic)); return trace;
                }
                // The visual heap is bounded. Previously freed symbolic addresses may be
                // reallocated; historical frames remain immutable deep clones.
                int freedSlot = -1;
                if (work.Heap.Count >= MaxNodes)
                    for (int i = 0; i < work.Heap.Count; i++)
                        if (work.Heap[i].Freed) {freedSlot = i; break;}
                TLSListNode node;
                if (freedSlot >= 0)
                {
                    node = new TLSListNode { Id = work.Heap[freedSlot].Id, Value = value };
                    work.Heap[freedSlot] = node;
                }
                else
                {
                    node = new TLSListNode { Id = work.NextId++, Value = value };
                    work.Heap.Add(node);
                }
                trace.Frames.Add(new TLSListFrame(work, 1,
                    "Modeled malloc() succeeded. Fields initialized before links are published." +
                    (freedSlot >= 0 ? " A freed symbolic address was reused." : ""), node.Id));
                List<int> revised = new List<int>(ids); revised.Insert(slot, node.Id);
                Rewire(work, revised.ToArray());
                trace.Frames.Add(new TLSListFrame(work, 4, "Neighbors and head/tail updated. New node is reachable and links satisfy the " + work.Kind + " invariant.", node.Id));
            }
            else if (action == TLSListAction.RemoveAt)
            {
                if (index < 0 || index >= ids.Length)
                {
                    trace.Diagnostic = "Rejected: removal requires a valid occupied logical index.";
                    trace.Frames.Add(new TLSListFrame(work, 0, trace.Diagnostic)); return trace;
                }
                int removed = ids[index];
                List<int> revised = new List<int>(ids); revised.RemoveAt(index);
                Rewire(work, revised.ToArray());
                work.Find(removed).Freed = true;
                work.Find(removed).Next = -1; work.Find(removed).Prev = -1;
                trace.Frames.Add(new TLSListFrame(work, 4, "Relink first, then free removed node. Any stale alias would be dangling, not safe to dereference.", removed));
            }
            if (!work.Validate()) throw new InvalidOperationException("Linked operation violated list invariants.");
            trace.Succeeded = true;
            trace.Diagnostic = "MODELED " + action.ToString().ToUpperInvariant() + " COMPLETE";
            trace.Frames.Add(new TLSListFrame(work, 5, trace.Diagnostic));
            return trace;
        }
        private static void Rewire(TLSListState s, int[] order)
        {
            s.Head = order.Length == 0 ? -1 : order[0];
            s.Tail = order.Length == 0 ? -1 : order[order.Length - 1];
            for (int i = 0; i < order.Length; i++)
            {
                TLSListNode n = s.Find(order[i]);
                n.Next = i+1 < order.Length ? order[i+1] : s.Kind == TLSListKind.Circular ? s.Head : -1;
                n.Prev = s.Kind == TLSListKind.Doubly && i > 0 ? order[i-1] : -1;
            }
        }
    }

    public sealed class TLSLinkedLabSession
    {
        public TLSListState Current { get; private set; }
        public TLSListTrace Trace { get; private set; }
        public int Step { get; private set; }
        public bool PredictionUsed { get; private set; }
        public int FirstIndependentPredictions { get; private set; }
        public string PredictionReview { get; private set; } = "Predict whether this operation changes the number of reachable nodes.";
        private int? prediction;
        private bool applied;
        public TLSLinkedLabSession() { SelectKind(TLSListKind.Singly); }
        public TLSListFrame Frame { get { return Trace.Frames[Step]; } }
        public void SelectKind(TLSListKind kind)
        {
            if ((int)kind < 0 || (int)kind > 2) throw new ArgumentOutOfRangeException("kind");
            Current = new TLSListState(kind); Trace = null; Step = 0; prediction = null; PredictionUsed = false; applied = false;
            PredictionReview = "New list initialized. No save or mastery written.";
        }
        public void Start(TLSListAction action, int value = 0, int index = 0)
        {
            Trace = TLSLinkedListEngine.Make(Current, action, value, index); Step = 0;
            prediction = null; PredictionUsed = false; applied = false;
            PredictionReview = "Predict: more nodes / fewer nodes / same count before stepping.";
        }
        public void Predict(int choice)
        {
            if (Trace == null || Step != 0 || prediction.HasValue || choice < -1 || choice > 1) return;
            prediction = choice;
            PredictionUsed = true;
            PredictionReview = "Prediction recorded; finish trace to compare with modeled result.";
        }
        public void Next()
        {
            if (Trace == null || Step >= Trace.Frames.Count - 1) return;
            Step++;
            if (Step == Trace.Frames.Count - 1 && !applied)
            {
                applied = true;
                int oldCount = Current.Count;
                int newCount = Trace.Final.Count;
                if (prediction.HasValue)
                {
                    int outcome = Math.Sign(newCount - oldCount);
                    bool right = outcome == prediction.Value;
                    if (right) FirstIndependentPredictions++;
                    PredictionReview = (right ? "Correct. " : "Compare again. ") +
                        "Reachable nodes changed from " + oldCount + " to " + newCount + ". Formative prediction only.";
                }
                Current = Trace.Final; // authoritative commit only at final step
            }
        }
        public void Previous()
        {
            if (Trace != null && Step > 0) Step--; // visual rewind never undoes an already committed operation
        }
        public void Finish() { if (Trace != null) while (Step < Trace.Frames.Count - 1) Next(); }
        public void ResetTrace() { Trace = null; Step = 0; prediction = null; PredictionReview = "Trace reset. Committed list retained."; }
    }
}
