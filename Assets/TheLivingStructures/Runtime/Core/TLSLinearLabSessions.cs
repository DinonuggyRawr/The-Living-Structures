using System;

namespace TLS.Core
{
    // Session-only guided prediction/trace state. No progress file or mastery writes.
    // A complete trace is committed only on its final frame; rewinding is a read-only preview.
    public sealed class CArrayLabSession
    {
        public CArrayState Current { get; private set; } = new CArrayState();
        public CArrayTrace Trace { get; private set; }
        public int FrameIndex { get; private set; }
        public string ValueText = "12";
        public string IndexText = "0";
        public string InputError { get; private set; }
        public string PendingLabel { get { return pendingOp.ToString().ToUpperInvariant() +
            (pendingOp == CArrayOperation.Append ? " value=" + pendingValue :
             pendingOp == CArrayOperation.Insert ? " index=" + pendingIndex + " value=" + pendingValue :
             " index=" + pendingIndex); } }
        public string Message { get; private set; } = "Choose an operation. Predict count before watching the C trace.";
        public bool PredictionPending { get; private set; }
        public int PredictionAttempts { get; private set; }
        public int CorrectPredictions { get; private set; }
        public string PredictionReview { get; private set; } = "Practice only; no mastery.";
        private CArrayOperation pendingOp;
        private int pendingIndex, pendingValue;
        private int? chosenDelta;
        private bool committed;
        public bool CanStart { get { return !PredictionPending && (Trace == null || FrameIndex == Trace.Frames.Count - 1); } }
        public CArrayState Visible { get { return Trace == null ? Current : Trace.Frames[FrameIndex].State; } }
        public CArrayFrame VisibleFrame { get { return Trace == null ? null : Trace.Frames[FrameIndex]; } }

        public bool Request(CArrayOperation operation)
        {
            if (!CanStart) return false;
            InputError = null;
            int index = 0, value = 0;
            if (operation == CArrayOperation.Insert || operation == CArrayOperation.Remove || operation == CArrayOperation.Get)
            {
                if (!int.TryParse(IndexText, out index))
                {
                    Message = "Index must be a valid integer. Out-of-range indices are rejected by the C bounds check.";
                    InputError = Message;
                    return false;
                }
            }
            if ((operation == CArrayOperation.Append || operation == CArrayOperation.Insert) && !int.TryParse(ValueText, out value))
            {
                Message = "Value must be a 32-bit signed integer.";
                InputError = Message;
                return false;
            }
            pendingOp = operation; pendingIndex = index; pendingValue = value;
            PredictionPending = true;
            Message = "Predict whether the operation will change count, then reveal the trace.";
            return true;
        }
        public void ChoosePrediction(int? change)
        {
            if (!PredictionPending) throw new InvalidOperationException("No array prediction is pending.");
            if (change.HasValue && change.Value != -1 && change.Value != 0 && change.Value != 1)
                throw new ArgumentOutOfRangeException(nameof(change));
            chosenDelta = change;
            InputError = null;
            PredictionPending = false;
            Trace = CArraySimulation.Execute(Current, pendingOp, pendingIndex, pendingValue);
            FrameIndex = 0; committed = false;
            Message = "Step through the C code. Look for raw writes versus logical count changes.";
            PredictionReview = change.HasValue ? "Locked; see result at final step." :
                "Skipped; observe the trace.";
        }
        public void CancelPrediction() { PredictionPending = false; Message = "Prediction canceled; no array state was changed."; }
        public void Next()
        {
            if (Trace == null) return;
            FrameIndex = Math.Min(FrameIndex + 1, Trace.Frames.Count - 1);
            CommitAtEnd();
        }
        public void Previous() { if (Trace != null) FrameIndex = Math.Max(0, FrameIndex - 1); }
        public void Finish()
        {
            if (Trace == null) return;
            FrameIndex = Trace.Frames.Count - 1;
            CommitAtEnd();
        }
        private void CommitAtEnd()
        {
            if (committed || Trace == null || FrameIndex != Trace.Frames.Count - 1) return;
            Current = Trace.FinalState.Clone(); committed = true;
            Message = Trace.Succeeded ? "Operation completed. You can inspect earlier trace frames or start another operation." :
                "Bounds/capacity guard rejected the operation. The array remains unchanged.";
            if (chosenDelta.HasValue)
            {
                int actual = Trace.FinalState.Count - Trace.Frames[0].State.Count;
                PredictionAttempts++;
                bool correct = chosenDelta.Value == actual;
                if (correct) CorrectPredictions++;
                PredictionReview = (correct ? "Correct" : "Incorrect") + ": count " +
                    (actual > 0 ? "+" : "") + actual + " (practice only).";
            }
            else PredictionReview = "Skipped; no independent credit.";
        }
        public void Reset()
        {
            Current = new CArrayState(); Trace = null; FrameIndex = 0;
            PredictionPending = false; chosenDelta = null; committed = false; InputError = null;
            Message = "Array reset to empty. Session prediction totals remain until you leave Play Mode.";
            PredictionReview = "Practice only; no mastery.";
        }
    }

