using System;
using TLS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TLS.UI
{
    /// <summary>Phase 2.2 structured C11 exercises: never compiles student code or writes saved progress.</summary>
    public static class TLSLinearPracticeScreen
    {
        private static Vector2 V(float x, float y) { return new Vector2(x, y); }
        private static string TrackName(LinearPracticeTrack t)
        {
            return t == LinearPracticeTrack.Trace ? "1  TRACE C" :
                t == LinearPracticeTrack.Complete ? "2  COMPLETE C" : "3  REPAIR C";
        }
        public static void Render(Transform root, LinearPracticeSession state, Action returnToLab, Action redraw)
        {
            if (root == null || state == null || returnToLab == null || redraw == null)
                throw new ArgumentNullException("Linear C Practice needs a root, session and callbacks.");
            LinearPracticeTask task = state.Current;
            TLSUiFactory.Panel(root, "Linear C practice background", V(0, 0), V(1, 0.905f), TLSUiFactory.Navy);

            RectTransform selector = TLSUiFactory.Panel(root, "Linear practice tracks", V(0.022f, 0.785f), V(0.978f, 0.885f), TLSUiFactory.Slate);
            for (int i = 0; i < 3; i++)
            {
                LinearPracticeTrack selected = (LinearPracticeTrack)i;
                float left = 0.014f + 0.332f * i;
                TLSUiFactory.MakeButton(selector, TrackName(selected), V(left, 0.14f), V(left + 0.322f, 0.86f),
                    () => { state.SelectTrack(selected); redraw(); },
                    state.Track == selected ? TLSUiFactory.Deep : TLSUiFactory.Navy, 19);
            }
            RectTransform work = TLSUiFactory.Panel(root, "Linear C exercise", V(0.022f, 0.199f), V(0.612f, 0.773f), TLSUiFactory.Slate);
            TLSUiFactory.Label(work, "Exercise title", task.Title, V(0.035f, 0.9f), V(0.975f, 0.984f),
                24, TLSUiFactory.Cyan, TextAnchor.MiddleLeft, true);
            TLSUiFactory.Label(work, "Exercise objective", task.Objective, V(0.035f, 0.826f), V(0.97f, 0.901f),
                16, TLSUiFactory.Muted);
            TLSUiFactory.Panel(work, "C source background", V(0.034f, 0.297f), V(0.973f, 0.817f), TLSUiFactory.Deep);
            Text listing = TLSUiFactory.Label(work, "C exercise source", task.Source,
                V(0.057f, 0.309f), V(0.955f, 0.806f), task.Source.Length > 360 ? 15 : task.Source.Length > 250 ? 17 : 19,
                TLSUiFactory.Pale, TextAnchor.MiddleLeft);
            listing.resizeTextForBestFit = true;
            listing.resizeTextMinSize = 13;
            listing.resizeTextMaxSize = task.Source.Length > 360 ? 15 : 19;
            TLSUiFactory.Label(work, "C exercise question", task.Question,
                V(0.039f, 0.197f), V(0.969f, 0.29f), 18, TLSUiFactory.Pale, TextAnchor.MiddleLeft, true);
            TLSUiFactory.Label(work, "Answer type", task.IntegerAnswer ? "YOUR INTEGER ANSWER" : "YOUR ONE-LINE C ANSWER",
                V(0.043f, 0.137f), V(0.96f, 0.199f), 16, TLSUiFactory.Violet, TextAnchor.MiddleLeft, true);
            InputField answer = TLSUiFactory.Field(work, "Linear C answer", state.Draft,
                task.IntegerAnswer ? "integer" : "C statement (reference-token match)",
                V(0.04f, 0.019f), V(0.713f, 0.134f), s => state.Draft = s);
            answer.characterLimit = task.IntegerAnswer ? 16 : 512;
            TLSUiFactory.MakeButton(work, "CHECK", V(0.735f, 0.019f), V(0.964f, 0.134f),
                () => { state.Submit(); redraw(); }, TLSUiFactory.Deep, 19);

            RectTransform why = TLSUiFactory.Panel(root, "Linear practice feedback", V(0.622f, 0.199f), V(0.978f, 0.773f), TLSUiFactory.Slate);
            TLSUiFactory.Label(why, "Feedback heading", "LEARNING / REASONING", V(0.05f, 0.9f), V(0.96f, 0.981f),
                21, TLSUiFactory.Cyan, TextAnchor.MiddleLeft, true);
            TLSUiFactory.Label(why, "Task number", (state.Domain == LinearPracticeDomain.Array ? "ARRAY" : "QUEUE") +
                "  |  Exercise " + (state.Index + 1) + "/" + LinearPracticeCatalog.TasksPerTrack + "  |  " + state.Track.ToString().ToUpperInvariant(),
                V(0.05f, 0.828f), V(0.96f, 0.899f), 16, TLSUiFactory.Pale);
            TLSUiFactory.Label(why, "Assistance label", "ASSISTANCE: " + state.Help.ToString().ToUpperInvariant(),
                V(0.05f, 0.756f), V(0.96f, 0.823f), 17, TLSUiFactory.Violet, TextAnchor.MiddleLeft, true);
            TLSUiFactory.MakeButton(why, "GUIDED", V(0.046f, 0.676f), V(0.492f, 0.755f),
                () => { state.SetHelp(LinearPracticeHelp.Guided); redraw(); },
                state.Help == LinearPracticeHelp.Guided ? TLSUiFactory.Deep : TLSUiFactory.Navy, 16);
            TLSUiFactory.MakeButton(why, "INDEPENDENT", V(0.509f, 0.676f), V(0.953f, 0.755f),
                () => { state.SetHelp(LinearPracticeHelp.Independent); redraw(); },
                state.Help == LinearPracticeHelp.Independent ? TLSUiFactory.Deep : TLSUiFactory.Navy, 16);
            TLSUiFactory.MakeButton(why, "SHOW HINT", V(0.046f, 0.580f), V(0.492f, 0.659f),
                () => { state.ShowHint(); redraw(); }, TLSUiFactory.Deep, 16,
                state.Help == LinearPracticeHelp.Guided);
            TLSUiFactory.MakeButton(why, "REVEAL / REVIEW", V(0.509f, 0.580f), V(0.953f, 0.659f),
                () => { state.Reveal(); redraw(); }, TLSUiFactory.Deep, 16);
            TLSUiFactory.Label(why, "Hint content", state.HintVisible ? "HINT: " + task.Hint :
                (state.Help == LinearPracticeHelp.Guided ?
                "SHOW HINT offers help but forfeits unseen first-try credit." :
                "Hints disabled. Revealing a reference also forfeits unseen first-try credit."),
                V(0.053f, 0.434f), V(0.958f, 0.572f), 16, TLSUiFactory.Muted);
            TLSUiFactory.Label(why, "Answer status", state.JustCorrect ? "RESULT: CORRECT" : "FEEDBACK",
                V(0.053f, 0.363f), V(0.958f, 0.428f), 17,
                state.JustCorrect ? TLSUiFactory.Green : TLSUiFactory.Violet, TextAnchor.MiddleLeft, true);
            TLSUiFactory.Label(why, "Answer feedback", state.Feedback, V(0.053f, 0.13f), V(0.958f, 0.361f),
                16, TLSUiFactory.Pale);
            TLSUiFactory.Label(why, "Reference answer", state.ReferenceVisible ? "REFERENCE: " + task.Reference :
                "Reference hidden until correct or revealed.",
                V(0.053f, 0.015f), V(0.958f, 0.126f), 15,
                state.ReferenceVisible ? TLSUiFactory.Amber : TLSUiFactory.Muted);

            RectTransform footer = TLSUiFactory.Panel(root, "Linear C practice footer", V(0.022f, 0.024f), V(0.978f, 0.188f), TLSUiFactory.Deep);
            TLSUiFactory.Label(footer, "Session progress", "THIS SESSION: " + state.CompletedCount + "/" + LinearPracticeCatalog.TotalPerDomain +
                " correct  |  " + state.IndependentFirstTryCount + "/" + LinearPracticeCatalog.TotalPerDomain + " unseen independent first attempts",
                V(0.023f, 0.65f), V(0.98f, 0.949f), 17, TLSUiFactory.Pale);
            TLSUiFactory.Label(footer, "Assessment scope",
                "C11 tracing and one-reference-token answers only; NOT actual code compilation, semantic equivalence, save progress, or independent C-function mastery.",
                V(0.023f, 0.355f), V(0.981f, 0.647f), 15, TLSUiFactory.Muted);
            TLSUiFactory.MakeButton(footer, "< PREVIOUS", V(0.018f, 0.032f), V(0.226f, 0.329f),
                () => { state.SelectIndex(state.Index - 1); redraw(); }, TLSUiFactory.Slate, 15, state.Index > 0);
            TLSUiFactory.MakeButton(footer, "NEXT >", V(0.240f, 0.032f), V(0.431f, 0.329f),
                () => { state.SelectIndex(state.Index + 1); redraw(); }, TLSUiFactory.Slate, 15,
                state.Index < LinearPracticeCatalog.TasksPerTrack - 1);
            TLSUiFactory.MakeButton(footer, "RETURN TO " + (state.Domain == LinearPracticeDomain.Array ? "ARRAY" : "QUEUE") + " LAB",
                V(0.73f, 0.032f), V(0.983f, 0.329f), returnToLab, TLSUiFactory.Slate, 15);
        }
    }
}
