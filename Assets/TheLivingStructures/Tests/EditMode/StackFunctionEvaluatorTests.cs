using NUnit.Framework;
using TLS.Core;

namespace TLS.Tests
{
    public sealed class StackFunctionEvaluatorTests
    {
        [TestCase(StackFunctionEvaluator.Function.Push)]
        [TestCase(StackFunctionEvaluator.Function.Pop)]
        [TestCase(StackFunctionEvaluator.Function.Peek)]
        public void ReferenceFunctionPassesEightModeledTests(StackFunctionEvaluator.Function f)
        {
            var r=StackFunctionEvaluator.Check(f,StackFunctionEvaluator.Reference(f));
            Assert.IsTrue(r.Accepted,r.Message); Assert.AreEqual(8,r.Passed); Assert.AreEqual(8,r.Total);
        }

        [TestCase(StackFunctionEvaluator.Function.Push)]
        [TestCase(StackFunctionEvaluator.Function.Pop)]
        [TestCase(StackFunctionEvaluator.Function.Peek)]
        public void StarterCannotPass(StackFunctionEvaluator.Function f)
        {Assert.IsFalse(StackFunctionEvaluator.Check(f,StackFunctionEvaluator.Starter(f)).Accepted);}

        [Test] public void AlternativePushWithBlockBodyAndCountAssignmentIsAccepted()
        {
            string c=@"bool stack_push(Stack *stack, int value) {
                if (stack == NULL || stack->count == CAPACITY) { return false; }
                stack->data[stack->count] = value;
                stack->count = stack->count + 1;
                return true;
            }";
            Assert.IsTrue(StackFunctionEvaluator.Check(StackFunctionEvaluator.Function.Push,c).Accepted);
        }
        [Test] public void AlternativePopWithLocalAndCountUpdateIsAccepted()
        {
            string c=@"bool stack_pop(Stack *stack, int *out_value) {
                if (stack == NULL) return false;
                if (out_value == NULL) return false;
                if (stack->count <= 0) return false;
                int index = stack->count - 1;
                *out_value = stack->data[index];
                stack->count = index;
                return true;
            }";
            Assert.IsTrue(StackFunctionEvaluator.Check(StackFunctionEvaluator.Function.Pop,c).Accepted);
        }
        [Test] public void AlternativePeekWithIfElseWorks()
        {
            string c=@"bool stack_peek(const Stack *stack, int *out_value) {
                if (stack != NULL && out_value != NULL && stack->count > 0) {
                    *out_value = stack->data[stack->count - 1];
                    return true;
                } else {
                    return false;
                }
            }";
            Assert.IsTrue(StackFunctionEvaluator.Check(StackFunctionEvaluator.Function.Peek,c).Accepted);
        }
        [Test] public void MissingOverflowGuardRejected()
        {
            string c=StackFunctionEvaluator.Reference(StackFunctionEvaluator.Function.Push)
                .Replace("stack->count >= CAPACITY", "stack->count < 0");
            Assert.IsFalse(StackFunctionEvaluator.Check(StackFunctionEvaluator.Function.Push,c).Accepted);
        }
        [Test] public void MissingNullOutputGuardRejected()
        {
            string c=StackFunctionEvaluator.Reference(StackFunctionEvaluator.Function.Pop)
                .Replace("out_value == NULL || ", "");
            Assert.IsFalse(StackFunctionEvaluator.Check(StackFunctionEvaluator.Function.Pop,c).Accepted);
        }
        [Test] public void PopOffByOneRejected()
        {
            string c=StackFunctionEvaluator.Reference(StackFunctionEvaluator.Function.Pop)
                .Replace("stack->data[stack->count]", "stack->data[stack->count - 1]");
            Assert.IsFalse(StackFunctionEvaluator.Check(StackFunctionEvaluator.Function.Pop,c).Accepted);
        }
        [Test] public void PeekMustNotMutateCount()
        {
            string c=StackFunctionEvaluator.Reference(StackFunctionEvaluator.Function.Peek)
                .Replace("    return true;", "    stack->count--;\n    return true;");
            Assert.IsFalse(StackFunctionEvaluator.Check(StackFunctionEvaluator.Function.Peek,c).Accepted);
        }
        [Test] public void ReturningTrueWithoutMutationDoesNotPass()
        {
            string c="bool stack_push(Stack *stack, int value) { return true; }";
            Assert.IsFalse(StackFunctionEvaluator.Check(StackFunctionEvaluator.Function.Push,c).Accepted);
        }
        [Test] public void NativeCPreprocessorIsReportedUnsupported()
        {
            string c="#include <stdio.h>\n"+StackFunctionEvaluator.Reference(StackFunctionEvaluator.Function.Push);
            var r=StackFunctionEvaluator.Check(StackFunctionEvaluator.Function.Push,c);
            Assert.IsFalse(r.Accepted); StringAssert.Contains("subset",r.Message.ToLowerInvariant());
        }
        [Test] public void NoLoopsOrCallsAllowed()
        {
            string c="bool stack_push(Stack *stack, int value) { while (1) { } return false; }";
            Assert.IsFalse(StackFunctionEvaluator.Check(StackFunctionEvaluator.Function.Push,c).Accepted);
        }
        [Test] public void RejectMissingReturn()
        {
            string c="bool stack_push(Stack *stack, int value) { stack->count++; }";
            Assert.IsFalse(StackFunctionEvaluator.Check(StackFunctionEvaluator.Function.Push,c).Accepted);
        }
        [Test] public void RejectWrongSignature()
        {
            string c="int stack_push(Stack *stack, int value) { return 1; }";
            Assert.IsFalse(StackFunctionEvaluator.Check(StackFunctionEvaluator.Function.Push,c).Accepted);
        }
        [Test] public void RejectOversizedInput()
        {
            var r=StackFunctionEvaluator.Check(StackFunctionEvaluator.Function.Push,new string(' ',5001));
            Assert.IsFalse(r.Accepted);
        }
        [Test] public void UnsignedUndefinedBehaviorNotPresentedAsNativeExecution()
        {
            var r=StackFunctionEvaluator.Check(StackFunctionEvaluator.Function.Push,"bool stack_push(Stack *stack, int value) { return false; }");
            StringAssert.Contains("Passed",r.Message);
        }
    }
}
