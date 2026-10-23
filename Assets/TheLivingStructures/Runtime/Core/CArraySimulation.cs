using System;
using System.Collections.Generic;

namespace TLS.Core
{
    // A deliberately bounded, deterministic model of the C11 TeachingArray in Examples/arrays.c.
    // Raw cells survive removal; only indices below Count are logically part of the array.
    public sealed class CArrayState
    {
        public const int Capacity = 6;
        private readonly int[] data;
        public int Count { get; private set; }
        public CArrayState() : this(new int[Capacity], 0) { }
        private CArrayState(int[] values, int count)
        {
            if (values == null || values.Length != Capacity || count < 0 || count > Capacity)
                throw new ArgumentException("Invalid C array simulation state.");
            data = (int[])values.Clone();
            Count = count;
        }
        public CArrayState Clone() { return new CArrayState(data, Count); }
        public int RawAt(int index)
        {
            if (index < 0 || index >= Capacity) throw new ArgumentOutOfRangeException(nameof(index));
            return data[index];
        }
        public bool IsActive(int index) { return index >= 0 && index < Count; }
        internal CArrayState Write(int index, int value)
        {
            CArrayState copy = Clone();
            copy.data[index] = value;
            return copy;
        }
        internal CArrayState SetCount(int count) { return new CArrayState(data, count); }
    }

    public enum CArrayOperation { Append, Insert, Remove, Get }

    public sealed class CArrayFrame
    {
        public CArrayState State { get; private set; }
        public int CodeLine { get; private set; }
        public int ReadCell { get; private set; }
        public int WriteCell { get; private set; }
        public string Explanation { get; private set; }
        public CArrayFrame(CArrayState state, int line, string explanation, int readCell = -1, int writeCell = -1)
        {
            State = state.Clone(); CodeLine = line; Explanation = explanation;
            ReadCell = readCell; WriteCell = writeCell;
        }
    }

    public sealed class CArrayTrace
    {
        public CArrayOperation Operation { get; private set; }
        public bool Succeeded { get; private set; }
        public int? Output { get; private set; }
        public CArrayState FinalState { get; private set; }
        public IReadOnlyList<CArrayFrame> Frames { get; private set; }
        internal CArrayTrace(CArrayOperation op, bool success, int? output, CArrayState state, List<CArrayFrame> frames)
        {
            Operation = op; Succeeded = success; Output = output;
            FinalState = state.Clone(); Frames = frames.AsReadOnly();
        }
    }

