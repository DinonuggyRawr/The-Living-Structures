using System;
using TLS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TLS.UI
{
    // A deliberately small structured C-practice workspace; not an embedded editor/compiler.
    // All task answers and attempt state are session-only and never alter StudyProgress.
    public static class TLSCodePracticeScreen
    {
        private static Vector2 V(float x, float y) { return new Vector2(x, y); }

        private static string TrackName(StackPracticeTrack track)
        {
            if (track == StackPracticeTrack.Trace) return "1  TRACE C";
            if (track == StackPracticeTrack.Complete) return "2  COMPLETE C";
            return "3  REPAIR C";
        }

        public static void Render(Transform root, StackPracticeSession state, Action returnToLab, Action refresh)
        {
            if (root == null || state == null || returnToLab == null || refresh == null)
                throw new ArgumentNullException("Practice screen requires a parent, session, and navigation callbacks.");
            StackPracticeTask task = state.Current;
            TLSUiFactory.Panel(root, "Coding practice background", V(0, 0), V(1, 0.906f), TLSUiFactory.Navy);

            RectTransform selector = TLSUiFactory.Panel(root, "Practice tracks", V(0.022f, 0.79f), V(0.978f, 0.885f), TLSUiFactory.Slate);
            for (int i = 0; i < 3; i++)
            {
                StackPracticeTrack selected = (StackPracticeTrack)i;
                float left = 0.014f + i * 0.332f;
                TLSUiFactory.MakeButton(selector, TrackName(selected), V(left, 0.15f), V(left + 0.322f, 0.86f),
                    () => { state.SelectTrack(selected); refresh(); },
                    state.Track == selected ? TLSUiFactory.Deep : TLSUiFactory.Navy, 19);
            }

            RectTransform work = TLSUiFactory.Panel(root, "C reading and practice", V(0.022f, 0.196f), V(0.608f, 0.78f), TLSUiFactory.Slate);
            TLSUiFactory.Label(work, "Exercise name", task.Title, V(0.035f, 0.902f), V(0.975f, 0.989f),
                25, TLSUiFactory.Cyan, TextAnchor.MiddleLeft, true);
            TLSUiFactory.Label(work, "Objective", task.Objective, V(0.035f, 0.834f), V(0.972f, 0.902f),
                16, TLSUiFactory.Muted);
            TLSUiFactory.Panel(work, "Source code backdrop", V(0.034f, 0.316f), V(0.974f, 0.824f), TLSUiFactory.Deep);
            TLSUiFactory.Label(work, "C source listing", task.ContextCode,
                V(0.056f, 0.331f), V(0.953f, 0.809f), task.ContextCode.Length > 250 ? 18 : 20,
                TLSUiFactory.Pale, TextAnchor.MiddleLeft);
            TLSUiFactory.Label(work, "Problem", task.Question, V(0.039f, 0.215f), V(0.967f, 0.31f),
                19, TLSUiFactory.Pale, TextAnchor.MiddleLeft, true);
            TLSUiFactory.Label(work, "Input label", task.ExpectsInteger ? "YOUR INTEGER ANSWER" : "YOUR ONE-LINE C STATEMENT",
                V(0.043f, 0.152f), V(0.96f, 0.217f), 16, TLSUiFactory.Violet, TextAnchor.MiddleLeft, true);
            TLSUiFactory.Field(work, "Student C answer", state.Draft,
                task.ExpectsInteger ? "integer" : "C statement (reference-match exercise)",
                V(0.04f, 0.027f), V(0.71f, 0.149f), s => state.Draft = s);
            TLSUiFactory.MakeButton(work, "CHECK", V(0.733f, 0.027f), V(0.966f, 0.149f),
                () => { state.Submit(); refresh(); }, TLSUiFactory.Deep, 19);

            RectTransform sidebar = TLSUiFactory.Panel(root, "Learning assistance", V(0.618f, 0.196f), V(0.978f, 0.78f), TLSUiFactory.Slate);
            TLSUiFactory.Label(sidebar, "Module", "PRACTICE / REASONING", V(0.05f, 0.902f), V(0.96f, 0.985f),
                21, TLSUiFactory.Cyan, TextAnchor.MiddleLeft, true);
            TLSUiFactory.Label(sidebar, "Number", "Exercise " + (state.Index + 1) + " of " + StackPracticeCatalog.TasksPerTrack +
                "   |   " + state.Track.ToString().ToUpperInvariant(), V(0.05f, 0.84f), V(0.96f, 0.907f),
                16, TLSUiFactory.Pale);
            TLSUiFactory.Label(sidebar, "Mode info", "ASSISTANCE: " + state.Assistance.ToString().ToUpperInvariant(),
                V(0.05f, 0.776f), V(0.96f, 0.839f), 17, TLSUiFactory.Violet, TextAnchor.MiddleLeft, true);
            TLSUiFactory.MakeButton(sidebar, "GUIDED", V(0.046f, 0.691f), V(0.491f, 0.774f),
                () => { state.SetAssistance(StackPracticeAssistance.Guided); refresh(); },
                state.Assistance == StackPracticeAssistance.Guided ? TLSUiFactory.Deep : TLSUiFactory.Navy, 16);
            TLSUiFactory.MakeButton(sidebar, "INDEPENDENT", V(0.509f, 0.691f), V(0.952f, 0.774f),
                () => { state.SetAssistance(StackPracticeAssistance.Independent); refresh(); },
                state.Assistance == StackPracticeAssistance.Independent ? TLSUiFactory.Deep : TLSUiFactory.Navy, 16);
            TLSUiFactory.MakeButton(sidebar, "SHOW HINT", V(0.046f, 0.586f), V(0.491f, 0.672f),
                () => { state.ShowHint(); refresh(); }, TLSUiFactory.Deep, 16,
                state.Assistance == StackPracticeAssistance.Guided);
            TLSUiFactory.MakeButton(sidebar, "REVEAL / REVIEW", V(0.509f, 0.586f), V(0.952f, 0.672f),
                () => { state.RevealSolution(); refresh(); }, TLSUiFactory.Deep, 16);
            TLSUiFactory.Label(sidebar, "Hints and guidance", state.HintVisible ? "HINT: " + task.Hint :
                (state.Assistance == StackPracticeAssistance.Guided ?
                "Use SHOW HINT for help. Hints prevent an unseen first-attempt result for this exercise." :
                "Hints are disabled. REVEAL / REVIEW remains available, but forfeits unseen-first-attempt status."),
                V(0.055f, 0.434f), V(0.955f, 0.583f), 16, TLSUiFactory.Muted);
            TLSUiFactory.Label(sidebar, "Feedback heading", state.JustCorrect ? "RESULT: CORRECT" : "FEEDBACK",
                V(0.055f, 0.361f), V(0.955f, 0.436f), 17,
                state.JustCorrect ? TLSUiFactory.Green : TLSUiFactory.Violet, TextAnchor.MiddleLeft, true);
            TLSUiFactory.Label(sidebar, "Detailed feedback", state.Feedback, V(0.055f, 0.126f), V(0.955f, 0.36f),
                16, TLSUiFactory.Pale);
            TLSUiFactory.Label(sidebar, "Reference", state.SolutionVisible ?
                "REFERENCE: " + task.ReferenceAnswer : "Reference answer hidden until checked correctly or revealed.",
                V(0.055f, 0.018f), V(0.955f, 0.12f), 16,
                state.SolutionVisible ? TLSUiFactory.Amber : TLSUiFactory.Muted);

            RectTransform footer = TLSUiFactory.Panel(root, "Code practice footer", V(0.022f, 0.025f), V(0.978f, 0.185f), TLSUiFactory.Deep);
            TLSUiFactory.Label(footer, "Progress", "THIS SESSION: " + state.CompletedCount + "/" + StackPracticeCatalog.Total +
                " structured answers correct  |  " + state.IndependentFirstTryCount + "/" + StackPracticeCatalog.Total +
                " unseen first-attempt answers", V(0.024f, 0.635f), V(0.98f, 0.94f), 17,
                TLSUiFactory.Pale);
            TLSUiFactory.Label(footer, "Limits", "One reference C statement / integer per question; no compilation, semantic-equivalence checking, save writes, or C implementation mastery.",
                V(0.024f, 0.36f), V(0.982f, 0.635f), 15, TLSUiFactory.Muted);
            TLSUiFactory.MakeButton(footer, "< PREVIOUS", V(0.018f, 0.04f), V(0.226f, 0.34f),
                () => { state.SelectIndex(state.Index - 1); refresh(); }, TLSUiFactory.Slate, 15,
                state.Index > 0);
            TLSUiFactory.MakeButton(footer, "NEXT >", V(0.24f, 0.04f), V(0.431f, 0.34f),
                () => { state.SelectIndex(state.Index + 1); refresh(); }, TLSUiFactory.Slate, 15,
                state.Index < StackPracticeCatalog.TasksPerTrack - 1);
            TLSUiFactory.MakeButton(footer, "RETURN TO STACK LAB", V(0.725f, 0.04f), V(0.979f, 0.34f),
                returnToLab, TLSUiFactory.Slate, 15);
        }
    }
}
