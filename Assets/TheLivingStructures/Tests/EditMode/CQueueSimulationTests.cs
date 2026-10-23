using System;
using NUnit.Framework;
using TLS.Core;

namespace TLS.Tests
{
    public sealed class CQueueSimulationTests
    {
        private static CQueueState Apply(CQueueState s, CQueueOperation op, int value = 0)
        { return CQueueSimulation.Execute(s, op, value).FinalState; }
        [Test] public void EmptyQueueHasHeadZeroCountZero()
        {
            var s = new CQueueState();
            Assert.AreEqual(0, s.Head); Assert.AreEqual(0, s.Count); Assert.AreEqual(5, CQueueState.Capacity);
            Assert.AreEqual("(empty)", s.LogicalOrder());
        }
        [Test] public void EnqueueWritesBeforeIncreasingCount()
        {
            var t = CQueueSimulation.Execute(new CQueueState(), CQueueOperation.Enqueue, 42);
            Assert.IsTrue(t.Succeeded); Assert.AreEqual(4, t.Frames[3].CodeLine);
            Assert.AreEqual(42, t.Frames[3].State.RawAt(0));
            Assert.AreEqual(0, t.Frames[3].State.Count);
            Assert.AreEqual(1, t.Frames[4].State.Count);
        }
        [Test] public void DequeueRemovesOldestValueFirst()
        {
            var s = new CQueueState();
            s = Apply(s, CQueueOperation.Enqueue, 10);
            s = Apply(s, CQueueOperation.Enqueue, 20);
            var t = CQueueSimulation.Execute(s, CQueueOperation.Dequeue);
            Assert.AreEqual(10, t.Output); Assert.AreEqual(1, t.FinalState.Count);
            Assert.AreEqual(1, t.FinalState.Head);
            Assert.AreEqual("20", t.FinalState.LogicalOrder());
            Assert.AreEqual(10, t.FinalState.RawAt(0)); // stale raw cell
        }
        [Test] public void FullQueueDoesNotOverwriteOldest()
        {
            var s = new CQueueState();
            for (int i = 0; i < 5; i++) s = Apply(s, CQueueOperation.Enqueue, i + 1);
            var t = CQueueSimulation.Execute(s, CQueueOperation.Enqueue, 999);
            Assert.IsFalse(t.Succeeded); Assert.AreEqual(5, t.FinalState.Count);
            Assert.AreEqual("1 -> 2 -> 3 -> 4 -> 5", t.FinalState.LogicalOrder());
            for (int i = 0; i < 5; i++) Assert.AreEqual(i + 1, t.FinalState.RawAt(i));
        }
        [Test] public void EmptyDequeueDoesNotChangeAnySlot()
        {
            var t = CQueueSimulation.Execute(new CQueueState(), CQueueOperation.Dequeue);
            Assert.IsFalse(t.Succeeded); Assert.IsNull(t.Output); Assert.AreEqual(0, t.FinalState.Head);
            Assert.AreEqual(0, t.FinalState.Count);
            foreach (var f in t.Frames) Assert.AreEqual(-1, f.ReadCell);
        }
        [Test] public void EmptyPeekRejected()
        { Assert.IsFalse(CQueueSimulation.Execute(new CQueueState(), CQueueOperation.Peek).Succeeded); }
        [Test] public void PeekLeavesHeadCountAndAllRawSlotsUnchanged()
        {
            var s = Apply(new CQueueState(), CQueueOperation.Enqueue, int.MaxValue);
            var t = CQueueSimulation.Execute(s, CQueueOperation.Peek);
            Assert.IsTrue(t.Succeeded); Assert.AreEqual(int.MaxValue, t.Output);
            Assert.AreEqual(s.Head, t.FinalState.Head); Assert.AreEqual(s.Count, t.FinalState.Count);
            for (int i = 0; i < 5; i++) Assert.AreEqual(s.RawAt(i), t.FinalState.RawAt(i));
        }
        [Test] public void EnqueueAfterTwoDequeuesWrapsTailToZeroAndOne()
        {
            var s = new CQueueState();
            for (int i = 1; i <= 5; i++) s = Apply(s, CQueueOperation.Enqueue, i * 10);
            s = Apply(s, CQueueOperation.Dequeue);
            s = Apply(s, CQueueOperation.Dequeue);
            Assert.AreEqual(2, s.Head); Assert.AreEqual(3, s.Count);
            s = Apply(s, CQueueOperation.Enqueue, 60);
            Assert.AreEqual(60, s.RawAt(0)); Assert.AreEqual(4, s.Count);
            s = Apply(s, CQueueOperation.Enqueue, 70);
            Assert.AreEqual(70, s.RawAt(1)); Assert.AreEqual(5, s.Count);
            Assert.AreEqual("30 -> 40 -> 50 -> 60 -> 70", s.LogicalOrder());
            Assert.AreEqual(2, s.Head);
        }
        [Test] public void RingSlotRoleIsCalculatedFromHeadAndCount()
        {
            var s = new CQueueState();
            for (int i = 1; i <= 5; i++) s = Apply(s, CQueueOperation.Enqueue, i);
            s = Apply(s, CQueueOperation.Dequeue);
            s = Apply(s, CQueueOperation.Dequeue);
            s = Apply(s, CQueueOperation.Enqueue, 6);
            Assert.AreEqual(0, s.LogicalPositionAt(2));
            Assert.AreEqual(3, s.LogicalPositionAt(0));
            Assert.AreEqual(-1, s.LogicalPositionAt(1));
            Assert.AreEqual(0, s.PhysicalIndex(3));
        }
        [Test] public void EmptyAfterWrapRetainsHeadAndRawValues()
        {
            var s = new CQueueState();
            for (int i = 0; i < 5; i++) s = Apply(s, CQueueOperation.Enqueue, i);
            for (int i = 0; i < 5; i++) s = Apply(s, CQueueOperation.Dequeue);
            Assert.AreEqual(0, s.Count); Assert.AreEqual(0, s.Head);
            s = Apply(s, CQueueOperation.Enqueue, 44);
            s = Apply(s, CQueueOperation.Dequeue);
            Assert.AreEqual(1, s.Head); Assert.AreEqual(0, s.Count);
            Assert.AreEqual(44, s.RawAt(0));
        }
        [Test] public void FIFOOrderContinuesAfterEverySlotIsReused()
        {
            var s = new CQueueState();
            for (int i = 0; i < 20; i++)
            {
                s = Apply(s, CQueueOperation.Enqueue, i);
                var read = CQueueSimulation.Execute(s, CQueueOperation.Dequeue);
                Assert.AreEqual(i, read.Output);
                s = read.FinalState;
            }
            Assert.AreEqual(0, s.Count); Assert.AreEqual(0, s.Head);
        }
        [Test] public void InputStateRemainsUnmodified()
        {
            var s = Apply(new CQueueState(), CQueueOperation.Enqueue, 10);
            CQueueSimulation.Execute(s, CQueueOperation.Dequeue);
            Assert.AreEqual(0, s.Head); Assert.AreEqual(1, s.Count); Assert.AreEqual(10, s.RawAt(0));
        }
        [Test] public void FramesAreHistoricalClones()
        {
            var t = CQueueSimulation.Execute(new CQueueState(), CQueueOperation.Enqueue, 10);
            Assert.AreEqual(0, t.Frames[0].State.RawAt(0)); Assert.AreEqual(10, t.FinalState.RawAt(0));
        }
        [Test] public void CodeForReturnsDefensiveCopy()
        {
            var source = CQueueSimulation.CodeFor(CQueueOperation.Dequeue);
            source[0] = "wrong";
            StringAssert.Contains("queue_dequeue", CQueueSimulation.CodeFor(CQueueOperation.Dequeue)[0]);
        }
        [Test] public void InvalidLogicalIndexThrowsRatherThanReadsInvalidMemory()
        {
            var s = new CQueueState();
            Assert.Throws<ArgumentOutOfRangeException>(() => s.PhysicalIndex(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => s.RawAt(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => s.LogicalPositionAt(5));
        }
        [Test] public void NullModelThrows()
        { Assert.Throws<ArgumentNullException>(() => CQueueSimulation.Execute(null, CQueueOperation.Peek)); }
    }
}
