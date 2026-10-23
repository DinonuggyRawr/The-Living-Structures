using NUnit.Framework;
using TLS.Core;
using TLS.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TLS.Tests
{
    public sealed class TLSLinearCodingLayoutTests
    {
        [Test] public void NewPracticeScreenIsIndependentOfMonoBehaviour()
        {
            Assert.IsFalse(typeof(MonoBehaviour).IsAssignableFrom(typeof(LinearPracticeSession)));
            Assert.IsNotNull(typeof(TLSLinearPracticeScreen).GetMethod("Render"));
        }
        [Test] public void BothLegacyVisualLabsAndStackCompilerRemainPresent()
        {
            Assert.IsNotNull(typeof(TLSLinearLabsScreen).GetMethod("RenderArray"));
            Assert.IsNotNull(typeof(TLSLinearLabsScreen).GetMethod("RenderQueue"));
            Assert.IsNotNull(typeof(TLSWasmCompilerLabScreen).GetMethod("Render"));
            Assert.IsNotNull(typeof(TLSCodePracticeScreen).GetMethod("Render"));
        }
        [TestCase(LinearPracticeDomain.Array)] [TestCase(LinearPracticeDomain.Queue)]
        public void NewPracticeScreenRendersAllActions(LinearPracticeDomain domain)
        {
            var go = new GameObject("Exercise root", typeof(RectTransform));
            try
            {
                TLSLinearPracticeScreen.Render(go.transform, new LinearPracticeSession(domain), () => { }, () => { });
                Assert.IsNotNull(DirectName(go.transform, "Button_CHECK"));
                Assert.IsNotNull(DirectName(go.transform, "Button_GUIDED"));
                Assert.IsNotNull(DirectName(go.transform, "Button_INDEPENDENT"));
                Assert.IsNotNull(DirectName(go.transform, "Button_SHOW HINT"));
                Assert.IsNotNull(DirectName(go.transform, "Button_REVEAL / REVIEW"));
                Assert.IsNotNull(DirectName(go.transform, "Button_RETURN TO " + (domain == LinearPracticeDomain.Array ? "ARRAY" : "QUEUE") + " LAB"));
                Assert.IsNotNull(DirectName(go.transform, "Linear C answer"));
            }
            finally { Object.DestroyImmediate(go); }
        }
        private static Transform DirectName(Transform root, string name)
        {
            // Recursive name comparison: Transform.Find treats '/' as a path separator.
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = DirectName(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
