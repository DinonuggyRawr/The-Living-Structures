using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TLS.Core
{
    public enum TLSLinkedTaskType { Trace, Complete, Repair, Challenge }
    public sealed class TLSLinkedTask
    {
        public TLSLinkedTaskType Type;
        public string Title;
        public string Question;
        public string Answer;
        public string Hint;
        public string Why;
        public TLSLinkedTask(TLSLinkedTaskType type, string title, string q, string a, string hint, string why)
        {Type=type;Title=title;Question=q;Answer=a;Hint=hint;Why=why;}
    }
    public static class TLSLinkedCodingCatalog
    {
        public static readonly TLSLinkedTask[] All = {
            new TLSLinkedTask(TLSLinkedTaskType.Trace,"Head update", "Starting with 10 -> 20, prepend 5. What value is at head?", "5", "Head always points to the first node.", "Prepending publishes the new node as head."),
            new TLSLinkedTask(TLSLinkedTaskType.Trace,"Remove head", "Starting 10 -> 20 -> 30, remove index 0. New head value?", "20", "Removing the first node changes head.", "The former second node becomes the first reachable node."),
            new TLSLinkedTask(TLSLinkedTaskType.Trace,"Circular traversal", "A circular list has 4 nodes. How many unique nodes are visited before returning to head?", "4", "Stop at head on the next visit.", "Circular traversal needs a boundary; next never becomes NULL on a nonempty ring."),
            new TLSLinkedTask(TLSLinkedTaskType.Trace,"Reverse head", "Reverse singly list 2 -> 7 -> 9. What is the new head value?", "9", "The former tail becomes head.", "Reversal changes pointers rather than moving allocations."),
            new TLSLinkedTask(TLSLinkedTaskType.Trace,"Recursive count", "count(NULL) returns 0. How many nodes does count(1 -> 2 -> 3 -> NULL) return?", "3", "Each non-NULL frame contributes one.", "Three recursive frames unwind to 3."),
            new TLSLinkedTask(TLSLinkedTaskType.Complete,"C malloc", "Complete: Node *node = ____;", "malloc(sizeof *node)", "Allocate exactly sizeof *node.", "malloc returns uninitialized storage; guard NULL and initialize its fields."),
            new TLSLinkedTask(TLSLinkedTaskType.Complete,"Prepend next", "Write the C statement linking a new node to the previous head.", "node->next = *head;", "The caller's head is a Node **.", "Set next before replacing *head or the old chain may be lost."),
            new TLSLinkedTask(TLSLinkedTaskType.Complete,"Doubly prev", "Write the C assignment connecting second->prev to first.", "second->prev = first;", "A doubly linked list carries reverse links.", "Every forward link must agree with its reverse link."),
            new TLSLinkedTask(TLSLinkedTaskType.Complete,"Circular tail link", "Write the assignment closing a circular ring, given tail and head.", "tail->next = head;", "The final node points back to head.", "A nonempty circular singly linked list's tail->next equals head."),
            new TLSLinkedTask(TLSLinkedTaskType.Complete,"Recursive base", "Complete the statement inside count(): if (head == NULL) ____;", "return 0;", "An empty suffix contains zero nodes.", "The base case terminates recursive traversal."),
            new TLSLinkedTask(TLSLinkedTaskType.Repair,"Head alias", "Fix broken code: Node *old = *head; free(old); *head = old->next;", "Node *old = *head; *head = old->next; free(old);", "Never dereference after free.", "Save or reassign the next link before ending the object's lifetime."),
            new TLSLinkedTask(TLSLinkedTaskType.Repair,"Guard malloc", "Repair guard: if (node != NULL) return false;", "if (node == NULL) return false;", "Failure returns NULL.", "Only dereference a successfully allocated node."),
            new TLSLinkedTask(TLSLinkedTaskType.Repair,"Doubly back link", "Repair: first->next = second; second->prev = NULL;", "first->next = second; second->prev = first;", "The reverse link should identify first.", "A doubly linked list's next/prev pointers must agree."),
            new TLSLinkedTask(TLSLinkedTaskType.Repair,"Circular stop", "Repair circular search condition: while (current != NULL) { ... } (express a safe do-while stop condition)", "while (current != head);", "After processing the first element, stop when returning to head.", "This is a do-while terminator, not a complete search function. Handle an empty head separately."),
            new TLSLinkedTask(TLSLinkedTaskType.Repair,"Recursive progress", "Repair: return 1 + count(head);", "return 1 + count(head->next);", "Recursive calls must approach the base case.", "Passing the same pointer causes unbounded recursion."),
            new TLSLinkedTask(TLSLinkedTaskType.Challenge,"Singly independent", "Starting with 1 -> 2 -> 3, remove index 1, then prepend 9. Final head value?", "9", "Predict the new list after each operation.", "Final order 9 -> 1 -> 3."),
            new TLSLinkedTask(TLSLinkedTaskType.Challenge,"Doubly independent", "List A <-> B <-> C. After removing B, how many neighboring links must agree between A and C?", "2", "Forward and backward links.", "A.next=C and C.prev=A must both be correct."),
            new TLSLinkedTask(TLSLinkedTaskType.Challenge,"Circular independent", "A circular list has values 4 -> 5 -> 6 -> (head). Starting at head, how many next steps return to head?", "3", "Count the final tail-to-head hop.", "Three hops complete the ring."),
            new TLSLinkedTask(TLSLinkedTaskType.Challenge,"Recursive independent", "A linear list holds 7 -> 8 -> NULL. What does recursive count return?", "2", "Count calls that are not the base case.", "Two non-NULL frames add one each; NULL returns zero.")
        };
        public static int Count(TLSLinkedTaskType kind) {int n=0;foreach (TLSLinkedTask t in All)if(t.Type==kind)n++;return n;}
        public static int Find(TLSLinkedTaskType kind, int index)
        {int n=0;for(int i=0;i<All.Length;i++)if(All[i].Type==kind){if(n==index)return i;n++;}throw new ArgumentOutOfRangeException("index");}
        // Reference-token matching only: does not compile C or accept all semantically equivalent C.
        public static bool Matches(TLSLinkedTask task, string answer)
        {
            if (task == null || string.IsNullOrWhiteSpace(answer)) return false;
            if (task.Type == TLSLinkedTaskType.Trace || task.Type == TLSLinkedTaskType.Challenge)
                return string.Equals(answer.Trim(),task.Answer,StringComparison.Ordinal);
            string lex = @"(?:->|==|!=|<=|>=|&&|\|\||\+\+|--|[a-zA-Z_]\w*|\d+|[^\s])";
            MatchCollection one=Regex.Matches(task.Answer,lex),two=Regex.Matches(answer,lex);
            if (one.Count!=two.Count) return false;
            for(int i=0;i<one.Count;i++)if(one[i].Value!=two[i].Value)return false;
            return true;
        }
    }
    public sealed class TLSLinkedCodingSession
    {
        private readonly bool[] attempted = new bool[TLSLinkedCodingCatalog.All.Length];
        private readonly bool[] exposed = new bool[TLSLinkedCodingCatalog.All.Length];
        private readonly bool[] firstIndependent = new bool[TLSLinkedCodingCatalog.All.Length];
        private readonly bool[] solved = new bool[TLSLinkedCodingCatalog.All.Length];
        public TLSLinkedTaskType Type {get;private set;} = TLSLinkedTaskType.Trace;
        public int Index {get;private set;}
        public bool Guided {get;private set;}=true;
        public bool HintVisible {get;private set;}
        public bool ReferenceVisible {get;private set;}
        public string Answer {get;set;}="";
        public string Feedback {get;private set;}="Answer before revealing the solution. No coding mastery awarded.";
        public int SelectedId {get{return TLSLinkedCodingCatalog.Find(Type,Index);}}
        public TLSLinkedTask Selected {get{return TLSLinkedCodingCatalog.All[SelectedId];}}
        public int SolvedCount {get{int n=0;foreach(bool x in solved)if(x)n++;return n;}}
        public int IndependentCount {get{int n=0;foreach(bool x in firstIndependent)if(x)n++;return n;}}
        public void Select(TLSLinkedTaskType kind,int index)
        {
            if(index<0||index>=TLSLinkedCodingCatalog.Count(kind))throw new ArgumentOutOfRangeException("index");
            Type=kind;Index=index;Answer="";HintVisible=false;ReferenceVisible=false;Feedback="Unseen? Attempt before showing hints.";
        }
        public void SetGuided(bool guided){Guided=guided;HintVisible=false;ReferenceVisible=false;}
        public void ShowHint(){if(!Guided)return;exposed[SelectedId]=true;HintVisible=true;}
        public void Reveal(){exposed[SelectedId]=true;ReferenceVisible=true;HintVisible=false;}
        public bool Check()
        {
            int id=SelectedId;bool correct=TLSLinkedCodingCatalog.Matches(Selected,Answer);
            if(!attempted[id] && !Guided && !exposed[id] && correct) firstIndependent[id]=true;
            attempted[id]=true;if(correct)solved[id]=true;
            Feedback=correct?"Correct. "+Selected.Why:"Try again. "+(Guided?Selected.Hint:"Review the node links and C invariants before retrying.");
            return correct;
        }
    }
}
