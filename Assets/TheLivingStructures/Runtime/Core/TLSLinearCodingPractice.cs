using System;
using System.Collections.Generic;
using System.Globalization;

namespace TLS.Core
{
    // Structured C11 practice: NOT arbitrary C compilation or semantic-equivalence checking.
    public enum LinearPracticeDomain { Array, Queue }
    public enum LinearPracticeTrack { Trace, Complete, Repair }
    public enum LinearPracticeHelp { Guided, Independent }

    public sealed class LinearPracticeTask
    {
        public readonly string Id, Title, Objective, Source, Question, Hint, Reasoning, Reference, Pitfall;
        public readonly LinearPracticeDomain Domain;
        public readonly LinearPracticeTrack Track;
        public readonly bool IntegerAnswer;
        internal LinearPracticeTask(string id, LinearPracticeDomain domain, LinearPracticeTrack track,
            string title, string objective, string source, string question, string hint,
            string reasoning, string reference, string pitfall, bool integerAnswer = false)
        {
            Id = id; Domain = domain; Track = track; Title = title; Objective = objective;
            Source = source; Question = question; Hint = hint; Reasoning = reasoning;
            Reference = reference; Pitfall = pitfall; IntegerAnswer = integerAnswer;
        }
    }

    public static class LinearPracticeCatalog
    {
        public const int TasksPerTrack = 3;
        public const int TotalPerDomain = 9;
        private static CArrayState ArrayAfter(CArrayState state, CArrayOperation operation, int index = 0, int value = 0)
        {
            return CArraySimulation.Execute(state, operation, index, value).FinalState;
        }
        private static CQueueState QueueAfter(CQueueState state, CQueueOperation operation, int value = 0)
        {
            return CQueueSimulation.Execute(state, operation, value).FinalState;
        }

        public static LinearPracticeTask Get(LinearPracticeDomain domain, LinearPracticeTrack track, int index)
        {
            if (!Enum.IsDefined(typeof(LinearPracticeDomain), domain) || !Enum.IsDefined(typeof(LinearPracticeTrack), track))
                throw new ArgumentOutOfRangeException("Invalid domain or track.");
            if (index < 0 || index >= TasksPerTrack) throw new ArgumentOutOfRangeException(nameof(index));
            if (domain == LinearPracticeDomain.Array) return ArrayTask(track, index);
            return QueueTask(track, index);
        }

