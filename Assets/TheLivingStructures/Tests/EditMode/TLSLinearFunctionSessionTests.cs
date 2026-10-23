using System;
using NUnit.Framework;
using TLS.Core;

namespace TLS.Tests
{
    public sealed class TLSLinearFunctionSessionTests
    {
        [TestCase(true,4)] [TestCase(false,3)]
        public void DomainHasCorrectIndependentFunctionCount(bool array,int total)
        {
            var s=new TLSLinearFunctionLabSession(array);
            Assert.AreEqual(total,s.DomainTotal);
            Assert.AreEqual(0,s.CompiledPassCount);
            Assert.AreEqual(0,s.FirstIndependentPassCount);
        }
        [Test] public void CannotSelectFunctionFromOtherDomain()
        {
            var a=new TLSLinearFunctionLabSession(true);
            Assert.Throws<ArgumentOutOfRangeException>(()=>a.Select(TLSLinearFunction.QueueEnqueue));
        }
        [Test] public void DraftsStaySeparateForEveryFunction()
        {
            var a=new TLSLinearFunctionLabSession(true);
            a.Draft="my array append";
            a.Select(TLSLinearFunction.ArrayInsert);
            a.Draft="my array insert";
            a.Select(TLSLinearFunction.ArrayAppend);
            Assert.AreEqual("my array append",a.Draft);
            a.Select(TLSLinearFunction.ArrayInsert);
            Assert.AreEqual("my array insert",a.Draft);
        }
        [Test] public void FirstUnseenIndependentPassingRunIsCredited()
        {
            var s=new TLSLinearFunctionLabSession(false);
            s.SetHelp(TLSLinearCodingHelp.Independent);
            s.RecordCompilerResult(TLSLinearFunction.QueueEnqueue,true,true,true);
            Assert.AreEqual(1,s.FirstIndependentPassCount);
            Assert.AreEqual(1,s.CompiledPassCount);
        }
        [Test] public void GuidedPassingRunNeverAwardsIndependentFirstTry()
        {
            var s=new TLSLinearFunctionLabSession(true);
            s.RecordCompilerResult(TLSLinearFunction.ArrayAppend,true,true,false);
            s.SetHelp(TLSLinearCodingHelp.Independent);
            s.RecordCompilerResult(TLSLinearFunction.ArrayAppend,true,true,true);
            Assert.AreEqual(0,s.FirstIndependentPassCount);
            Assert.AreEqual(1,s.CompiledPassCount);
        }
        [Test] public void FirstFailedCompileForfeitsSubsequentIndependentFirstTry()
        {
            var s=new TLSLinearFunctionLabSession(true);
            s.SetHelp(TLSLinearCodingHelp.Independent);
            s.RecordCompilerResult(TLSLinearFunction.ArrayInsert,true,false,true);
            s.RecordCompilerResult(TLSLinearFunction.ArrayInsert,true,true,true);
            Assert.AreEqual(0,s.FirstIndependentPassCount);
        }
        [Test] public void MissingToolchainDoesNotConsumeFirstTry()
        {
            var s=new TLSLinearFunctionLabSession(true);
            s.SetHelp(TLSLinearCodingHelp.Independent);
            s.RecordCompilerResult(TLSLinearFunction.ArrayRemove,false,false,true);
            Assert.IsFalse(s.WasAttempted(TLSLinearFunction.ArrayRemove));
            s.RecordCompilerResult(TLSLinearFunction.ArrayRemove,true,true,true);
            Assert.AreEqual(1,s.FirstIndependentPassCount);
        }
        [Test] public void HintExposureSurvivesSwitchToIndependentAndReset()
        {
            var s=new TLSLinearFunctionLabSession(false);
            s.ShowHint();
            s.SetHelp(TLSLinearCodingHelp.Independent);
            s.RestoreStarter();
            s.RecordCompilerResult(TLSLinearFunction.QueueEnqueue,true,true,true);
            Assert.AreEqual(0,s.FirstIndependentPassCount);
            Assert.IsTrue(s.HasExposure(TLSLinearFunction.QueueEnqueue));
        }
        [Test] public void ReferenceCannotEarnUnseenIndependentCredit()
        {
            var s=new TLSLinearFunctionLabSession(true);
            s.RevealReference();
            s.SetHelp(TLSLinearCodingHelp.Independent);
            s.RecordCompilerResult(TLSLinearFunction.ArrayAppend,true,true,true);
            Assert.AreEqual(0,s.FirstIndependentPassCount);
            Assert.IsTrue(s.HasExposure(TLSLinearFunction.ArrayAppend));
        }
        [Test] public void IndependentModeDoesNotShowHint()
        {
            var s=new TLSLinearFunctionLabSession(true);
            s.SetHelp(TLSLinearCodingHelp.Independent);
            s.ShowHint();
            Assert.IsFalse(s.HintVisible);
            Assert.IsFalse(s.HasExposure(TLSLinearFunction.ArrayAppend));
        }
        [Test] public void IndependentFirstTryCreditDoesNotDuplicate()
        {
            var s=new TLSLinearFunctionLabSession(false);
            s.SetHelp(TLSLinearCodingHelp.Independent);
            s.RecordCompilerResult(TLSLinearFunction.QueuePeek,true,true,true);
            s.RecordCompilerResult(TLSLinearFunction.QueuePeek,true,true,true);
            Assert.AreEqual(1,s.FirstIndependentPassCount);
        }
        [Test] public void DifferentTasksAccumulateIndependentResults()
        {
            var s=new TLSLinearFunctionLabSession(false);
            s.SetHelp(TLSLinearCodingHelp.Independent);
            s.RecordCompilerResult(TLSLinearFunction.QueueEnqueue,true,true,true);
            s.Select(TLSLinearFunction.QueueDequeue);
            s.RecordCompilerResult(TLSLinearFunction.QueueDequeue,true,true,true);
            Assert.AreEqual(2,s.FirstIndependentPassCount);
        }
    }
}
