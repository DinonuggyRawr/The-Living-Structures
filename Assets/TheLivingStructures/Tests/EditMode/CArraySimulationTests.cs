using System;
using NUnit.Framework;
using TLS.Core;

namespace TLS.Tests
{
    public sealed class CArraySimulationTests
    {
        private static CArrayState Apply(CArrayState state, CArrayOperation op, int index = 0, int value = 0)
        { return CArraySimulation.Execute(state, op, index, value).FinalState; }

        [Test] public void StartsEmptyWithSixPhysicalCells()
        {
            var s = new CArrayState();
            Assert.AreEqual(0, s.Count);
            Assert.AreEqual(6, CArrayState.Capacity);
            for (int i = 0; i < 6; i++) { Assert.AreEqual(0, s.RawAt(i)); Assert.IsFalse(s.IsActive(i)); }
        }
        [Test] public void AppendValueBecomesActiveOnlyAfterCountStep()
        {
            var trace = CArraySimulation.Execute(new CArrayState(), CArrayOperation.Append, 0, 12);
            Assert.IsTrue(trace.Succeeded);
            Assert.AreEqual(0, trace.Frames[2].State.Count); // line 3 writes physical cell
            Assert.AreEqual(12, trace.Frames[2].State.RawAt(0));
            Assert.IsFalse(trace.Frames[2].State.IsActive(0));
            Assert.AreEqual(1, trace.Frames[3].State.Count);
            Assert.IsTrue(trace.Frames[3].State.IsActive(0));
            Assert.AreEqual(3, trace.Frames[2].CodeLine);
        }
        [Test] public void AppendFullIsSafeAndDoesNotAlterRawCells()
        {
            var state = new CArrayState();
            for (int i = 0; i < 6; i++) state = Apply(state, CArrayOperation.Append, 0, i + 1);
            var t = CArraySimulation.Execute(state, CArrayOperation.Append, 0, 999);
            Assert.IsFalse(t.Succeeded); Assert.AreEqual(6, t.FinalState.Count);
            for (int i = 0; i < 6; i++) Assert.AreEqual(i + 1, t.FinalState.RawAt(i));
        }
        [Test] public void InsertInMiddleShiftsRightWithoutLosingData()
        {
            var s = new CArrayState();
            foreach (int v in new[] { 10, 20, 30 }) s = Apply(s, CArrayOperation.Append, 0, v);
            var t = CArraySimulation.Execute(s, CArrayOperation.Insert, 1, 15);
            Assert.IsTrue(t.Succeeded); Assert.AreEqual(4, t.FinalState.Count);
            CollectionAssert.AreEqual(new[] { 10, 15, 20, 30 }, new[] {
                t.FinalState.RawAt(0),t.FinalState.RawAt(1),t.FinalState.RawAt(2),t.FinalState.RawAt(3)});
            // One right-shift write for each element formerly at or after index one.
            int count = 0;
            foreach (var frame in t.Frames) if (frame.CodeLine == 4) count++;
            Assert.AreEqual(2, count);
        }
        [Test] public void InsertAtZeroAndAtCountBothWork()
        {
            var s = Apply(new CArrayState(), CArrayOperation.Insert, 0, 42);
            s = Apply(s, CArrayOperation.Insert, 0, -7);
            s = Apply(s, CArrayOperation.Insert, 2, 99);
            Assert.AreEqual(3, s.Count);
            Assert.AreEqual(-7, s.RawAt(0)); Assert.AreEqual(42, s.RawAt(1)); Assert.AreEqual(99, s.RawAt(2));
        }
        [TestCase(-1)] [TestCase(1)] [TestCase(7)]
        public void InsertOutOfRangeOnEmptyStateRejected(int index)
        {
            var t = CArraySimulation.Execute(new CArrayState(), CArrayOperation.Insert, index, 55);
            Assert.IsFalse(t.Succeeded); Assert.AreEqual(0, t.FinalState.Count);
            for (int i = 0; i < 6; i++) Assert.AreEqual(0, t.FinalState.RawAt(i));
        }
        [Test] public void RemoveMiddleReturnsOriginalAndShiftsLeft()
        {
            var s = new CArrayState();
            foreach (int v in new[] { 10, 20, 30 }) s = Apply(s, CArrayOperation.Append, 0, v);
            var t = CArraySimulation.Execute(s, CArrayOperation.Remove, 1);
            Assert.IsTrue(t.Succeeded); Assert.AreEqual(20, t.Output);
            Assert.AreEqual(2, t.FinalState.Count); Assert.AreEqual(10, t.FinalState.RawAt(0));
            Assert.AreEqual(30, t.FinalState.RawAt(1));
            Assert.AreEqual(30, t.FinalState.RawAt(2)); // retained raw value is inactive
            Assert.IsFalse(t.FinalState.IsActive(2));
        }
        [Test] public void RemoveLastLeavesStaleRawAndNoShift()
        {
            var s = Apply(new CArrayState(), CArrayOperation.Append, 0, int.MinValue);
            var t = CArraySimulation.Execute(s, CArrayOperation.Remove, 0);
            Assert.IsTrue(t.Succeeded); Assert.AreEqual(int.MinValue, t.Output);
            Assert.AreEqual(0, t.FinalState.Count); Assert.AreEqual(int.MinValue, t.FinalState.RawAt(0));
            foreach (var f in t.Frames) Assert.AreNotEqual(5, f.CodeLine);
        }
        [TestCase(-1)] [TestCase(0)] [TestCase(6)]
        public void RemoveEmptyOrInvalidIndexIsRejected(int index)
        {
            var t = CArraySimulation.Execute(new CArrayState(), CArrayOperation.Remove, index);
            Assert.IsFalse(t.Succeeded); Assert.IsNull(t.Output); Assert.AreEqual(0, t.FinalState.Count);
        }
        [Test] public void GetIsConstantReadWithoutWriting()
        {
            var s = Apply(new CArrayState(), CArrayOperation.Append, 0, 42);
            var t = CArraySimulation.Execute(s, CArrayOperation.Get, 0);
            Assert.IsTrue(t.Succeeded); Assert.AreEqual(42, t.Output);
            Assert.AreEqual(s.Count, t.FinalState.Count); Assert.AreEqual(42, t.FinalState.RawAt(0));
            foreach (var f in t.Frames) Assert.AreEqual(-1, f.WriteCell);
        }
        [TestCase(-1)] [TestCase(0)] [TestCase(6)]
        public void GetEmptyRejectsInvalidIndex(int index)
        {
            Assert.IsFalse(CArraySimulation.Execute(new CArrayState(), CArrayOperation.Get, index).Succeeded);
        }
        [Test] public void SimulationNeverMutatesInputState()
        {
            var source = Apply(new CArrayState(), CArrayOperation.Append, 0, 7);
            CArraySimulation.Execute(source, CArrayOperation.Insert, 0, 88);
            Assert.AreEqual(1, source.Count); Assert.AreEqual(7, source.RawAt(0));
        }
        [Test] public void FrameSnapshotsAreDistinctFromLaterWrites()
        {
            var t = CArraySimulation.Execute(new CArrayState(), CArrayOperation.Append, 0, 123);
            Assert.AreEqual(0, t.Frames[0].State.RawAt(0));
            Assert.AreEqual(123, t.FinalState.RawAt(0));
        }
        [Test] public void CodeArraysAreDefensiveCopies()
        {
            string[] lines = CArraySimulation.CodeFor(CArrayOperation.Insert);
            string original = lines[0]; lines[0] = "unexpected";
            Assert.AreEqual(original, CArraySimulation.CodeFor(CArrayOperation.Insert)[0]);
        }
        [Test] public void NullModelIsRejected()
        { Assert.Throws<ArgumentNullException>(() => CArraySimulation.Execute(null, CArrayOperation.Get)); }
    }
}
