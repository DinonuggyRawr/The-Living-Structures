using System;
using NUnit.Framework;
using TLS.Core;

namespace TLS.Tests
{
    public sealed class TLSLinearCodingSessionTests
    {
        [TestCase(LinearPracticeDomain.Array)] [TestCase(LinearPracticeDomain.Queue)]
        public void NewSessionStartsEmptyWithNoMastery(LinearPracticeDomain domain)
        {
            var s = new LinearPracticeSession(domain);
            Assert.AreEqual(0, s.CompletedCount);
            Assert.AreEqual(0, s.IndependentFirstTryCount);
            Assert.AreEqual(LinearPracticeHelp.Guided, s.Help);
            Assert.AreEqual(LinearPracticeTrack.Trace, s.Track);
        }
        [Test] public void IndependentFirstAnswerCorrectEarnsStructuredFirstTry()
        {
            var s = new LinearPracticeSession(LinearPracticeDomain.Array);
            s.SetHelp(LinearPracticeHelp.Independent);
            s.Draft = s.Current.Reference;
            Assert.IsTrue(s.Submit());
            Assert.AreEqual(1, s.CompletedCount); Assert.AreEqual(1, s.IndependentFirstTryCount);
        }
        [Test] public void GuidedCorrectCountsCompletionNotIndependent()
        {
            var s = new LinearPracticeSession(LinearPracticeDomain.Queue);
            s.Draft = s.Current.Reference;
            Assert.IsTrue(s.Submit()); Assert.AreEqual(1, s.CompletedCount);
            Assert.AreEqual(0, s.IndependentFirstTryCount);
        }
        [Test] public void BlankDoesNotConsumeFirstTry()
        {
            var s = new LinearPracticeSession(LinearPracticeDomain.Array);
            s.SetHelp(LinearPracticeHelp.Independent);
            s.Draft = "  "; Assert.IsFalse(s.Submit());
            s.Draft = s.Current.Reference; Assert.IsTrue(s.Submit());
            Assert.AreEqual(1, s.IndependentFirstTryCount);
        }
        [Test] public void WrongAttemptForfeitsFirstTryEvenAfterCorrection()
        {
            var s = new LinearPracticeSession(LinearPracticeDomain.Queue);
            s.SetHelp(LinearPracticeHelp.Independent);
            s.Draft = "123456"; Assert.IsFalse(s.Submit());
            s.Draft = s.Current.Reference; Assert.IsTrue(s.Submit());
            Assert.AreEqual(1, s.CompletedCount); Assert.AreEqual(0, s.IndependentFirstTryCount);
            StringAssert.Contains("CORRECT", s.Feedback);
        }
        [Test] public void HintOnlyInGuidedAndPreventsIndependentFirstTry()
        {
            var s = new LinearPracticeSession(LinearPracticeDomain.Array);
            Assert.IsTrue(s.ShowHint());
            s.SetHelp(LinearPracticeHelp.Independent);
            Assert.IsFalse(s.ShowHint()); Assert.IsFalse(s.HintVisible);
            s.Draft = s.Current.Reference; Assert.IsTrue(s.Submit());
            Assert.AreEqual(0, s.IndependentFirstTryCount);
        }
        [Test] public void RevealPreventsCreditEvenIfLaterSwitchedToIndependent()
        {
            var s = new LinearPracticeSession(LinearPracticeDomain.Queue);
            s.Reveal(); s.SetHelp(LinearPracticeHelp.Independent);
            s.Draft = s.Current.Reference; s.Submit();
            Assert.AreEqual(0, s.IndependentFirstTryCount);
        }
        [Test] public void RepeatingSolvedQuestionDoesNotDuplicateScore()
        {
            var s = new LinearPracticeSession(LinearPracticeDomain.Array);
            s.SetHelp(LinearPracticeHelp.Independent); s.Draft = s.Current.Reference; s.Submit();
            s.SelectIndex(1); s.SelectIndex(0); s.Draft = s.Current.Reference; s.Submit();
            Assert.AreEqual(1, s.CompletedCount); Assert.AreEqual(1, s.IndependentFirstTryCount);
        }
        [Test] public void TracksAccumulateDistinctCompletedItems()
        {
            var s = new LinearPracticeSession(LinearPracticeDomain.Array);
            foreach (LinearPracticeTrack track in Enum.GetValues(typeof(LinearPracticeTrack)))
            {
                s.SelectTrack(track); s.Draft = s.Current.Reference;
                Assert.IsTrue(s.Submit());
            }
            Assert.AreEqual(3, s.CompletedCount);
        }
        [Test] public void IndependentCreditingIsSeparateAcrossDomains()
        {
            var a = new LinearPracticeSession(LinearPracticeDomain.Array);
            var q = new LinearPracticeSession(LinearPracticeDomain.Queue);
            a.SetHelp(LinearPracticeHelp.Independent); a.Draft = a.Current.Reference; a.Submit();
            Assert.AreEqual(1, a.IndependentFirstTryCount); Assert.AreEqual(0, q.IndependentFirstTryCount);
        }
        [Test] public void SwitchingTrackClearsVisibleDraftButNotExposure()
        {
            var s = new LinearPracticeSession(LinearPracticeDomain.Queue);
            s.ShowHint(); s.Draft = "abc";
            s.SelectTrack(LinearPracticeTrack.Complete);
            Assert.AreEqual("", s.Draft); Assert.IsFalse(s.HintVisible);
            s.SelectTrack(LinearPracticeTrack.Trace);
            s.SetHelp(LinearPracticeHelp.Independent);
            s.Draft = s.Current.Reference; s.Submit();
            Assert.AreEqual(0, s.IndependentFirstTryCount);
        }
        [Test] public void FreshSessionDoesNotInheritPreviousSessionAchievements()
        {
            var a = new LinearPracticeSession(LinearPracticeDomain.Array);
            a.Draft = a.Current.Reference; a.Submit();
            var b = new LinearPracticeSession(LinearPracticeDomain.Array);
            Assert.AreEqual(0, b.CompletedCount); Assert.AreEqual(0, b.IndependentFirstTryCount);
        }
        [Test] public void InvalidNavigationThrows()
        {
            var s = new LinearPracticeSession(LinearPracticeDomain.Array);
            Assert.Throws<ArgumentOutOfRangeException>(() => s.SelectIndex(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => s.SelectIndex(3));
            Assert.Throws<ArgumentOutOfRangeException>(() => s.SelectTrack((LinearPracticeTrack)99));
            Assert.Throws<ArgumentOutOfRangeException>(() => s.SetHelp((LinearPracticeHelp)99));
        }
    }
}
