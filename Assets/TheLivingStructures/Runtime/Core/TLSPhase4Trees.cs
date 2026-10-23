using System;
using System.Collections.Generic;

namespace TLS.Core
{
    public enum TLSTreeKind { General, Binary, BST }
    public enum TLSTreeAction { AddRoot, AddChild, AddLeft, AddRight, Insert, Search, Delete, Preorder, Inorder, Postorder, LevelOrder, Reset }

    public sealed class TLSTreeNode
    {
        public int Id, Value, Parent = -1, Left = -1, Right = -1;
        public readonly List<int> Children = new List<int>();
        public TLSTreeNode Copy()
        {
            TLSTreeNode n = new TLSTreeNode { Id = Id, Value = Value, Parent = Parent, Left = Left, Right = Right };
            n.Children.AddRange(Children);
            return n;
        }
    }
    /// <summary>Session-only symbolic nodes. IDs are not process addresses. All links are checked and acyclic.</summary>
    public sealed class TLSTreeState
    {
        public const int MaximumNodes = 15;
        public TLSTreeKind Kind;
        public int Root = -1;
        public int NextId;
        public readonly List<TLSTreeNode> Nodes = new List<TLSTreeNode>();
        public TLSTreeState(TLSTreeKind kind) { Kind = kind; }
        public TLSTreeNode Find(int id)
        {
            foreach (TLSTreeNode n in Nodes) if (n.Id == id) return n;
            return null;
        }
        public TLSTreeNode FindValue(int value)
        {
            foreach (TLSTreeNode n in Nodes) if (n.Value == value) return n;
            return null;
        }
        public string Address(int id) { return id < 0 ? "NULL" : "0x" + (0x1000 + id * 0x20).ToString("X4"); }
        public TLSTreeState Copy()
        {
            TLSTreeState s = new TLSTreeState(Kind) { Root = Root, NextId = NextId };
            foreach (TLSTreeNode n in Nodes) s.Nodes.Add(n.Copy());
            return s;
        }
        public int Add(int value, int parent = -1)
        {
            if (Nodes.Count >= MaximumNodes) throw new InvalidOperationException("The educational tree is full (15 nodes).");
            if (FindValue(value) != null) throw new InvalidOperationException("Use a unique value so the pointer inspector stays unambiguous.");
            TLSTreeNode n = new TLSTreeNode { Id = NextId++, Value = value, Parent = parent };
            Nodes.Add(n);
            return n.Id;
        }
        public int Count { get { return Nodes.Count; } }
        public int Depth(int id)
        {
            int depth = 0, guard = 0;
            TLSTreeNode n = Find(id);
            if (n == null) return -1;
            while (n.Parent >= 0)
            {
                if (++guard > MaximumNodes) throw new InvalidOperationException("Cycle in parent links.");
                n = Find(n.Parent);
                if (n == null) throw new InvalidOperationException("Invalid parent link.");
                depth++;
            }
            return depth;
        }
        public int Height { get { return HeightAt(Root, new HashSet<int>()); } }
        private int HeightAt(int id, HashSet<int> visited)
        {
            if (id < 0) return 0;
            TLSTreeNode n = Find(id);
            if (n == null || !visited.Add(id)) throw new InvalidOperationException("Broken tree or cycle.");
            int height = 0;
            if (Kind == TLSTreeKind.General)
            {
                foreach (int child in n.Children) height = Math.Max(height, HeightAt(child, visited));
            }
            else
            {
                height = Math.Max(HeightAt(n.Left, visited), HeightAt(n.Right, visited));
            }
            return height + 1; // height in LEVELS: empty = 0, one node = 1
        }
        private void Pre(int id, List<int> values)
        {
            if (id < 0) return;
            TLSTreeNode n = Find(id);
            values.Add(n.Value);
            if (Kind == TLSTreeKind.General) foreach (int child in n.Children) Pre(child, values);
            else { Pre(n.Left, values); Pre(n.Right, values); }
        }
        private void In(int id, List<int> values)
        {
            if (id < 0) return;
            TLSTreeNode n = Find(id);
            In(n.Left, values); values.Add(n.Value); In(n.Right, values);
        }
        private void Post(int id, List<int> values)
        {
            if (id < 0) return;
            TLSTreeNode n = Find(id);
            if (Kind == TLSTreeKind.General) foreach (int child in n.Children) Post(child, values);
            else { Post(n.Left, values); Post(n.Right, values); }
            values.Add(n.Value);
        }
        public List<int> Traverse(TLSTreeAction action)
        {
            List<int> values = new List<int>();
            if (action == TLSTreeAction.Inorder)
            {
                if (Kind == TLSTreeKind.General) throw new InvalidOperationException("Inorder requires a binary tree.");
                In(Root, values);
            }
            else if (action == TLSTreeAction.Preorder) Pre(Root, values);
            else if (action == TLSTreeAction.Postorder) Post(Root, values);
            else if (action == TLSTreeAction.LevelOrder)
            {
                if (Root >= 0)
                {
                    Queue<int> queue = new Queue<int>(); queue.Enqueue(Root);
                    while (queue.Count > 0)
                    {
                        TLSTreeNode n = Find(queue.Dequeue()); values.Add(n.Value);
                        if (Kind == TLSTreeKind.General) foreach (int child in n.Children) queue.Enqueue(child);
                        else { if (n.Left >= 0) queue.Enqueue(n.Left); if (n.Right >= 0) queue.Enqueue(n.Right); }
                    }
                }
            }
            else throw new ArgumentOutOfRangeException("action");
            return values;
        }
        public bool Invariants()
        {
            try
            {
                if (Nodes.Count == 0) return Root == -1;
                if (Root < 0 || Find(Root) == null || Find(Root).Parent != -1) return false;
                HashSet<int> ids = new HashSet<int>(), values = new HashSet<int>();
                foreach (TLSTreeNode n in Nodes)
                    if (!ids.Add(n.Id) || !values.Add(n.Value)) return false;
                HashSet<int> found = new HashSet<int>();
                Queue<int> pending = new Queue<int>(); pending.Enqueue(Root);
                while (pending.Count > 0)
                {
                    int id = pending.Dequeue();
                    TLSTreeNode n = Find(id);
                    if (n == null || !found.Add(id)) return false;
                    if (Kind == TLSTreeKind.General)
                    {
                        if (n.Left >= 0 || n.Right >= 0) return false;
                        foreach (int child in n.Children)
                        { if (Find(child) == null || Find(child).Parent != id) return false; pending.Enqueue(child); }
                    }
                    else
                    {
                        if (n.Children.Count > 0) return false;
                        if (n.Left >= 0) { if (Find(n.Left) == null || Find(n.Left).Parent != id) return false; pending.Enqueue(n.Left); }
                        if (n.Right >= 0) { if (Find(n.Right) == null || Find(n.Right).Parent != id) return false; pending.Enqueue(n.Right); }
                    }
                }
                if (found.Count != Nodes.Count) return false;
                if (Kind == TLSTreeKind.BST) return Ordered(Root, null, null);
                return true;
            }
            catch { return false; }
        }
        private bool Ordered(int id, long? min, long? max)
        {
            if (id < 0) return true;
            TLSTreeNode n = Find(id);
            if (n == null || (min.HasValue && n.Value <= min.Value) || (max.HasValue && n.Value >= max.Value)) return false;
            return Ordered(n.Left, min, n.Value) && Ordered(n.Right, n.Value, max);
        }
        public string ValuesString(TLSTreeAction order)
        { return string.Join(", ", Traverse(order).ConvertAll(v => v.ToString()).ToArray()); }
    }

