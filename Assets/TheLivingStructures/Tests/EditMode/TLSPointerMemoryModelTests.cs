using System;
using NUnit.Framework;
using TLS.Core;

namespace TLS.Tests
{
    public sealed class TLSPointerMemoryModelTests
    {
        private static TLSPointerMemoryFrame Final(TLSPointerLessonId id)
        {
            var lesson=TLSPointerMemoryCatalog.Build(id);
            return lesson.Frames[lesson.Frames.Count-1];
        }

        [Test] public void AllNineScenariosHaveValidTraceLines()
        {
            Assert.AreEqual(9,Enum.GetValues(typeof(TLSPointerLessonId)).Length);
            foreach(TLSPointerLessonId id in Enum.GetValues(typeof(TLSPointerLessonId)))
            {
                var lesson=TLSPointerMemoryCatalog.Build(id);
                Assert.Greater(lesson.Frames.Count,2,id.ToString());
                Assert.AreEqual(-1,lesson.Frames[0].CodeLine,id.ToString());
                Assert.IsTrue(!string.IsNullOrEmpty(lesson.Title));
                Assert.IsTrue(!string.IsNullOrEmpty(lesson.Goal));
                Assert.IsTrue(!string.IsNullOrEmpty(lesson.PredictionQuestion));
                Assert.That(lesson.CorrectChoice,Is.InRange(0,1));
                foreach(var frame in lesson.Frames)
                {
                    Assert.That(frame.CodeLine,Is.InRange(-1,lesson.Code.Length-1),id.ToString());
                    Assert.IsTrue(!string.IsNullOrEmpty(frame.Explanation));
                    Assert.IsNotNull(frame.State);
                }
            }
        }

