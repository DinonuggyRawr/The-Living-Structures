using System;

namespace TLS.Core
{
    // Fixed, transparent educational examples. These are tracing checks, not C-coding mastery.
    public sealed class StackChallenge
    {
        public string Description { get; private set; }
        public int ExpectedTop { get; private set; }
        public int ExpectedCount { get; private set; }

        private StackChallenge(string description, int top, int count)
        {
            Description = description;
            ExpectedTop = top;
            ExpectedCount = count;
        }

        // The answer is calculated by the same authoritative model, not hard-coded into the UI.
        public static StackChallenge Get(int index)
        {
            StackOperation[] operations;
            int[] values;
            string description;
            switch (index)
            {
                case 0:
                    description = "Starting EMPTY: push(4), push(9), pop(), push(7).";
                    operations = new[] { StackOperation.Push, StackOperation.Push, StackOperation.Pop, StackOperation.Push };
                    values = new[] { 4, 9, 0, 7 };
                    break;
                case 1:
                    description = "Starting EMPTY: push(2), push(8), push(5), pop(), pop().";
                    operations = new[] { StackOperation.Push, StackOperation.Push, StackOperation.Push, StackOperation.Pop, StackOperation.Pop };
                    values = new[] { 2, 8, 5, 0, 0 };
                    break;
                case 2:
                    description = "Starting EMPTY, capacity 5: push(10), push(20), push(30), push(40), push(50), push(60). The last push fails when full.";
                    operations = new[] { StackOperation.Push, StackOperation.Push, StackOperation.Push, StackOperation.Push, StackOperation.Push, StackOperation.Push };
                    values = new[] { 10, 20, 30, 40, 50, 60 };
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
            StackState state = new StackState();
            for (int i = 0; i < operations.Length; i++)
                state = StackSimulation.Execute(state, operations[i], values[i]).FinalState;
            StackTrace result = StackSimulation.Execute(state, StackOperation.Peek);
            if (!result.Succeeded || !result.Output.HasValue)
                throw new InvalidOperationException("Challenge fixture unexpectedly empty.");
            return new StackChallenge(description, result.Output.Value, state.Count);
        }

        public bool Check(int top, int count) { return ExpectedTop == top && ExpectedCount == count; }
        public const int Total = 3;
    }
}
