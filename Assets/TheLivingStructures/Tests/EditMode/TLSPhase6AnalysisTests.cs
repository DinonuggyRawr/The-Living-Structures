using System;
using NUnit.Framework;
using TLS.Core;
namespace TLS.Tests
{
    public sealed class TLSPhase6AnalysisTests
    {
        [TestCase(4)][TestCase(8)][TestCase(16)][TestCase(32)][TestCase(64)]
        public void ArrayAccessCountDoesNotScale(int n)
        {
            var t=TLSAnalysisEngine.Build(TLSComplexityAlgorithm.ArrayAccess,n);
            Assert.AreEqual(1,t.End.Reads);Assert.AreEqual(1,t.End.Visits);Assert.AreEqual(0,t.End.Comparisons);
        }
        [TestCase(4)][TestCase(8)][TestCase(16)][TestCase(32)][TestCase(64)]
        public void LinearMissComparesEachValueOnce(int n)
        {
            var t=TLSAnalysisEngine.Build(TLSComplexityAlgorithm.LinearSearchMissing,n);
            Assert.AreEqual(n,t.End.Comparisons);Assert.AreEqual(n,t.End.Reads);Assert.AreEqual(n,t.End.Visits);
        }
        [TestCase(4)][TestCase(8)][TestCase(16)][TestCase(32)][TestCase(64)]
        public void BinarySearchMakesLogarithmicNumberOfComparisons(int n)
        {
            var t=TLSAnalysisEngine.Build(TLSComplexityAlgorithm.BinarySearchMissing,n);
            Assert.LessOrEqual(t.End.Comparisons,(int)Math.Ceiling(Math.Log(n+1,2)));Assert.Greater(t.End.Comparisons,0);
        }
        [TestCase(4)][TestCase(8)][TestCase(16)][TestCase(32)][TestCase(64)]
        public void InsertionSortProducesAscendingOutputAndQuadraticComparisons(int n)
        {
            var t=TLSAnalysisEngine.Build(TLSComplexityAlgorithm.InsertionSortReverse,n);
            for(int i=0;i<n;i++)Assert.AreEqual(i+1,t.Frames[t.Frames.Count-1].Snapshot[i]);
            Assert.AreEqual((long)n*(n-1)/2,t.End.Comparisons);
        }
        [TestCase(4)][TestCase(8)][TestCase(16)][TestCase(32)][TestCase(64)]
        public void MergeSortProducesSortedOutputAndBoundedComparisons(int n)
        {
            var t=TLSAnalysisEngine.Build(TLSComplexityAlgorithm.MergeSortReverse,n);
            for(int i=0;i<n;i++)Assert.AreEqual(i+1,t.Frames[t.Frames.Count-1].Snapshot[i]);
            Assert.LessOrEqual(t.End.Comparisons,(long)n*(long)Math.Ceiling(Math.Log(n,2)));
            Assert.AreEqual(1,t.End.Allocations);
        }
        [Test] public void FramesContainImmutableSnapshots()
        {
            var t=TLSAnalysisEngine.Build(TLSComplexityAlgorithm.InsertionSortReverse,8);
            Assert.AreEqual(8,t.Frames[0].Snapshot[0]);Assert.AreEqual(1,t.Frames[t.Frames.Count-1].Snapshot[0]);
        }
        [Test] public void AnalysisSessionRewindDoesNotRecalculateWrongState()
        {
            var s=new TLSAnalysisSession();s.Build();s.Finish();var final=s.Trace.End.Total;s.Previous();s.Next();
            Assert.AreEqual(final,s.Trace.End.Total);
        }
        [Test] public void PredictionIsHiddenUntilFinish()
        {
            var s=new TLSAnalysisSession();s.Build();Assert.IsTrue(s.Predict(999));
            StringAssert.DoesNotContain("observed",s.PredictionFeedback);s.Finish();StringAssert.Contains("observed",s.PredictionFeedback);
        }
        [Test] public void RewindingAfterSeeingOutcomeCannotEarnPrediction()
        {var s=new TLSAnalysisSession();s.Build();s.Finish();while(s.Step>0)s.Previous();Assert.IsFalse(s.Predict(5));}
        [Test] public void SecondPredictionIsNotRecorded()
        {var s=new TLSAnalysisSession();s.Build();Assert.IsTrue(s.Predict(4));Assert.IsFalse(s.Predict(5));Assert.AreEqual(4,s.PredictedComparisons);}
        [Test] public void FreshInputSizeInvalidatesPreviousTrace()
        {var s=new TLSAnalysisSession();s.Build();s.SetSize(16);Assert.IsNull(s.Trace);Assert.AreEqual(16,s.Size);}
        [Test] public void InvalidInputSizeThrows()
        {Assert.Throws<ArgumentOutOfRangeException>(()=>TLSAnalysisEngine.Build(TLSComplexityAlgorithm.ArrayAccess,65));}
        [Test] public void WrongGrowthClassIsNotClaimedForBinarySearch()
        {Assert.AreEqual("O(log n)",TLSAnalysisEngine.Complexity(TLSComplexityAlgorithm.BinarySearchMissing));}
    }
}
