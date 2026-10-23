using System;
using NUnit.Framework;
using TLS.Core;
namespace TLS.Tests
{
    public sealed class TLSPhase4PracticeTests
    {
        [Test] public void TwentyFourTasksOrganizedInFourCategories(){Assert.AreEqual(24,TLSPhase4PracticeCatalog.All.Length);foreach(TLSPhase4TaskType k in Enum.GetValues(typeof(TLSPhase4TaskType)))Assert.AreEqual(6,TLSPhase4PracticeCatalog.Count(k));}
        [Test] public void AllTasksHaveExplanations(){foreach(var task in TLSPhase4PracticeCatalog.All){Assert.IsNotEmpty(task.Title);Assert.IsNotEmpty(task.Prompt);Assert.IsNotEmpty(task.Answer);Assert.IsNotEmpty(task.Hint);Assert.IsNotEmpty(task.Explanation);}}
        [Test] public void AllReferenceAnswersAreAccepted(){foreach(var task in TLSPhase4PracticeCatalog.All)Assert.IsTrue(TLSPhase4PracticeCatalog.Matches(task,task.Answer),task.Title);}
        [Test] public void NumericAnswersRejectWrongValue(){var task=TLSPhase4PracticeCatalog.All[0];Assert.IsFalse(TLSPhase4PracticeCatalog.Matches(task,"11"));}
        [Test] public void CompleteCWhitespaceIsLegal(){var task=TLSPhase4PracticeCatalog.All[TLSPhase4PracticeCatalog.Find(TLSPhase4TaskType.Complete,0)];Assert.IsTrue(TLSPhase4PracticeCatalog.Matches(task,"return   0 ;"));}
        [Test] public void OperatorsMustMatch(){var task=TLSPhase4PracticeCatalog.All[TLSPhase4PracticeCatalog.Find(TLSPhase4TaskType.Complete,2)];Assert.IsFalse(TLSPhase4PracticeCatalog.Matches(task,task.Answer.Replace("->",".")));}
        [Test] public void NullAndBlankAnswersRejected(){foreach(var task in TLSPhase4PracticeCatalog.All){Assert.IsFalse(TLSPhase4PracticeCatalog.Matches(task,null));Assert.IsFalse(TLSPhase4PracticeCatalog.Matches(task," "));}}
        [Test] public void FirstIndependentCreditOnlyOnce(){var s=new TLSPhase4PracticeSession();s.SetGuided(false);s.Answer=s.Selected.Answer;Assert.IsTrue(s.Check());Assert.AreEqual(1,s.Independent);Assert.IsTrue(s.Check());Assert.AreEqual(1,s.Independent);}
        [Test] public void AssistedAnswerNoIndependentCredit(){var s=new TLSPhase4PracticeSession();s.ShowHint();s.SetGuided(false);s.Answer=s.Selected.Answer;Assert.IsTrue(s.Check());Assert.AreEqual(0,s.Independent);}
        [Test] public void RevealNoIndependentCredit(){var s=new TLSPhase4PracticeSession();s.Reveal();s.SetGuided(false);s.Answer=s.Selected.Answer;Assert.IsTrue(s.Check());Assert.AreEqual(0,s.Independent);}
        [Test] public void FailedFirstTryDisablesLaterIndependentCredit(){var s=new TLSPhase4PracticeSession();s.SetGuided(false);s.Answer="wrong";Assert.IsFalse(s.Check());s.Answer=s.Selected.Answer;s.Check();Assert.AreEqual(0,s.Independent);}
        [Test] public void GuidedCorrectIsNotIndependent(){var s=new TLSPhase4PracticeSession();s.Answer=s.Selected.Answer;s.Check();Assert.AreEqual(0,s.Independent);Assert.AreEqual(1,s.Solved);}
        [Test] public void SelectionResetsInputButPreservesEvidence(){var s=new TLSPhase4PracticeSession();s.SetGuided(false);s.Answer=s.Selected.Answer;s.Check();s.Select(TLSPhase4TaskType.Trace,1);Assert.AreEqual("",s.Answer);Assert.AreEqual(1,s.Solved);}
        [Test] public void InvalidQuestionIndexIsRejected(){var s=new TLSPhase4PracticeSession();Assert.Throws<ArgumentOutOfRangeException>(()=>s.Select(TLSPhase4TaskType.Repair,99));}
        [Test] public void CorrectRepairRequiresExactSemantics(){var t=TLSPhase4PracticeCatalog.All[TLSPhase4PracticeCatalog.Find(TLSPhase4TaskType.Repair,2)];Assert.IsFalse(TLSPhase4PracticeCatalog.Matches(t,"free(victim); *root = victim->right;"));}
    }
}
