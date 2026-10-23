using System;
using System.Text;

namespace TLS.Core
{
    /// <summary>
    /// Educational annotations derived solely from the authoritative StackTrace snapshots.
    /// No separate stack state or independent animation timeline is maintained here.
    /// </summary>
    public sealed class StackStepDelta
    {
        public int PreviousCount { get; private set; }
        public int CurrentCount { get; private set; }
        public int CountDelta { get { return CurrentCount - PreviousCount; } }
        public int ChangedCell { get; private set; }
        public int PreviousRaw { get; private set; }
        public int CurrentRaw { get; private set; }
        public bool WroteRawCell { get { return ChangedCell >= 0; } }
        public int FocusCell { get; private set; }

        private StackStepDelta(int before, int after, int changedCell, int rawBefore, int rawAfter, int focusCell)
        {
            PreviousCount = before;
            CurrentCount = after;
            ChangedCell = changedCell;
            PreviousRaw = rawBefore;
            CurrentRaw = rawAfter;
            FocusCell = focusCell;
        }

        public static StackStepDelta At(StackTrace trace, int frameIndex)
        {
            if (trace == null) throw new ArgumentNullException(nameof(trace));
            if (frameIndex < 0 || frameIndex >= trace.Frames.Count) throw new ArgumentOutOfRangeException(nameof(frameIndex));
            StackFrame current = trace.Frames[frameIndex];
            StackState before = frameIndex == 0 ? current.State : trace.Frames[frameIndex - 1].State;
            int changedCell = -1, rawBefore = 0, rawAfter = 0;
            for (int i = 0; i < StackState.Capacity; i++)
            {
                if (before.RawAt(i) == current.State.RawAt(i)) continue;
                changedCell = i;
                rawBefore = before.RawAt(i);
                rawAfter = current.State.RawAt(i);
                break; // Every modeled stack step writes at most one backing cell.
            }
            // A C assignment is still a write when the assigned value equals the old value.
            // Detect that event from the authoritative C statement and focused cell.
            if (changedCell < 0 && frameIndex > 0 && trace.Operation == StackOperation.Push &&
                current.HighlightLine == 3 && current.FocusCell >= 0)
            {
                changedCell = current.FocusCell;
                rawBefore = before.RawAt(changedCell);
                rawAfter = current.State.RawAt(changedCell);
            }
            return new StackStepDelta(before.Count, current.State.Count, changedCell, rawBefore, rawAfter, current.FocusCell);
        }

        public string ChangeLabel()
        {
            if (WroteRawCell)
                return "BACKING ARRAY: data[" + ChangedCell + "]  " + PreviousRaw + " -> " + CurrentRaw +
                    (PreviousRaw == CurrentRaw ? "  (same value written)" : "") +
                    "   |   count still " + CurrentCount;
            if (CountDelta != 0)
                return "LOGICAL STACK: count " + PreviousCount + " -> " + CurrentCount +
                    (CountDelta > 0 ? "   |   the written slot becomes active" : "   |   the former top becomes inactive");
            return "NO STORED-STATE CHANGE  |  check, read, or return";
        }

        public string WhyThisMatters(StackTrace trace, int frameIndex)
        {
            if (trace == null) throw new ArgumentNullException(nameof(trace));
            if (frameIndex < 0 || frameIndex >= trace.Frames.Count) throw new ArgumentOutOfRangeException(nameof(frameIndex));
            if (WroteRawCell)
                return "C evaluates the assignment before count++. The new value exists in raw array storage, " +
                    "but data[0..count-1] still describes the logical stack.";
            if (CountDelta > 0)
                return "Incrementing count now includes the previously written slot. " +
                    "The newest active element is the LIFO top.";
            if (CountDelta < 0)
                return "Decrementing count removes the old top from the logical stack. " +
                    "The underlying array cell is still readable in this modeled C function.";
            StackFrame frame = trace.Frames[frameIndex];
            if (!trace.Succeeded && frameIndex == trace.Frames.Count - 1)
                return trace.Operation == StackOperation.Push
                    ? "The capacity guard prevents writing outside the five-element array. No values or count change."
                    : "The empty check prevents reading outside the active stack. No values or count change.";
            if (frame.FocusCell >= 0 && trace.Operation != StackOperation.Push)
                return trace.Operation == StackOperation.Peek
                    ? "Peek copies the top value through an output pointer, but count stays unchanged."
                    : "Pop copies the old top to the caller's output after decrementing count. It does not erase the slot.";
            return "The if condition protects access to array elements. In the simulator, valid input pointers " +
                   "are assumed; the example C function also checks for NULL pointers.";
        }
    }

    public enum StackCountPrediction { Increases, Decreases, Unchanged }

    public static class StackPredictionFeedback
    {
        public static StackCountPrediction Actual(StackTrace trace)
        {
            if (trace == null) throw new ArgumentNullException(nameof(trace));
            int difference = trace.FinalState.Count - trace.Frames[0].State.Count;
            if (difference > 0) return StackCountPrediction.Increases;
            if (difference < 0) return StackCountPrediction.Decreases;
            return StackCountPrediction.Unchanged;
        }

        public static bool IsCorrect(StackTrace trace, StackCountPrediction predicted)
        {
            return Actual(trace) == predicted;
        }

        public static string Explain(StackTrace trace, StackCountPrediction predicted)
        {
            StackCountPrediction actual = Actual(trace);
            string outcome = actual == StackCountPrediction.Increases ? "increases by one" :
                             actual == StackCountPrediction.Decreases ? "decreases by one" : "does not change";
            string because;
            if (!trace.Succeeded)
                because = trace.Operation == StackOperation.Push ? "The capacity guard rejects a full-stack push." :
                    "The empty-stack guard rejects the operation.";
            else if (trace.Operation == StackOperation.Push)
                because = "PUSH writes into data[count], then increments count.";
            else if (trace.Operation == StackOperation.Pop)
                because = "POP decrements count before reading the old top.";
            else
                because = "PEEK reads the top without changing count.";
            return (predicted == actual ? "PREDICTION CORRECT. " : "PREDICTION REVIEW. ") +
                "Count " + outcome + ". " + because;
        }
    }

    /// <summary>Limited rich-text coloring for fixed C snippets; not a C parser or compiler.</summary>
    public static class CCodeDisplay
    {
        public static string HighlightTokens(string source)
        {
            if (source == null) return "";
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < source.Length;)
            {
                if (char.IsLetter(source[i]) || source[i] == '_')
                {
                    int first = i++;
                    while (i < source.Length && (char.IsLetterOrDigit(source[i]) || source[i] == '_')) i++;
                    string token = source.Substring(first, i - first);
                    string color = token == "bool" || token == "int" || token == "const" || token == "if" || token == "return"
                        ? "#A78BFA" : token == "true" || token == "false" || token == "NULL" || token == "CAPACITY"
                        ? "#F2B84B" : null;
                    if (color == null) result.Append(token);
                    else result.Append("<color=").Append(color).Append(">").Append(token).Append("</color>");
                }
                else
                {
                    char c = source[i++];
                    // The fixed shipped snippets contain operators such as -> and >=.
                    // Unity's legacy rich-text renderer does not HTML-decode entities.
                    // A bare '>' is safe; fixed snippets do not include '<tag' content.
                    result.Append(c);
                }
            }
            return result.ToString();
        }
    }
}
