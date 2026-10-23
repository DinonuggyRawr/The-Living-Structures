using System;
using NUnit.Framework;
using TLS.Core;

namespace TLS.Tests
{
    public class StackCodingPracticeCatalogTests
    {
        [TestCase(StackPracticeTrack.Trace, 0, "TR-01")]
        [TestCase(StackPracticeTrack.Trace, 1, "TR-02")]
        [TestCase(StackPracticeTrack.Trace, 2, "TR-03")]
        [TestCase(StackPracticeTrack.Complete, 0, "CO-01")]
        [TestCase(StackPracticeTrack.Complete, 1, "CO-02")]
        [TestCase(StackPracticeTrack.Complete, 2, "CO-03")]
        [TestCase(StackPracticeTrack.Repair, 0, "RE-01")]
        [TestCase(StackPracticeTrack.Repair, 1, "RE-02")]
        [TestCase(StackPracticeTrack.Repair, 2, "RE-03")]
        public void EveryExerciseHasUniqueIdAndExpectedTrack(StackPracticeTrack track, int index, string id)
        {
            StackPracticeTask t = StackPracticeCatalog.Get(track, index);
            Assert.AreEqual(id, t.Id);
            Assert.AreEqual(track, t.Track);
            Assert.IsNotEmpty(t.Title);
            Assert.IsNotEmpty(t.Question);
            Assert.IsNotEmpty(t.ContextCode);
            Assert.IsNotEmpty(t.Hint);
            Assert.IsNotEmpty(t.Explanation);
            Assert.IsTrue(StackPracticeCatalog.IsReferenceAnswer(t, t.ReferenceAnswer));
        }

        [Test]
        public void HasExactlyNineExercises()
        {
            Assert.AreEqual(3, StackPracticeCatalog.TasksPerTrack);
            Assert.AreEqual(9, StackPracticeCatalog.Total);
        }

        [TestCase(StackPracticeTrack.Trace)]
        [TestCase(StackPracticeTrack.Complete)]
        [TestCase(StackPracticeTrack.Repair)]
        public void RejectsInvalidIndices(StackPracticeTrack track)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => StackPracticeCatalog.Get(track, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => StackPracticeCatalog.Get(track, 3));
        }

        [Test]
        public void TracePopOutIsCalculatedFromExecutionModel()
        {
            StackState state = StackSimulation.Execute(new StackState(), StackOperation.Push, 6).FinalState;
            state = StackSimulation.Execute(state, StackOperation.Push, 9).FinalState;
            StackTrace t = StackSimulation.Execute(state, StackOperation.Pop);
            Assert.AreEqual(t.Output.Value.ToString(), StackPracticeCatalog.Get(StackPracticeTrack.Trace, 0).ReferenceAnswer);
        }

        [Test]
        public void RawSlotAfterPopAgreesWithSimulatedMemory()
        {
            StackState state = StackSimulation.Execute(new StackState(), StackOperation.Push, 4).FinalState;
            state = StackSimulation.Execute(state, StackOperation.Pop).FinalState;
            Assert.AreEqual(0, state.Count);
            Assert.IsFalse(state.IsActive(0));
            Assert.AreEqual(state.RawAt(0).ToString(), StackPracticeCatalog.Get(StackPracticeTrack.Trace, 1).ReferenceAnswer);
        }

        [Test]
        public void CountAfterSequenceAgreesWithSimulatedModel()
        {
            StackState state = new StackState();
            foreach (int v in new[] {7, 8, 9}) state = StackSimulation.Execute(state, StackOperation.Push, v).FinalState;
            state = StackSimulation.Execute(state, StackOperation.Pop).FinalState;
            Assert.AreEqual(state.Count.ToString(), StackPracticeCatalog.Get(StackPracticeTrack.Trace, 2).ReferenceAnswer);
        }

        [TestCase("9", true)]
        [TestCase("  +9  ", true)]
        [TestCase("8", false)]
        [TestCase("9;", false)]
        [TestCase("999999999999999", false)]
        [TestCase("9.0", false)]
        public void IntegerExerciseUsesNumericAnswerNotCode(string text, bool expected)
        {
            Assert.AreEqual(expected, StackPracticeCatalog.IsReferenceAnswer(StackPracticeCatalog.Get(StackPracticeTrack.Trace, 0), text));
        }

