using System;
using UnityEngine;
using UnityEngine.UI;

namespace TLS.UI
{
    /// <summary>
    /// Small always-accessible, two-click exit control. UI only: no changes to
    /// learner evidence, C editor drafts, WASM compilation or persistent saves.
    /// Placed between the header title and global Aa options on every screen.
    /// </summary>
    public static class TLSPhase8ExitMenu
    {
        public const string ConfirmationName = "Phase8 Exit Confirmation";
        public const string ExitCaption = "EXIT APP";
        public const string CancelCaption = "KEEP WORKING";
        public const string ConfirmCaption = "EXIT APPLICATION";

        public static readonly Vector2 ExitAnchorMin = new Vector2(.565f, .928f);
        public static readonly Vector2 ExitAnchorMax = new Vector2(.665f, .978f);

        private static Vector2 V(float x, float y) { return new Vector2(x, y); }

        public static void Draw(Transform root, Action quit)
        {
            if (root == null) throw new ArgumentNullException("root");
            if (quit == null) throw new ArgumentNullException("quit");
            TLSUiFactory.MakeButton(root, ExitCaption, ExitAnchorMin, ExitAnchorMax,
                () => Confirm(root, quit), TLSUiFactory.Slate, 13);
        }

        public static void Confirm(Transform root, Action quit)
        {
            if (root == null) throw new ArgumentNullException("root");
            if (quit == null) throw new ArgumentNullException("quit");
            // Clicking the button repeatedly cannot stack multiple blocking dialogs.
            if (root.Find(ConfirmationName) != null) return;
            RectTransform shade = TLSUiFactory.Panel(root, ConfirmationName,
                V(0f, 0f), V(1f, 1f), new Color(0f, .018f, .037f, .82f));
            Image hitBlocker = shade.GetComponent<Image>();
            hitBlocker.raycastTarget = true;
            RectTransform dialog = TLSUiFactory.Panel(shade, "Exit dialog",
                V(.305f, .335f), V(.695f, .665f), TLSUiFactory.Slate);
            TLSUiFactory.Label(dialog, "Exit dialog heading", "EXIT THE LIVING STRUCTURES?",
                V(.06f, .74f), V(.94f, .94f), 20, TLSUiFactory.Pale, TextAnchor.MiddleCenter, true);
            TLSUiFactory.Label(dialog, "Exit warning",
                "Unsaved in-session work may be lost. Previously saved progress is not reset.",
                V(.075f, .40f), V(.925f, .70f), 16, TLSUiFactory.Muted,
                TextAnchor.MiddleCenter);
            TLSUiFactory.MakeButton(dialog, CancelCaption, V(.065f, .095f), V(.485f, .33f),
                () => {
                    // EditMode tests do not run a player loop, so Destroy() is not
                    // permitted there. In Play Mode and standalone builds, defer
                    // destruction normally after disabling the blocking overlay.
                    GameObject shadeObject = shade.gameObject;
                    shadeObject.SetActive(false);
                    if (Application.isPlaying)
                        UnityEngine.Object.Destroy(shadeObject);
                    else
                        UnityEngine.Object.DestroyImmediate(shadeObject);
                }, TLSUiFactory.Deep, 15);
            TLSUiFactory.MakeButton(dialog,
                Application.isEditor ? "STOP PLAY MODE" : ConfirmCaption,
                V(.515f, .095f), V(.935f, .33f), quit, TLSUiFactory.Violet, 15);
            // Newly opened dialogs also respect the user's Phase 8 font/contrast preferences.
            TLSPhase8Accessibility.Apply(shade);
        }
    }
}
