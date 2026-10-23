using System;
using System.Collections.Generic;
using System.Globalization;

namespace TLS.Core
{
    // These are C11 structured-response exercises, NOT a C compiler or free-form code grader.
    // They deliberately use the same capacity-five, array-backed Stack as StackSimulation.
    public enum StackPracticeTrack { Trace, Complete, Repair }
    public enum StackPracticeAssistance { Guided, Independent }

    public sealed class StackPracticeTask
    {
        public readonly string Id;
        public readonly StackPracticeTrack Track;
        public readonly string Title;
        public readonly string Objective;
        public readonly string ContextCode;
        public readonly string Question;
        public readonly string Hint;
        public readonly string Explanation;
        public readonly string ReferenceAnswer;
        public readonly string CommonMistake;
        public readonly bool ExpectsInteger;

        internal StackPracticeTask(string id, StackPracticeTrack track, string title, string objective,
            string context, string question, string hint, string explanation, string answer,
            string commonMistake, bool integer)
        {
            Id = id;
            Track = track;
            Title = title;
            Objective = objective;
            ContextCode = context;
            Question = question;
            Hint = hint;
            Explanation = explanation;
            ReferenceAnswer = answer;
            CommonMistake = commonMistake;
            ExpectsInteger = integer;
        }
    }

    public static class StackPracticeCatalog
    {
        public const int TasksPerTrack = 3;
        public const int Total = 9;

        private static StackState Push(StackState state, int value)
        {
            return StackSimulation.Execute(state, StackOperation.Push, value).FinalState;
        }

        private static StackState Pop(StackState state)
        {
            return StackSimulation.Execute(state, StackOperation.Pop).FinalState;
        }

