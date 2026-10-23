using System;
using NUnit.Framework;
using TLS.Core;

namespace TLS.Tests
{
    public class StackCodingPracticeSessionTests
    {
        private static StackPracticeSession StartIndependent(StackPracticeTrack track, int index)
        {
            StackPracticeSession s = new StackPracticeSession();
            s.SelectTrack(track);
            s.SelectIndex(index);
            s.SetAssistance(StackPracticeAssistance.Independent);
            return s;
        }

        [Test]
        public void NewSessionHasNoStoredOrEarnedResults()
        {
            StackPracticeSession s = new StackPracticeSession();
            Assert.AreEqual(0, s.CompletedCount);
            Assert.AreEqual(0, s.IndependentFirstTryCount);
            Assert.AreEqual(StackPracticeAssistance.Guided, s.Assistance);
        }

        [Test]
        public void GuidedAnswerCompletesExerciseButDoesNotEarnIndependentCredit()
        {
            StackPracticeSession s = new StackPracticeSession();
            s.Draft = s.Current.ReferenceAnswer;
            Assert.IsTrue(s.Submit());
            Assert.AreEqual(1, s.CompletedCount);
            Assert.AreEqual(0, s.IndependentFirstTryCount);
            Assert.IsTrue(s.CurrentCompleted);
            Assert.IsTrue(s.SolutionVisible);
        }

        [Test]
        public void FirstCorrectIndependentAnswerEarnsOnlyStructuredFirstTry()
        {
            StackPracticeSession s = StartIndependent(StackPracticeTrack.Complete, 0);
            s.Draft = s.Current.ReferenceAnswer;
            Assert.IsTrue(s.Submit());
            Assert.AreEqual(1, s.IndependentFirstTryCount);
            Assert.IsTrue(s.CurrentIndependentFirstTry);
            Assert.IsTrue(s.Feedback.Contains("structured exercise"));
        }

        [Test]
        public void IncorrectFirstAttemptForfeitsUnseenFirstTryEvenAfterCorrection()
        {
            StackPracticeSession s = StartIndependent(StackPracticeTrack.Repair, 2);
            s.Draft = "*out_value = stack->data[stack->count];";
            Assert.IsFalse(s.Submit());
            Assert.IsFalse(s.SolutionVisible);
            s.Draft = s.Current.ReferenceAnswer;
            Assert.IsTrue(s.Submit());
            Assert.AreEqual(0, s.IndependentFirstTryCount);
            Assert.AreEqual(1, s.CompletedCount);
        }

        [Test]
        public void BlankSubmissionDoesNotConsumeFirstTry()
        {
            StackPracticeSession s = StartIndependent(StackPracticeTrack.Trace, 0);
            s.Draft = "  ";
            Assert.IsFalse(s.Submit());
            s.Draft = s.Current.ReferenceAnswer;
            Assert.IsTrue(s.Submit());
            Assert.AreEqual(1, s.IndependentFirstTryCount);
        }

        [Test]
        public void IndependentModeDoesNotOfferHints()
        {
            StackPracticeSession s = StartIndependent(StackPracticeTrack.Repair, 0);
            Assert.IsFalse(s.ShowHint());
            Assert.IsFalse(s.HintVisible);
            s.Draft = s.Current.ReferenceAnswer;
            Assert.IsTrue(s.Submit());
            Assert.AreEqual(1, s.IndependentFirstTryCount);
        }

        [Test]
        public void GuidedHintForfeitsIndependentFirstTryAfterSwitchingModes()
        {
            StackPracticeSession s = new StackPracticeSession();
            Assert.IsTrue(s.ShowHint());
            Assert.IsTrue(s.HintVisible);
            s.SetAssistance(StackPracticeAssistance.Independent);
            s.Draft = s.Current.ReferenceAnswer;
            Assert.IsTrue(s.Submit());
            Assert.AreEqual(0, s.IndependentFirstTryCount);
        }

        [Test]
        public void RevealingAnswerForfeitsIndependentFirstTry()
        {
            StackPracticeSession s = StartIndependent(StackPracticeTrack.Trace, 1);
            s.RevealSolution();
            Assert.IsTrue(s.SolutionVisible);
            Assert.IsFalse(s.JustCorrect);
            s.Draft = s.Current.ReferenceAnswer;
            Assert.IsTrue(s.Submit());
            Assert.AreEqual(0, s.IndependentFirstTryCount);
        }

