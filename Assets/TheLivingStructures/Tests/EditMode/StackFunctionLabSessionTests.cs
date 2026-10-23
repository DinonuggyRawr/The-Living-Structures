using NUnit.Framework;
using TLS.Core;

namespace TLS.Tests
{
    public sealed class StackFunctionLabSessionTests
    {
        [Test] public void DefaultIsGuidedAndNoProgress()
        {
            var s=new StackFunctionLabSession(); Assert.IsTrue(s.Guided);
            Assert.AreEqual(0,s.PassedCount);Assert.AreEqual(0,s.IndependentFirstAttemptCount);
        }
        [Test] public void UnseenIndependentPassIsCountedOnce()
        {
            var s=new StackFunctionLabSession();s.SetGuided(false);
            s.Draft=StackFunctionEvaluator.Reference(StackFunctionEvaluator.Function.Push);
            Assert.IsTrue(s.Run().Accepted);Assert.AreEqual(1,s.IndependentFirstAttemptCount);
            s.Run();Assert.AreEqual(1,s.IndependentFirstAttemptCount);
        }
        [Test] public void GuidedSuccessDoesNotEarnFirstAttempt()
        {
            var s=new StackFunctionLabSession();s.Draft=StackFunctionEvaluator.Reference(StackFunctionEvaluator.Function.Push);
            Assert.IsTrue(s.Run().Accepted);Assert.AreEqual(0,s.IndependentFirstAttemptCount);
        }
        [Test] public void HintForfeitsFirstAttemptEvenAfterChangingToIndependent()
        {
            var s=new StackFunctionLabSession();s.ShowHint();s.SetGuided(false);
            s.Draft=StackFunctionEvaluator.Reference(StackFunctionEvaluator.Function.Push);
            Assert.IsTrue(s.Run().Accepted);Assert.AreEqual(0,s.IndependentFirstAttemptCount);
        }
        [Test] public void RevealForfeitsFirstAttemptAndLoadsReference()
        {
            var s=new StackFunctionLabSession();s.SetGuided(false);s.Reveal();
            Assert.AreEqual(StackFunctionEvaluator.Reference(StackFunctionEvaluator.Function.Push),s.Draft);
            Assert.IsTrue(s.Run().Accepted);Assert.AreEqual(0,s.IndependentFirstAttemptCount);
        }
        [Test] public void WrongFirstSubmissionForfeitsFirstAttempt()
        {
            var s=new StackFunctionLabSession();s.SetGuided(false);s.Run();
            s.Draft=StackFunctionEvaluator.Reference(StackFunctionEvaluator.Function.Push);
            Assert.IsTrue(s.Run().Accepted);Assert.AreEqual(0,s.IndependentFirstAttemptCount);
        }
        [Test] public void ResetStarterKeepsPreviousAttemptHistory()
        {
            var s=new StackFunctionLabSession();s.SetGuided(false);s.Run();s.ResetDraft();
            s.Draft=StackFunctionEvaluator.Reference(StackFunctionEvaluator.Function.Push);
            Assert.IsTrue(s.Run().Accepted);Assert.AreEqual(0,s.IndependentFirstAttemptCount);
        }
        [Test] public void DraftsAreIndependentAcrossTabs()
        {
            var s=new StackFunctionLabSession();s.Draft="my push draft";
            s.Select(StackFunctionEvaluator.Function.Pop);s.Draft="my pop draft";
            s.Select(StackFunctionEvaluator.Function.Push);Assert.AreEqual("my push draft",s.Draft);
            s.Select(StackFunctionEvaluator.Function.Pop);Assert.AreEqual("my pop draft",s.Draft);
        }
        [Test] public void AllThreeCanPassButStillOnlyModeledSessionCredit()
        {
            var s=new StackFunctionLabSession();s.SetGuided(false);
            foreach(StackFunctionEvaluator.Function f in new[]{StackFunctionEvaluator.Function.Push,StackFunctionEvaluator.Function.Pop,StackFunctionEvaluator.Function.Peek})
            {s.Select(f);s.Draft=StackFunctionEvaluator.Reference(f);Assert.IsTrue(s.Run().Accepted);}
            Assert.AreEqual(3,s.PassedCount);Assert.AreEqual(3,s.IndependentFirstAttemptCount);
        }
    }
}
