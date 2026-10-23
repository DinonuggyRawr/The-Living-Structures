using System;
using NUnit.Framework;
using TLS.Core;
namespace TLS.Tests
{
    public sealed class TLSPhase6QualityTests
    {
        [Test] public void QualityCatalogCoversTwelveDistinctTopics()
        {
            Assert.AreEqual(12,TLSQualityCatalog.Questions.Length);
            var set=new System.Collections.Generic.HashSet<string>();
            foreach(var q in TLSQualityCatalog.Questions){Assert.IsTrue(set.Add(q.Topic));Assert.IsTrue(q.Correct=="A"||q.Correct=="B");}
        }
        [TestCase(0)][TestCase(1)][TestCase(2)][TestCase(3)][TestCase(4)][TestCase(5)]
        [TestCase(6)][TestCase(7)][TestCase(8)][TestCase(9)][TestCase(10)][TestCase(11)]
        public void EveryQualityCardHasExplanationAndHint(int i)
        {var q=TLSQualityCatalog.Questions[i];Assert.Greater(q.Explanation.Length,20);Assert.Greater(q.Hint.Length,10);Assert.Greater(q.OptionA.Length,10);Assert.Greater(q.OptionB.Length,10);}
        [Test] public void CorrectFirstIndependentReviewCountsOnce()
        {var s=new TLSQualitySession();s.SetGuided(false);Assert.IsTrue(s.Choose(s.Current.Correct));Assert.AreEqual(1,s.Independent);s.Choose(s.Current.Correct);Assert.AreEqual(1,s.Independent);}
        [Test] public void GuidedReviewCannotBecomeIndependentAfterModeChange()
        {var s=new TLSQualitySession();s.Choose(s.Current.Correct);s.SetGuided(false);s.Choose(s.Current.Correct);Assert.AreEqual(0,s.Independent);}
        [Test] public void RevealedAnswerDoesNotAwardUnseenEvidence()
        {var s=new TLSQualitySession();s.Reveal();s.SetGuided(false);s.Choose(s.Current.Correct);Assert.AreEqual(0,s.Independent);}
        [Test] public void WrongAttemptPreventsIndependentCredit()
        {var s=new TLSQualitySession();s.SetGuided(false);string wrong=s.Current.Correct=="A"?"B":"A";s.Choose(wrong);s.Choose(s.Current.Correct);Assert.AreEqual(0,s.Independent);}
        [Test] public void EachCardCanBeSelectedWithoutChangingOtherProgress()
        {var s=new TLSQualitySession();s.SetGuided(false);s.Choose(s.Current.Correct);s.Select(1);Assert.AreEqual(1,s.Independent);s.Choose(s.Current.Correct);Assert.AreEqual(2,s.Independent);}
        [Test] public void OutOfBoundsQualityCardThrows()
        {Assert.Throws<ArgumentOutOfRangeException>(()=>new TLSQualitySession().Select(999));}
        [Test] public void FreshQualitySessionHasNoMastery()
        {Assert.AreEqual(0,new TLSQualitySession().Independent);}
    }
}
