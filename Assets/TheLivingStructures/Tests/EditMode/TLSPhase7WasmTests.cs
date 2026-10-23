using System;
using System.Threading;
using NUnit.Framework;
using TLS.Core;
namespace TLS.Tests
{
    public sealed class TLSPhase7WasmTests
    {
        [Test] public void TwoTransferFunctionsHaveFullC11BodiesAndDifferentObjectives()
        {
            Assert.AreEqual(2,TLSPhase7FunctionCatalog.Count);
            foreach(TLSPhase7Function f in new[]{TLSPhase7Function.ArrayRemove,TLSPhase7Function.RecursiveCount})
            {Assert.Greater(TLSPhase7FunctionCatalog.Reference(f).Length,100);Assert.IsTrue(TLSPhase7FunctionCatalog.Reference(f).Contains("return"));}
        }
        [Test] public void TrustedHarnessContainsBothSelectorsAndAllEightCases()
        {
            string h=TLSPhase7WasmHarness.Source;Assert.IsTrue(h.Contains("#if TLS_SELECTED == 0"));
            Assert.IsTrue(h.Contains("shift_left_remove"));Assert.IsTrue(h.Contains("count_target_recursive"));
            Assert.IsTrue(h.Contains("k>=8"));
        }
        [TestCase(0)][TestCase(1)]
        public void CompilerArgumentsTargetWasiAndSelectedFunction(int selected)
        {
            string args=TLSPhase7WasmRunner.BuildCompileArguments("a.c","trusted.c","out.wasm",(TLSPhase7Function)selected);
            Assert.IsTrue(args.Contains("-target wasm32-wasi"));Assert.IsTrue(args.Contains("-std=c11"));
            Assert.IsTrue(args.Contains("-DTLS_SELECTED="+selected));Assert.IsTrue(args.Contains("-Werror"));
        }
        [TestCase(0)][TestCase(7)]
        public void WasmtimeArgumentsAreBoundedWithoutFolderOrNetworkPermissions(int index)
        {
            string args=TLSPhase7WasmRunner.BuildRunArguments("safe.wasm",index);
            Assert.IsTrue(args.Contains("fuel=20000000"));Assert.IsTrue(args.Contains("max-memory-size=16777216"));
            Assert.IsFalse(args.Contains("--dir"));Assert.IsFalse(args.Contains("--env"));Assert.IsFalse(args.Contains("--tcplisten"));
        }
        [Test] public void BadCaseIndexIsRejected()
        {Assert.Throws<ArgumentOutOfRangeException>(()=>TLSPhase7WasmRunner.CaseName(TLSPhase7Function.ArrayRemove,8));}
        [Test] public void FirstValidC11IndependentPassCountsOnce()
        {
            var session=new TLSPhase7FunctionSession();session.SetGuided(false);
            Assert.IsTrue(session.Record(TLSPhase7Function.ArrayRemove,true,true,true,false));
            Assert.IsFalse(session.Record(TLSPhase7Function.ArrayRemove,true,true,true,false));Assert.AreEqual(1,session.Independent);
        }
        [Test] public void FailedFirstCompilerAttemptForfeitsCleanFirstTry()
        {
            var s=new TLSPhase7FunctionSession();s.SetGuided(false);
            Assert.IsFalse(s.Record(TLSPhase7Function.ArrayRemove,true,false,true,false));
            Assert.IsFalse(s.Record(TLSPhase7Function.ArrayRemove,true,true,true,false));
        }
        [Test] public void ReferenceAndHintsForfeitCleanFirstTryAcrossModes()
        {
            var s=new TLSPhase7FunctionSession();s.Reveal();s.SetGuided(false);
            Assert.IsFalse(s.Record(TLSPhase7Function.ArrayRemove,true,true,true,true));Assert.AreEqual(0,s.Independent);
        }
        [Test] public void SourceSizeAndNulAreRejectedBeforeProcessLaunch()
        {
            Assert.Throws<ArgumentException>(()=>TLSPhase7WasmRunner.BuildSubmission(""));
            Assert.Throws<ArgumentException>(()=>TLSPhase7WasmRunner.BuildSubmission("a\0b"));
            Assert.Throws<ArgumentException>(()=>TLSPhase7WasmRunner.BuildSubmission(new string('x',8000)));
        }
        [Test] public void WasmRunnerDoesNotStartUnlessExplicitlyEnabled()
        {
            var s=new TLSPhase7WasmSession();Assert.IsFalse(s.OptedIn);
            Assert.IsFalse(s.Start(new TLSPhase7FunctionSession(),new TLSPhase7ProgressBook(),"C:/Unity/Project_Data"));
            Assert.IsFalse(s.Busy);
        }
    }
}