        public static StackPracticeTask Get(StackPracticeTrack track, int index)
        {
            if (index < 0 || index >= TasksPerTrack) throw new ArgumentOutOfRangeException(nameof(index));
            if (track == StackPracticeTrack.Trace)
            {
                if (index == 0)
                {
                    StackState s = Push(Push(new StackState(), 6), 9);
                    StackTrace pop = StackSimulation.Execute(s, StackOperation.Pop);
                    return new StackPracticeTask("TR-01", track, "Trace the POP output", "Read C output parameters.",
                        "Stack stack = {{0}, 0};\nint out = -1;\nstack_push(&stack, 6);\nstack_push(&stack, 9);\nstack_pop(&stack, &out);",
                        "What integer does out contain after these successful calls?", "POP returns the most recently pushed value, using *out_value.",
                        "The top value was 9. POP writes 9 to the caller's out variable, then the logical count becomes 1.",
                        pop.Output.Value.ToString(CultureInfo.InvariantCulture), "Trace the stack from bottom to top, then inspect the last POP.", true);
                }
                if (index == 1)
                {
                    StackState s = Pop(Push(new StackState(), 4));
                    return new StackPracticeTask("TR-02", track, "Raw memory after POP", "Distinguish logical count from physical array contents.",
                        "Stack stack = {{0}, 0};\nint out;\nstack_push(&stack, 4);\nstack_pop(&stack, &out);\n/* inspect stack.data[0] */",
                        "What integer remains in raw stack.data[0] after POP?", "The sample POP decrements count but does not zero the array cell.",
                        "The stack has count 0, but data[0] still stores 4. Reading that initialized array slot is valid; it is simply not a logical stack element.",
                        s.RawAt(0).ToString(CultureInfo.InvariantCulture), "POP makes the top inactive; it does not call memset.", true);
                }
                StackState final = Pop(Push(Push(Push(new StackState(), 7), 8), 9));
                return new StackPracticeTask("TR-03", track, "Count after a sequence", "Track state across multiple C function calls.",
                    "Stack stack = {{0}, 0};\nstack_push(&stack, 7);\nstack_push(&stack, 8);\nstack_push(&stack, 9);\nint out;\nstack_pop(&stack, &out);",
                    "What is stack.count at the end?", "Successful PUSH increases count; successful POP decreases count.",
                    "Three successful pushes followed by one successful pop leave two active elements, 7 and 8.",
                    final.Count.ToString(CultureInfo.InvariantCulture), "Count successful pushes and pops; don't subtract for a peek.", true);
            }

            if (track == StackPracticeTrack.Complete)
            {
                if (index == 0)
                    return new StackPracticeTask("CO-01", track, "Write the array assignment", "Translate the visual cell write into C.",
                        "bool stack_push(Stack *stack, int value) {\n    if (stack == NULL || stack->count >= CAPACITY)\n        return false;\n    /* YOUR ONE C STATEMENT HERE */\n    stack->count++;\n    return true;\n}",
                        "Write the statement that stores value at the next free index.", "The next available index equals count before count is incremented.",
                        "The assignment writes raw data[count] first. Only the following count++ makes the new value part of the logical stack.",
                        "stack->data[stack->count] = value;", "Do not use count - 1 for PUSH; that would overwrite the current top.", false);
                if (index == 1)
                    return new StackPracticeTask("CO-02", track, "Advance the logical count", "Connect a C increment to the active element boundary.",
                        "bool stack_push(Stack *stack, int value) {\n    if (stack == NULL || stack->count >= CAPACITY)\n        return false;\n    stack->data[stack->count] = value;\n    /* YOUR ONE C STATEMENT HERE */\n    return true;\n}",
                        "Write the statement that increases the active element count by one.", "Increment the count field through the struct pointer.",
                        "After writing data[count], count++ exposes that cell as the new logical top. A capacity check happened before this code.",
                        "stack->count++;", "Increment count exactly once, after writing the array cell.", false);
                return new StackPracticeTask("CO-03", track, "Return the popped value", "Use out parameters safely in C.",
                    "bool stack_pop(Stack *stack, int *out_value) {\n    if (stack == NULL || out_value == NULL || stack->count == 0)\n        return false;\n    stack->count--;\n    /* YOUR ONE C STATEMENT HERE */\n    return true;\n}",
                    "Write the statement that stores the old top in *out_value.", "After count--, the former top is at index count. Dereference out_value to assign to its caller.",
                    "Decrementing first makes stack->data[stack->count] the former top. *out_value writes the result through the validated pointer.",
                    "*out_value = stack->data[stack->count];", "Using count - 1 here would read the item below the popped element.", false);
            }

            if (track == StackPracticeTrack.Repair)
            {
                if (index == 0)
                    return new StackPracticeTask("RE-01", track, "Fix the overflow condition", "Repair a classic array bounds bug.",
                        "bool stack_push(Stack *stack, int value) {\n    /* BUG: stack->count > CAPACITY */\n    if (stack == NULL || stack->count > CAPACITY)\n        return false;\n    stack->data[stack->count] = value;\n    stack->count++;\n    return true;\n}",
                        "Replace only the entire buggy if (...) condition line (no semicolon).", "At count == CAPACITY, the next array index is already invalid.",
                        "The >= check rejects count 5 before a write to data[5]. The || short-circuits to avoid dereferencing NULL.",
                        "if (stack == NULL || stack->count >= CAPACITY)", "The old > check incorrectly permits a write when count equals capacity.", false);
                if (index == 1)
                    return new StackPracticeTask("RE-02", track, "Repair the empty-POP guard", "Prevent invalid dereferences and unsigned underflow.",
                        "bool stack_pop(Stack *stack, int *out_value) {\n    /* BUG: count < 0 cannot detect empty size_t */\n    if (stack == NULL || out_value == NULL || stack->count < 0)\n        return false;\n    stack->count--;\n    *out_value = stack->data[stack->count];\n    return true;\n}",
                        "Replace only the entire buggy if (...) condition line (no semicolon).", "count is size_t, so it cannot be negative. Test exactly for the empty value.",
                        "Checking count == 0 before decrement prevents unsigned wraparound. Checking both pointers also prevents invalid dereferences.",
                        "if (stack == NULL || out_value == NULL || stack->count == 0)", "A negative check is unsuitable for an unsigned size_t count.", false);
                return new StackPracticeTask("RE-03", track, "Fix PEEK indexing", "Prevent an off-by-one out-of-bounds read.",
                    "bool stack_peek(const Stack *stack, int *out_value) {\n    if (stack == NULL || out_value == NULL || stack->count == 0)\n        return false;\n    /* BUGGY LINE BELOW */\n    *out_value = stack->data[stack->count];\n    return true;\n}",
                    "Replace the buggy assignment with the correct C statement.", "count is one past the last active index. PEEK must not change count.",
                    "PEEK reads the element at count - 1 without modifying count. This works after the nonempty guard.",
                    "*out_value = stack->data[stack->count - 1];", "Using data[count] reads the next free slot, and may go out of bounds.", false);
            }
            throw new ArgumentOutOfRangeException(nameof(track));
        }

        // Match a narrowly specified statement by C token sequence rather than naive space stripping.
        // This deliberately does NOT claim semantic equivalence with arbitrary user-authored C.
        public static bool IsReferenceAnswer(StackPracticeTask task, string answer)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (string.IsNullOrWhiteSpace(answer)) return false;
            if (task.ExpectsInteger)
            {
                int given, expected;
                return int.TryParse(answer.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out given) &&
                    int.TryParse(task.ReferenceAnswer, NumberStyles.Integer, CultureInfo.InvariantCulture, out expected) && given == expected;
            }
            List<string> expectedTokens = CStatementTokens(task.ReferenceAnswer);
            List<string> actualTokens = CStatementTokens(answer);
            if (expectedTokens == null || actualTokens == null || expectedTokens.Count != actualTokens.Count)
                return false;
            for (int i = 0; i < expectedTokens.Count; i++)
                if (!string.Equals(expectedTokens[i], actualTokens[i], StringComparison.Ordinal)) return false;
            return true;
        }

