using System;
using System.Text.RegularExpressions;

namespace TLS.Core
{
    public enum TLSPhase4TaskType { Trace, Complete, Repair, Challenge }
    public sealed class TLSPhase4Task
    {
        public TLSPhase4TaskType Type;
        public string Title, Prompt, Answer, Hint, Explanation;
        public TLSPhase4Task(TLSPhase4TaskType kind,string title,string prompt,string answer,string hint,string why)
        {Type=kind;Title=title;Prompt=prompt;Answer=answer;Hint=hint;Explanation=why;}
    }
    public static class TLSPhase4PracticeCatalog
    {
        public static readonly TLSPhase4Task[] All = {
            new TLSPhase4Task(TLSPhase4TaskType.Trace,"Recursive sum","sum(n) returns 0 for n=0 and otherwise n+sum(n-1). What is sum(4)?","10","Add 4 + 3 + 2 + 1.","Each frame waits for the next frame's return; sum(4)=10."),
            new TLSPhase4Task(TLSPhase4TaskType.Trace,"Factorial","factorial(0)=1; factorial(n)=n*factorial(n-1). What is factorial(4)?","24","4 * 3 * 2 * 1.","The base case returns 1 and unwinding multiplies results."),
            new TLSPhase4Task(TLSPhase4TaskType.Trace,"Fibonacci","fib(0)=0, fib(1)=1. What does fib(5) return?","5","Two recursive calls for n>=2.","fib(5)=fib(4)+fib(3)=3+2=5."),
            new TLSPhase4Task(TLSPhase4TaskType.Trace,"BST inorder","Insert keys 8, 3, 10, 1, 6. What value is visited third in inorder traversal?","6","Visit left, node, right.","The full order is 1,3,6,8,10."),
            new TLSPhase4Task(TLSPhase4TaskType.Trace,"Height in levels","BST 8 root, left 3, left-left 1, right 10. Height in LEVELS?","3","Empty tree 0, leaf 1.","Longest root-to-leaf path contains three nodes."),
            new TLSPhase4Task(TLSPhase4TaskType.Trace,"Delete successor","BST root 8 with left 3 and right 10; right subtree root 10 has left 9. Delete 8; successor value?","9","Minimum of right subtree.","Successor 9 replaces 8; successor's original node is unlinked."),
            new TLSPhase4Task(TLSPhase4TaskType.Complete,"Recursive base","Complete sum: if (n == 0) ____;","return 0;","Zero contributes nothing.","Base case stops recursion."),
            new TLSPhase4Task(TLSPhase4TaskType.Complete,"Factorial recurrence","Complete factorial: return ____;","n * factorial(n - 1)","Multiply current n by smaller return.","n shrinks toward zero each call."),
            new TLSPhase4Task(TLSPhase4TaskType.Complete,"BST branch","Complete: if (key < root->value) return ____;","bst_search(root->left, key)","Smaller keys lie to the left.","Binary search ordering reduces search to one child."),
            new TLSPhase4Task(TLSPhase4TaskType.Complete,"NULL guard","Write the C statement that returns false if root is NULL.","if (root == NULL) return false;","Check a NULL pointer before reading fields.","Dereferencing NULL has undefined behavior."),
            new TLSPhase4Task(TLSPhase4TaskType.Complete,"Count children","Complete recursive count: return ____;","1 + tree_count(root->left) + tree_count(root->right)","Count this node and both subtrees.","Every non-NULL node contributes one."),
            new TLSPhase4Task(TLSPhase4TaskType.Complete,"Allocate node","Write a C allocation statement for TreeNode *node.","TreeNode *node = malloc(sizeof *node);","Use sizeof *node, not sizeof pointer.","Check for NULL before initializing the new storage."),
            new TLSPhase4Task(TLSPhase4TaskType.Repair,"Missing base case","Repair: int sum(int n) { return n + sum(n-1); } (write proper early guard)","if (n == 0) return 0;","Recursion needs a terminating case.","Without a base case the function recurses until stack exhaustion."),
            new TLSPhase4Task(TLSPhase4TaskType.Repair,"Wrong BST direction","Fix BST search: if (key < root->value) return bst_search(root->right, key);","if (key < root->value) return bst_search(root->left, key);","Smaller values branch left.","Wrong direction misses present keys."),
            new TLSPhase4Task(TLSPhase4TaskType.Repair,"Free before use","Repair: free(victim); *root = victim->right;","*root = victim->right; free(victim);","Read child link before free.","Accessing victim after free is use-after-free."),
            new TLSPhase4Task(TLSPhase4TaskType.Repair,"Missing recursive child","Repair: return 1 + tree_count(root->left);","return 1 + tree_count(root->left) + tree_count(root->right);","Both subtrees matter.","One-sided count undercounts the tree."),
            new TLSPhase4Task(TLSPhase4TaskType.Repair,"Incorrect empty height","Correct: if (root == NULL) return 1;","if (root == NULL) return 0;","Height here counts levels.","Empty tree has zero levels; a leaf has one."),
            new TLSPhase4Task(TLSPhase4TaskType.Repair,"Preorder order","Repair incorrect order: visit(node); preorder(node->right); preorder(node->left);","visit(node); preorder(node->left); preorder(node->right);","Preorder uses node, left, right.","Order controls observable traversal results."),
            new TLSPhase4Task(TLSPhase4TaskType.Challenge,"BST delete root","BST values 10,5,15,12,18. Delete 10. New root value when using inorder successor?","12","Smallest value in right subtree.","Successor 12 replaces root key 10."),
            new TLSPhase4Task(TLSPhase4TaskType.Challenge,"BST duplicate","Insert 7,3,11,3 into a duplicate-rejecting BST. Final node count?","3","The second key 3 is not inserted.","Duplicates are explicitly rejected in this teaching model."),
            new TLSPhase4Task(TLSPhase4TaskType.Challenge,"Binary vs BST","In a manually assembled binary tree with root 10, left child 20, is it a valid BST? Answer 0 for no or 1 for yes.","0","BST requires every left key < root key.","A binary tree is not necessarily a search tree."),
            new TLSPhase4Task(TLSPhase4TaskType.Challenge,"Recursive depth","Calling factorial(4) reaches factorial(0). Maximum simultaneously ACTIVE C frames?","5","Count calls 4,3,2,1,0.","Five frames exist when base-case factorial(0) runs."),
            new TLSPhase4Task(TLSPhase4TaskType.Challenge,"Inorder sorted","BST values 5,1,8,3. What value is second in inorder?","3","Inorder produces ascending keys for a valid BST.","Sorted order is 1,3,5,8."),
            new TLSPhase4Task(TLSPhase4TaskType.Challenge,"Leaf height","A one-node tree has height in levels?","1","Leaf is one level.","The project explicitly uses height in levels (not edges).")
        };
        public static int Count(TLSPhase4TaskType type)
        {int n=0;foreach(TLSPhase4Task t in All)if(t.Type==type)n++;return n;}
        public static int Find(TLSPhase4TaskType type,int index)
        {int n=0;for(int i=0;i<All.Length;i++)if(All[i].Type==type){if(n==index)return i;n++;}throw new ArgumentOutOfRangeException("index");}
        // Exact reference-token comparison: NOT C compilation and not proof of general semantic equivalence.
        public static bool Matches(TLSPhase4Task task,string submitted)
        {
            if(task==null||string.IsNullOrWhiteSpace(submitted))return false;
            if(task.Type==TLSPhase4TaskType.Trace||task.Type==TLSPhase4TaskType.Challenge)
                return string.Equals(task.Answer,submitted.Trim(),StringComparison.Ordinal);
            const string pattern=@"(?:->|==|!=|<=|>=|&&|\|\||\+\+|--|[a-zA-Z_]\w*|\d+|[^\s])";
            MatchCollection x=Regex.Matches(task.Answer,pattern),y=Regex.Matches(submitted,pattern);
            if(x.Count!=y.Count)return false;
            for(int i=0;i<x.Count;i++)if(x[i].Value!=y[i].Value)return false;
            return true;
        }
    }
    public sealed class TLSPhase4PracticeSession
    {
        private readonly bool[] attempted=new bool[TLSPhase4PracticeCatalog.All.Length], exposed=new bool[TLSPhase4PracticeCatalog.All.Length],
            solved=new bool[TLSPhase4PracticeCatalog.All.Length], firstIndependent=new bool[TLSPhase4PracticeCatalog.All.Length];
        public TLSPhase4TaskType Type{get;private set;}=TLSPhase4TaskType.Trace;
        public int Index{get;private set;}
        public bool Guided{get;private set;}=true;
        public bool HintVisible{get;private set;}
        public bool ReferenceVisible{get;private set;}
        public string Answer{get;set;}="";
        public string Feedback{get;private set;}="Respond first; support is available, but no saved mastery is awarded.";
        public int SelectedId{get{return TLSPhase4PracticeCatalog.Find(Type,Index);}}
        public TLSPhase4Task Selected{get{return TLSPhase4PracticeCatalog.All[SelectedId];}}
        public int Solved{get{int n=0;foreach(bool x in solved)if(x)n++;return n;}}
        public int Independent{get{int n=0;foreach(bool x in firstIndependent)if(x)n++;return n;}}
        public void Select(TLSPhase4TaskType type,int index)
        {if(index<0||index>=TLSPhase4PracticeCatalog.Count(type))throw new ArgumentOutOfRangeException("index");
            Type=type;Index=index;Answer="";HintVisible=false;ReferenceVisible=false;Feedback="Attempt before hints or reveals.";}
        public void SetGuided(bool yes){Guided=yes;HintVisible=false;ReferenceVisible=false;}
        public void ShowHint(){if(Guided){exposed[SelectedId]=true;HintVisible=true;}}
        public void Reveal(){exposed[SelectedId]=true;ReferenceVisible=true;HintVisible=false;}
        public bool Check()
        {
            int id=SelectedId;bool correct=TLSPhase4PracticeCatalog.Matches(Selected,Answer);
            if(!attempted[id]&&!Guided&&!exposed[id]&&correct)firstIndependent[id]=true;
            attempted[id]=true;if(correct)solved[id]=true;
            Feedback=correct?"Correct. "+Selected.Explanation:"Review your reasoning. "+(Guided?Selected.Hint:"You may retry; initial unseen credit is no longer available.");
            return correct;
        }
    }
}
