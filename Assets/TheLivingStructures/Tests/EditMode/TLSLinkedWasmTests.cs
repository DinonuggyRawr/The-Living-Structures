using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using TLS.Core;
namespace TLS.Tests
{
    public sealed class TLSLinkedWasmTests
    {
        [Test] public void FiveFunctionsTenCasesEach(){Assert.AreEqual(5,TLSLinkedFunctionCatalog.Count);Assert.AreEqual(10,TLSLinkedWasmRunner.CasesPerFunction);for(int i=0;i<5;i++)for(int j=0;j<10;j++)Assert.IsNotEmpty(TLSLinkedWasmRunner.CaseName((TLSLinkedFunction)i,j));}
        [Test] public void StudentPreludeIsActualC11NodeStructure(){string c=TLSLinkedWasmRunner.BuildSubmission(TLSLinkedFunctionCatalog.Reference(TLSLinkedFunction.Prepend));StringAssert.Contains("typedef struct Node",c);StringAssert.Contains("#include <stdlib.h>",c);}
        [Test] public void HarnessIncludesAllocationAndLinkChecks(){StringAssert.Contains("tls_match",TLSLinkedWasmHarness.Source);StringAssert.Contains("free(head)",TLSLinkedWasmHarness.Source);StringAssert.Contains("TLS_SELECTED",TLSLinkedWasmHarness.Source);}
        [TestCase("")] [TestCase("  ")] [TestCase("x\0y")]
        public void BadSourceRejected(string value){Assert.Throws<ArgumentException>(()=>TLSLinkedWasmRunner.BuildSubmission(value));}
        [Test] public void LengthBounded(){Assert.Throws<ArgumentException>(()=>TLSLinkedWasmRunner.BuildSubmission(new string('x',5001)));}
        [Test] public void CompileArgumentsSelectFunctionSafely(){var a=TLSLinkedWasmRunner.BuildCompileArguments("a.c","b.c","out.wasm",TLSLinkedFunction.Reverse);StringAssert.Contains("-target wasm32-wasi",a);StringAssert.Contains("-DTLS_SELECTED=4",a);Assert.Throws<ArgumentOutOfRangeException>(()=>TLSLinkedWasmRunner.BuildCompileArguments("a","b","c",(TLSLinkedFunction)9));}
        [Test] public void WasmLimitsRemainInPlace(){var a=TLSLinkedWasmRunner.BuildRunArguments("test.wasm",9);StringAssert.Contains("fuel=20000000",a);StringAssert.Contains("max-memory-size=16777216",a);StringAssert.Contains("timeout=2s",a);Assert.That(a,Does.Not.Contain("--dir"));Assert.That(a,Does.Not.Contain("--env"));}
        [Test] public void MissingToolchainCannotPass(){var r=TLSLinkedWasmRunner.Run(TLSLinkedFunction.Prepend,TLSLinkedFunctionCatalog.Reference(TLSLinkedFunction.Prepend),Path.Combine(Path.GetTempPath(),"tls-certainly-missing-tools-3-experimental"),CancellationToken.None);Assert.IsFalse(r.AllPassed);}
        [Test] public void UnfinishedResultNeverPasses(){Assert.IsFalse(new TLSLinkedWasmRunner.Result{CompilerRan=true,Compiled=true,Passed=10}.AllPassed);}
        [Test] public void EvidenceIsOnlyFirstUnseen(){var s=new TLSLinkedFunctionLabSession();s.SetGuided(false);s.Record(TLSLinkedFunction.Prepend,true,true,true);Assert.AreEqual(1,s.IndependentFunctions);s.Record(TLSLinkedFunction.Prepend,true,true,true);Assert.AreEqual(1,s.IndependentFunctions);}
        [Test] public void HintInvalidatesUnseen(){var s=new TLSLinkedFunctionLabSession();s.Hint();s.SetGuided(false);s.Record(TLSLinkedFunction.Prepend,true,true,true);Assert.AreEqual(0,s.IndependentFunctions);}
        [Test] public void ReferenceInvalidatesUnseen(){var s=new TLSLinkedFunctionLabSession();s.Reveal();s.SetGuided(false);s.Record(TLSLinkedFunction.Prepend,true,true,true);Assert.AreEqual(0,s.IndependentFunctions);}
        [Test] public void MissingToolchainDoesNotConsumeAttempt(){var s=new TLSLinkedFunctionLabSession();s.SetGuided(false);s.Record(TLSLinkedFunction.Prepend,false,false,true);s.Record(TLSLinkedFunction.Prepend,true,true,true);Assert.AreEqual(1,s.IndependentFunctions);}
        [Test] public void FailedFirstAttemptPrecludesLaterFirstTry(){var s=new TLSLinkedFunctionLabSession();s.SetGuided(false);s.Record(TLSLinkedFunction.Prepend,true,false,true);s.Record(TLSLinkedFunction.Prepend,true,true,true);Assert.AreEqual(0,s.IndependentFunctions);}
        [Test] public void FunctionDraftsAreIndependent(){var s=new TLSLinkedFunctionLabSession();s.Draft="student prepend";s.Select(TLSLinkedFunction.Append);Assert.AreNotEqual("student prepend",s.Draft);s.Select(TLSLinkedFunction.Prepend);Assert.AreEqual("student prepend",s.Draft);}
        [Test] public void ToolRootMatchesExistingBundle(){string root=TLSLinkedWasmRunner.ToolchainRoot(Path.Combine("C:","Demo","Assets"));Assert.That(root,Does.EndWith("TLS_Toolchain_Windows"));}
    }
}
