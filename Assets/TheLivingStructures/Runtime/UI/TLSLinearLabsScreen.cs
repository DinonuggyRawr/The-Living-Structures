using System;
using TLS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TLS.UI
{
    /// <summary>
    /// Phase 2.1 Array & Circular Queue learning views. Same deterministic model backs the
    /// visible C code, raw memory cells, logical membership and explanation timeline.
    /// All prediction results are formative session-only evidence, not C mastery.
    /// </summary>
    public static class TLSLinearLabsScreen
    {
        private static Vector2 V(float x, float y) { return new Vector2(x, y); }

        public static void RenderArray(Transform root, CArrayLabSession session, Action redraw)
        {
            if (root == null || session == null || redraw == null) throw new ArgumentNullException("Array view needs root, state, redraw.");
            Background(root);
            CArrayState shown = session.Visible;
            CArrayFrame frame = session.VisibleFrame;
            CArrayOperation op = session.Trace == null ? CArrayOperation.Append : session.Trace.Operation;
            DrawCode(root, "ARRAYS  /  EXECUTING C11", CArraySimulation.CodeFor(op), frame == null ? -1 : frame.CodeLine,
                "Examples/arrays.c  |  modeled C execution only");

            RectTransform memory = TLSUiFactory.Panel(root, "Array visualization", V(0.429f, 0.266f), V(0.760f, 0.889f), TLSUiFactory.Deep);
            TLSUiFactory.Label(memory, "Array title", "ARRAY / LOGICAL VS RAW", V(0.04f, 0.903f), V(0.97f, 0.982f),
                20, TLSUiFactory.Cyan, TextAnchor.MiddleLeft, true);
            TLSUiFactory.Label(memory, "Array subtitle", "Contiguous cells. Only indices below count are active.",
                V(0.042f, 0.819f), V(0.967f, 0.892f), 15, TLSUiFactory.Muted);
            for (int i = 0; i < CArrayState.Capacity; i++)
            {
                float y = 0.187f + i * 0.103f;
                bool active = shown.IsActive(i);
                bool writing = frame != null && frame.WriteCell == i;
                bool reading = frame != null && frame.ReadCell == i;
                Color tint = writing ? new Color(0.43f, 0.31f, 0.13f, 1) :
                    reading ? new Color(0.16f, 0.30f, 0.41f, 1) : active ? TLSUiFactory.Slate : TLSUiFactory.Navy;
                RectTransform row = TLSUiFactory.Panel(memory, "Array cell " + i,
                    V(0.075f, y), V(0.925f, y + 0.092f), tint);
                TLSUiFactory.Label(row, "Index", "[" + i + "]", V(0.03f, 0.10f), V(0.19f, 0.91f),
                    17, TLSUiFactory.Muted);
                string label = shown.RawAt(i).ToString() + (active ? "   ACTIVE" : "   inactive (raw)");
                TLSUiFactory.Label(row, "Value", label, V(0.205f, 0.09f), V(0.965f, 0.92f),
                    active ? 20 : 16, writing ? TLSUiFactory.Amber : active ? TLSUiFactory.Pale : TLSUiFactory.Muted,
                    TextAnchor.MiddleCenter, active);
            }
            TLSUiFactory.Label(memory, "Array summary", "LOGICAL SIZE: " + shown.Count + " / " + CArrayState.Capacity,
                V(0.075f, 0.048f), V(0.925f, 0.16f), 20, TLSUiFactory.Amber, TextAnchor.MiddleCenter, true);

            RectTransform why = Inspector(root, "ARRAY  /  MEMORY & WHY");
            TLSUiFactory.Label(why, "Count", "count = " + shown.Count + " / " + CArrayState.Capacity,
                V(0.05f, 0.814f), V(0.96f, 0.897f), 21, TLSUiFactory.Pale, TextAnchor.MiddleLeft, true);
            TLSUiFactory.Label(why, "Index range", shown.Count == 0 ? "Active indices: none" :
                "Active indices: 0 through " + (shown.Count - 1), V(0.05f, 0.752f), V(0.96f, 0.816f), 16, TLSUiFactory.Muted);
            string focus = frame == null ? "No current write/read." :
                frame.WriteCell >= 0 ? "WRITING data[" + frame.WriteCell + "]" :
                frame.ReadCell >= 0 ? "READING data[" + frame.ReadCell + "]" : "No raw cell access on this line.";
            TLSUiFactory.Label(why, "Focus header", "MEMORY ACCESS", V(0.05f, 0.67f), V(0.96f, 0.733f),
                17, TLSUiFactory.Violet, TextAnchor.MiddleLeft, true);
            TLSUiFactory.Label(why, "Focused cell", focus, V(0.05f, 0.574f), V(0.96f, 0.662f),
                17, frame != null && frame.WriteCell >= 0 ? TLSUiFactory.Amber : TLSUiFactory.Pale);
            string result = VisibleArrayOutput(session);
            TLSUiFactory.Label(why, "Array output", result, V(0.05f, 0.468f), V(0.96f, 0.565f), 17, TLSUiFactory.Green);
            TLSUiFactory.Label(why, "Costs title", "PERFORMANCE / C REASONING", V(0.05f, 0.36f), V(0.96f, 0.449f),
                16, TLSUiFactory.Violet, TextAnchor.MiddleLeft, true);
            TLSUiFactory.Label(why, "Costs", "GET: O(1)  •  APPEND: O(1) fixed capacity\nINSERT / REMOVE: O(n) worst case due to shifting.",
                V(0.05f, 0.167f), V(0.96f, 0.353f), 16, TLSUiFactory.Pale);
            TLSUiFactory.Label(why, "Scope", "These are modeled operations, not student-written C compilation or coding mastery.",
                V(0.05f, 0.018f), V(0.96f, 0.159f), 14, TLSUiFactory.Amber);

            RectTransform controls = Controls(root, session.InputError != null ? session.InputError : session.VisibleFrame == null ? session.Message : session.VisibleFrame.Explanation,
                session.PredictionReview, session.CorrectPredictions, session.PredictionAttempts);
            TLSUiFactory.Label(controls, "Value field label", "VALUE", V(0.022f, 0.692f), V(0.142f, 0.786f), 12, TLSUiFactory.Muted);
            TLSUiFactory.Label(controls, "Index field label", "INDEX", V(0.155f, 0.692f), V(0.276f, 0.786f), 12, TLSUiFactory.Muted);
            InputField value = TLSUiFactory.Field(controls, "Array value input", session.ValueText,
                "value", V(0.02f, 0.40f), V(0.143f, 0.69f), s => session.ValueText = s);
            value.characterLimit = 12;
            InputField index = TLSUiFactory.Field(controls, "Array index input", session.IndexText,
                "index", V(0.153f, 0.40f), V(0.276f, 0.69f), s => session.IndexText = s);
            index.characterLimit = 12;
            TLSUiFactory.MakeButton(controls, "APPEND", V(0.286f, 0.40f), V(0.455f, 0.69f),
                () => { session.Request(CArrayOperation.Append); redraw(); }, TLSUiFactory.Deep, 17, session.CanStart);
            TLSUiFactory.MakeButton(controls, "INSERT AT", V(0.465f, 0.40f), V(0.635f, 0.69f),
                () => { session.Request(CArrayOperation.Insert); redraw(); }, TLSUiFactory.Deep, 17, session.CanStart);
            TLSUiFactory.MakeButton(controls, "REMOVE AT", V(0.645f, 0.40f), V(0.815f, 0.69f),
                () => { session.Request(CArrayOperation.Remove); redraw(); }, TLSUiFactory.Deep, 17, session.CanStart);
            TLSUiFactory.MakeButton(controls, "GET AT", V(0.825f, 0.40f), V(0.98f, 0.69f),
                () => { session.Request(CArrayOperation.Get); redraw(); }, TLSUiFactory.Deep, 17, session.CanStart);
            StepButtons(controls, () => { session.Previous(); redraw(); }, () => { session.Next(); redraw(); },
                () => { session.Finish(); redraw(); }, () => { session.Reset(); redraw(); },
                session.Trace != null && session.FrameIndex > 0,
                session.Trace != null && session.FrameIndex < session.Trace.Frames.Count - 1,
                session.Trace != null, "RESET ARRAY");
            if (session.PredictionPending)
                PredictionPrompt(root, "ARRAY " + session.PendingLabel + " | " + session.Current.Count + "/" + CArrayState.Capacity,
                    change => { session.ChoosePrediction(change); redraw(); },
                    () => { session.CancelPrediction(); redraw(); });
        }

        public static void RenderQueue(Transform root, CQueueLabSession session, Action redraw)
        {
            if (root == null || session == null || redraw == null) throw new ArgumentNullException("Queue view needs root, state, redraw.");
            Background(root);
            CQueueState shown = session.Visible;
            CQueueFrame frame = session.VisibleFrame;
            CQueueOperation op = session.Trace == null ? CQueueOperation.Enqueue : session.Trace.Operation;
            DrawCode(root, "CIRCULAR QUEUE / EXECUTING C11", CQueueSimulation.CodeFor(op), frame == null ? -1 : frame.CodeLine,
                "Examples/queues.c  |  modeled C execution only");

            RectTransform memory = TLSUiFactory.Panel(root, "Queue visualization", V(0.429f, 0.266f), V(0.760f, 0.889f), TLSUiFactory.Deep);
            TLSUiFactory.Label(memory, "Queue title", "FIFO  /  PHYSICAL RING BUFFER", V(0.04f, 0.903f), V(0.97f, 0.982f),
                20, TLSUiFactory.Cyan, TextAnchor.MiddleLeft, true);
            TLSUiFactory.Label(memory, "Queue subtitle", "head + count determine logical order. Raw slots may be stale.",
                V(0.04f, 0.82f), V(0.97f, 0.892f), 15, TLSUiFactory.Muted);
            for (int i = 0; i < CQueueState.Capacity; i++)
            {
                float y = 0.207f + i * 0.118f;
                int logical = shown.LogicalPositionAt(i);
                bool active = logical >= 0;
                bool writing = frame != null && frame.WriteCell == i;
                bool reading = frame != null && frame.ReadCell == i;
                Color tint = writing ? new Color(0.43f, 0.31f, 0.13f, 1) :
                    reading ? new Color(0.16f, 0.30f, 0.41f, 1) : active ? TLSUiFactory.Slate : TLSUiFactory.Navy;
                RectTransform row = TLSUiFactory.Panel(memory, "Queue cell " + i, V(0.075f, y), V(0.925f, y + 0.103f), tint);
                TLSUiFactory.Label(row, "Index", "[" + i + "]", V(0.028f, 0.08f), V(0.17f, 0.92f),
                    16, TLSUiFactory.Muted);
                string role = !active ? "inactive (raw)" : logical == 0 ? "FRONT" : "FIFO #" + (logical + 1);
                TLSUiFactory.Label(row, "Value", shown.RawAt(i) + "  " + role,
                    V(0.18f, 0.08f), V(0.96f, 0.93f), active ? 19 : 15,
                    writing ? TLSUiFactory.Amber : active ? TLSUiFactory.Pale : TLSUiFactory.Muted,
                    TextAnchor.MiddleCenter, active);
            }
            TLSUiFactory.Label(memory, "FIFO order", "FIFO: " + shown.LogicalOrder(), V(0.04f, 0.037f), V(0.965f, 0.164f),
                17, TLSUiFactory.Amber, TextAnchor.MiddleCenter, true);

            RectTransform why = Inspector(root, "QUEUE  /  POINTERS & WHY");
            TLSUiFactory.Label(why, "Queue count", "count = " + shown.Count + " / " + CQueueState.Capacity,
                V(0.05f, 0.823f), V(0.96f, 0.901f), 20, TLSUiFactory.Pale, TextAnchor.MiddleLeft, true);
            TLSUiFactory.Label(why, "Head", "head (physical) = " + shown.Head,
                V(0.05f, 0.754f), V(0.96f, 0.827f), 18, TLSUiFactory.Pale);
            TLSUiFactory.Label(why, "Tail", "tail slot (if room) = " + shown.NextWriteSlot,
                V(0.05f, 0.652f), V(0.96f, 0.756f), 16, TLSUiFactory.Muted);
            string focus = frame == null ? "No raw cell access yet." :
                frame.WriteCell >= 0 ? "WRITING data[" + frame.WriteCell + "]" :
                frame.ReadCell >= 0 ? "READING data[" + frame.ReadCell + "]" : "No raw access on this line.";
            TLSUiFactory.Label(why, "Focus header", "MEMORY ACCESS", V(0.05f, 0.565f), V(0.96f, 0.635f),
                17, TLSUiFactory.Violet, TextAnchor.MiddleLeft, true);
            TLSUiFactory.Label(why, "Focus", focus, V(0.05f, 0.463f), V(0.96f, 0.553f), 17, TLSUiFactory.Amber);
            TLSUiFactory.Label(why, "Output", VisibleQueueOutput(session), V(0.05f, 0.376f), V(0.96f, 0.468f),
                17, TLSUiFactory.Green);
            TLSUiFactory.Label(why, "Lesson", "FIFO order is not physical index order after wraparound. ENQUEUE / DEQUEUE / PEEK are O(1).",
                V(0.05f, 0.15f), V(0.96f, 0.366f), 17, TLSUiFactory.Pale);
            TLSUiFactory.Label(why, "Scope", "Transient code-step states may briefly differ from the finished queue. No C mastery is awarded.",
                V(0.05f, 0.015f), V(0.96f, 0.146f), 14, TLSUiFactory.Amber);

            RectTransform controls = Controls(root, session.InputError != null ? session.InputError : frame == null ? session.Message : frame.Explanation,
                session.PredictionReview, session.CorrectPredictions, session.PredictionAttempts);
            TLSUiFactory.Label(controls, "Queue value field label", "VALUE", V(0.022f, 0.692f), V(0.217f, 0.786f), 12, TLSUiFactory.Muted);
            InputField value = TLSUiFactory.Field(controls, "Queue value input", session.ValueText,
                "value", V(0.02f, 0.40f), V(0.22f, 0.69f), s => session.ValueText = s);
            value.characterLimit = 12;
            TLSUiFactory.MakeButton(controls, "ENQUEUE", V(0.245f, 0.40f), V(0.485f, 0.69f),
                () => { session.Request(CQueueOperation.Enqueue); redraw(); }, TLSUiFactory.Deep, 17, session.CanStart);
            TLSUiFactory.MakeButton(controls, "DEQUEUE", V(0.505f, 0.40f), V(0.735f, 0.69f),
                () => { session.Request(CQueueOperation.Dequeue); redraw(); }, TLSUiFactory.Deep, 17, session.CanStart);
            TLSUiFactory.MakeButton(controls, "PEEK FRONT", V(0.755f, 0.40f), V(0.98f, 0.69f),
                () => { session.Request(CQueueOperation.Peek); redraw(); }, TLSUiFactory.Deep, 17, session.CanStart);
            StepButtons(controls, () => { session.Previous(); redraw(); }, () => { session.Next(); redraw(); },
                () => { session.Finish(); redraw(); }, () => { session.Reset(); redraw(); },
                session.Trace != null && session.FrameIndex > 0,
                session.Trace != null && session.FrameIndex < session.Trace.Frames.Count - 1,
                session.Trace != null, "RESET QUEUE");
            if (session.PredictionPending)
                PredictionPrompt(root, "QUEUE " + session.PendingLabel + " | " + session.Current.Count + "/" + CQueueState.Capacity,
                    change => { session.ChoosePrediction(change); redraw(); },
                    () => { session.CancelPrediction(); redraw(); });
        }

        private static string VisibleArrayOutput(CArrayLabSession session)
        {
            CArrayFrame frame = session.VisibleFrame;
            if (frame == null || session.Trace == null || !session.Trace.Succeeded || !session.Trace.Output.HasValue)
                return "Output: not yet available";
            if (session.Trace.Operation == CArrayOperation.Get || session.Trace.Operation == CArrayOperation.Remove)
                return frame.CodeLine >= 3 ? "out = " + session.Trace.Output.Value : "Output not read yet";
            return "Output: not used";
        }
        private static string VisibleQueueOutput(CQueueLabSession session)
        {
            CQueueFrame frame = session.VisibleFrame;
            if (frame == null || session.Trace == null || !session.Trace.Succeeded || !session.Trace.Output.HasValue)
                return "Output: not yet available";
            return frame.CodeLine >= 3 ? "out = " + session.Trace.Output.Value : "Output not read yet";
        }
        private static void Background(Transform root)
        {
            TLSUiFactory.Panel(root, "Linear labs backdrop", V(0, 0), V(1, 0.907f), TLSUiFactory.Navy);
        }
        private static RectTransform Inspector(Transform root, string title)
        {
            RectTransform panel = TLSUiFactory.Panel(root, "Learning inspector", V(0.773f, 0.266f), V(0.982f, 0.889f), TLSUiFactory.Slate);
            TLSUiFactory.Label(panel, "Inspector title", title, V(0.045f, 0.902f), V(0.966f, 0.985f),
                18, TLSUiFactory.Cyan, TextAnchor.MiddleLeft, true);
            return panel;
        }
        private static void DrawCode(Transform root, string title, string[] code, int highlighted, string source)
        {
            RectTransform panel = TLSUiFactory.Panel(root, "Linear C code", V(0.018f, 0.266f), V(0.419f, 0.889f), TLSUiFactory.Slate);
            TLSUiFactory.Label(panel, "Title", title, V(0.045f, 0.898f), V(0.97f, 0.985f),
                20, TLSUiFactory.Cyan, TextAnchor.MiddleLeft, true);
            for (int i = 0; i < code.Length; i++)
            {
                float top = 0.858f - i * 0.075f;
                float bottom = top - 0.070f;
                if (highlighted == i)
                {
                    TLSUiFactory.Panel(panel, "Active line " + i, V(0.025f, bottom), V(0.985f, top), TLSUiFactory.Deep);
                    TLSUiFactory.Panel(panel, "Line focus marker " + i, V(0.025f, bottom), V(0.033f, top), TLSUiFactory.Cyan);
                }
                Text line = TLSUiFactory.Label(panel, "C statement " + i,
                    (i + 1) + "  " + CCodeDisplay.HighlightTokens(code[i]), V(0.045f, bottom), V(0.978f, top),
                    16, TLSUiFactory.Pale);
                line.supportRichText = true;
                line.resizeTextForBestFit = true;
                line.resizeTextMinSize = 13;
                line.resizeTextMaxSize = 17;
            }
            TLSUiFactory.Label(panel, "Source notice", source,
                V(0.045f, 0.061f), V(0.97f, 0.159f), 15, TLSUiFactory.Muted);
            TLSUiFactory.Label(panel, "Model notice", "Not executing arbitrary student C; use the existing Stack WASM lab for compiler-backed stack practice.",
                V(0.045f, 0.003f), V(0.975f, 0.065f), 13, TLSUiFactory.Amber);
        }
        private static RectTransform Controls(Transform root, string explanation, string prediction, int correct, int total)
        {
            RectTransform panel = TLSUiFactory.Panel(root, "Linear labs controls", V(0.018f, 0.023f), V(0.982f, 0.254f), TLSUiFactory.Slate);
            TLSUiFactory.Label(panel, "Trace explanation", explanation,
                V(0.024f, 0.795f), V(0.75f, 0.986f), 17, TLSUiFactory.Pale);
            TLSUiFactory.Label(panel, "Prediction results", "PREDICT " + correct + "/" + total + " | " + prediction,
                V(0.765f, 0.792f), V(0.983f, 0.98f), 13, TLSUiFactory.Muted);
            return panel;
        }
        private static void StepButtons(RectTransform panel, Action prev, Action next, Action finish, Action reset,
            bool canPrev, bool canNext, bool canFinish, string resetLabel)
        {
            TLSUiFactory.MakeButton(panel, "< STEP", V(0.02f, 0.08f), V(0.258f, 0.34f),
                prev, TLSUiFactory.Deep, 16, canPrev);
            TLSUiFactory.MakeButton(panel, "STEP >", V(0.268f, 0.08f), V(0.506f, 0.34f),
                next, TLSUiFactory.Deep, 16, canNext);
            TLSUiFactory.MakeButton(panel, "FINISH", V(0.516f, 0.08f), V(0.754f, 0.34f),
                finish, TLSUiFactory.Deep, 16, canFinish);
            TLSUiFactory.MakeButton(panel, resetLabel, V(0.764f, 0.08f), V(0.98f, 0.34f),
                reset, TLSUiFactory.Deep, 16);
        }
        private static void PredictionPrompt(Transform root, string current, Action<int?> choose, Action cancel)
        {
            TLSUiFactory.Panel(root, "Linear prediction scrim", V(0, 0), V(1, 0.908f),
                new Color(0.02f, 0.04f, 0.08f, 0.91f));
            RectTransform sheet = TLSUiFactory.Panel(root, "Linear prediction question", V(0.285f, 0.13f), V(0.725f, 0.845f), TLSUiFactory.Slate);
            TLSUiFactory.Label(sheet, "Heading", "PREDICT BEFORE EXECUTING", V(0.055f, 0.85f), V(0.97f, 0.96f),
                26, TLSUiFactory.Cyan, TextAnchor.MiddleLeft, true);
            TLSUiFactory.Label(sheet, "Current", current + "  |  Focus on whether COUNT changes.",
                V(0.055f, 0.733f), V(0.97f, 0.837f), 19, TLSUiFactory.Pale);
            TLSUiFactory.Label(sheet, "Prompt", "Will the number of logical elements increase, decrease, or stay the same?",
                V(0.055f, 0.603f), V(0.96f, 0.727f), 20, TLSUiFactory.Pale);
            TLSUiFactory.MakeButton(sheet, "INCREASE BY 1", V(0.07f, 0.502f), V(0.93f, 0.598f),
                () => choose(1), TLSUiFactory.Deep, 19);
            TLSUiFactory.MakeButton(sheet, "DECREASE BY 1", V(0.07f, 0.386f), V(0.93f, 0.482f),
                () => choose(-1), TLSUiFactory.Deep, 19);
            TLSUiFactory.MakeButton(sheet, "STAY THE SAME", V(0.07f, 0.27f), V(0.93f, 0.366f),
                () => choose(0), TLSUiFactory.Deep, 19);
            TLSUiFactory.Label(sheet, "Assistance scope", "Formative prediction only. No independent C coding mastery or progress-file writes.",
                V(0.07f, 0.157f), V(0.93f, 0.263f), 16, TLSUiFactory.Muted);
            TLSUiFactory.MakeButton(sheet, "SKIP", V(0.07f, 0.038f), V(0.48f, 0.14f),
                () => choose(null), TLSUiFactory.Deep, 18);
            TLSUiFactory.MakeButton(sheet, "CANCEL", V(0.52f, 0.038f), V(0.93f, 0.14f),
                cancel, TLSUiFactory.Deep, 18);
        }
    }
}