        [Test] public void NullGuardHasNoHeapAndNullPointer()
        {
            var end=Final(TLSPointerLessonId.NullGuard);
            Assert.AreEqual(0,end.State.Nodes.Count);
            Assert.AreEqual("NULL",end.State.PointerStatus("head"));
        }
        [Test] public void MallocReturnsUninitializedFieldsFirst()
        {
            var lesson=TLSPointerMemoryCatalog.Build(TLSPointerLessonId.Allocation);
            var node=lesson.Frames[2].State.Node("object1");
            Assert.IsNotNull(node);Assert.IsFalse(node.DataInitialized);Assert.IsFalse(node.NextInitialized);
            Assert.AreEqual("0x1000",node.Address);
        }
        [Test] public void ExplicitInitializationUpdatesIndependentFields()
        {
            var lesson=TLSPointerMemoryCatalog.Build(TLSPointerLessonId.Allocation);
            var afterData=lesson.Frames[4].State.Node("object1");
            Assert.IsTrue(afterData.DataInitialized);Assert.AreEqual(42,afterData.Data);
            Assert.IsFalse(afterData.NextInitialized);
            var afterNext=lesson.Frames[5].State.Node("object1");
            Assert.IsTrue(afterNext.NextInitialized);Assert.IsNull(afterNext.NextId);
        }
        [Test] public void HeadAndNodeAliasSingleAllocation()
        {
            var state=Final(TLSPointerLessonId.Allocation).State;
            Assert.AreEqual(state.PointerId("head"),state.PointerId("node"));
            Assert.AreEqual(1,state.Nodes.Count);
        }
        [Test] public void AliasWriteChangesOnlySharedObject()
        {
            var state=Final(TLSPointerLessonId.Aliasing).State;
            Assert.AreEqual(1,state.Nodes.Count);
            Assert.AreEqual(state.PointerId("head"),state.PointerId("alias"));
            Assert.AreEqual(99,state.Node("object1").Data);
        }
        [Test] public void LinkingTwoNodesProducesCorrectHeadAndNext()
        {
            var state=Final(TLSPointerLessonId.LinkNodes).State;
            Assert.AreEqual(2,state.Nodes.Count);
            Assert.AreEqual("object2",state.PointerId("head"));
            Assert.AreEqual(20,state.Node("object2").Data);
            Assert.AreEqual("object1",state.Node("object2").NextId);
            Assert.AreEqual(10,state.Node("object1").Data);
            Assert.IsNull(state.Node("object1").NextId);
        }
        [Test] public void LinkingNodesDoesNotLeakFirstNodeWhenHeadMoves()
        {
            var state=Final(TLSPointerLessonId.LinkNodes).State;
            Assert.IsFalse(state.IsUnreachable("object1"));
            Assert.IsFalse(state.IsUnreachable("object2"));
        }
        [Test] public void DanglingAliasRemainsAfterFree()
        {
            var state=Final(TLSPointerLessonId.Dangling).State;
            Assert.AreEqual("NULL",state.PointerStatus("head"));
            StringAssert.StartsWith("DANGLING",state.PointerStatus("alias"));
            Assert.IsTrue(state.Node("object1").Released);
        }
        [Test] public void SafeCleanupClearsBothAliases()
        {
            var state=Final(TLSPointerLessonId.SafeCleanup).State;
            Assert.AreEqual("NULL",state.PointerStatus("head"));
            Assert.AreEqual("NULL",state.PointerStatus("alias"));
            Assert.IsTrue(state.Node("object1").Released);
        }
        [Test] public void LostPointerIsNotEquivalentToFree()
        {
            var state=Final(TLSPointerLessonId.LostAllocation).State;
            Assert.AreEqual("NULL",state.PointerStatus("head"));
            Assert.IsFalse(state.Node("object1").Released);
            Assert.IsTrue(state.IsUnreachable("object1"));
        }
        [Test] public void FailedMallocDoesNotCreatePhantomNode()
        {
            var state=Final(TLSPointerLessonId.AllocationFailure).State;
            Assert.AreEqual(0,state.Nodes.Count);
            Assert.AreEqual("NULL",state.PointerStatus("node"));
        }
        [Test] public void InvalidNullDereferenceIsBlockedWithoutMutation()
        {
            var end=Final(TLSPointerLessonId.InvalidDereference);
            Assert.IsTrue(end.UnsafeAccessBlocked);
            Assert.AreEqual(TLSPointerEvent.InvalidAccessPrevented,end.Event);
            Assert.AreEqual("NULL",end.State.PointerStatus("head"));
            Assert.AreEqual(0,end.State.Nodes.Count);
        }
        [Test] public void ReleasedAllocationRejectsFieldWrites()
        {
            var state=new TLSPointerMemoryState();string id=state.Allocate();state.Release(id);
            Assert.Throws<InvalidOperationException>(()=>state.SetData(id,12));
            Assert.Throws<InvalidOperationException>(()=>state.SetNext(id,null));
            Assert.Throws<InvalidOperationException>(()=>state.Release(id));
        }
        [Test] public void UndeclaredAndUnknownPointerWritesFailClosed()
        {
            var state=new TLSPointerMemoryState();
            Assert.Throws<InvalidOperationException>(()=>state.Assign("head",null));
            Assert.Throws<InvalidOperationException>(()=>state.Declare("head","unknown"));
        }
        [Test] public void ModelHasBoundedTwoNodeHeap()
        {
            var state=new TLSPointerMemoryState();
            Assert.AreEqual("object1",state.Allocate());Assert.AreEqual("object2",state.Allocate());
            Assert.Throws<InvalidOperationException>(()=>state.Allocate());
        }
        [Test] public void StateExposesCopiesNotMutableInternals()
        {
            var state=Final(TLSPointerLessonId.Aliasing).State;
            var clone=state.Node("object1");clone.Data=12345;
            Assert.AreEqual(99,state.Node("object1").Data);
            state.Pointers["head"]=null;
            Assert.AreEqual("object1",state.PointerId("head"));
        }
        [Test] public void RepeatedCatalogBuildsAreIndependent()
        {
            var a=TLSPointerMemoryCatalog.Build(TLSPointerLessonId.Allocation);
            var b=TLSPointerMemoryCatalog.Build(TLSPointerLessonId.Allocation);
            a.Frames.Clear();
            Assert.Greater(b.Frames.Count,3);
        }
        [Test] public void NoEarlierFrameChangesAfterLaterWrites()
        {
            var frames=TLSPointerMemoryCatalog.Build(TLSPointerLessonId.Aliasing).Frames;
            Assert.AreEqual(10,frames[3].State.Node("object1").Data);
            Assert.AreEqual(99,frames[frames.Count-1].State.Node("object1").Data);
        }
        [Test] public void ScenarioNamesAreDistinct()
        {
            var seen=new System.Collections.Generic.HashSet<string>();
            foreach(TLSPointerLessonId id in Enum.GetValues(typeof(TLSPointerLessonId)))
                Assert.IsTrue(seen.Add(TLSPointerMemoryCatalog.ShortName(id)));
        }
    }
}
