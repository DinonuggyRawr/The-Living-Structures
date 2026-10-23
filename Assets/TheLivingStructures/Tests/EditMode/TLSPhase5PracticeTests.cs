using NUnit.Framework;
using TLS.Core;
namespace TLS.Tests
{
    public sealed class TLSPhase5PracticeTests
    {
        [Test] public void CatalogHas24DistinctExercises(){Assert.AreEqual(24,TLSPhase5PracticeCatalog.All.Length);}
        [TestCase(TLSPhase5Domain.Heap)]
        [TestCase(TLSPhase5Domain.Graph)]
        [TestCase(TLSPhase5Domain.Hash)]
        public void EachDomainHasTwoQuestionsOfEachType(TLSPhase5Domain d)
        {for(int i=0;i<4;i++)Assert.AreEqual(2,TLSPhase5PracticeCatalog.Count(d,(TLSPhase5QuestionType)i));}
        [Test] public void AllReferenceAnswersAccepted(){foreach(var q in TLSPhase5PracticeCatalog.All)Assert.IsTrue(TLSPhase5PracticeCatalog.Matches(q,q.Answer),q.Title);}
        [Test] public void WrongCOperatorsDoNotPass()
        {var q=TLSPhase5PracticeCatalog.All[18];Assert.IsFalse(TLSPhase5PracticeCatalog.Matches(q,"(key % 7 - 7) % 7"));}
        [Test] public void IndependentCleanFirstAttemptCounts()
        {var s=new TLSPhase5PracticeSession();s.SetGuided(false);s.Answer=s.Selected.Answer;Assert.IsTrue(s.Check());Assert.AreEqual(1,s.Independent);}
        [Test] public void GuidedNeverAwardsIndependentCredit()
        {var s=new TLSPhase5PracticeSession();s.Answer=s.Selected.Answer;Assert.IsTrue(s.Check());Assert.AreEqual(0,s.Independent);}
        [Test] public void HintDisqualifiesUnseenFirstAttemptAfterModeSwitch()
        {var s=new TLSPhase5PracticeSession();s.Hint();s.SetGuided(false);s.Answer=s.Selected.Answer;s.Check();Assert.AreEqual(0,s.Independent);}
        [Test] public void ReferenceRevealDisqualifiesIndependentCredit()
        {var s=new TLSPhase5PracticeSession();s.Reveal();s.SetGuided(false);s.Answer=s.Selected.Answer;s.Check();Assert.AreEqual(0,s.Independent);}
        [Test] public void WrongFirstAttemptSpendsFirstCredit()
        {var s=new TLSPhase5PracticeSession();s.SetGuided(false);s.Answer="999";Assert.IsFalse(s.Check());s.Answer=s.Selected.Answer;s.Check();Assert.AreEqual(0,s.Independent);}
        [Test] public void SessionScoresDoNotPersistIntoNewInstances()
        {var s=new TLSPhase5PracticeSession();s.Answer=s.Selected.Answer;s.Check();Assert.AreEqual(0,new TLSPhase5PracticeSession().Solved);}
        [Test] public void SeparateDomainsHaveDistinctScoring()
        {var s=new TLSPhase5PracticeSession();s.Answer=s.Selected.Answer;s.Check();s.Select(TLSPhase5Domain.Graph,TLSPhase5QuestionType.Trace,0);Assert.AreNotEqual("",s.Selected.Answer);Assert.AreEqual(1,s.Solved);}
    }
}
