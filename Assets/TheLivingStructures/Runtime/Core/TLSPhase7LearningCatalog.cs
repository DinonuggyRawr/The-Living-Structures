using System;
using System.Collections.Generic;

namespace TLS.Core
{
    /// <summary>Curriculum guide only; prerequisites are recommendations, never access gates.</summary>
    public sealed class TLSLearningTopic
    {
        public readonly string Id, Name, Hub, Prerequisite, Objective, ReviewHint;
        public TLSLearningTopic(string id, string name, string hub, string prerequisite, string objective, string reviewHint)
        { Id=id; Name=name; Hub=hub; Prerequisite=prerequisite; Objective=objective; ReviewHint=reviewHint; }
    }
    public static class TLSPhase7LearningCatalog
    {
        public static readonly TLSLearningTopic[] Topics = {
            new TLSLearningTopic("stack","Stacks","Linear Structures Center","None","Explain LIFO and implement guarded push/pop.","Review empty/full guards and logical count."),
            new TLSLearningTopic("array","Arrays","Memory Workshop","Stacks recommended","Explain indexed memory, insertion shifts and bounds.","Revisit right-shift order when inserting."),
            new TLSLearningTopic("queue","Circular queues","Linear Structures Center","Arrays recommended","Explain FIFO and ring-buffer wraparound.","Trace head + count modulo capacity."),
            new TLSLearningTopic("pointer","Pointers & memory","Memory Workshop","Arrays recommended","Track NULL, aliases, allocation, free and dangling references.","Review what happens to aliases after free."),
            new TLSLearningTopic("list","Linked lists","Linear Structures Center","Pointers recommended","Safely link, traverse, delete and free nodes.","Preserve next links before removing a node."),
            new TLSLearningTopic("recursion","Recursion","Recursion Chamber","Stacks recommended","Identify base cases, calls and return unwinding.","Trace both descent and return values."),
            new TLSLearningTopic("tree","Trees & traversals","Tree Observatory","Recursion recommended","Trace preorder, inorder, postorder and tree properties.","Distinguish structural depth from visit order."),
            new TLSLearningTopic("bst","Binary search trees","Tree Observatory","Trees recommended","Maintain BST invariants during insertion and deletion.","Trace the two-child deletion successor case."),
            new TLSLearningTopic("heap","Heaps","Heap Workshop","Arrays and trees recommended","Relate heap order to array-backed parent/child indices.","A heap is not necessarily a sorted array."),
            new TLSLearningTopic("graph","Graphs","Graph Exploration Center","Queues and recursion recommended","Compare BFS and DFS on directed/undirected graphs.","Review visited sets and the frontier."),
            new TLSLearningTopic("hash","Hash tables","Hashing Laboratory","Arrays recommended","Explain collision handling and tombstones.","Deleted probe slots cannot simply become EMPTY."),
            new TLSLearningTopic("analysis","Complexity","Algorithm Analysis Center","Any structure","Specify assumptions behind asymptotic bounds.","Separate O-growth from exact operation counts."),
            new TLSLearningTopic("testing","Testing & debugging","Debugging & Testing Center","Any C lab","Design targeted tests, explain failures and document contracts.","Target a specific edge case rather than retesting a typical example.")
        };
        public static int Find(string id)
        { for(int i=0;i<Topics.Length;i++)if(Topics[i].Id==id)return i;return -1; }
    }