        private static LinearPracticeTask ArrayTask(LinearPracticeTrack track, int index)
        {
            if (track == LinearPracticeTrack.Trace)
            {
                if (index == 0)
                {
                    CArrayState s = ArrayAfter(ArrayAfter(new CArrayState(), CArrayOperation.Append, value: 10), CArrayOperation.Append, value: 20);
                    s = ArrayAfter(s, CArrayOperation.Insert, 1, 15);
                    return new LinearPracticeTask("A-TR-01", LinearPracticeDomain.Array, track,
                        "Trace insertion and right shift", "Predict an index after insertion.",
                        "TeachingArray a = {{0}, 0};\narray_append(&a, 10);\narray_append(&a, 20);\narray_insert(&a, 1, 15);",
                        "What is the final integer a.data[2]?", "Insertion at index 1 shifts the old element at index 1 to index 2.",
                        "The insertion shifts 20 right, then places 15 at index 1. The active array is [10, 15, 20].",
                        s.RawAt(2).ToString(CultureInfo.InvariantCulture), "Shifting must happen from right to left, not left to right.", true);
                }
                if (index == 1)
                {
                    CArrayState s = ArrayAfter(ArrayAfter(new CArrayState(), CArrayOperation.Append, value: 4), CArrayOperation.Append, value: 8);
                    s = ArrayAfter(s, CArrayOperation.Remove, 0);
                    return new LinearPracticeTask("A-TR-02", LinearPracticeDomain.Array, track,
                        "Trace raw memory after removal", "Separate active length from raw storage.",
                        "TeachingArray a = {{0}, 0};\nint out = -1;\narray_append(&a, 4);\narray_append(&a, 8);\narray_remove(&a, 0, &out);\n/* inspect a.data[1] */",
                        "What integer remains in raw a.data[1]?", "The left shift changes data[0], but the removed final slot is not wiped.",
                        "Removal copies the 8 into data[0] and decreases count to 1. Raw data[1] remains 8 but is inactive.",
                        s.RawAt(1).ToString(CultureInfo.InvariantCulture), "Logical removal need not zero inactive raw cells.", true);
                }
                CArrayState before = ArrayAfter(ArrayAfter(new CArrayState(), CArrayOperation.Append, value: 3), CArrayOperation.Append, value: 7);
                int output = CArraySimulation.Execute(before, CArrayOperation.Get, 1).Output.Value;
                return new LinearPracticeTask("A-TR-03", LinearPracticeDomain.Array, track,
                    "Trace GET output", "Read a validated array index.",
                    "TeachingArray a = {{0}, 0};\nint out = -1;\narray_append(&a, 3);\narray_append(&a, 7);\narray_get(&a, 1, &out);",
                    "What integer is stored in out?", "Index 1 refers to the second active element.",
                    "The array contains [3, 7], so GET copies 7 into out without changing count or memory.",
                    output.ToString(CultureInfo.InvariantCulture), "C arrays use zero-based indices.", true);
            }
            if (track == LinearPracticeTrack.Complete)
            {
                if (index == 0) return new LinearPracticeTask("A-CO-01", LinearPracticeDomain.Array, track,
                    "Write APPEND's memory assignment", "Translate a new active slot into a C assignment.",
                    "bool array_append(TeachingArray *a, int value) {\n    if (a == NULL || a->count >= ARRAY_CAPACITY) return false;\n    /* YOUR SINGLE STATEMENT */\n    a->count++;\n    return true;\n}",
                    "Write the missing C statement (include the semicolon).", "The free index is the current count, before it increments.",
                    "Write the next raw cell first; incrementing count afterwards makes it active.",
                    "a->data[a->count] = value;", "Using count - 1 would overwrite the previous final element.");
                if (index == 1) return new LinearPracticeTask("A-CO-02", LinearPracticeDomain.Array, track,
                    "Write the right-shift assignment", "Preserve values during insertion.",
                    "for (int i = a->count; i > index; --i) {\n    /* YOUR SINGLE STATEMENT */\n}",
                    "Write the assignment that shifts an element right.", "The destination is i; copy from i - 1.",
                    "Descending i prevents overwriting an original value before it has been copied.",
                    "a->data[i] = a->data[i - 1];", "Reversing source and destination loses data during right shifting.");
                return new LinearPracticeTask("A-CO-03", LinearPracticeDomain.Array, track,
                    "Write the left-shift assignment", "Close the gap after removing an element.",
                    "for (int i = index; i + 1 < a->count; ++i) {\n    /* YOUR SINGLE STATEMENT */\n}",
                    "Write the assignment that shifts the next element left.", "The destination is i; read from i + 1.",
                    "The old data[i+1] moves into data[i]. The count decreases only after shifting.",
                    "a->data[i] = a->data[i + 1];", "Left shift and right shift use opposite source indices.");
            }
            if (index == 0) return new LinearPracticeTask("A-RE-01", LinearPracticeDomain.Array, track,
                "Fix insert-at-end validation", "Keep a valid insertion at index count.",
                "/* BUG: index >= a->count rejects insert-at-end */\nif (a == NULL || a->count >= ARRAY_CAPACITY || index < 0 || index >= a->count)\n    return false;",
                "Replace only the if condition line (no semicolon).", "Appending at index count is valid if capacity remains.",
                "Reject index > count, but allow index == count. Check capacity before a write.",
                "if (a == NULL || a->count >= ARRAY_CAPACITY || index < 0 || index > a->count)",
                "The boundary differs between insertion (index <= count) and reads/removal (index < count).");
            if (index == 1) return new LinearPracticeTask("A-RE-02", LinearPracticeDomain.Array, track,
                "Repair reversed insertion shift", "Protect unread elements during insertion.",
                "for (int i = a->count; i > index; --i) {\n    /* BUGGY LINE BELOW */\n    a->data[i - 1] = a->data[i];\n}",
                "Replace the buggy assignment with one corrected C statement.", "The earlier cell is the source, not the destination.",
                "Right-shift from high index to low index by writing data[i] = data[i-1].",
                "a->data[i] = a->data[i - 1];", "Wrong-direction assignments can overwrite needed data.");
            return new LinearPracticeTask("A-RE-03", LinearPracticeDomain.Array, track,
                "Repair REMOVE bounds", "Never read data[count].",
                "/* BUG: index == count is incorrectly accepted */\nif (a == NULL || out == NULL || index < 0 || index > a->count)\n    return false;",
                "Replace the buggy if condition line (no semicolon).", "The highest valid occupied index is count - 1.",
                "For removal, index >= count must be rejected before reading data[index].",
                "if (a == NULL || out == NULL || index < 0 || index >= a->count)",
                "Index count is not an active element, even when raw memory happens to contain a value.");
        }

