using NUnit.Framework;
using TLS.Core;
namespace TLS.Tests
{
    public sealed class TLSLinkedPracticeTests
    {
        [Test] public void HasAllFourExerciseTypes(){Assert.AreEqual(5,TLSLinkedCodingCatalog.Count(TLSLinkedTaskType.Trace));Assert.AreEqual(5,TLSLinkedCodingCatalog.Count(TLSLinkedTaskType.Complete));Assert.AreEqual(5,TLSLinkedCodingCatalog.Count(TLSLinkedTaskType.Repair));Assert.AreEqual(4,TLSLinkedCodingCatalog.Count(TLSLinkedTaskType.Challenge));}
        [Test] public void AllReferenceAnswersAreAccepted(){foreach(var t in TLSLinkedCodingCatalog.All)Assert.IsTrue(TLSLinkedCodingCatalog.Matches(t,t.Answer),t.Title);}
        [Test] public void BadAnswersAreRejected(){foreach(var t in TLSLinkedCodingCatalog.All)Assert.IsFalse(TLSLinkedCodingCatalog.Matches(t,"this is not the answer"),t.Title);}
        [Test] public void MissingPointerArrowIsNotAccepted(){var task=TLSLinkedCodingCatalog.All[6];Assert.IsFalse(TLSLinkedCodingCatalog.Matches(task,"node.next = *head;"));}
        [Test] public void TokenizerAcceptsLegalWhitespace(){var task=TLSLinkedCodingCatalog.All[6];Assert.IsTrue(TLSLinkedCodingCatalog.Matches(task,"node -> next = * head ;"));}
        [Test] public void GuidedSuccessIsNotIndependent(){var s=new TLSLinkedCodingSession();s.Answer=s.Selected.Answer;Assert.IsTrue(s.Check());Assert.AreEqual(0,s.IndependentCount);}
        [Test] public void IndependentFirstTryIsCredited(){var s=new TLSLinkedCodingSession();s.SetGuided(false);s.Answer=s.Selected.Answer;Assert.IsTrue(s.Check());Assert.AreEqual(1,s.IndependentCount);}
        [Test] public void RetryIsNeverFirstUnseen(){var s=new TLSLinkedCodingSession();s.SetGuided(false);s.Answer="wrong";Assert.IsFalse(s.Check());s.Answer=s.Selected.Answer;Assert.IsTrue(s.Check());Assert.AreEqual(0,s.IndependentCount);}
        [Test] public void HintThenIndependentBlocksCredit(){var s=new TLSLinkedCodingSession();s.ShowHint();s.SetGuided(false);s.Answer=s.Selected.Answer;s.Check();Assert.AreEqual(0,s.IndependentCount);}
        [Test] public void RevealBlocksCredit(){var s=new TLSLinkedCodingSession();s.Reveal();s.SetGuided(false);s.Answer=s.Selected.Answer;s.Check();Assert.AreEqual(0,s.IndependentCount);}
        [Test] public void ChallengesDoNotClaimCompilerMastery(){var s=new TLSLinkedCodingSession();s.Select(TLSLinkedTaskType.Challenge,0);s.SetGuided(false);s.Answer=s.Selected.Answer;Assert.IsTrue(s.Check());Assert.AreEqual(1,s.IndependentCount);}
        [Test] public void SelectionRejectsOutOfRange(){var s=new TLSLinkedCodingSession();Assert.Throws<System.ArgumentOutOfRangeException>(()=>s.Select(TLSLinkedTaskType.Trace,99));}
        [Test] public void IndependentTokenCaseSensitive(){var t=TLSLinkedCodingCatalog.All[6];Assert.IsFalse(TLSLinkedCodingCatalog.Matches(t,"Node->next = *head;"));}
    }
}