    /// <summary>Constrained, authored C11 tracing items. Final items differ from baseline items.</summary>
    public sealed class TLSPhase7Question
    {
        public readonly string Id, TopicId, Prompt, OptionA, OptionB, OptionC, Correct, Explanation, Misconception;
        public TLSPhase7Question(string id,string topic,string prompt,string a,string b,string c,string correct,string explanation,string misconception)
        {Id=id;TopicId=topic;Prompt=prompt;OptionA=a;OptionB=b;OptionC=c;Correct=correct;Explanation=explanation;Misconception=misconception;}
    }
    public static class TLSPhase7QuestionBank
    {
        // Assessment questions are fixed finite examples, not secure proctored secrets.
        public static readonly TLSPhase7Question[] Baseline={
            Q("stack","After push(4), push(9), pop(&out), what is out?","4","9","0","B","A stack removes its newest element first.","LIFO ordering"),
            Q("array","Insert value 4 at index 1 in [2,6,8] (capacity 5). Which final sequence?","[2,8,6,0]","[2,6,8,0]","[2,4,6,8] if value 4","C","Shift right from the end before inserting 4.","Array shift direction"),
            Q("queue","Circular buffer of capacity 5: head=4, count=2. Next enqueue writes physical slot?","1","0","4","A","(head + count) % 5 = 1.","Ring modulo"),
            Q("pointer","After free(p), alias q still stores the old address. Can q be dereferenced?","Yes","Only for struct data","No","C","free invalidates that allocation; aliases do not magically become NULL.","Dangling alias"),
            Q("list","Before free(node) while traversing a list, what must be saved?","node->next","node->data only","the value of head always","A","Preserve a valid successor before freeing the current node.","List cleanup"),
            Q("recursion","int f(int n){ if(n==0)return 0; return n+f(n-1); } What is f(3)?","3","6","9","B","The recursive calls add 3 + 2 + 1 + 0; the base case returns zero, and the stack unwinds to produce 6.","Recursive returns"),
            Q("tree","For root 8 with left 3 and right 10, preorder is?","3,8,10","3,10,8","8,3,10","C","Preorder visits root before children.","Traversal order"),
            Q("bst","A BST must satisfy which property?","Each left subtree value < parent; right > parent (unique keys)","All levels filled","Root is always smallest","A","BST ordering is recursive over subtrees, not a shape guarantee.","BST invariant"),
            Q("heap","Min-heap root relative to other elements is?","Always largest","A smallest key","Always index n-1","B","Heap order guarantees a minimum at the root, not a sorted array.","Heap order"),
            Q("graph","Which structure drives breadth-first search?","Queue","Recursion only","Min-heap","A","BFS explores by layers using a FIFO frontier.","BFS frontier"),
            Q("hash","With linear probing, what follows deletion in a collision chain?","Set slot EMPTY always","Keep a DELETED/tombstone marker","Clear whole table","B","A tombstone keeps later probes reachable.","Probe-chain deletion"),
            Q("analysis","Array indexing a[i] on a valid fixed-size array has what asymptotic time?","O(n)","O(log n)","O(1)","C","Indexed access is constant-time under the standard RAM model.","Growth vs runtime"),
            Q("testing","Which test specifically probes a queue wraparound bug?","Enqueue once at head=0","Fill, dequeue, then enqueue across end","Only test a NULL pointer","B","Force the logical tail to cross the physical array boundary.","Boundary test design")
        };
        public static readonly TLSPhase7Question[] Transfer={
            Q("stack","Stack holds 5 then 7; pop twice. What is the second popped value?","7","5","0","B","LIFO returns 7 first, then 5.","LIFO ordering"),
            Q("array","To insert at 0 into [3,4,5], which loop direction avoids overwriting values?","Move last element toward index 0","Move index 0 toward last","Never shift","A","Right shifting starts at the last occupied slot.","Array shift direction"),
            Q("queue","Capacity 5, head=3, count=4. Tail index for enqueue?","2","3","4","A","The next physical tail slot is (head + count) % capacity = (3 + 4) % 5 = 2; modulo wraps the index around the ring.","Ring modulo"),
            Q("pointer","If p and q both alias one live allocation, after free(p) q is...","Still owning live data","Dangling","Automatically NULL","B","Free affects the allocation, not the bytes stored in every alias.","Dangling alias"),
            Q("list","Deleting head node safely with head != NULL: which sequence?","free(head); head=head->next;","head=head->next; free(head);","old=head; head=old->next; free(old);","C","Preserve old and successor before free.","List cleanup"),
            Q("recursion","int f(int n){if(n<=1)return 1;return n*f(n-1);} What is f(4)?","24","10","16","A","4*3*2*1 = 24; return frames unwind.","Recursive returns"),
            Q("tree","Root 5 with left 2 and right 8. Inorder traversal?","5,2,8","2,5,8","2,8,5","B","Inorder visits left, root, right.","Traversal order"),
            Q("bst","Deleting a BST node with two children often uses...","its inorder successor/predecessor","arbitrary leaf","new root always","A","Replacement preserves relative key ordering.","BST deletion"),
            Q("heap","For array-backed binary heap, left child index of i is...","i/2","2*i+2","2*i+1","C","Zero-based left child = 2*i + 1.","Heap indexing"),
            Q("graph","When performing DFS, which behavior is typical?","FIFO by layers","Follow depth before backtracking","No visited check needed","B","DFS follows a branch deeply; marking visited prevents cycles.","DFS frontier"),
            Q("hash","Keys 1 and 8 both hash to bucket 1 mod 7. This is...","a collision","a memory leak","an invalid key","A","Distinct keys can share an initial bucket.","Hash collisions"),
            Q("analysis","Unsuccessful linear search of n elements has worst-case growth...","O(1)","O(n)","O(log n)","B","All n cells may need comparison.","Complexity assumptions"),
            Q("testing","To expose array insertion overflow, test...","only count=0","only count=1","count == capacity","C","The full-capacity boundary tests rejection before any memory write.","Boundary test design")
        };
        public static readonly TLSPhase7Question[] Review={
            Q("stack","Top of [1,2,3] after one pop?","1","2","3","B","The most recent element 3 is removed.","LIFO order"),
            Q("array","When removing element at index 1 from [4,5,6], which moves?","6 into index 1","4 into index 1","No shift","A","Move succeeding active elements left.","Array shifts"),
            Q("queue","Queue capacity 5, head 4, count 1: next tail slot?","4","0","1","B","(4+1)%5 wraps to zero.","Modulo head"),
            Q("pointer","Does p=NULL automatically free previously allocated memory?","No","Yes","Only with structs","A","Assigning NULL does not call free.","Memory lifetime"),
            Q("list","Why store node->next before free(node)?","To sort list","To avoid dangling dereference","To allocate a node","B","Once the allocation is freed its fields cannot be accessed.","Freed dereference"),
            Q("recursion","A recursive function without a reachable base case risks...","Constant time","Automatic correction","Unbounded recursion","C","A base case bounds recursion depth.","Recursion base case"),
            Q("tree","Inorder order for leaf-only tree containing 7?","7","No output","0,7","A","All depth-first traversals visit the one node.","Tree traversals"),
            Q("bst","BST search for a key greater than root follows...","Left subtree","Right subtree","Both subtrees always","B","Ordered search narrows to the right for greater keys.","BST ordering"),
            Q("heap","Binary min heap parent of index 5 (0-based) is?","5","1","2","C","Parent index is (i-1)/2.","Heap indexes"),
            Q("graph","Why mark visited in cyclic BFS?","Prevent repeated exploration","Increase O-growth","Eliminate adjacency list","A","Visited marking bounds traversals in cyclic graphs.","Visited set"),
            Q("hash","Linear probing search may stop at a true EMPTY slot because...","Any key may skip slots","No inserted key could probe through an always-empty slot","Deletion always clears chain","B","An never-occupied slot ends the cluster; tombstones do not.","Probe termination"),
            Q("analysis","For n elements, which growth eventually exceeds O(n)?","O(1)","O(log n)","O(n squared)","C","Quadratic growth dominates linear asymptotically.","Complexity growth"),
            Q("testing","A strong unit test should include...","Expected result and edge input","Only happy path","No assertions","A","A test without meaningful assertions cannot detect the targeted defect.","Test oracle")
        };
        public static TLSPhase7Question GetReview(int i)
        {if(i<0||i>=Review.Length)throw new ArgumentOutOfRangeException("i");return Review[i];}
        public static TLSPhase7Question Q(string t,string p,string a,string b,string c,string correct,string why,string misconception)
        {return new TLSPhase7Question("",t,p,a,b,c,correct,why,misconception);}
        public static TLSPhase7Question Get(bool final,int i)
        {var a=final?Transfer:Baseline;if(i<0||i>=a.Length)throw new ArgumentOutOfRangeException("i");return a[i];}
    }
}
