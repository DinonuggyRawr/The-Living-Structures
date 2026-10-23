using System;
using System.Collections.Generic;
using NUnit.Framework;
using TLS.Core;
namespace TLS.Tests
{
    public sealed class TLSPhase7LearningTests
    {
        private static readonly DateTime Now=new DateTime(2026,10,21,20,0,0,DateTimeKind.Utc);
        [Test] public void AllThirteenTopicsHaveUniqueIdsAndLearningHubs()
        {
            Assert.AreEqual(13,TLSPhase7LearningCatalog.Topics.Length);
            var seen=new HashSet<string>();
            foreach(var t in TLSPhase7LearningCatalog.Topics)
            {Assert.IsTrue(seen.Add(t.Id));Assert.Greater(t.Hub.Length,6);Assert.Greater(t.Objective.Length,16);Assert.Greater(t.ReviewHint.Length,15);}
        }
        [Test] public void PrerequisitesAreRecommendationsNotAccessLocks()
        {foreach(var t in TLSPhase7LearningCatalog.Topics)Assert.IsNotNull(t.Prerequisite);}
        [Test] public void QuestionBanksCoverAllTopicsWithoutRepeatedPrompts()
        {
            Assert.AreEqual(13,TLSPhase7QuestionBank.Baseline.Length);
            Assert.AreEqual(13,TLSPhase7QuestionBank.Transfer.Length);
            Assert.AreEqual(13,TLSPhase7QuestionBank.Review.Length);
            var prompts=new HashSet<string>();
            for(int i=0;i<13;i++){
                var a=TLSPhase7QuestionBank.Baseline[i];var b=TLSPhase7QuestionBank.Transfer[i];var c=TLSPhase7QuestionBank.Review[i];
                Assert.AreEqual(TLSPhase7LearningCatalog.Topics[i].Id,a.TopicId);
                Assert.AreEqual(a.TopicId,b.TopicId);Assert.AreEqual(a.TopicId,c.TopicId);
                foreach(var q in new[]{a,b,c}){
                    Assert.IsTrue(prompts.Add(q.Prompt));Assert.Contains(q.Correct,new[]{"A","B","C"});
                    Assert.Greater(q.Explanation.Length,20);Assert.Greater(q.Misconception.Length,4);
                }
            }
        }
        [Test] public void WrongDiagnosticResponseCreatesMisconceptionWithoutCompilerMastery()
        {
            var p=new TLSPhase7ProgressBook();var a=new TLSPhase7Assessment(false);
            string bad=a.Current.Correct=="A"?"B":"A";
            Assert.IsTrue(a.Answer(bad,p,Now));Assert.IsFalse(a.Answer(a.Current.Correct,p,Now));
            Assert.AreEqual(1,p.Topic("stack").misconceptions);Assert.AreEqual(0,p.Topic("stack").compilerCases);
            Assert.AreEqual(0,a.CorrectCount);
        }
        [Test] public void FirstCompletedDiagnosticIsStoredSeparatelyFromFinal()
        {
            var p=new TLSPhase7ProgressBook();var a=new TLSPhase7Assessment(false);
            for(int i=0;i<13;i++){Assert.IsTrue(a.Answer(a.Current.Correct,p,Now));Assert.IsTrue(a.Next(p));}
            Assert.IsTrue(a.Complete);Assert.AreEqual(13,p.Data.diagnosticCorrect);
            Assert.AreEqual(-1,p.Data.finalCorrect);
            p.RecordAssessment(false,0,13);Assert.AreEqual(13,p.Data.diagnosticCorrect);
        }
        [Test] public void ReviewAnswerOnlyMovesReviewDateWhenCorrect()
        {
            var p=new TLSPhase7ProgressBook();var r=new TLSPhase7ReviewSession();
            string wrong=r.Current.Correct=="A"?"B":"A";
            Assert.IsTrue(r.Choose(wrong,p,Now));Assert.AreEqual(0,p.Topic("stack").nextReviewUtcTicks);
            r.Next();r.Pick(0);Assert.IsTrue(r.Choose(r.Current.Correct,p,Now));
            Assert.IsTrue(p.ReviewDue("stack",Now.AddDays(8)));
        }
        [Test] public void CorrectIndependentCheckupIsConceptEvidenceNotCCompilerEvidence()
        {
            var p=new TLSPhase7ProgressBook();var a=new TLSPhase7Assessment(true);
            Assert.IsTrue(a.Answer(a.Current.Correct,p,Now));
            Assert.AreEqual(1,p.Topic("stack").independent);Assert.AreEqual(0,p.Topic("stack").compilerCases);
        }
    }
}