    public sealed class CQueueLabSession
    {
        public CQueueState Current { get; private set; } = new CQueueState();
        public CQueueTrace Trace { get; private set; }
        public int FrameIndex { get; private set; }
        public string ValueText = "12";
        public string InputError { get; private set; }
        public string PendingLabel { get { return pendingOp.ToString().ToUpperInvariant() +
            (pendingOp == CQueueOperation.Enqueue ? " value=" + pendingValue : ""); } }
        public string Message { get; private set; } = "FIFO: first in, first out. Watch head, tail, count and physical cells.";
        public bool PredictionPending { get; private set; }
        public int PredictionAttempts { get; private set; }
        public int CorrectPredictions { get; private set; }
        public string PredictionReview { get; private set; } = "Practice only; no mastery.";
        private CQueueOperation pendingOp;
        private int pendingValue;
        private int? chosenDelta;
        private bool committed;
        public bool CanStart { get { return !PredictionPending && (Trace == null || FrameIndex == Trace.Frames.Count - 1); } }
        public CQueueState Visible { get { return Trace == null ? Current : Trace.Frames[FrameIndex].State; } }
        public CQueueFrame VisibleFrame { get { return Trace == null ? null : Trace.Frames[FrameIndex]; } }

        public bool Request(CQueueOperation operation)
        {
            if (!CanStart) return false;
            InputError = null;
            int value = 0;
            if (operation == CQueueOperation.Enqueue && !int.TryParse(ValueText, out value))
            {
                Message = "Value must be a 32-bit signed integer.";
                InputError = Message;
                return false;
            }
            pendingOp = operation; pendingValue = value;
            PredictionPending = true;
            Message = "Predict count before tracing the circular queue operation.";
            return true;
        }
        public void ChoosePrediction(int? change)
        {
            if (!PredictionPending) throw new InvalidOperationException("No queue prediction is pending.");
            if (change.HasValue && change.Value != -1 && change.Value != 0 && change.Value != 1)
                throw new ArgumentOutOfRangeException(nameof(change));
            chosenDelta = change;
            InputError = null;
            PredictionPending = false;
            Trace = CQueueSimulation.Execute(Current, pendingOp, pendingValue);
            FrameIndex = 0; committed = false;
            Message = "Step through C and watch the wraparound index arithmetic.";
            PredictionReview = change.HasValue ? "Locked; see result at final step." :
                "Skipped; observe the trace.";
        }
        public void CancelPrediction() { PredictionPending = false; Message = "Prediction canceled; queue unchanged."; }
        public void Next()
        {
            if (Trace == null) return;
            FrameIndex = Math.Min(FrameIndex + 1, Trace.Frames.Count - 1);
            CommitAtEnd();
        }
        public void Previous() { if (Trace != null) FrameIndex = Math.Max(0, FrameIndex - 1); }
        public void Finish()
        {
            if (Trace == null) return;
            FrameIndex = Trace.Frames.Count - 1;
            CommitAtEnd();
        }
        private void CommitAtEnd()
        {
            if (committed || Trace == null || FrameIndex != Trace.Frames.Count - 1) return;
            Current = Trace.FinalState.Clone(); committed = true;
            Message = Trace.Succeeded ? "Operation complete. FIFO front and raw physical index are distinct concepts." :
                "Overflow/underflow rejected the operation safely; no queue state changed.";
            if (chosenDelta.HasValue)
            {
                int actual = Trace.FinalState.Count - Trace.Frames[0].State.Count;
                PredictionAttempts++;
                bool correct = chosenDelta.Value == actual;
                if (correct) CorrectPredictions++;
                PredictionReview = (correct ? "Correct" : "Incorrect") + ": count " +
                    (actual > 0 ? "+" : "") + actual + " (practice only).";
            }
            else PredictionReview = "Skipped; no independent credit.";
        }
        public void Reset()
        {
            Current = new CQueueState(); Trace = null; FrameIndex = 0;
            PredictionPending = false; chosenDelta = null; committed = false; InputError = null;
            Message = "Queue reset to empty; prediction totals are session-only.";
            PredictionReview = "Practice only; no mastery.";
        }
    }
}
