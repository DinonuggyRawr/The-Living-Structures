using System;
using System.Collections.Generic;
using System.Text;

namespace TLS.Core
{
    // Ring-buffer FIFO with physical slots distinct from logical queue order.
    public sealed class CQueueState
    {
        public const int Capacity = 5;
        private readonly int[] data;
        public int Head { get; private set; }
        public int Count { get; private set; }
        public CQueueState() : this(new int[Capacity], 0, 0) { }
        private CQueueState(int[] raw, int head, int count)
        {
            if (raw == null || raw.Length != Capacity || head < 0 || head >= Capacity || count < 0 || count > Capacity)
                throw new ArgumentException("Invalid ring queue state.");
            data = (int[])raw.Clone(); Head = head; Count = count;
        }
        public CQueueState Clone() { return new CQueueState(data, Head, Count); }
        public int RawAt(int index)
        {
            if (index < 0 || index >= Capacity) throw new ArgumentOutOfRangeException(nameof(index));
            return data[index];
        }
        public int PhysicalIndex(int logicalIndex)
        {
            if (logicalIndex < 0 || logicalIndex >= Count) throw new ArgumentOutOfRangeException(nameof(logicalIndex));
            return (Head + logicalIndex) % Capacity;
        }
        public int NextWriteSlot { get { return (Head + Count) % Capacity; } }
        public int LogicalPositionAt(int physical)
        {
            if (physical < 0 || physical >= Capacity) throw new ArgumentOutOfRangeException(nameof(physical));
            for (int i = 0; i < Count; i++) if (PhysicalIndex(i) == physical) return i;
            return -1;
        }
        public string LogicalOrder()
        {
            if (Count == 0) return "(empty)";
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < Count; i++)
            {
                if (i > 0) result.Append(" -> ");
                result.Append(RawAt(PhysicalIndex(i)));
            }
            return result.ToString();
        }
        internal CQueueState Write(int index, int value)
        {
            CQueueState next = Clone(); next.data[index] = value; return next;
        }
        internal CQueueState SetHead(int head) { return new CQueueState(data, head, Count); }
        internal CQueueState SetCount(int count) { return new CQueueState(data, Head, count); }
    }

    public enum CQueueOperation { Enqueue, Dequeue, Peek }
    public sealed class CQueueFrame
    {
        public CQueueState State { get; private set; }
        public int CodeLine { get; private set; }
        public int ReadCell { get; private set; }
        public int WriteCell { get; private set; }
        public string Explanation { get; private set; }
        public CQueueFrame(CQueueState state, int line, string explanation, int read = -1, int write = -1)
        {
            State = state.Clone(); CodeLine = line; Explanation = explanation;
            ReadCell = read; WriteCell = write;
        }
    }
    public sealed class CQueueTrace
    {
        public CQueueOperation Operation { get; private set; }
        public bool Succeeded { get; private set; }
        public int? Output { get; private set; }
        public CQueueState FinalState { get; private set; }
        public IReadOnlyList<CQueueFrame> Frames { get; private set; }
        internal CQueueTrace(CQueueOperation op, bool ok, int? output, CQueueState state, List<CQueueFrame> frames)
        {
            Operation = op; Succeeded = ok; Output = output;
            FinalState = state.Clone(); Frames = frames.AsReadOnly();
        }
    }
    public static class CQueueSimulation
    {
        private static readonly string[] EnqueueCode = {
            "bool queue_enqueue(TeachingQueue *q, int value) {",
            "    if (q == NULL || q->count >= QUEUE_CAPACITY)",
            "        return false;",
            "    int tail = (q->head + q->count) % QUEUE_CAPACITY;",
            "    q->data[tail] = value;",
            "    q->count++;",
            "    return true;",
            "}"
        };
        private static readonly string[] DequeueCode = {
            "bool queue_dequeue(TeachingQueue *q, int *out) {",
            "    if (q == NULL || out == NULL || q->count == 0)",
            "        return false;",
            "    *out = q->data[q->head];",
            "    q->head = (q->head + 1) % QUEUE_CAPACITY;",
            "    q->count--;",
            "    return true;",
            "}"
        };
        private static readonly string[] PeekCode = {
            "bool queue_peek(const TeachingQueue *q, int *out) {",
            "    if (q == NULL || out == NULL || q->count == 0)",
            "        return false;",
            "    *out = q->data[q->head];",
            "    return true;",
            "}"
        };
        public static string[] CodeFor(CQueueOperation op)
        {
            return op == CQueueOperation.Enqueue ? (string[])EnqueueCode.Clone() :
                op == CQueueOperation.Dequeue ? (string[])DequeueCode.Clone() : (string[])PeekCode.Clone();
        }
        public static CQueueTrace Execute(CQueueState original, CQueueOperation op, int value = 0)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            CQueueState state = original.Clone();
            var frames = new List<CQueueFrame>();
            frames.Add(new CQueueFrame(state, 0, "Enter the C function. head=" + state.Head + ", count=" + state.Count + "."));
            if (op == CQueueOperation.Enqueue)
            {
                frames.Add(new CQueueFrame(state, 1, "Check that the queue pointer is valid and capacity is available."));
                if (state.Count >= CQueueState.Capacity)
                {
                    frames.Add(new CQueueFrame(state, 2, "Overflow: no slot can be overwritten; return false."));
                    return new CQueueTrace(op, false, null, state, frames);
                }
                int tail = state.NextWriteSlot;
                frames.Add(new CQueueFrame(state, 3, "Compute tail = (head + count) % capacity = " + tail + ". Modulo wraps physical position to index 0 when needed.", tail));
                state = state.Write(tail, value);
                frames.Add(new CQueueFrame(state, 4, "Write " + value + " into physical data[" + tail + "]. The slot is still logically inactive until count grows.", -1, tail));
                state = state.SetCount(state.Count + 1);
                frames.Add(new CQueueFrame(state, 5, "Increase count. The new element becomes the FIFO back.", -1, tail));
                frames.Add(new CQueueFrame(state, 6, "Enqueue succeeds. Existing front remains the first value to be removed.", -1, tail));
                return new CQueueTrace(op, true, null, state, frames);
            }
            frames.Add(new CQueueFrame(state, 1, "Check pointer, output pointer, and count > 0 before reading."));
            if (state.Count == 0)
            {
                frames.Add(new CQueueFrame(state, 2, "Underflow: empty queue. No cell is read or changed."));
                return new CQueueTrace(op, false, null, state, frames);
            }
            int front = state.Head;
            int output = state.RawAt(front);
            frames.Add(new CQueueFrame(state, 3, "Read front data[" + front + "] = " + output + " into the caller's output parameter.", front));
            if (op == CQueueOperation.Dequeue)
            {
                state = state.SetHead((state.Head + 1) % CQueueState.Capacity);
                frames.Add(new CQueueFrame(state, 4, "Advance head modulo capacity. Count has NOT decreased yet: this is an intermediate in-function state, not the final queue.", front));
                state = state.SetCount(state.Count - 1);
                frames.Add(new CQueueFrame(state, 5, "Decrease count. Physical data[" + front + "] may still contain its old value.", front));
                frames.Add(new CQueueFrame(state, 6, "Return true with dequeued value " + output + ". FIFO removes the oldest element.", front));
                return new CQueueTrace(op, true, output, state, frames);
            }
            frames.Add(new CQueueFrame(state, 4, "Return true. Peek reads the front without moving head or changing count.", front));
            return new CQueueTrace(op, true, output, state, frames);
        }
    }
}