        // Recognizes only the elementary C tokens required for these fixed-answer activities.
        private static List<string> CStatementTokens(string text)
        {
            if (text == null || text.Length > 512) return null;
            List<string> tokens = new List<string>();
            int i = 0;
            while (i < text.Length)
            {
                if (char.IsWhiteSpace(text[i])) { i++; continue; }
                int start = i;
                char c = text[i];
                if (char.IsLetter(c) || c == '_')
                {
                    i++;
                    while (i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] == '_')) i++;
                }
                else if (char.IsDigit(c))
                {
                    i++;
                    while (i < text.Length && char.IsDigit(text[i])) i++;
                }
                else if (i + 1 < text.Length &&
                    (text.Substring(i, 2) == "->" || text.Substring(i, 2) == ">=" ||
                     text.Substring(i, 2) == "==" || text.Substring(i, 2) == "&&" ||
                     text.Substring(i, 2) == "||" ||
                     text.Substring(i, 2) == "++" || text.Substring(i, 2) == "--")) i += 2;
                else if ("[]();=+-*<>".IndexOf(c) >= 0) i++;
                else return null;
                tokens.Add(text.Substring(start, i - start));
                if (tokens.Count > 100) return null;
            }
            return tokens;
        }
    }

    public sealed class StackPracticeSession
    {
        private readonly HashSet<string> attempted = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> exposed = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> completed = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> independent = new HashSet<string>(StringComparer.Ordinal);

        public StackPracticeTrack Track { get; private set; }
        public StackPracticeAssistance Assistance { get; private set; }
        public int Index { get; private set; }
        public string Draft { get; set; }
        public string Feedback { get; private set; }
        public bool HintVisible { get; private set; }
        public bool SolutionVisible { get; private set; }
        public bool JustCorrect { get; private set; }
        public StackPracticeTask Current { get { return StackPracticeCatalog.Get(Track, Index); } }
        public int CompletedCount { get { return completed.Count; } }
        public int IndependentFirstTryCount { get { return independent.Count; } }
        public bool CurrentCompleted { get { return completed.Contains(Current.Id); } }
        public bool CurrentIndependentFirstTry { get { return independent.Contains(Current.Id); } }

        public StackPracticeSession()
        {
            Track = StackPracticeTrack.Trace;
            Assistance = StackPracticeAssistance.Guided;
            Index = 0;
            Draft = "";
            Feedback = "Select a track and solve one exercise at a time. This is structured practice, not a C compiler.";
        }

        public void SelectTrack(StackPracticeTrack track)
        {
            if (!Enum.IsDefined(typeof(StackPracticeTrack), track)) throw new ArgumentOutOfRangeException(nameof(track));
            Track = track;
            Index = 0;
            ClearVisible();
        }

        public void SelectIndex(int index)
        {
            if (index < 0 || index >= StackPracticeCatalog.TasksPerTrack) throw new ArgumentOutOfRangeException(nameof(index));
            Index = index;
            ClearVisible();
        }

        public void SetAssistance(StackPracticeAssistance mode)
        {
            if (!Enum.IsDefined(typeof(StackPracticeAssistance), mode)) throw new ArgumentOutOfRangeException(nameof(mode));
            Assistance = mode;
            ClearVisible();
        }

        private void ClearVisible()
        {
            Draft = "";
            HintVisible = false;
            SolutionVisible = false;
            JustCorrect = false;
            Feedback = CurrentCompleted ? "Solved earlier in this session; revisit or try another exercise." :
                "New exercise. Your answer has not been checked.";
        }

        public bool ShowHint()
        {
            if (Assistance == StackPracticeAssistance.Independent) return false;
            exposed.Add(Current.Id);
            HintVisible = true;
            Feedback = "Hint shown. This exercise is now guided practice, not an unseen first attempt.";
            return true;
        }

        public void RevealSolution()
        {
            exposed.Add(Current.Id);
            SolutionVisible = true;
            JustCorrect = false;
            Feedback = "Reference answer revealed for review; this is not an independent first-attempt pass.";
        }

        public bool Submit()
        {
            StackPracticeTask task = Current;
            if (string.IsNullOrWhiteSpace(Draft))
            {
                JustCorrect = false;
                Feedback = "Enter an answer before checking; an empty response does not use an attempt.";
                return false;
            }
            bool firstAttempt = attempted.Add(task.Id);
            bool correct = StackPracticeCatalog.IsReferenceAnswer(task, Draft);
            JustCorrect = correct;
            if (correct)
            {
                bool firstTryIndependent = Assistance == StackPracticeAssistance.Independent && firstAttempt &&
                    !exposed.Contains(task.Id) && !completed.Contains(task.Id);
                completed.Add(task.Id);
                if (firstTryIndependent) independent.Add(task.Id);
                SolutionVisible = true;
                Feedback = (firstTryIndependent ? "UNASSISTED FIRST-TRY (structured exercise). " : "CORRECT (practice). ") +
                    task.Explanation;
            }
            else
            {
                Feedback = string.IsNullOrWhiteSpace(Draft) ? "Enter an answer before checking." :
                    "Not a match for this reference answer. " + task.CommonMistake +
                    " Retry, or choose GUIDED and request a hint.";
            }
            return correct;
        }
    }
}
