using NUnit.Framework;
using TLS.Core;

namespace TLS.Tests
{
    public sealed class TLSPointerMemorySessionTests
    {
        [Test] public void SessionStartsAtFirstFrame()
        {
            var s=new TLSPointerMemorySession();Assert.AreEqual(0,s.Step);
            Assert.IsFalse(s.IsFinished);Assert.IsFalse(s.Prediction.HasValue);
        }
        [Test] public void NextAndPreviousBoundedByTrace()
        {
            var s=new TLSPointerMemorySession();s.Previous();Assert.AreEqual(0,s.Step);
            for(int i=0;i<100;i++)s.Next();Assert.IsTrue(s.IsFinished);
            int atEnd=s.Step;s.Next();Assert.AreEqual(atEnd,s.Step);
            s.Previous();Assert.AreEqual(atEnd-1,s.Step);
        }
        [Test] public void FinishAndResetDoNotMutateTraceData()
        {
            var s=new TLSPointerMemorySession();s.Select(TLSPointerLessonId.Aliasing);
            s.Finish();Assert.AreEqual(99,s.Frame.State.Node("object1").Data);
            s.Reset();Assert.AreEqual(0,s.Step);
            Assert.AreEqual(0,s.Frame.State.Nodes.Count);
        }
        [Test] public void ResetClearsCurrentPredictionButNotSessionStatistics()
        {
            var s=new TLSPointerMemorySession();s.Choose(s.Lesson.CorrectChoice);
            s.Reset();Assert.IsFalse(s.Prediction.HasValue);
            Assert.AreEqual(1,s.Attempts);Assert.AreEqual(1,s.Correct);
        }
        [Test] public void ChoiceAcceptedOnlyBeforeTraceBegins()
        {
            var s=new TLSPointerMemorySession();s.Next();
            Assert.IsFalse(s.Choose(0));Assert.AreEqual(0,s.Attempts);
        }
        [Test] public void ChoiceOnlyAcceptedOncePerRun()
        {
            var s=new TLSPointerMemorySession();Assert.IsTrue(s.Choose(0));
            Assert.IsFalse(s.Choose(1));Assert.AreEqual(1,s.Attempts);
        }
        [Test] public void InvalidChoiceNotRecorded()
        {
            var s=new TLSPointerMemorySession();Assert.IsFalse(s.Choose(-1));
            Assert.IsFalse(s.Choose(2));Assert.AreEqual(0,s.Attempts);
        }
        [Test] public void FeedbackNotRevealedEarly()
        {
            var s=new TLSPointerMemorySession();s.Choose(1-s.Lesson.CorrectChoice);
            StringAssert.DoesNotContain("REVISIT",s.PredictionReview);
            s.Finish();StringAssert.Contains("REVISIT",s.PredictionReview);
        }
        [Test] public void CorrectPredictionIncrementsCount()
        {
            var s=new TLSPointerMemorySession();s.Choose(s.Lesson.CorrectChoice);
            Assert.AreEqual(1,s.Correct);s.Finish();StringAssert.Contains("CORRECT",s.PredictionReview);
        }
        [Test] public void SwitchingScenarioRebuildsTraceAndResetsChoice()
        {
            var s=new TLSPointerMemorySession();s.Choose(0);s.Finish();
            s.Select(TLSPointerLessonId.LinkNodes);
            Assert.AreEqual(0,s.Step);Assert.IsFalse(s.Prediction.HasValue);
            Assert.AreEqual(TLSPointerLessonId.LinkNodes,s.Lesson.Id);
        }
        [Test] public void SessionsRemainIndependent()
        {
            var first=new TLSPointerMemorySession();var second=new TLSPointerMemorySession();
            first.Select(TLSPointerLessonId.Dangling);first.Choose(1);first.Finish();
            Assert.AreEqual(TLSPointerLessonId.NullGuard,second.Lesson.Id);
            Assert.AreEqual(0,second.Step);Assert.AreEqual(0,second.Attempts);
        }
        [Test] public void DoesNotDeriveFromUnityMonoBehaviour()
        {
            Assert.IsFalse(typeof(UnityEngine.MonoBehaviour).IsAssignableFrom(typeof(TLSPointerMemorySession)));
        }
    }
}
