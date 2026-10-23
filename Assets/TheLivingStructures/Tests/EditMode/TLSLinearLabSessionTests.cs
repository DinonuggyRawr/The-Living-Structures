using System;
using NUnit.Framework;
using TLS.Core;

namespace TLS.Tests
{
    public sealed class TLSLinearLabSessionTests
    {
        [Test] public void ArrayPredictionPromptDoesNotRevealOrModifyBeforeChoice()
        {
            var s = new CArrayLabSession();
            Assert.IsTrue(s.Request(CArrayOperation.Append));
            Assert.IsTrue(s.PredictionPending); Assert.IsNull(s.Trace); Assert.AreEqual(0, s.Current.Count);
        }
        [Test] public void ArrayCountPredictionCreditOnlyAfterFinishing()
        {
            var s = new CArrayLabSession();
            Assert.IsTrue(s.Request(CArrayOperation.Append)); s.ChoosePrediction(1);
            Assert.AreEqual(0, s.PredictionAttempts);
            s.Finish(); Assert.AreEqual(1, s.Current.Count);
            Assert.AreEqual(1, s.PredictionAttempts); Assert.AreEqual(1, s.CorrectPredictions);
            s.Previous(); s.Next();
            Assert.AreEqual(1, s.PredictionAttempts); // no duplicate awards on replay
        }
        [Test] public void ArrayWrongPredictionIsRecordedWithoutMastery()
        {
            var s = new CArrayLabSession(); s.Request(CArrayOperation.Append);
            s.ChoosePrediction(-1); s.Finish();
            Assert.AreEqual(1, s.PredictionAttempts); Assert.AreEqual(0, s.CorrectPredictions);
            StringAssert.Contains("Incorrect", s.PredictionReview);
        }
        [Test] public void ArraySkippingPredictionNeverAwardsCredit()
        {
            var s = new CArrayLabSession(); s.Request(CArrayOperation.Append);
            s.ChoosePrediction(null); s.Finish();
            Assert.AreEqual(1, s.Current.Count); Assert.AreEqual(0, s.PredictionAttempts);
        }
        [Test] public void ArrayRejectedInsertLeavesCountAndCanBePredictedUnchanged()
        {
            var s = new CArrayLabSession(); s.IndexText = "42"; s.Request(CArrayOperation.Insert);
            s.ChoosePrediction(0); s.Finish();
            Assert.IsFalse(s.Trace.Succeeded); Assert.AreEqual(0, s.Current.Count);
            Assert.AreEqual(1, s.CorrectPredictions);
        }
        [Test] public void ArrayBadIntegerNeverOpensPrediction()
        {
            var s = new CArrayLabSession(); s.ValueText = "not an integer";
            Assert.IsFalse(s.Request(CArrayOperation.Append)); Assert.IsFalse(s.PredictionPending);
            Assert.IsNull(s.Trace); Assert.AreEqual(0, s.Current.Count);
        }
        [Test] public void ArrayBadIndexNeverChangesState()
        {
            var s = new CArrayLabSession(); s.IndexText = "NaN";
            Assert.IsFalse(s.Request(CArrayOperation.Get)); Assert.AreEqual(0, s.Current.Count);
        }
        [Test] public void ArrayCannotRunNewOperationWhileTracePartiallyRewound()
        {
            var s = new CArrayLabSession(); s.Request(CArrayOperation.Append); s.ChoosePrediction(1);
            Assert.IsFalse(s.CanStart);
            s.Finish(); Assert.IsTrue(s.CanStart);
            s.Previous(); Assert.IsFalse(s.CanStart);
            Assert.IsFalse(s.Request(CArrayOperation.Append));
            s.Finish(); Assert.IsTrue(s.CanStart);
        }
        [Test] public void ArrayResetClearsModelAndTraceButNotPredictionTotals()
        {
            var s = new CArrayLabSession(); s.Request(CArrayOperation.Append); s.ChoosePrediction(1); s.Finish();
            s.Reset(); Assert.AreEqual(0, s.Current.Count); Assert.IsNull(s.Trace);
            Assert.AreEqual(1, s.PredictionAttempts);
        }
        [Test] public void ArrayCancelPredictionDoesNotCreateTrace()
        {
            var s = new CArrayLabSession(); s.Request(CArrayOperation.Append); s.CancelPrediction();
            Assert.IsNull(s.Trace); Assert.IsTrue(s.CanStart); Assert.AreEqual(0, s.PredictionAttempts);
        }
        [Test] public void InvalidPredictionDeltaThrows()
        {
            var s = new CArrayLabSession(); s.Request(CArrayOperation.Append);
            Assert.Throws<ArgumentOutOfRangeException>(() => s.ChoosePrediction(100));
            Assert.IsTrue(s.PredictionPending);
        }
        [Test] public void QueuePredictionHasNoSideEffectsBeforeChoice()
        {
            var s = new CQueueLabSession(); s.Request(CQueueOperation.Enqueue);
            Assert.IsTrue(s.PredictionPending); Assert.IsNull(s.Trace); Assert.AreEqual(0, s.Current.Count);
        }
        [Test] public void QueuePredictionCorrectAfterEnqueue()
        {
            var s = new CQueueLabSession(); s.Request(CQueueOperation.Enqueue); s.ChoosePrediction(1); s.Finish();
            Assert.AreEqual(1, s.Current.Count); Assert.AreEqual(1, s.CorrectPredictions);
            Assert.AreEqual(1, s.PredictionAttempts);
        }
        [Test] public void QueueUnderflowPredictionSameIsCorrect()
        {
            var s = new CQueueLabSession(); s.Request(CQueueOperation.Dequeue); s.ChoosePrediction(0); s.Finish();
            Assert.IsFalse(s.Trace.Succeeded); Assert.AreEqual(0, s.Current.Count);
            Assert.AreEqual(1, s.CorrectPredictions);
        }
        [Test] public void QueueWrongPredictionCountsOnceOnReplay()
        {
            var s = new CQueueLabSession(); s.Request(CQueueOperation.Enqueue); s.ChoosePrediction(0); s.Finish();
            s.Previous(); s.Next();
            Assert.AreEqual(1, s.PredictionAttempts); Assert.AreEqual(0, s.CorrectPredictions);
        }
        [Test] public void QueueSkippedPredictionDoesNotCount()
        {
            var s = new CQueueLabSession(); s.Request(CQueueOperation.Peek); s.ChoosePrediction(null); s.Finish();
            Assert.AreEqual(0, s.PredictionAttempts);
        }
        [Test] public void QueueInvalidValueCannotRun()
        {
            var s = new CQueueLabSession(); s.ValueText = "oops";
            Assert.IsFalse(s.Request(CQueueOperation.Enqueue)); Assert.IsNull(s.Trace);
        }
        [Test] public void QueueCannotRunAnotherActionUntilFinalFrame()
        {
            var s = new CQueueLabSession(); s.Request(CQueueOperation.Enqueue); s.ChoosePrediction(1);
            Assert.IsFalse(s.CanStart); Assert.IsFalse(s.Request(CQueueOperation.Peek));
            s.Finish(); Assert.IsTrue(s.CanStart);
        }
        [Test] public void QueueResetDoesNotClaimMastery()
        {
            var s = new CQueueLabSession(); s.Request(CQueueOperation.Enqueue); s.ChoosePrediction(1); s.Finish();
            s.Reset(); Assert.AreEqual(0, s.Current.Count); Assert.AreEqual(0, s.Current.Head);
            Assert.IsNull(s.Trace); Assert.AreEqual(1, s.CorrectPredictions);
        }
        [Test] public void QueueCancelPreventsChanges()
        {
            var s = new CQueueLabSession(); s.Request(CQueueOperation.Enqueue); s.CancelPrediction();
            Assert.AreEqual(0, s.Current.Count); Assert.IsNull(s.Trace); Assert.IsTrue(s.CanStart);
        }
        [Test] public void QueueRejectsUnexpectedPredictionValues()
        {
            var s = new CQueueLabSession(); s.Request(CQueueOperation.Enqueue);
            Assert.Throws<ArgumentOutOfRangeException>(() => s.ChoosePrediction(99));
        }
        [Test] public void ArrayAndQueueStatesAreIndependent()
        {
            var arrays = new CArrayLabSession(); var queues = new CQueueLabSession();
            arrays.Request(CArrayOperation.Append); arrays.ChoosePrediction(1); arrays.Finish();
            Assert.AreEqual(1, arrays.Current.Count); Assert.AreEqual(0, queues.Current.Count);
            queues.Request(CQueueOperation.Enqueue); queues.ChoosePrediction(1); queues.Finish();
            Assert.AreEqual(1, arrays.Current.Count); Assert.AreEqual(1, queues.Current.Count);
        }
    }
}