        private static LinearPracticeTask QueueTask(LinearPracticeTrack track, int index)
        {
            if (track == LinearPracticeTrack.Trace)
            {
                if (index == 0)
                {
                    CQueueState s = QueueAfter(QueueAfter(new CQueueState(), CQueueOperation.Enqueue, 6), CQueueOperation.Enqueue, 9);
                    int outValue = CQueueSimulation.Execute(s, CQueueOperation.Dequeue).Output.Value;
                    return new LinearPracticeTask("Q-TR-01", LinearPracticeDomain.Queue, track,
                        "Trace the DEQUEUE output", "Distinguish FIFO from LIFO.",
                        "TeachingQueue q = {{0}, 0, 0};\nint out = -1;\nqueue_enqueue(&q, 6);\nqueue_enqueue(&q, 9);\nqueue_dequeue(&q, &out);",
                        "What integer is written to out?", "The oldest element is at the queue's front.",
                        "FIFO dequeues 6, even though 9 was enqueued more recently.",
                        outValue.ToString(CultureInfo.InvariantCulture), "Queues remove the oldest item; stacks remove the newest.", true);
                }
                if (index == 1)
                {
                    CQueueState s = new CQueueState();
                    for (int n = 10; n <= 50; n += 10) s = QueueAfter(s, CQueueOperation.Enqueue, n);
                    s = QueueAfter(QueueAfter(s, CQueueOperation.Dequeue), CQueueOperation.Dequeue);
                    s = QueueAfter(QueueAfter(s, CQueueOperation.Enqueue, 60), CQueueOperation.Enqueue, 70);
                    return new LinearPracticeTask("Q-TR-02", LinearPracticeDomain.Queue, track,
                        "Trace physical head after wraparound", "Separate head index from the FIFO sequence.",
                        "TeachingQueue q = {{0}, 0, 0};\nint out;\nqueue_enqueue(&q, 10);\nqueue_enqueue(&q, 20);\nqueue_enqueue(&q, 30);\nqueue_enqueue(&q, 40);\nqueue_enqueue(&q, 50);\nqueue_dequeue(&q, &out);\nqueue_dequeue(&q, &out);\nqueue_enqueue(&q, 60);\nqueue_enqueue(&q, 70);\n/* inspect q.head */",
                        "What integer is q.head after these operations?", "Each successful dequeue advances head by one modulo 5.",
                        "After two dequeues, head is physical index 2. Later enqueues wrap into slots 0 and 1 without moving head.",
                        s.Head.ToString(CultureInfo.InvariantCulture), "Tail wraparound does not reset or advance the front index.", true);
                }
                CQueueState state = new CQueueState();
                for (int n = 10; n <= 50; n += 10) state = QueueAfter(state, CQueueOperation.Enqueue, n);
                state = QueueAfter(QueueAfter(state, CQueueOperation.Dequeue), CQueueOperation.Dequeue);
                state = QueueAfter(QueueAfter(state, CQueueOperation.Enqueue, 60), CQueueOperation.Enqueue, 70);
                return new LinearPracticeTask("Q-TR-03", LinearPracticeDomain.Queue, track,
                    "Trace raw ring slot after wraparound", "Identify the physical cell overwritten on wraparound.",
                    "TeachingQueue q = {{0}, 0, 0};\nint out;\nqueue_enqueue(&q, 10);\nqueue_enqueue(&q, 20);\nqueue_enqueue(&q, 30);\nqueue_enqueue(&q, 40);\nqueue_enqueue(&q, 50);\nqueue_dequeue(&q, &out);\nqueue_dequeue(&q, &out);\nqueue_enqueue(&q, 60);\nqueue_enqueue(&q, 70);\n/* inspect raw q.data[0] */",
                    "What integer is now in q.data[0]?", "The next tail after a full round wraps to physical cell zero.",
                    "The first re-enqueue stores 60 in raw data[0]. FIFO order is 30,40,50,60,70.",
                    state.RawAt(0).ToString(CultureInfo.InvariantCulture), "Physical slot zero is not necessarily the logical front.", true);
            }
            if (track == LinearPracticeTrack.Complete)
            {
                if (index == 0) return new LinearPracticeTask("Q-CO-01", LinearPracticeDomain.Queue, track,
                    "Calculate the circular tail", "Use modulo to prevent invalid physical indices.",
                    "bool queue_enqueue(TeachingQueue *q, int value) {\n    if (q == NULL || q->count >= QUEUE_CAPACITY) return false;\n    /* YOUR SINGLE STATEMENT */\n    q->data[tail] = value;\n    q->count++;\n    return true;\n}",
                    "Declare and compute int tail in one C statement.", "The next free physical index is (head + count) modulo capacity.",
                    "Modulo keeps tail within the buffer even after the queue wraps.",
                    "int tail = (q->head + q->count) % QUEUE_CAPACITY;", "Omitting modulo can write past the buffer end.");
                if (index == 1) return new LinearPracticeTask("Q-CO-02", LinearPracticeDomain.Queue, track,
                    "Store the new queue value", "Connect tail calculation to raw memory.",
                    "int tail = (q->head + q->count) % QUEUE_CAPACITY;\n/* YOUR SINGLE STATEMENT */\nq->count++;",
                    "Write the assignment at the computed tail index.", "Use q->data[tail], not q->data[count].",
                    "The new element is written at physical tail. Incrementing count then makes it part of FIFO order.",
                    "q->data[tail] = value;", "The tail index can differ from count after wraparound.");
                return new LinearPracticeTask("Q-CO-03", LinearPracticeDomain.Queue, track,
                    "Move head after DEQUEUE", "Implement constant-time circular advance.",
                    "*out = q->data[q->head];\n/* YOUR SINGLE STATEMENT */\nq->count--;",
                    "Write the head-advance assignment.", "Head increases by one modulo QUEUE_CAPACITY.",
                    "Advancing physical head makes the next oldest element the front. Count decreases separately.",
                    "q->head = (q->head + 1) % QUEUE_CAPACITY;", "A simple head++ would become invalid after the last physical slot.");
            }
            if (index == 0) return new LinearPracticeTask("Q-RE-01", LinearPracticeDomain.Queue, track,
                "Fix tail wraparound", "Prevent a physical out-of-range write.",
                "/* BUG: tail may exceed the last physical index */\nint tail = q->head + q->count;\nq->data[tail] = value;",
                "Replace the buggy tail declaration with one C statement.", "Reduce the sum using modulo QUEUE_CAPACITY.",
                "The tail location must wrap back to 0 instead of exceeding the raw array length.",
                "int tail = (q->head + q->count) % QUEUE_CAPACITY;", "Logical count is not automatically a valid raw index.");
            if (index == 1) return new LinearPracticeTask("Q-RE-02", LinearPracticeDomain.Queue, track,
                "Repair empty-DEQUEUE guard", "Avoid reading an empty queue's front.",
                "/* BUG: negative count is not the empty case */\nif (q == NULL || out == NULL || q->count < 0)\n    return false;",
                "Replace the buggy if condition line (no semicolon).", "The empty count is exactly zero.",
                "Check q->count == 0 before accessing data[head]; NULL checks short-circuit first.",
                "if (q == NULL || out == NULL || q->count == 0)", "Checking < 0 misses the empty case, count == 0.");
            return new LinearPracticeTask("Q-RE-03", LinearPracticeDomain.Queue, track,
                "Repair PEEK's front read", "Read without moving head.",
                "/* BUG: reads the second element, not the front */\n*out = q->data[(q->head + 1) % QUEUE_CAPACITY];",
                "Replace the buggy assignment with the correct read.", "PEEK uses q->head as-is and does not move it.",
                "The front item is stored at data[head]; PEEK leaves both head and count unchanged.",
                "*out = q->data[q->head];", "Advancing head belongs to DEQUEUE, not PEEK.");
        }

