using System;
using NUnit.Framework;
using TLS.Core;
namespace TLS.Tests
{
    public sealed class TLSPhase6DebugTests
    {
        [TestCase(TLSDebugFault.LinearOffByOne)]
        [TestCase(TLSDebugFault.BinarySkipMiddle)]
        [TestCase(TLSDebugFault.ShiftStopsAtOne)]
        [TestCase(TLSDebugFault.QueueMissingModulo)]
        [TestCase(TLSDebugFault.HashStopsAtTombstone)]
        public void EachSeededFaultHasEightDeterministicCasesAndAtLeastOneFailure(TLSDebugFault f)
        {var s=TLSDebugCatalog.Create(f);Assert.AreEqual(8,s.Cases.Count);Assert.Greater(s.Failing,0);}
        [Test] public void OffByOneIsCaughtByLastElement()
        {
            // Case index 4 searches for 5 in [3, 5]. The seeded off-by-one loop
            // skips the last cell, unlike case index 3, which finds 3 at index 0.
            var s=TLSDebugCatalog.Create(TLSDebugFault.LinearOffByOne);
            Assert.IsFalse(s.Cases[4].Pass, "Last-element case must distinguish the faulty loop.");
        }
        [Test] public void ShiftBugIsDetectedByRawArrayNotOnlyCount()
        {var s=TLSDebugCatalog.Create(TLSDebugFault.ShiftStopsAtOne);Assert.IsFalse(s.Cases[1].Pass);}
        [Test] public void QueueWraparoundMustHaveFailingCase()
        {var s=TLSDebugCatalog.Create(TLSDebugFault.QueueMissingModulo);Assert.Greater(s.Failing,0);}
        [Test] public void TombstoneMustNotTerminateProbe()
        {var s=TLSDebugCatalog.Create(TLSDebugFault.HashStopsAtTombstone);Assert.IsFalse(s.Cases[2].Pass);}
        [Test] public void CannotSubmitBeforeRunningTests()
        {var s=new TLSDebugSession();Assert.IsFalse(s.Submit("A"));Assert.AreEqual(0,s.Passed);}
        [Test] public void FirstUnseenIndependentDiagnosisRecordsEvidence()
        {var s=new TLSDebugSession();s.SetGuided(false);s.Run();Assert.IsTrue(s.Submit("A"));Assert.AreEqual(1,s.Passed);}
        [Test] public void GuidedCorrectDiagnosisDoesNotCountAsIndependent()
        {var s=new TLSDebugSession();s.Run();Assert.IsTrue(s.Submit("A"));Assert.AreEqual(0,s.Passed);}
        [Test] public void HintForfeitsUnseenCreditAfterSwitchingToIndependent()
        {var s=new TLSDebugSession();s.Hint();s.SetGuided(false);s.Run();Assert.IsTrue(s.Submit("A"));Assert.AreEqual(0,s.Passed);}
        [Test] public void WrongFirstAttemptForfeitsCredit()
        {var s=new TLSDebugSession();s.SetGuided(false);s.Run();Assert.IsFalse(s.Submit("B"));Assert.IsTrue(s.Submit("A"));Assert.AreEqual(0,s.Passed);}
        [Test] public void RepeatedCorrectAnswerCannotDuplicateCredit()
        {var s=new TLSDebugSession();s.SetGuided(false);s.Run();s.Submit("A");s.Submit("A");Assert.AreEqual(1,s.Passed);}
        [Test] public void SeparateNewSessionDoesNotShareAchievements()
        {var first=new TLSDebugSession();first.SetGuided(false);first.Run();first.Submit("A");Assert.AreEqual(0,new TLSDebugSession().Passed);}
        [Test] public void SelectingInvalidFaultThrows()
        {Assert.Throws<ArgumentOutOfRangeException>(()=>TLSDebugCatalog.Create((TLSDebugFault)99));}
    }
}