    public sealed class TLSTreeFrame
    {
        public TLSTreeState State;
        public int Focus = -1;
        public int Line;
        public string Explanation = "";
        public string Stack = "";
        public TLSTreeFrame(TLSTreeState state, int focus, int line, string explanation, string stack)
        { State = state.Copy(); Focus = focus; Line = line; Explanation = explanation; Stack = stack; }
    }
    public sealed class TLSTreeTrace
    {
        public TLSTreeAction Action;
        public string[] Code;
        public readonly List<TLSTreeFrame> Frames = new List<TLSTreeFrame>();
        public TLSTreeState Before, After;
        public bool Success, Mutates;
        public string Diagnostic = "";
    }
    public sealed class TLSTreeSession
    {
        private readonly TLSTreeState[] states = {
            new TLSTreeState(TLSTreeKind.General), new TLSTreeState(TLSTreeKind.Binary), new TLSTreeState(TLSTreeKind.BST)
        };
        public TLSTreeKind Kind { get; private set; } = TLSTreeKind.BST;
        public TLSTreeState Current { get { return states[(int)Kind]; } }
        public TLSTreeTrace Trace { get; private set; }
        public int Step { get; private set; }
        public TLSTreeFrame Frame { get { return Trace == null ? null : Trace.Frames[Step]; } }
        public string Prediction = "Predict: will the node count increase, decrease, or stay unchanged?";
        public bool Predicted { get; private set; }
        private int guess;
        public string InputError { get; private set; } = "";
        public void RejectInput(string explanation) { InputError = explanation; }
        public void Select(TLSTreeKind kind)
        { if ((int)kind < 0 || (int)kind > 2) throw new ArgumentOutOfRangeException("kind"); Kind=kind; Trace=null;Step=0;Predicted=false;Prediction="Select an operation and predict the count change."; }
        private static string[] CodeFor(TLSTreeAction a, TLSTreeKind kind)
        {
            if (kind == TLSTreeKind.General && (a == TLSTreeAction.Preorder || a == TLSTreeAction.Postorder))
                return a == TLSTreeAction.Preorder ? new[] {"// GeneralNode teaching sketch", "if (node == NULL) return;", "visit(node->value);", "for (size_t i=0; i<node->child_count; ++i)", "    general_preorder(node->children[i]);", "// recurse through all children"}:
                    new[] {"// GeneralNode teaching sketch", "if (node == NULL) return;", "for (size_t i=0; i<node->child_count; ++i)", "    general_postorder(node->children[i]);", "visit(node->value);", "// return to parent"};
            switch (a)
            {
                case TLSTreeAction.Insert: return new[] { "bool bst_insert(TreeNode **root, int value) {", "  if (*root == NULL) allocate_and_link();", "  else if (value < (*root)->value) recurse_left();", "  else if (value > (*root)->value) recurse_right();", "  else return false; // duplicate", "  return true;" };
                case TLSTreeAction.Search: return new[] { "bool bst_search(const TreeNode *root, int target) {", "  if (root == NULL) return false;", "  if (root->value == target) return true;", "  return bst_search(target < root->value ? root->left : root->right, target);", "}" };
                case TLSTreeAction.Delete: return new[] { "bool bst_delete(TreeNode **root, int target) {", "  // find target by ordering comparison", "  // zero or one child: relink parent before free", "  // two children: copy inorder successor value", "  // unlink and free inorder successor", "}" };
                case TLSTreeAction.Preorder: return new[] { "void preorder(const TreeNode *node) {", "  if (node == NULL) return;", "  visit(node->value);", "  preorder(node->left);", "  preorder(node->right);", "}" };
                case TLSTreeAction.Inorder: return new[] { "void inorder(const TreeNode *node) {", "  if (node == NULL) return;", "  inorder(node->left);", "  visit(node->value);", "  inorder(node->right);", "}" };
                case TLSTreeAction.Postorder: return new[] { "void postorder(const TreeNode *node) {", "  if (node == NULL) return;", "  postorder(node->left);", "  postorder(node->right);", "  visit(node->value);", "}" };
                case TLSTreeAction.LevelOrder: return new[] { "// Level-order traversal uses a queue", "enqueue(root);", "while (!empty()) {", "  TreeNode *node = dequeue(); visit(node->value);", "  enqueue(non_NULL_children(node));", "}" };
                default: return new[] { "// Guided tree topology operation", "// Validate parent/slot/root and capacity", "// Update links to preserve acyclic structure", "// Inspect new head, children, height, depth" };
            }
        }
        private static bool IsMutation(TLSTreeAction a)
        { return a == TLSTreeAction.AddRoot || a == TLSTreeAction.AddChild || a == TLSTreeAction.AddLeft || a == TLSTreeAction.AddRight || a == TLSTreeAction.Insert || a == TLSTreeAction.Delete || a == TLSTreeAction.Reset; }
        private void Visit(TLSTreeTrace t, TLSTreeState s, int focus, int line, string text, string stack = "")
        { t.Frames.Add(new TLSTreeFrame(s, focus, line, text, stack)); }
        public void Begin(TLSTreeAction action, int value, int parentValue = 0)
        {
            TLSTreeState before = Current.Copy(), working = before.Copy();
            TLSTreeTrace t = new TLSTreeTrace { Action = action, Before = before, After = before.Copy(), Mutates = IsMutation(action), Code = CodeFor(action, Kind) };
            Trace=t;Step=0;Predicted=false;InputError="";Prediction="Predict the final node count before stepping.";
            Visit(t, working, -1, 0, "Inspect current topology. No changes are committed before FINISH.");
            try
            {
                if (action == TLSTreeAction.AddRoot)
                {
                    if (working.Root >= 0) throw new InvalidOperationException("Root already exists.");
                    int id=working.Add(value);working.Root=id;
                    Visit(t,working,id,2,"Allocate a symbolic root node. Its parent is NULL.");
                }
                else if (action == TLSTreeAction.AddChild || action == TLSTreeAction.AddLeft || action == TLSTreeAction.AddRight)
                {
                    if (working.Root < 0) throw new InvalidOperationException("Create the root first.");
                    if ((action == TLSTreeAction.AddChild) != (Kind == TLSTreeKind.General)) throw new InvalidOperationException("Use ADD CHILD for general trees and LEFT/RIGHT for binary trees.");
                    if (Kind == TLSTreeKind.BST) throw new InvalidOperationException("BST children must be inserted by key ordering.");
                    TLSTreeNode parent=working.FindValue(parentValue);
                    if (parent==null) throw new InvalidOperationException("Parent value not found.");
                    if (action==TLSTreeAction.AddLeft && parent.Left>=0 || action==TLSTreeAction.AddRight && parent.Right>=0)
                        throw new InvalidOperationException("The selected child slot is occupied.");
                    int id=working.Add(value,parent.Id);
                    if (action == TLSTreeAction.AddChild) parent.Children.Add(id);
                    else if (action == TLSTreeAction.AddLeft) parent.Left=id; else parent.Right=id;
                    Visit(t,working,id,2,"Link child to parent " + parentValue + ". Depth = " + working.Depth(id) + ".");
                }
                else if (action == TLSTreeAction.Insert)
                {
                    if (Kind != TLSTreeKind.BST) throw new InvalidOperationException("BST insertion requires BST mode.");
                    int at=working.Root,parentId=-1;bool left=false;
                    while (at>=0)
                    {
                        TLSTreeNode n=working.Find(at);
                        Visit(t,working,n.Id,value<n.Value?2:value>n.Value?3:4,"Compare "+value+" to node "+n.Value+"; choose one subtree.","insert("+value+")");
                        if (value==n.Value) throw new InvalidOperationException("Duplicate key rejected; tree remains unchanged.");
                        parentId=at;left=value<n.Value;at=left?n.Left:n.Right;
                    }
                    int id=working.Add(value,parentId);
                    if (parentId < 0) working.Root=id;
                    else {TLSTreeNode p=working.Find(parentId);if(left)p.Left=id;else p.Right=id;}
                    Visit(t,working,id,1,"Link newly allocated node " +value+"; BST ordering preserved.");
                }
                else if (action == TLSTreeAction.Search)
                {
                    if (Kind != TLSTreeKind.BST) throw new InvalidOperationException("Ordered search is for BST mode only.");
                    int at=working.Root;bool found=false;
                    while(at>=0)
                    {
                        TLSTreeNode n=working.Find(at);
                        Visit(t,working,at,n.Value==value?2:3,"Compare target "+value+" with "+n.Value+".","search("+value+")");
                        if(n.Value==value){found=true;break;}
                        at=value<n.Value?n.Left:n.Right;
                    }
                    Visit(t,working,at,found?2:1,found?"Target found. No links changed.":"Reached NULL. Target not present.");
                }
                else if (action == TLSTreeAction.Delete)
                {
                    if (Kind != TLSTreeKind.BST) throw new InvalidOperationException("BST deletion requires BST mode.");
                    TLSTreeNode n=working.Find(working.Root);
                    while(n!=null && n.Value!=value)
                    {
                        Visit(t,working,n.Id,1,"Compare while locating "+value+".","delete("+value+")");
                        n=working.Find(value<n.Value?n.Left:n.Right);
                    }
                    if(n==null)throw new InvalidOperationException("Key absent. Nothing to delete.");
                    Visit(t,working,n.Id,1,"Found node with "+(n.Left>=0?"left":"no left")+" and "+(n.Right>=0?"right":"no right")+" child.");
                    if(n.Left>=0 && n.Right>=0)
                    {
                        TLSTreeNode successor=working.Find(n.Right);
                        while(successor.Left>=0)successor=working.Find(successor.Left);
                        int replacement=successor.Value;
                        Visit(t,working,successor.Id,3,"Inorder successor "+replacement+" is the smallest key in the right subtree.");
                        // Two-child deletion: copy the successor value and remove the successor (at most one child).
                        n.Value=replacement;
                        n=successor;
                    }
                    int child=n.Left>=0?n.Left:n.Right;
                    if(n.Parent<0) working.Root=child;
                    else
                    {
                        TLSTreeNode parent=working.Find(n.Parent);
                        if(parent.Left==n.Id)parent.Left=child;
                        else if(parent.Right==n.Id)parent.Right=child;
                        else throw new InvalidOperationException("Invalid parent pointer.");
                    }
                    if(child>=0)working.Find(child).Parent=n.Parent;
                    working.Nodes.Remove(n); // conceptual free; no dangling live child references
                    Visit(t,working,child,4,"Relink before freeing detached node; retain "+working.Count+" live nodes.");
                }
                else if (action == TLSTreeAction.Reset)
                {
                    working = new TLSTreeState(Kind);
                    Visit(t,working,-1,2,"Reset this fictional tree only; no saved progress changed.");
                }
                else if (action == TLSTreeAction.Preorder || action == TLSTreeAction.Inorder || action == TLSTreeAction.Postorder || action == TLSTreeAction.LevelOrder)
                {
                    List<int> values=working.Traverse(action);
                    int i=0;
                    foreach(int v in values)
                    {
                        int id=working.FindValue(v).Id;
                        Visit(t,working,id,(action==TLSTreeAction.Preorder?2:action==TLSTreeAction.Inorder?3:action==TLSTreeAction.Postorder?4:3),
                            "VISIT "+v+" ("+(++i)+"/"+values.Count+"). Output: "+string.Join(", ",values.GetRange(0,i).ConvertAll(x=>x.ToString()).ToArray()),"recursive traversal / queue");
                    }
                    Visit(t,working,-1,5,"Traversal complete: "+string.Join(", ",values.ConvertAll(x=>x.ToString()).ToArray()));
                }
                else throw new InvalidOperationException("Unsupported tree operation.");
                if(!working.Invariants())throw new InvalidOperationException("Tree invariant failed; operation not committed.");
                t.Success=true;t.Diagnostic="Valid "+Kind+" operation. Node count: "+before.Count+" -> "+working.Count+".";
                t.After=working.Copy();
            }
            catch(Exception e)
            {
                t.Success=false;t.After=before.Copy();t.Diagnostic=e.Message;
                Visit(t,before,-1,0,"Operation rejected safely: "+e.Message);
            }
            Visit(t,t.Success?t.After:before,-1,t.Code.Length-1,t.Success?
                "End of trace. "+t.Diagnostic+" Use FINISH to commit successful mutations.":"Nothing committed. "+t.Diagnostic);
        }
        public void Next()
        {
            if(Trace==null || Step>=Trace.Frames.Count-1)return;
            Step++;
            if(Step==Trace.Frames.Count-1 && Trace.Success && Trace.Mutates)
                states[(int)Kind]=Trace.After.Copy();
        }
        public void Previous()
        {
            if(Trace==null || Step==0)return;
            if(Step==Trace.Frames.Count-1 && Trace.Success && Trace.Mutates)
                states[(int)Kind]=Trace.Before.Copy();
            Step--;
        }
        public void Finish(){if(Trace!=null)while(Step<Trace.Frames.Count-1)Next();}
        public void ResetTrace(){if(Trace!=null && Step==Trace.Frames.Count-1 && Trace.Success && Trace.Mutates)states[(int)Kind]=Trace.Before.Copy();Trace=null;Step=0;Predicted=false;Prediction="";}
        public void Predict(int delta)
        {
            if(Trace==null || Step>0 || Predicted)return;
            Predicted=true;guess=delta;
            int actual=Trace.After.Count-Trace.Before.Count;
            int expected=actual>0?1:actual<0?-1:0;
            Prediction=(expected==guess?"Correct prediction. ":"Prediction revised. ")+"Count "+Trace.Before.Count+" -> "+Trace.After.Count+". "+Trace.Diagnostic;
        }
    }
}
