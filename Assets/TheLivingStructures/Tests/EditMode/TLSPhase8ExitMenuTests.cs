using NUnit.Framework;
using TLS.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TLS.Tests
{
    public sealed class TLSPhase8ExitMenuTests
    {
        [Test]
        public void ExitButtonIsNextToAccessibilityWithoutOverlappingHeaderTitle()
        {
            Assert.GreaterOrEqual(TLSPhase8ExitMenu.ExitAnchorMin.x, .56f);
            Assert.LessOrEqual(TLSPhase8ExitMenu.ExitAnchorMax.x, .671f);
            Assert.GreaterOrEqual(TLSPhase8ExitMenu.ExitAnchorMin.y, .928f);
            Assert.LessOrEqual(TLSPhase8ExitMenu.ExitAnchorMax.y, .978f);
        }

        [Test]
        public void ExitButtonRendersOnAnyScreenRoot()
        {
            var root = NewRoot();
            try
            {
                TLSPhase8ExitMenu.Draw(root.transform, () => {});
                var button = root.transform.Find("Button_" + TLSPhase8ExitMenu.ExitCaption);
                Assert.IsNotNull(button);
                Assert.IsNotNull(button.GetComponent<Button>());
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void FirstClickOpensConfirmationWithoutExiting()
        {
            var root = NewRoot();
            bool quit = false;
            try
            {
                TLSPhase8ExitMenu.Draw(root.transform, () => quit = true);
                root.transform.Find("Button_" + TLSPhase8ExitMenu.ExitCaption)
                    .GetComponent<Button>().onClick.Invoke();
                Assert.IsFalse(quit);
                var modal = root.transform.Find(TLSPhase8ExitMenu.ConfirmationName);
                Assert.IsNotNull(modal);
                Assert.IsTrue(modal.gameObject.activeSelf);
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void CancelClosesDialogWithoutExiting()
        {
            var root = NewRoot();
            bool quit = false;
            try
            {
                TLSPhase8ExitMenu.Draw(root.transform, () => quit = true);
                TLSPhase8ExitMenu.Confirm(root.transform, () => quit = true);
                Transform modal = root.transform.Find(TLSPhase8ExitMenu.ConfirmationName);
                Transform cancel = modal.Find("Exit dialog/Button_" + TLSPhase8ExitMenu.CancelCaption);
                Assert.IsNotNull(cancel);
                cancel.GetComponent<Button>().onClick.Invoke();
                Assert.IsFalse(quit);
                // In EditMode the dialog is destroyed immediately, so never
                // dereference its old Transform after clicking KEEP WORKING.
                Assert.IsNull(root.transform.Find(TLSPhase8ExitMenu.ConfirmationName));
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void ExplicitConfirmationInvokesQuitExactlyOnce()
        {
            var root = NewRoot();
            int quits = 0;
            try
            {
                TLSPhase8ExitMenu.Confirm(root.transform, () => quits++);
                Transform dialog = root.transform.Find(TLSPhase8ExitMenu.ConfirmationName).Find("Exit dialog");
                var confirm = dialog.Find("Button_" +
                    (Application.isEditor ? "STOP PLAY MODE" : TLSPhase8ExitMenu.ConfirmCaption));
                Assert.IsNotNull(confirm);
                Assert.AreEqual(0, quits);
                confirm.GetComponent<Button>().onClick.Invoke();
                Assert.AreEqual(1, quits);
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void RepeatedOpenDoesNotCreateStackedDialogs()
        {
            var root = NewRoot();
            try
            {
                TLSPhase8ExitMenu.Confirm(root.transform, () => {});
                TLSPhase8ExitMenu.Confirm(root.transform, () => {});
                int count = 0;
                foreach (Transform child in root.transform)
                    if (child.name == TLSPhase8ExitMenu.ConfirmationName) count++;
                Assert.AreEqual(1, count);
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void DialogBlocksUnderlyingInteractions()
        {
            var root = NewRoot();
            try
            {
                TLSPhase8ExitMenu.Confirm(root.transform, () => {});
                var modal = root.transform.Find(TLSPhase8ExitMenu.ConfirmationName);
                Assert.IsTrue(modal.GetComponent<Image>().raycastTarget);
                var rect = modal.GetComponent<RectTransform>();
                Assert.AreEqual(Vector2.zero, rect.anchorMin);
                Assert.AreEqual(Vector2.one, rect.anchorMax);
            }
            finally { Object.DestroyImmediate(root); }
        }

        private static GameObject NewRoot()
        {
            return new GameObject("Exit test root", typeof(RectTransform));
        }
    }
}
