using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using TLS.Core;
namespace TLS.Tests
{
    public sealed class TLSPhase6WasmTests
    {
        [TestCase(TLSPhase6Function.LinearSearch)]
        [TestCase(TLSPhase6Function.BinarySearch)]
        [TestCase(TLSPhase6Function.InsertionSort)]
        public void EachFunctionProvidesValidCompleteReference(TLSPhase6Function f)
        {
            StringAssert.Contains("return",TLSPhase6FunctionCatalog.Reference(f));
            StringAssert.Contains("TODO",TLSPhase6FunctionCatalog.Starter(f));
            Assert.Greater(TLSPhase6FunctionCatalog.Reference(f).Length,TLSPhase6FunctionCatalog.Starter(f).Length);
        }
        [Test] public void ThreeDifferentC11FunctionsHaveEightCasesEach()
        {
            Assert.AreEqual(3,TLSPhase6FunctionCatalog.Count);
            for(int i=0;i<3;i++)for(int j=0;j<8;j++)Assert.IsNotEmpty(TLSPhase6WasmRunner.CaseName((TLSPhase6Function)i,j));
        }
        [Test] public void SourceIsPrependedWithStandardC11Includes()
        {StringAssert.Contains("#include <stdbool.h>",TLSPhase6WasmRunner.BuildSubmission(TLSPhase6FunctionCatalog.Reference(TLSPhase6Function.InsertionSort)));}
        [Test] public void MalformedInputIsRejected()
        {Assert.Throws<ArgumentException>(()=>TLSPhase6WasmRunner.BuildSubmission("\0"));}
        [Test] public void OversizedInputIsRejected()
        {Assert.Throws<ArgumentException>(()=>TLSPhase6WasmRunner.BuildSubmission(new string('a',TLSPhase6WasmRunner.SourceLimit+1)));}
        [Test] public void CompilerUsesOnlyWebAssemblyTargetAndStrictC11()
        {var args=TLSPhase6WasmRunner.BuildCompileArguments("a.c","h.c","o.wasm",TLSPhase6Function.LinearSearch);
            StringAssert.Contains("-target wasm32-wasi",args);StringAssert.Contains("-std=c11",args);StringAssert.Contains("-Werror",args);}
        [Test] public void RuntimeHasFuelStackMemoryAndGuestTimeout()
        {var args=TLSPhase6WasmRunner.BuildRunArguments("o.wasm",2);
            StringAssert.Contains("fuel=",args);StringAssert.Contains("max-memory-size=",args);StringAssert.Contains("timeout=",args);StringAssert.Contains("max-wasm-stack=",args);
            Assert.IsFalse(args.Contains("--dir"));Assert.IsFalse(args.Contains("--env"));}
        [Test] public void EmbeddedHarnessIncludesAllThreeFunctionsAndEightCases()
        {StringAssert.Contains("linear_search(",TLSPhase6WasmHarness.Source);
            StringAssert.Contains("binary_search(",TLSPhase6WasmHarness.Source);
            StringAssert.Contains("insertion_sort(",TLSPhase6WasmHarness.Source);
            StringAssert.Contains("num>=8",TLSPhase6WasmHarness.Source);}
        [Test] public void FirstUnseenSuccessfulRunReceivesSessionOnlyEvidence()
        {var s=new TLSPhase6FunctionSession();s.SetGuided(false);s.Record(s.Selected,true,true,true);Assert.AreEqual(1,s.Passed);Assert.AreEqual(1,s.Independent);}
        [Test] public void RevealedReferenceCanPassButNotClaimUnseen()
        {var s=new TLSPhase6FunctionSession();s.Reveal();s.SetGuided(false);s.Record(s.Selected,true,true,true);Assert.AreEqual(1,s.Passed);Assert.AreEqual(0,s.Independent);}
        [Test] public void CompilerRunNotEvaluatedDoesNotForfeitCredit()
        {var s=new TLSPhase6FunctionSession();s.SetGuided(false);s.Record(s.Selected,false,false,true);s.Record(s.Selected,true,true,true);Assert.AreEqual(1,s.Independent);}
        [Test] public void AFailedFirstEvaluatedSubmissionForfeitsCredit()
        {var s=new TLSPhase6FunctionSession();s.SetGuided(false);s.Record(s.Selected,true,false,true);s.Record(s.Selected,true,true,true);Assert.AreEqual(0,s.Independent);}
        [Test] public void OptInStartsDisabledAndCannotRunWithoutOptIn()
        {var s=new TLSPhase6WasmLabSession();Assert.IsFalse(s.OptedIn);Assert.IsFalse(s.Start(new TLSPhase6FunctionSession(),"some/path"));}
        [Test] public void IsWasmFileRejectsMissingPath()
        {Assert.IsFalse(TLSPhase6WasmRunner.IsWasmFile(Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N")+".wasm")));}
        [Test] public void InvalidFunctionIndexThrows()
        {Assert.Throws<ArgumentOutOfRangeException>(()=>TLSPhase6WasmRunner.CaseName((TLSPhase6Function)10,0));}
    }
}
