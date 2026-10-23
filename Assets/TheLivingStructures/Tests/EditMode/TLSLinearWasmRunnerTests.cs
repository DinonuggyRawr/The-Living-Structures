using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using TLS.Core;

namespace TLS.Tests
{
    public sealed class TLSLinearWasmRunnerTests
    {
        [Test] public void AllSevenOperationsHaveTenCaseNames()
        {
            Assert.AreEqual(10,TLSLinearWasmRunner.CasesPerFunction);
            for (int i=0;i<TLSLinearFunctionCatalog.Count;i++)
                for (int j=0;j<10;j++)
                    Assert.IsNotEmpty(TLSLinearWasmRunner.CaseName((TLSLinearFunction)i,j));
        }
        [Test] public void PreprocessorPreambleDefinesRealCStructures()
        {
            string c=TLSLinearWasmRunner.BuildSubmission("bool array_append(TeachingArray *a,int v){return false;}");
            Assert.That(c,Does.Contain("#define ARRAY_CAPACITY 6"));
            Assert.That(c,Does.Contain("#define QUEUE_CAPACITY 5"));
            Assert.That(c,Does.Contain("#line 1 \"student.c\""));
        }
        [TestCase("")] [TestCase(" \t")] [TestCase("a\0b")]
        public void BadSourceRejected(string code)
        {Assert.Throws<ArgumentException>(()=>TLSLinearWasmRunner.BuildSubmission(code));}
        [Test] public void SourceLengthIsBounded()
        {Assert.Throws<ArgumentException>(()=>TLSLinearWasmRunner.BuildSubmission(new string('x',5001)));}
        [Test] public void CompileTargetsWasiAndCannotSelectOutsideCatalog()
        {
            string cmd=TLSLinearWasmRunner.BuildCompileArguments("a.c","h.c","o.wasm",TLSLinearFunction.QueueDequeue);
            Assert.That(cmd,Does.Contain("cc -target wasm32-wasi"));
            Assert.That(cmd,Does.Contain("-std=c11"));
            Assert.That(cmd,Does.Contain("-DTLS_SELECTED=5"));
            Assert.Throws<ArgumentOutOfRangeException>(()=>TLSLinearWasmRunner.BuildCompileArguments("a","b","c",(TLSLinearFunction)7));
        }
        [Test] public void NoHostFileOrNetworkRightsProvidedToWasm()
        {
            string cmd=TLSLinearWasmRunner.BuildRunArguments("a.wasm",9);
            Assert.That(cmd,Does.Contain("fuel=20000000"));
            Assert.That(cmd,Does.Contain("max-memory-size=16777216"));
            Assert.That(cmd,Does.Contain("timeout=2s"));
            Assert.That(cmd,Does.Not.Contain("--dir"));
            Assert.That(cmd,Does.Not.Contain("--env"));
            Assert.That(cmd,Does.Not.Contain("--network"));
        }
        [Test] public void InvalidCaseRejectedBeforeRun()
        {Assert.Throws<ArgumentOutOfRangeException>(()=>TLSLinearWasmRunner.BuildRunArguments("a.wasm",10));}
        [Test] public void MissingCompilerCannotProducePassingResult()
        {
            var r=TLSLinearWasmRunner.Run(TLSLinearFunction.ArrayInsert,TLSLinearFunctionCatalog.Reference(TLSLinearFunction.ArrayInsert),
                Path.Combine(Path.GetTempPath(),"missing-tls-2-3-compiler"),CancellationToken.None);
            Assert.IsFalse(r.CompilerRan);
            Assert.IsFalse(r.AllPassed);
        }
        [Test] public void IncompleteCaseRunCannotBePassing()
        {Assert.IsFalse(new TLSLinearWasmRunner.Result{CompilerRan=true,Compiled=true,Passed=10}.AllPassed);}
        [Test] public void HarnessChecksEveryRawCellAndOutput()
        {
            string c=TLSLinearWasmHarness.Source;
            Assert.That(c,Does.Contain("if (actual.data[i] != expected.data[i]) return 4"));
            Assert.That(c,Does.Contain("if (has_output && out != expected_out) return 5"));
            Assert.That(c,Does.Contain("return run_queue((int)case_id)"));
        }
        [Test] public void ToolchainLocationIsProjectLocal()
        {
            string root=TLSLinearWasmRunner.ToolchainRoot(Path.Combine("C:","Project","Assets"));
            Assert.That(root,Does.EndWith(TLSLinearWasmRunner.Folder));
        }
    }
}
