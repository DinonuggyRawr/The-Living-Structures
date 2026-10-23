using System;
using NUnit.Framework;
using TLS.Core;
namespace TLS.Tests
{
    public sealed class TLSPhase4RecursionTests
    {
        [TestCase(0,0)] [TestCase(1,1)] [TestCase(4,10)] [TestCase(10,55)]
        public void SumValues(int n,int result){var s=new TLSRecursionLab();s.Begin(TLSRecursionExample.SumToN,n);Assert.AreEqual(result,s.FinalValue);}
        [TestCase(0,1)] [TestCase(1,1)] [TestCase(4,24)] [TestCase(7,5040)]
        public void FactorialValues(int n,int result){var s=new TLSRecursionLab();s.Begin(TLSRecursionExample.Factorial,n);Assert.AreEqual(result,s.FinalValue);}
        [TestCase(0,0)] [TestCase(1,1)] [TestCase(4,3)] [TestCase(6,8)]
        public void FibonacciValues(int n,int result){var s=new TLSRecursionLab();s.Begin(TLSRecursionExample.Fibonacci,n);Assert.AreEqual(result,s.FinalValue);}
        [Test] public void BaseCasesDoNotRecurse(){var s=new TLSRecursionLab();s.Begin(TLSRecursionExample.SumToN,0);Assert.LessOrEqual(s.Frames.Count,5);}
        [Test] public void CallStackAppearsAfterStepping(){var s=new TLSRecursionLab();s.Begin(TLSRecursionExample.Factorial,3);s.Next();StringAssert.Contains("factorial(3)",s.Current.Calls);}
        [Test] public void RewindRestoresPreviousFrame(){var s=new TLSRecursionLab();s.Next();var t=s.Current;s.Next();s.Previous();Assert.AreSame(t,s.Current);}
        [Test] public void PredictionCanOnlyBeGradedOnce(){var s=new TLSRecursionLab();Assert.IsTrue(s.Predict("10"));Assert.IsFalse(s.Predict("10"));}
        [Test] public void WrongPredictionStillExplainsResult(){var s=new TLSRecursionLab();Assert.IsFalse(s.Predict("0"));StringAssert.Contains("10",s.PredictionFeedback);}
        [Test] public void PredictionNotAcceptedAfterTraceBegins(){var s=new TLSRecursionLab();s.Next();Assert.IsFalse(s.Predict("10"));}
        [Test] public void NegativeArgumentRejected(){var s=new TLSRecursionLab();Assert.Throws<ArgumentOutOfRangeException>(()=>s.Begin(TLSRecursionExample.SumToN,-1));}
        [Test] public void DeepFibonacciBlocked(){var s=new TLSRecursionLab();Assert.Throws<ArgumentOutOfRangeException>(()=>s.Begin(TLSRecursionExample.Fibonacci,7));}
        [Test] public void FactorialOverflowPreventedByLimit(){var s=new TLSRecursionLab();Assert.Throws<ArgumentOutOfRangeException>(()=>s.Begin(TLSRecursionExample.Factorial,11));}
        [Test] public void FinishShowsReturn(){var s=new TLSRecursionLab();s.Begin(TLSRecursionExample.SumToN,4);s.Finish();Assert.AreEqual(s.Frames.Count-1,s.Step);}
        [Test] public void FibonacciBranchesHaveMoreFramesThanLinearSum(){var s=new TLSRecursionLab();s.Begin(TLSRecursionExample.SumToN,6);int sum=s.Frames.Count;s.Begin(TLSRecursionExample.Fibonacci,6);Assert.Greater(s.Frames.Count,sum);}
    }
}