    public static class CArraySimulation
    {
        // The exact C code is supplied separately in Examples/arrays.c. The model never
        // represents itself as arbitrary student-authored C execution.
        private static readonly string[] AppendCode = {
            "bool array_append(TeachingArray *a, int value) {",
            "    if (a == NULL || a->count >= ARRAY_CAPACITY)",
            "        return false;",
            "    a->data[a->count] = value;",
            "    a->count++;",
            "    return true;",
            "}"
        };
        private static readonly string[] InsertCode = {
            "bool array_insert(TeachingArray *a, int index, int value) {",
            "    if (a == NULL || a->count >= ARRAY_CAPACITY ||",
            "        index < 0 || index > a->count) return false;",
            "    for (int i = a->count; i > index; --i)",
            "        a->data[i] = a->data[i - 1];",
            "    a->data[index] = value;",
            "    a->count++;",
            "    return true;",
            "}"
        };
        private static readonly string[] RemoveCode = {
            "bool array_remove(TeachingArray *a, int index, int *out) {",
            "    if (a == NULL || out == NULL ||",
            "        index < 0 || index >= a->count) return false;",
            "    *out = a->data[index];",
            "    for (int i = index; i + 1 < a->count; ++i)",
            "        a->data[i] = a->data[i + 1];",
            "    a->count--;",
            "    return true;",
            "}"
        };
        private static readonly string[] GetCode = {
            "bool array_get(const TeachingArray *a, int index, int *out) {",
            "    if (a == NULL || out == NULL ||",
            "        index < 0 || index >= a->count) return false;",
            "    *out = a->data[index];",
            "    return true;",
            "}"
        };
        public static string[] CodeFor(CArrayOperation op)
        {
            return op == CArrayOperation.Append ? (string[])AppendCode.Clone() :
                op == CArrayOperation.Insert ? (string[])InsertCode.Clone() :
                op == CArrayOperation.Remove ? (string[])RemoveCode.Clone() : (string[])GetCode.Clone();
        }
        public static CArrayTrace Execute(CArrayState original, CArrayOperation operation, int index = 0, int value = 0)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            CArrayState state = original.Clone();
            var frames = new List<CArrayFrame>();
            frames.Add(new CArrayFrame(state, 0, "Enter the C function. count = " + state.Count + ", capacity = " + CArrayState.Capacity + "."));
            if (operation == CArrayOperation.Append)
            {
                frames.Add(new CArrayFrame(state, 1, "Check the pointer and whether there is room for one more element."));
                if (state.Count >= CArrayState.Capacity)
                {
                    frames.Add(new CArrayFrame(state, 2, "Full array: return false without touching memory or count."));
                    return new CArrayTrace(operation, false, null, state, frames);
                }
                int target = state.Count;
                state = state.Write(target, value);
                frames.Add(new CArrayFrame(state, 3, "Write data[" + target + "] = " + value + ". The slot exists in raw memory but is not active until count grows.", -1, target));
                state = state.SetCount(target + 1);
                frames.Add(new CArrayFrame(state, 4, "Increment count. The new slot now belongs to the logical array.", -1, target));
                frames.Add(new CArrayFrame(state, 5, "Return true after a successful append.", -1, target));
                return new CArrayTrace(operation, true, null, state, frames);
            }
            if (operation == CArrayOperation.Insert)
            {
                frames.Add(new CArrayFrame(state, 1, "Check capacity, pointer and insertion index (0 through count)."));
                if (state.Count >= CArrayState.Capacity || index < 0 || index > state.Count)
                {
                    frames.Add(new CArrayFrame(state, 2, "Invalid position or full array: reject without writing any cell."));
                    return new CArrayTrace(operation, false, null, state, frames);
                }
                for (int i = state.Count; i > index; i--)
                {
                    frames.Add(new CArrayFrame(state, 3, "Shift from RIGHT to LEFT to avoid overwriting unread values.", i - 1));
                    int copied = state.RawAt(i - 1);
                    state = state.Write(i, copied);
                    frames.Add(new CArrayFrame(state, 4, "Copy data[" + (i - 1) + "] to data[" + i + "] (value " + copied + ").", i - 1, i));
                }
                state = state.Write(index, value);
                frames.Add(new CArrayFrame(state, 5, "Place the inserted value " + value + " at index " + index + ".", -1, index));
                state = state.SetCount(state.Count + 1);
                frames.Add(new CArrayFrame(state, 6, "Increment count after all shifts and the insertion are complete.", -1, index));
                frames.Add(new CArrayFrame(state, 7, "Return true: elements at and after the insertion index moved right.", -1, index));
                return new CArrayTrace(operation, true, null, state, frames);
            }
            if (operation == CArrayOperation.Remove)
            {
                frames.Add(new CArrayFrame(state, 1, "Check pointer, output pointer and index (0 through count - 1)."));
                if (index < 0 || index >= state.Count)
                {
                    frames.Add(new CArrayFrame(state, 2, "Invalid or empty index: no access and no state change."));
                    return new CArrayTrace(operation, false, null, state, frames);
                }
                int removed = state.RawAt(index);
                frames.Add(new CArrayFrame(state, 3, "Copy removed value " + removed + " to the caller's output parameter.", index));
                for (int i = index; i + 1 < state.Count; i++)
                {
                    frames.Add(new CArrayFrame(state, 4, "Shift toward LEFT to close the gap at index " + i + ".", i + 1));
                    int copied = state.RawAt(i + 1);
                    state = state.Write(i, copied);
                    frames.Add(new CArrayFrame(state, 5, "Copy data[" + (i + 1) + "] into data[" + i + "].", i + 1, i));
                }
                state = state.SetCount(state.Count - 1);
                frames.Add(new CArrayFrame(state, 6, "Decrement count. The final raw cell may retain an old value but is inactive."));
                frames.Add(new CArrayFrame(state, 7, "Return true. The removed value was " + removed + "."));
                return new CArrayTrace(operation, true, removed, state, frames);
            }
            frames.Add(new CArrayFrame(state, 1, "Check the requested index and output pointer before reading."));
            if (index < 0 || index >= state.Count)
            {
                frames.Add(new CArrayFrame(state, 2, "Out-of-bounds read prevented. No memory is accessed."));
                return new CArrayTrace(operation, false, null, state, frames);
            }
            int result = state.RawAt(index);
            frames.Add(new CArrayFrame(state, 3, "Copy data[" + index + "] = " + result + " to the caller's output parameter.", index));
            frames.Add(new CArrayFrame(state, 4, "Read succeeded without changing array storage or count.", index));
            return new CArrayTrace(operation, true, result, state, frames);
        }
    }
}
