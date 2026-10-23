using System;
using System.Collections.Generic;

namespace TLS.Core
{
    // An instructional model of a C stack initialized as Stack stack = {{0}, 0};
    // The UI shows the raw backing cells separately from the active logical cells.
    public sealed class StackState
    {
        public const int Capacity = 5;
        private readonly int[] cells;
        public int Count { get; private set; }

        public StackState() : this(new int[Capacity], 0) { }

        private StackState(int[] source, int count)
        {
            if (source == null || source.Length != Capacity || count < 0 || count > Capacity)
                throw new ArgumentException("Invalid simulated stack state.");
            cells = (int[])source.Clone();
            Count = count;
        }

        public int RawAt(int index)
        {
            if (index < 0 || index >= Capacity) throw new ArgumentOutOfRangeException(nameof(index));
            return cells[index];
        }

        public bool IsActive(int index) { return index >= 0 && index < Count; }
        public StackState Clone() { return new StackState(cells, Count); }

        internal StackState WriteRaw(int index, int value)
        {
            StackState next = Clone();
            next.cells[index] = value;
            return next;
        }

        internal StackState WithCount(int count) { return new StackState(cells, count); }
    }

    public enum StackOperation { Push, Pop, Peek }

    public sealed class StackFrame
    {
        public StackState State { get; private set; }
        public int HighlightLine { get; private set; }
        public string Explanation { get; private set; }
        public int FocusCell { get; private set; }

        public StackFrame(StackState state, int highlightLine, string explanation, int focusCell = -1)
        {
            State = state.Clone();
            HighlightLine = highlightLine;
            Explanation = explanation;
            FocusCell = focusCell;
        }
    }

    public sealed class StackTrace
    {
        public StackOperation Operation { get; private set; }
        public bool Succeeded { get; private set; }
        public int? Output { get; private set; }
        public StackState FinalState { get; private set; }
        public IReadOnlyList<StackFrame> Frames { get; private set; }

        internal StackTrace(StackOperation operation, bool succeeded, int? output,
            StackState finalState, List<StackFrame> frames)
        {
            Operation = operation;
            Succeeded = succeeded;
            Output = output;
            FinalState = finalState.Clone();
            Frames = frames.AsReadOnly();
        }
    }

    public static class StackSimulation
    {
        // Displayed snippets correspond to the C11 implementation in Examples/stack.c.
        public static readonly string[] PushCode = {
            "bool stack_push(Stack *stack, int value) {",
            "    if (stack == NULL || stack->count >= CAPACITY)",
            "        return false;",
            "    stack->data[stack->count] = value;",
            "    stack->count++;",
            "    return true;",
            "}"
        };
        public static readonly string[] PopCode = {
            "bool stack_pop(Stack *stack, int *out_value) {",
            "    if (stack == NULL || out_value == NULL || stack->count == 0)",
            "        return false;",
            "    stack->count--;",
            "    *out_value = stack->data[stack->count];",
            "    return true;",
            "}"
        };
        public static readonly string[] PeekCode = {
            "bool stack_peek(const Stack *stack, int *out_value) {",
            "    if (stack == NULL || out_value == NULL || stack->count == 0)",
            "        return false;",
            "    *out_value = stack->data[stack->count - 1];",
            "    return true;",
            "}"
        };

        public static string[] CodeFor(StackOperation operation)
        {
            if (operation == StackOperation.Push) return PushCode;
            if (operation == StackOperation.Pop) return PopCode;
            return PeekCode;
        }

        public static StackTrace Execute(StackState source, StackOperation operation, int input = 0)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            StackState state = source.Clone();
            List<StackFrame> frames = new List<StackFrame>();
            frames.Add(new StackFrame(state, 0, "Enter the C function. The current stack has " + state.Count + " active element(s)."));

            if (operation == StackOperation.Push)
            {
                frames.Add(new StackFrame(state, 1, "Check that the stack pointer is valid and the array has space. In this model the pointer is valid."));
                if (state.Count == StackState.Capacity)
                {
                    frames.Add(new StackFrame(state, 2, "Overflow prevented: capacity is " + StackState.Capacity + ". No memory is written."));
                    return new StackTrace(operation, false, null, state, frames);
                }
                int insertionIndex = state.Count;
                state = state.WriteRaw(insertionIndex, input);
                frames.Add(new StackFrame(state, 3, "Write " + input + " into data[" + insertionIndex + "]. The count has NOT changed yet; the cell is not logically active.", insertionIndex));
                state = state.WithCount(insertionIndex + 1);
                frames.Add(new StackFrame(state, 4, "Increase count to " + state.Count + ". The newly written cell is now the top of the stack.", insertionIndex));
                frames.Add(new StackFrame(state, 5, "Return true. This is the last-in-first-out (LIFO) top.", insertionIndex));
                return new StackTrace(operation, true, null, state, frames);
            }

            frames.Add(new StackFrame(state, 1, "Check that the stack and output pointers are valid and that count is nonzero."));
            if (state.Count == 0)
            {
                frames.Add(new StackFrame(state, 2, "Underflow prevented: the empty stack has no value to read. Nothing changes."));
                return new StackTrace(operation, false, null, state, frames);
            }

            if (operation == StackOperation.Pop)
            {
                int topIndex = state.Count - 1;
                state = state.WithCount(topIndex);
                frames.Add(new StackFrame(state, 3, "Decrement count first. The old top is now logically inactive but remains in raw array memory.", topIndex));
                int value = state.RawAt(topIndex);
                frames.Add(new StackFrame(state, 4, "Read the old top value, " + value + ", into the caller's out_value variable.", topIndex));
                frames.Add(new StackFrame(state, 5, "Return true. Popped value = " + value + ". The backing cell is not automatically erased.", topIndex));
                return new StackTrace(operation, true, value, state, frames);
            }
            else
            {
                int topIndex = state.Count - 1;
                int value = state.RawAt(topIndex);
                frames.Add(new StackFrame(state, 3, "Read data[count - 1] = " + value + ". Peek does not modify the stack.", topIndex));
                frames.Add(new StackFrame(state, 4, "Return true. Top = " + value + "; count remains " + state.Count + ".", topIndex));
                return new StackTrace(operation, true, value, state, frames);
            }
        }
    }
}
