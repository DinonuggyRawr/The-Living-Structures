using NUnit.Framework;
using TLS.Core;

namespace TLS.Tests
{
    public class StackSimulationTests
    {
        [Test]
        public void EmptyStackStartsWithZeroActiveCells()
        {
            StackState s = new StackState();
            Assert.AreEqual(0, s.Count);
            for (int i = 0; i < StackState.Capacity; i++)
            {
                Assert.AreEqual(0, s.RawAt(i));
                Assert.IsFalse(s.IsActive(i));
            }
        }

        [Test]
        public void PushHasWriteBeforeLogicalCountIncrement()
        {
            StackTrace t = StackSimulation.Execute(new StackState(), StackOperation.Push, 42);
            Assert.IsTrue(t.Succeeded);
            Assert.AreEqual(0, t.Frames[2].State.Count);
            Assert.AreEqual(42, t.Frames[2].State.RawAt(0));
            Assert.IsFalse(t.Frames[2].State.IsActive(0));
            Assert.AreEqual(1, t.Frames[3].State.Count);
            Assert.AreEqual(42, t.FinalState.RawAt(0));
        }

        [Test]
        public void SourceSnapshotIsNeverMutated()
        {
            StackState original = new StackState();
            StackSimulation.Execute(original, StackOperation.Push, 9);
            Assert.AreEqual(0, original.Count);
            Assert.AreEqual(0, original.RawAt(0));
        }

        [Test]
        public void StackIsLifo()
        {
            StackState s = new StackState();
            s = StackSimulation.Execute(s, StackOperation.Push, 2).FinalState;
            s = StackSimulation.Execute(s, StackOperation.Push, 8).FinalState;
            StackTrace popped = StackSimulation.Execute(s, StackOperation.Pop);
            Assert.AreEqual(8, popped.Output);
            StackTrace top = StackSimulation.Execute(popped.FinalState, StackOperation.Peek);
            Assert.AreEqual(2, top.Output);
        }

        [Test]
        public void EmptyPopRejectsWithoutChangingState()
        {
            StackTrace t = StackSimulation.Execute(new StackState(), StackOperation.Pop);
            Assert.IsFalse(t.Succeeded);
            Assert.IsFalse(t.Output.HasValue);
            Assert.AreEqual(0, t.FinalState.Count);
        }

        [Test]
        public void EmptyPeekRejectsWithoutChangingState()
        {
            StackTrace t = StackSimulation.Execute(new StackState(), StackOperation.Peek);
            Assert.IsFalse(t.Succeeded);
            Assert.AreEqual(0, t.FinalState.Count);
        }

        [Test]
        public void PeekDoesNotChangeCountOrRawCell()
        {
            StackState s = StackSimulation.Execute(new StackState(), StackOperation.Push, -7).FinalState;
            StackTrace t = StackSimulation.Execute(s, StackOperation.Peek);
            Assert.IsTrue(t.Succeeded);
            Assert.AreEqual(-7, t.Output);
            Assert.AreEqual(1, t.FinalState.Count);
            Assert.AreEqual(-7, t.FinalState.RawAt(0));
        }

        [Test]
        public void FullStackRejectsSixthPush()
        {
            StackState s = new StackState();
            for (int i = 0; i < StackState.Capacity; i++)
                s = StackSimulation.Execute(s, StackOperation.Push, i + 1).FinalState;
            StackTrace rejected = StackSimulation.Execute(s, StackOperation.Push, 99);
            Assert.IsFalse(rejected.Succeeded);
            Assert.AreEqual(StackState.Capacity, rejected.FinalState.Count);
            Assert.AreEqual(5, rejected.FinalState.RawAt(4));
        }

        [Test]
        public void PoppedCellRetainsRawValueButIsInactive()
        {
            StackState s = StackSimulation.Execute(new StackState(), StackOperation.Push, 37).FinalState;
            StackState popped = StackSimulation.Execute(s, StackOperation.Pop).FinalState;
            Assert.AreEqual(0, popped.Count);
            Assert.AreEqual(37, popped.RawAt(0));
            Assert.IsFalse(popped.IsActive(0));
        }

        [Test]
        public void RepeatedPushPopNeverProducesNegativeCount()
        {
            StackState s = new StackState();
            for (int i = 0; i < 12; i++)
                s = StackSimulation.Execute(s, StackOperation.Pop).FinalState;
            Assert.AreEqual(0, s.Count);
        }

        [Test]
        public void ReplaySnapshotsRemainIndependent()
        {
            StackTrace t = StackSimulation.Execute(new StackState(), StackOperation.Push, 5);
            StackState final = t.FinalState.Clone();
            Assert.AreEqual(1, final.Count);
            Assert.AreEqual(0, t.Frames[0].State.Count);
            Assert.AreEqual(0, t.Frames[2].State.Count);
            Assert.AreEqual(1, t.Frames[3].State.Count);
        }

        [Test]
        public void NullSourceRejected()
        {
            Assert.Throws<System.ArgumentNullException>(() => StackSimulation.Execute(null, StackOperation.Push, 1));
        }

        [Test]
        public void OutOfBoundsRawReadRejected()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new StackState().RawAt(StackState.Capacity));
        }

        [Test]
        public void PopHighlightMatchesDecrementThenRead()
        {
            StackState s = StackSimulation.Execute(new StackState(), StackOperation.Push, 12).FinalState;
            StackTrace t = StackSimulation.Execute(s, StackOperation.Pop);
            Assert.AreEqual(3, t.Frames[2].HighlightLine);
            Assert.AreEqual(4, t.Frames[3].HighlightLine);
        }

        [Test]
        public void SuccessfulPushHighlightsReturnTrue()
        {
            StackTrace t = StackSimulation.Execute(new StackState(), StackOperation.Push, 1);
            Assert.AreEqual(5, t.Frames[t.Frames.Count - 1].HighlightLine);
        }

        [Test]
        public void CSourceSnippetsMentionActualCAllocationFreeTypes()
        {
            StringAssert.Contains("Stack *stack", StackSimulation.PushCode[0]);
            StringAssert.Contains("stack->data[stack->count]", StackSimulation.PushCode[3]);
            StringAssert.Contains("stack->count--", StackSimulation.PopCode[3]);
        }

        [TestCase(0, 7, 2)]
        [TestCase(1, 2, 1)]
        [TestCase(2, 50, 5)]
        public void ChallengeFixtureOutcomesAreComputed(int index, int expectedTop, int expectedCount)
        {
            StackChallenge c = StackChallenge.Get(index);
            Assert.AreEqual(expectedTop, c.ExpectedTop);
            Assert.AreEqual(expectedCount, c.ExpectedCount);
            Assert.IsTrue(c.Check(expectedTop, expectedCount));
            Assert.IsFalse(c.Check(expectedTop + 1, expectedCount));
        }

        [Test]
        public void ChallengeIndexOutsideRangeIsRejected()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => StackChallenge.Get(3));
        }
    }
}