        [Test]
        public void IndependentPassCannotBeEarnedAfterGuidedCompletion()
        {
            StackPracticeSession s = new StackPracticeSession();
            s.Draft = s.Current.ReferenceAnswer;
            Assert.IsTrue(s.Submit());
            s.SetAssistance(StackPracticeAssistance.Independent);
            s.Draft = s.Current.ReferenceAnswer;
            Assert.IsTrue(s.Submit());
            Assert.AreEqual(1, s.CompletedCount);
            Assert.AreEqual(0, s.IndependentFirstTryCount);
        }

        [Test]
        public void RevisitingCompletedItemDoesNotDuplicateResults()
        {
            StackPracticeSession s = StartIndependent(StackPracticeTrack.Trace, 0);
            s.Draft = s.Current.ReferenceAnswer;
            Assert.IsTrue(s.Submit());
            s.SelectIndex(1);
            s.SelectIndex(0);
            s.Draft = s.Current.ReferenceAnswer;
            Assert.IsTrue(s.Submit());
            Assert.AreEqual(1, s.CompletedCount);
            Assert.AreEqual(1, s.IndependentFirstTryCount);
        }

        [Test]
        public void MultipleTracksAccumulateDistinctSessionAchievements()
        {
            StackPracticeSession s = StartIndependent(StackPracticeTrack.Trace, 2);
            s.Draft = s.Current.ReferenceAnswer;
            Assert.IsTrue(s.Submit());
            s.SelectTrack(StackPracticeTrack.Repair);
            s.Draft = s.Current.ReferenceAnswer;
            Assert.IsTrue(s.Submit());
            Assert.AreEqual(2, s.CompletedCount);
            Assert.AreEqual(2, s.IndependentFirstTryCount);
        }

        [Test]
        public void IndependentCreditBelongsOnlyToCurrentUnseenTask()
        {
            StackPracticeSession s = StartIndependent(StackPracticeTrack.Complete, 1);
            s.Draft = s.Current.ReferenceAnswer;
            Assert.IsTrue(s.Submit());
            s.SelectIndex(2);
            s.Draft = "incorrect";
            Assert.IsFalse(s.Submit());
            Assert.AreEqual(1, s.CompletedCount);
            Assert.AreEqual(1, s.IndependentFirstTryCount);
        }

        [Test]
        public void SwitchingTrackClearsDraftAndVisibleHint()
        {
            StackPracticeSession s = new StackPracticeSession();
            s.Draft = "abc";
            s.ShowHint();
            s.SelectTrack(StackPracticeTrack.Repair);
            Assert.AreEqual("", s.Draft);
            Assert.IsFalse(s.HintVisible);
            Assert.AreEqual(StackPracticeTrack.Repair, s.Track);
        }

        [Test]
        public void SwitchingAssistanceClearsVisibleSolutionButNotPastExposure()
        {
            StackPracticeSession s = new StackPracticeSession();
            s.RevealSolution();
            s.SetAssistance(StackPracticeAssistance.Independent);
            Assert.IsFalse(s.SolutionVisible);
            s.Draft = s.Current.ReferenceAnswer;
            s.Submit();
            Assert.AreEqual(0, s.IndependentFirstTryCount);
        }

        [Test]
        public void TwoSessionsDoNotSharePracticeCounters()
        {
            StackPracticeSession first = StartIndependent(StackPracticeTrack.Trace, 0);
            first.Draft = first.Current.ReferenceAnswer;
            first.Submit();
            StackPracticeSession second = new StackPracticeSession();
            Assert.AreEqual(1, first.CompletedCount);
            Assert.AreEqual(0, second.CompletedCount);
        }

        [Test]
        public void InvalidTrackOrIndexOrModeThrows()
        {
            StackPracticeSession s = new StackPracticeSession();
            Assert.Throws<ArgumentOutOfRangeException>(() => s.SelectTrack((StackPracticeTrack)100));
            Assert.Throws<ArgumentOutOfRangeException>(() => s.SelectIndex(3));
            Assert.Throws<ArgumentOutOfRangeException>(() => s.SelectIndex(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => s.SetAssistance((StackPracticeAssistance)4));
        }
    }
}