        // Grading is exact token-sequence reference matching (flexible whitespace, not arbitrary C).
        // Supports only the C operators required for the authored one-line problems.
        public static bool MatchesReference(LinearPracticeTask task, string answer)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (string.IsNullOrWhiteSpace(answer)) return false;
            if (task.IntegerAnswer)
            {
                int given, target;
                return int.TryParse(answer.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out given) &&
                    int.TryParse(task.Reference, NumberStyles.Integer, CultureInfo.InvariantCulture, out target) && given == target;
            }
            List<string> expected = Tokenize(task.Reference);
            List<string> actual = Tokenize(answer);
            if (expected == null || actual == null || expected.Count != actual.Count) return false;
            for (int i = 0; i < expected.Count; i++)
                if (!string.Equals(expected[i], actual[i], StringComparison.Ordinal)) return false;
            return true;
        }

        private static List<string> Tokenize(string source)
        {
            if (source == null || source.Length > 512) return null;
            var tokens = new List<string>();
            int i = 0;
            while (i < source.Length)
            {
                if (char.IsWhiteSpace(source[i])) { i++; continue; }
                int begin = i;
                char c = source[i];
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_')
                {
                    i++;
                    while (i < source.Length && (char.IsLetterOrDigit(source[i]) || source[i] == '_')) i++;
                }
                else if (char.IsDigit(c))
                {
                    i++;
                    while (i < source.Length && char.IsDigit(source[i])) i++;
                }
                else if (i + 1 < source.Length &&
                    (source.Substring(i, 2) == "->" || source.Substring(i, 2) == ">=" ||
                     source.Substring(i, 2) == "<=" || source.Substring(i, 2) == "==" ||
                     source.Substring(i, 2) == "!=" || source.Substring(i, 2) == "&&" ||
                     source.Substring(i, 2) == "||" || source.Substring(i, 2) == "++" ||
                     source.Substring(i, 2) == "--")) i += 2;
                else if ("[]();=+-*/%<>,".IndexOf(c) >= 0) i++;
                else return null;
                tokens.Add(source.Substring(begin, i - begin));
                if (tokens.Count > 128) return null;
            }
            return tokens;
        }
    }

    public sealed class LinearPracticeSession
    {
        private readonly HashSet<string> attempted = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> exposed = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> completed = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> independent = new HashSet<string>(StringComparer.Ordinal);
        public LinearPracticeDomain Domain { get; private set; }
        public LinearPracticeTrack Track { get; private set; }
        public LinearPracticeHelp Help { get; private set; }
        public int Index { get; private set; }
        public string Draft { get; set; }
        public string Feedback { get; private set; }
        public bool HintVisible { get; private set; }
        public bool ReferenceVisible { get; private set; }
        public bool JustCorrect { get; private set; }
        public LinearPracticeTask Current { get { return LinearPracticeCatalog.Get(Domain, Track, Index); } }
        public int CompletedCount { get { return completed.Count; } }
        public int IndependentFirstTryCount { get { return independent.Count; } }
        public bool CurrentCompleted { get { return completed.Contains(Current.Id); } }
        public bool CurrentIndependentFirstTry { get { return independent.Contains(Current.Id); } }

        public LinearPracticeSession(LinearPracticeDomain domain)
        {
            if (!Enum.IsDefined(typeof(LinearPracticeDomain), domain)) throw new ArgumentOutOfRangeException(nameof(domain));
            Domain = domain; Track = LinearPracticeTrack.Trace; Help = LinearPracticeHelp.Guided;
            Index = 0; Draft = "";
            Feedback = "Choose TRACE, COMPLETE, or REPAIR. This is session-only structured C practice.";
        }
        public void SelectTrack(LinearPracticeTrack track)
        {
            if (!Enum.IsDefined(typeof(LinearPracticeTrack), track)) throw new ArgumentOutOfRangeException(nameof(track));
            Track = track; Index = 0; ClearVisible();
        }
        public void SelectIndex(int index)
        {
            if (index < 0 || index >= LinearPracticeCatalog.TasksPerTrack) throw new ArgumentOutOfRangeException(nameof(index));
            Index = index; ClearVisible();
        }
        public void SetHelp(LinearPracticeHelp help)
        {
            if (!Enum.IsDefined(typeof(LinearPracticeHelp), help)) throw new ArgumentOutOfRangeException(nameof(help));
            Help = help; ClearVisible();
        }
        private void ClearVisible()
        {
            Draft = ""; HintVisible = false; ReferenceVisible = false; JustCorrect = false;
            Feedback = CurrentCompleted ? "Previously solved this session. Revisit it, or try another exercise." :
                "New exercise. Enter your answer and select CHECK.";
        }
        public bool ShowHint()
        {
            if (Help != LinearPracticeHelp.Guided) return false;
            exposed.Add(Current.Id); HintVisible = true; JustCorrect = false;
            Feedback = "Hint shown: this exercise can no longer earn unseen-first-attempt credit.";
            return true;
        }
        public void Reveal()
        {
            exposed.Add(Current.Id); ReferenceVisible = true; JustCorrect = false;
            Feedback = "Reference revealed for review. Independent first-attempt credit is not available.";
        }
        public bool Submit()
        {
            LinearPracticeTask task = Current;
            if (string.IsNullOrWhiteSpace(Draft))
            {
                JustCorrect = false;
                Feedback = "Enter an answer. Blank input does not consume a first attempt.";
                return false;
            }
            bool first = attempted.Add(task.Id);
            bool right = LinearPracticeCatalog.MatchesReference(task, Draft);
            JustCorrect = right;
            if (right)
            {
                bool unseen = first && Help == LinearPracticeHelp.Independent &&
                    !exposed.Contains(task.Id) && !completed.Contains(task.Id);
                completed.Add(task.Id);
                if (unseen) independent.Add(task.Id);
                ReferenceVisible = true;
                Feedback = (unseen ? "UNASSISTED FIRST TRY (structured exercise). " : "CORRECT (practice). ") + task.Reasoning;
            }
            else
            {
                Feedback = "Not a match for this authored answer. Common misconception: " + task.Pitfall +
                    " Check the C trace or select GUIDED for a hint.";
            }
            return right;
        }
    }
}
