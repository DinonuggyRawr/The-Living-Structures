using System;
using NUnit.Framework;
using TLS.Core;

namespace TLS.Tests
{
    public sealed class StackNativeDockerRunnerTests
    {
        [Test] public void SubmissionHasC11PreambleAndUserFunction()
        {
            string s=StackNativeDockerRunner.BuildSubmission("bool stack_push(Stack *s, int v) {return false;}");
            Assert.That(s,Does.Contain("typedef struct"));Assert.That(s,Does.Contain("#line 1 \"student.c\""));
            Assert.That(s,Does.Contain("bool stack_push"));
        }
        [Test] public void EmptySourceRejected()
        { Assert.Throws<ArgumentException>(()=>StackNativeDockerRunner.BuildSubmission("")); }
        [Test] public void OversizedSourceRejected()
        { Assert.Throws<ArgumentException>(()=>StackNativeDockerRunner.BuildSubmission(new string('X',5001))); }
        [Test] public void NulSourceRejected()
        { Assert.Throws<ArgumentException>(()=>StackNativeDockerRunner.BuildSubmission("a\0b")); }
        [Test] public void DockerCommandDoesNotEmbedStudentSource()
        {
            string args=StackNativeDockerRunner.BuildDockerArguments("tls-native-123456789012",StackFunctionEvaluator.Function.Push);
            Assert.That(args,Does.Contain("--network none"));
            Assert.That(args,Does.Contain("--pull=never"));
            Assert.That(args,Does.Contain("--read-only"));
            Assert.That(args,Does.Contain("--cap-drop ALL"));
            Assert.That(args,Does.Contain("--pids-limit 64"));
            Assert.That(args,Does.Contain("--user 65534:65534"));
            Assert.That(args,Does.Not.Contain("student source"));
        }
        [Test] public void DockerRunnerUsesEightFixedCases()
        {
            Assert.AreEqual(8,StackNativeDockerRunner.CasesPerFunction);
            Assert.That(StackNativeDockerRunner.BuildContainerScript(StackFunctionEvaluator.Function.Pop),Does.Contain("TLS_SELECTED=1"));
            Assert.That(StackNativeDockerRunner.BuildContainerScript(StackFunctionEvaluator.Function.Peek),Does.Contain("TLS_SELECTED=2"));
            Assert.That(StackNativeDockerRunner.BuildContainerScript(StackFunctionEvaluator.Function.Push),Does.Contain("timeout 2s"));
        }
        [Test] public void FullNativeResponseCountsEight()
        {
            string s="TLS_STATUS|COMPILED\n";
            for(int i=0;i<8;i++) s+="TLS_CASE|"+i+"|PASS\n";
            s+="TLS_STATUS|DONE\n";
            var r=StackNativeDockerRunner.ParseOutput(s,0);
            Assert.IsTrue(r.AllPassed);Assert.AreEqual(8,r.Passed);
        }
        [Test] public void IncompleteResponseNeverCountsAsPass()
        {
            var r=StackNativeDockerRunner.ParseOutput("TLS_STATUS|COMPILED\nTLS_CASE|0|PASS\nTLS_STATUS|DONE",0);
            Assert.IsFalse(r.Completed);Assert.IsFalse(r.AllPassed);
        }
        [Test] public void DuplicateCaseDoesNotBoostScore()
        {
            var r=StackNativeDockerRunner.ParseOutput("TLS_STATUS|COMPILED\nTLS_CASE|0|PASS\nTLS_CASE|0|PASS\nTLS_STATUS|DONE",0);
            Assert.AreEqual(1,r.Passed);Assert.IsFalse(r.AllPassed);
        }
        [Test] public void CompilationErrorCannotAppearAsPass()
        {
            var r=StackNativeDockerRunner.ParseOutput("TLS_STATUS|COMPILE_ERROR\nTLS_DIAG|bad C syntax",0);
            Assert.IsTrue(r.CompilerRan);Assert.IsFalse(r.Compiled);Assert.IsFalse(r.AllPassed);
            Assert.That(r.Diagnostic,Does.Contain("bad C"));
        }
        [Test] public void NonzeroDockerExitBlocksPass()
        {
            var r=StackNativeDockerRunner.ParseOutput("TLS_STATUS|COMPILED\nTLS_STATUS|DONE",125);
            Assert.IsFalse(r.CompilerRan);Assert.IsFalse(r.AllPassed);
        }
        [Test] public void FailedCaseNotesDistinguishIncorrectMemory()
        {
            string s="TLS_STATUS|COMPILED\n";
            for(int i=0;i<8;i++) s+="TLS_CASE|"+i+(i==4?"|FAIL|4\n":"|PASS\n");
            s+="TLS_STATUS|DONE\n";
            var r=StackNativeDockerRunner.ParseOutput(s,0);
            Assert.IsTrue(r.Completed);Assert.AreEqual(7,r.Passed);Assert.IsFalse(r.AllPassed);
            Assert.That(StackNativeDockerRunner.FirstFailureDescription(r),Does.Contain("backing array mismatch"));
        }
        [Test] public void HarnessSeparateTranslationUnit()
        {
            Assert.That(StackNativeHarness.Source,Does.Contain("int main(int argc, char **argv)"));
            Assert.That(StackNativeHarness.Source,Does.Contain("#if TLS_SELECTED == 0"));
            Assert.That(StackNativeHarness.Source,Does.Contain("return test_case((int)which);"));
        }
        [Test] public void SameCaseNamesSharedBetweenPopPeek()
        {
            Assert.AreEqual(StackNativeDockerRunner.CaseName(StackFunctionEvaluator.Function.Pop,6),
                StackNativeDockerRunner.CaseName(StackFunctionEvaluator.Function.Peek,6));
        }
    }
}