        [TestCase("stack->count++;", true)]
        [TestCase(" stack -> count ++ ;  ", true)] // Spaces BETWEEN ->, identifier, ++, and ; are legal C.
        [TestCase("stack - > count++;", false)] // Spaces WITHIN -> split one C operator into two tokens.
        [TestCase("stack->count+ +;", false)] // Spaces WITHIN ++ change the operator.
        [TestCase("stack->count ++ ;", true)]
        [TestCase("stack->count + +;", false)]
        [TestCase("stack->count--;", false)]
        [TestCase("stack->count++; // note", false)]
        [TestCase("stack->count++; return true;", false)]
        [TestCase("stack->Count++;", false)]
        public void StatementMatcherRespectsCTokenBoundaries(string text, bool expected)
        {
            Assert.AreEqual(expected, StackPracticeCatalog.IsReferenceAnswer(StackPracticeCatalog.Get(StackPracticeTrack.Complete, 1), text));
        }

        [Test]
        public void CompletionAssignmentAllowsWhitespaceButNotWrongIndex()
        {
            StackPracticeTask t = StackPracticeCatalog.Get(StackPracticeTrack.Complete, 0);
            Assert.IsTrue(StackPracticeCatalog.IsReferenceAnswer(t, "  stack -> data [ stack -> count ] = value ;".Replace("stack ->", "stack->")));
            Assert.IsFalse(StackPracticeCatalog.IsReferenceAnswer(t, "stack->data[stack->count-1] = value;"));
        }

        [Test]
        public void PopCompletionUsesDereferencedOutputAndDecrementedCount()
        {
            StackPracticeTask t = StackPracticeCatalog.Get(StackPracticeTrack.Complete, 2);
            Assert.IsFalse(StackPracticeCatalog.IsReferenceAnswer(t, "out_value = stack->data[stack->count];"));
            Assert.IsFalse(StackPracticeCatalog.IsReferenceAnswer(t, "*out_value = stack->data[stack->count-1];"));
            Assert.IsTrue(StackPracticeCatalog.IsReferenceAnswer(t, "*out_value=stack->data[stack->count];"));
        }

        [TestCase("if (stack == NULL || stack->count >= CAPACITY)", true)]
        [TestCase("if(stack==NULL||stack->count>=CAPACITY)", true)]
        [TestCase("if(stack==NULL| |stack->count>=CAPACITY)", false)]
        public void LogicalOrInRepairGuardRespectsCTokenBoundaries(string answer, bool expected)
        {
            StackPracticeTask t = StackPracticeCatalog.Get(StackPracticeTrack.Repair, 0);
            Assert.AreEqual(expected, StackPracticeCatalog.IsReferenceAnswer(t, answer));
        }

        [Test]
        public void OverflowRepairRequiresCapacityEqualityToBeRejected()
        {
            StackPracticeTask t = StackPracticeCatalog.Get(StackPracticeTrack.Repair, 0);
            Assert.IsTrue(StackPracticeCatalog.IsReferenceAnswer(t, "if(stack==NULL||stack->count>=CAPACITY)"));
            Assert.IsFalse(StackPracticeCatalog.IsReferenceAnswer(t, "if(stack==NULL||stack->count>CAPACITY)"));
        }

        [Test]
        public void UnderflowRepairRejectsUnsignedLessThanZero()
        {
            StackPracticeTask t = StackPracticeCatalog.Get(StackPracticeTrack.Repair, 1);
            Assert.IsTrue(StackPracticeCatalog.IsReferenceAnswer(t, "if(stack==NULL||out_value==NULL||stack->count==0)"));
            Assert.IsFalse(StackPracticeCatalog.IsReferenceAnswer(t, "if(stack==NULL||out_value==NULL||stack->count<0)"));
        }

        [Test]
        public void PeekRepairCorrectsOnePastTopIndex()
        {
            StackPracticeTask t = StackPracticeCatalog.Get(StackPracticeTrack.Repair, 2);
            Assert.IsTrue(StackPracticeCatalog.IsReferenceAnswer(t, "*out_value=stack->data[stack->count - 1];"));
            Assert.IsFalse(StackPracticeCatalog.IsReferenceAnswer(t, "*out_value=stack->data[stack->count];"));
        }

        [Test]
        public void RejectsNullTaskAndEmptyAnswer()
        {
            StackPracticeTask t = StackPracticeCatalog.Get(StackPracticeTrack.Complete, 0);
            Assert.Throws<ArgumentNullException>(() => StackPracticeCatalog.IsReferenceAnswer(null, "x"));
            Assert.IsFalse(StackPracticeCatalog.IsReferenceAnswer(t, null));
            Assert.IsFalse(StackPracticeCatalog.IsReferenceAnswer(t, "   "));
        }

        [Test]
        public void RejectsNonCCharactersAndOverlongText()
        {
            StackPracticeTask t = StackPracticeCatalog.Get(StackPracticeTrack.Complete, 1);
            Assert.IsFalse(StackPracticeCatalog.IsReferenceAnswer(t, "stack->count++; ⚠"));
            Assert.IsFalse(StackPracticeCatalog.IsReferenceAnswer(t, new string('x', 513)));
        }
    }
}
