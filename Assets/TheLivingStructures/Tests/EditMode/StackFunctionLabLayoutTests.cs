using NUnit.Framework;
using TLS.Core;

namespace TLS.Tests
{
    public sealed class StackFunctionLabLayoutTests
    {
        [Test] public void ScopeLabelsNameModeledSubsetNotCompiler()
        {
            Assert.IsNotEmpty(StackFunctionEvaluator.Objective(StackFunctionEvaluator.Function.Push));
            Assert.IsNotEmpty(StackFunctionEvaluator.Hint(StackFunctionEvaluator.Function.Peek));
        }
        [TestCase(StackFunctionEvaluator.Function.Push)]
        [TestCase(StackFunctionEvaluator.Function.Pop)]
        [TestCase(StackFunctionEvaluator.Function.Peek)]
        public void TemplatesHaveCompleteCorrectSignatureAndTODO(StackFunctionEvaluator.Function f)
        {
            string s=StackFunctionEvaluator.Starter(f);
            StringAssert.Contains("bool stack_",s);StringAssert.Contains("TODO",s);
            Assert.IsFalse(StackFunctionEvaluator.Check(f,s).Accepted);
        }
        [Test] public void InputGreaterThanCharactersLimitIsSafelyRejected()
        {
            var r=StackFunctionEvaluator.Check(StackFunctionEvaluator.Function.Peek,new string('x',5100));
            Assert.IsFalse(r.Accepted);
        }
    }
}
