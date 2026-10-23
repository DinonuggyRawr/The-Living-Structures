using System;
using System.Threading;
using NUnit.Framework;
using TLS.Core;
namespace TLS.Tests
{
    public sealed class TLSPhase5WasmTests
    {
        [Test] public void SevenFunctionDefinitionsAreAvailable(){Assert.AreEqual(7,TLSPhase5FunctionCatalog.Count);}
        [Test] public void ReferenceBodiesExistForAllFunctions()
        {for(int i=0;i<7;i++){var f=(TLSPhase5Function)i;Assert.IsTrue(TLSPhase5FunctionCatalog.Reference(f).Contains("{"));Assert.IsTrue(TLSPhase5FunctionCatalog.Starter(f).Contains("TODO"));}}
        [Test] public void AllReferencesFitInputLimit()
        {for(int i=0;i<7;i++)Assert.Less(TLSPhase5FunctionCatalog.Reference((TLSPhase5Function)i).Length,TLSPhase5WasmRunner.SourceLimit);}
        [Test] public void OutputHasAllThreeTeachingStructs()
        {string s=TLSPhase5WasmRunner.BuildSubmission(TLSPhase5FunctionCatalog.Reference(TLSPhase5Function.HeapInsertMin));StringAssert.Contains("TeachingHeap",s);StringAssert.Contains("TeachingGraph",s);StringAssert.Contains("TeachingHash",s);}
        [Test] public void BadInputIsRejected(){Assert.Throws<ArgumentException>(()=>TLSPhase5WasmRunner.BuildSubmission(" "));Assert.Throws<ArgumentException>(()=>TLSPhase5WasmRunner.BuildSubmission(new string('x',7501)));}
        [Test] public void CompilerArgumentsConstrainOutputToWasm()
        {var a=TLSPhase5WasmRunner.BuildCompileArguments("C:/tmp/a.c","C:/tmp/b.c","C:/tmp/t.wasm",TLSPhase5Function.HashRemove);StringAssert.Contains("-target wasm32-wasi",a);StringAssert.Contains("-DTLS_SELECTED=6",a);StringAssert.Contains("--max-memory=16777216",a);}
        [Test] public void RunnerUsesFuelMemoryAndTimeoutLimits()
        {string a=TLSPhase5WasmRunner.BuildRunArguments("test.wasm",3);StringAssert.Contains("fuel=",a);StringAssert.Contains("max-memory-size=",a);StringAssert.Contains("timeout=2s",a);}
        [Test] public void RunnerDoesNotGrantDirectoriesOrNetwork()
        {string a=TLSPhase5WasmRunner.BuildRunArguments("test.wasm",1);Assert.IsFalse(a.Contains("--dir"));Assert.IsFalse(a.Contains("--env"));Assert.IsFalse(a.Contains("socket"));}
        [Test] public void EmbeddedHarnessContainsSevenSelectedBranches()
        {string s=TLSPhase5WasmHarness.Source;for(int i=0;i<7;i++)StringAssert.Contains("TLS_SELECTED == "+i,s);}
        [Test] public void HarnessChecksRawHashAndGraphState()
        {string s=TLSPhase5WasmHarness.Source;StringAssert.Contains("same_hash",s);StringAssert.Contains("memcmp(&g,&original",s);}
        [Test] public void NewSessionHasNoCredit()
        {var s=new TLSPhase5FunctionLabSession();Assert.AreEqual(0,s.PassedFunctions);Assert.AreEqual(0,s.IndependentFunctions);}
        [Test] public void RevealedReferenceCannotEarnIndependentCredit()
        {var s=new TLSPhase5FunctionLabSession();s.Reveal();s.SetGuided(false);s.Record(s.Selected,true,true,true);Assert.AreEqual(0,s.IndependentFunctions);Assert.AreEqual(1,s.PassedFunctions);}
        [Test] public void CompilerFailureCountsAsAttemptWhenEvaluated()
        {var s=new TLSPhase5FunctionLabSession();s.SetGuided(false);s.Record(s.Selected,true,false,true);s.Record(s.Selected,true,true,true);Assert.AreEqual(0,s.IndependentFunctions);}
        [Test] public void ReferencePassIsNotPersisted()
        {var s=new TLSPhase5FunctionLabSession();s.Record(s.Selected,true,true,false);Assert.AreEqual(0,new TLSPhase5FunctionLabSession().PassedFunctions);}
        [Test] public void WasmStartsDisabled()
        {Assert.IsFalse(new TLSPhase5WasmLabSession().OptedIn);}
        [Test] public void CannotStartWithoutOptIn()
        {var s=new TLSPhase5WasmLabSession();Assert.IsFalse(s.Start(new TLSPhase5FunctionLabSession(),"C:/NotMyUnityProject"));}
        [Test] public void NativeRunnerHasEightCasesPerFunction(){Assert.AreEqual(8,TLSPhase5WasmRunner.CasesPerFunction);}
        [TestCase(TLSPhase5Function.HeapInsertMin)]
        [TestCase(TLSPhase5Function.HeapExtractMin)]
        [TestCase(TLSPhase5Function.GraphBfs)]
        [TestCase(TLSPhase5Function.GraphDfs)]
        [TestCase(TLSPhase5Function.HashInsert)]
        [TestCase(TLSPhase5Function.HashFind)]
        [TestCase(TLSPhase5Function.HashRemove)]
        public void ReferenceCaseNamesAreDefined(TLSPhase5Function f)
        {for(int i=0;i<8;i++)Assert.IsFalse(string.IsNullOrWhiteSpace(TLSPhase5WasmRunner.CaseName(f,i)));}
    }
}
