using System;
using NUnit.Framework;
using TLS.Core;
namespace TLS.Tests
{
    public sealed class TLSLinkedListModelTests
    {
        private static TLSListState Do(TLSListState state,TLSListAction action,int value=0,int index=0)
        {var t=TLSLinkedListEngine.Make(state,action,value,index);Assert.IsTrue(t.Final.Validate());return t.Final;}
        private static void Values(TLSListState s,params int[] expect){CollectionAssert.AreEqual(expect,s.Values());Assert.IsTrue(s.Validate());}
        [TestCase(TLSListKind.Singly)] [TestCase(TLSListKind.Doubly)] [TestCase(TLSListKind.Circular)]
        public void EmptyStateValid(TLSListKind k){var s=new TLSListState(k);Assert.IsTrue(s.Validate());Assert.AreEqual(0,s.Count);}
        [TestCase(TLSListKind.Singly)] [TestCase(TLSListKind.Doubly)] [TestCase(TLSListKind.Circular)]
        public void PrependAndAppendPreserveOrder(TLSListKind k){var s=new TLSListState(k);s=Do(s,TLSListAction.Prepend,20);s=Do(s,TLSListAction.Prepend,10);s=Do(s,TLSListAction.Append,30);Values(s,10,20,30);}
        [TestCase(TLSListKind.Singly)] [TestCase(TLSListKind.Doubly)] [TestCase(TLSListKind.Circular)]
        public void InsertIntoMiddlePreservesAllValues(TLSListKind k){var s=new TLSListState(k);s=Do(s,TLSListAction.Append,3);s=Do(s,TLSListAction.Append,7);s=Do(s,TLSListAction.InsertAt,5,1);Values(s,3,5,7);}
        [TestCase(TLSListKind.Singly)] [TestCase(TLSListKind.Doubly)] [TestCase(TLSListKind.Circular)]
        public void RemoveHeadMiddleTail(TLSListKind k){var s=new TLSListState(k);for(int i=1;i<=4;i++)s=Do(s,TLSListAction.Append,i);s=Do(s,TLSListAction.RemoveAt,0,0);Values(s,2,3,4);s=Do(s,TLSListAction.RemoveAt,0,1);Values(s,2,4);s=Do(s,TLSListAction.RemoveAt,0,1);Values(s,2);s=Do(s,TLSListAction.RemoveAt,0,0);Values(s);}
        [Test] public void CircularTailReferencesHead(){var s=new TLSListState(TLSListKind.Circular);s=Do(s,TLSListAction.Append,1);s=Do(s,TLSListAction.Append,2);Assert.AreEqual(s.Head,s.Find(s.Tail).Next);}
        [Test] public void DoublyBacklinksAreConsistent(){var s=new TLSListState(TLSListKind.Doubly);s=Do(s,TLSListAction.Append,1);s=Do(s,TLSListAction.Append,2);s=Do(s,TLSListAction.Append,3);int[] ids=s.Walk();Assert.AreEqual(ids[1],s.Find(ids[2]).Prev);Assert.AreEqual(ids[0],s.Find(ids[1]).Prev);}
        [Test] public void DeleteFreesAllocationOnlyAfterRelinking(){var s=new TLSListState(TLSListKind.Singly);s=Do(s,TLSListAction.Append,11);s=Do(s,TLSListAction.Append,22);int oldHead=s.Head;s=Do(s,TLSListAction.RemoveAt,0,0);Assert.IsTrue(s.Find(oldHead).Freed);Assert.AreEqual(22,s.Find(s.Head).Value);}
        [Test] public void CloneNeverSharesMutableNodes(){var s=Do(new TLSListState(TLSListKind.Singly),TLSListAction.Prepend,5);var c=s.Clone();c.Find(c.Head).Value=999;Assert.AreEqual(5,s.Find(s.Head).Value);}
        [Test] public void InvalidIndicesDoNotMutate(){var s=new TLSListState(TLSListKind.Singly);var t=TLSLinkedListEngine.Make(s,TLSListAction.InsertAt,44,3);Assert.IsFalse(t.Succeeded);Assert.AreEqual(0,t.Final.Count);t=TLSLinkedListEngine.Make(s,TLSListAction.RemoveAt,0,0);Assert.IsFalse(t.Succeeded);}
        [Test] public void MaximumEightNodesIsEnforced(){var s=new TLSListState(TLSListKind.Circular);for(int i=0;i<8;i++)s=Do(s,TLSListAction.Append,i);Assert.AreEqual(8,s.Count);var t=TLSLinkedListEngine.Make(s,TLSListAction.Append,100);Assert.IsFalse(t.Succeeded);Assert.AreEqual(8,t.Final.Count);}
        [TestCase(TLSListKind.Singly)] [TestCase(TLSListKind.Doubly)]
        public void ReverseLinksWithoutAllocating(TLSListKind kind){var s=new TLSListState(kind);for(int i=1;i<=4;i++)s=Do(s,TLSListAction.Append,i);int before=s.Heap.Count;s=Do(s,TLSListAction.Reverse);Values(s,4,3,2,1);Assert.AreEqual(before,s.Heap.Count);}
        [Test] public void CircularReverseBlockedWithoutStateChange(){var s=Do(new TLSListState(TLSListKind.Circular),TLSListAction.Append,9);var t=TLSLinkedListEngine.Make(s,TLSListAction.Reverse);Assert.IsFalse(t.Succeeded);Values(t.Final,9);}
        [TestCase(TLSListKind.Singly)] [TestCase(TLSListKind.Doubly)] [TestCase(TLSListKind.Circular)]
        public void ClearReleasesReachableNodes(TLSListKind kind){var s=new TLSListState(kind);for(int i=0;i<3;i++)s=Do(s,TLSListAction.Append,i);s=Do(s,TLSListAction.Clear);Assert.AreEqual(0,s.Count);foreach(var node in s.Heap)Assert.IsTrue(node.Freed);}
        [Test] public void SearchReportsFirstMatch(){var s=new TLSListState(TLSListKind.Singly);s=Do(s,TLSListAction.Append,7);s=Do(s,TLSListAction.Append,9);s=Do(s,TLSListAction.Append,7);var t=TLSLinkedListEngine.Make(s,TLSListAction.Search,7);Assert.AreEqual(0,t.FoundIndex);}
        [Test] public void RecursiveCountExhibitsCallAndReturnFrames(){var s=new TLSListState(TLSListKind.Singly);s=Do(s,TLSListAction.Append,3);s=Do(s,TLSListAction.Append,4);var t=TLSLinkedListEngine.Make(s,TLSListAction.RecursiveCount);StringAssert.Contains("2",t.Diagnostic);Assert.Greater(t.Frames.Count,4);Assert.IsTrue(t.Succeeded);}
        [Test] public void RecursiveFindPropagatesFirstMatch(){var s=new TLSListState(TLSListKind.Doubly);s=Do(s,TLSListAction.Append,12);s=Do(s,TLSListAction.Append,14);var t=TLSLinkedListEngine.Make(s,TLSListAction.RecursiveFind,14);Assert.AreEqual(1,t.FoundIndex);}
        [Test] public void NullTerminatedRecursiveLessonRejectsCircularList(){var s=Do(new TLSListState(TLSListKind.Circular),TLSListAction.Append,3);Assert.IsFalse(TLSLinkedListEngine.Make(s,TLSListAction.RecursiveCount).Succeeded);}
        [Test] public void SymbolicAddressIsNotAHostPointer(){Assert.AreEqual("NULL",TLSListState.Address(-1));Assert.AreEqual("0x1000",TLSListState.Address(1));Assert.AreEqual("0x1020",TLSListState.Address(2));}
        [Test] public void ManyAllocateFreeCyclesStayWithinBoundedHeap()
        {
            var s=new TLSListState(TLSListKind.Singly);
            for(int i=0;i<20;i++)
            {
                s=Do(s,TLSListAction.Prepend,i);
                s=Do(s,TLSListAction.RemoveAt,0,0);
                Assert.LessOrEqual(s.Heap.Count,TLSLinkedListEngine.MaxNodes);
            }
            Assert.AreEqual(0,s.Count);
        }
        [TestCase(TLSListKind.Singly)] [TestCase(TLSListKind.Doubly)] [TestCase(TLSListKind.Circular)]
        public void ClearIntermediateFramesNeverDereferenceFreedHead(TLSListKind kind)
        {
            var s=new TLSListState(kind);
            for(int i=0;i<3;i++)s=Do(s,TLSListAction.Append,i);
            var t=TLSLinkedListEngine.Make(s,TLSListAction.Clear);
            foreach(var f in t.Frames)Assert.IsTrue(f.State.Validate());
        }
        [Test] public void SourceLinesAlwaysHaveValidIndexes(){var s=new TLSListState(TLSListKind.Singly);foreach(TLSListAction a in Enum.GetValues(typeof(TLSListAction))){var t=TLSLinkedListEngine.Make(s,a,2);foreach(var f in t.Frames)Assert.That(f.CodeLine,Is.InRange(-1,t.Code.Length-1),a.ToString());}}
        [Test] public void SessionCommitsOnlyOnFinalStep(){var session=new TLSLinkedLabSession();session.Start(TLSListAction.Append,44);Assert.AreEqual(0,session.Current.Count);session.Next();Assert.AreEqual(0,session.Current.Count);session.Finish();Values(session.Current,44);}
        [Test] public void SessionRewindDoesNotUndoCommittedList(){var session=new TLSLinkedLabSession();session.Start(TLSListAction.Append,12);session.Finish();session.Previous();Values(session.Current,12);session.Finish();Values(session.Current,12);}
        [Test] public void PredictionFeedbackIsAfterTrace(){var session=new TLSLinkedLabSession();session.Start(TLSListAction.Append,3);session.Predict(1);Assert.AreEqual(0,session.FirstIndependentPredictions);session.Finish();Assert.AreEqual(1,session.FirstIndependentPredictions);}
        [Test] public void SwitchingListModeResetsSessionState(){var session=new TLSLinkedLabSession();session.Start(TLSListAction.Append,5);session.Finish();session.SelectKind(TLSListKind.Doubly);Assert.AreEqual(0,session.Current.Count);Assert.IsNull(session.Trace);}
    }
}
