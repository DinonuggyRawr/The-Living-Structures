using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using TLS.Core;
namespace TLS.Tests
{
    public sealed class TLSPhase4WasmTests
    {
        [Test] public void SixFunctionsEightCasesEach(){Assert.AreEqual(6,TLSPhase4FunctionCatalog.Count);Assert.AreEqual(8,TLSPhase4WasmRunner.CasesPerFunction);}
        [Test] public void ReferenceBodiesSupplied(){for(int i=0;i<6;i++)Assert.IsNotEmpty(TLSPhase4FunctionCatalog.Reference((TLSPhase4Function)i));}
        [Test] public void AllCaseNamesSupplied(){for(int i=0;i<6;i++)for(int j=0;j<8;j++)Assert.IsNotEmpty(TLSPhase4WasmRunner.CaseName((TLSPhase4Function)i,j));}
        [Test] public void StudentPreludeContainsCorrectCStruct(){var s=TLSPhase4WasmRunner.BuildSubmission(TLSPhase4FunctionCatalog.Reference(TLSPhase4Function.Insert));StringAssert.Contains("typedef struct TreeNode",s);StringAssert.Contains("#include <stdlib.h>",s);}
        [Test] public void EmbeddedHarnessAndNativeEntrypointPresent(){StringAssert.Contains("TLS_SELECTED",TLSPhase4WasmHarness.Source);StringAssert.Contains("tls_matches",TLSPhase4WasmHarness.Source);StringAssert.Contains("int main",TLSPhase4WasmHarness.Source);}
        [Test] public void TrapsAreNeverPassingResults(){Assert.IsFalse(new TLSPhase4WasmRunner.Result {CompilerRan=true,Compiled=true,Passed=8}.AllPassed);}
        [Test] public void CompileArgumentsSelectLastFunction(){var s=TLSPhase4WasmRunner.BuildCompileArguments("a.c","b.c","out.wasm",TLSPhase4Function.Inorder);StringAssert.Contains("-DTLS_SELECTED=5",s);StringAssert.Contains("-target wasm32-wasi",s);}
        [Test] public void CompileArgumentsRejectUnknownFunction(){Assert.Throws<ArgumentOutOfRangeException>(()=>TLSPhase4WasmRunner.BuildCompileArguments("a","b","c",(TLSPhase4Function)8));}
        [Test] public void RunArgumentsKeepFuelMemoryAndTimeout(){var s=TLSPhase4WasmRunner.BuildRunArguments("t.wasm",7);StringAssert.Contains("fuel=20000000",s);StringAssert.Contains("max-memory-size=16777216",s);StringAssert.Contains("timeout=2s",s);Assert.That(s,Does.Not.Contain("--dir"));Assert.That(s,Does.Not.Contain("--env"));}
        [Test] public void RunArgumentsRejectOutOfRangeCase(){Assert.Throws<ArgumentOutOfRangeException>(()=>TLSPhase4WasmRunner.BuildRunArguments("t.wasm",8));}
        [TestCase("")] [TestCase("  ")] [TestCase("z\0bad")]
        public void BadSourceRejected(string code){Assert.Throws<ArgumentException>(()=>TLSPhase4WasmRunner.BuildSubmission(code));}
        [Test] public void StudentCodeHasLengthLimit(){Assert.Throws<ArgumentException>(()=>TLSPhase4WasmRunner.BuildSubmission(new string('z',7501)));}
        [Test] public void MissingToolsCannotPass(){var r=TLSPhase4WasmRunner.Run(TLSPhase4Function.Insert,TLSPhase4FunctionCatalog.Reference(TLSPhase4Function.Insert),Path.Combine(Path.GetTempPath(),"tls-missing-phase4-tools-zzzz"),CancellationToken.None);Assert.IsFalse(r.AllPassed);}
        [Test] public void FailedFirstAttemptCannotBecomeIndependent(){var s=new TLSPhase4FunctionLabSession();s.SetGuided(false);s.Record(TLSPhase4Function.Insert,true,false,true);s.Record(TLSPhase4Function.Insert,true,true,true);Assert.AreEqual(0,s.IndependentFunctions);}
        [Test] public void SuccessfulFirstIndependentRecordedOnce(){var s=new TLSPhase4FunctionLabSession();s.SetGuided(false);s.Record(TLSPhase4Function.Insert,true,true,true);s.Record(TLSPhase4Function.Insert,true,true,true);Assert.AreEqual(1,s.IndependentFunctions);}
        [Test] public void RevealedAnswersNeverIndependent(){var s=new TLSPhase4FunctionLabSession();s.Reveal();s.SetGuided(false);s.Record(TLSPhase4Function.Insert,true,true,true);Assert.AreEqual(0,s.IndependentFunctions);}
        [Test] public void HintNeverIndependent(){var s=new TLSPhase4FunctionLabSession();s.Hint();s.SetGuided(false);s.Record(TLSPhase4Function.Insert,true,true,true);Assert.AreEqual(0,s.IndependentFunctions);}
        [Test] public void RunnerNotEnabledByDefault(){var s=new TLSPhase4WasmLabSession();Assert.IsFalse(s.OptedIn);}
        [Test] public void DraftsAreIndependent(){var s=new TLSPhase4FunctionLabSession();s.Draft="student insert";s.Select(TLSPhase4Function.Delete);Assert.AreNotEqual("student insert",s.Draft);s.Select(TLSPhase4Function.Insert);Assert.AreEqual("student insert",s.Draft);}
        [Test] public void ToolRootMatchesExistingToolchain(){string root=TLSPhase4WasmRunner.ToolchainRoot(Path.Combine("C:","Demo","Assets"));Assert.That(root,Does.EndWith("TLS_Toolchain_Windows"));}
        [Test] public void NoExternalProgressWriteAPIsOnSessions(){Assert.IsNull(typeof(TLSPhase4WasmLabSession).GetMethod("Save"));Assert.IsNull(typeof(TLSPhase4FunctionLabSession).GetMethod("Save"));}
    }
}
