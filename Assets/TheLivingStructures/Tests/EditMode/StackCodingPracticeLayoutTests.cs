using NUnit.Framework;
using TLS.Core;
using TLS.UI;
using UnityEngine;

namespace TLS.Tests
{
    public class StackCodingPracticeLayoutTests
    {
        private GameObject host;
        private StackPracticeSession session;

        [SetUp]
        public void SetUp()
        {
            host = new GameObject("Practice test canvas root", typeof(RectTransform));
            session = new StackPracticeSession();
        }

        [TearDown]
        public void TearDown()
        {
            if (host != null) Object.DestroyImmediate(host);
        }

        private void Build()
        {
            TLSCodePracticeScreen.Render(host.transform, session, () => {}, () => {});
        }

        private RectTransform Section(string section)
        {
            Transform node = host.transform.Find(section);
            Assert.IsNotNull(node, "Expected section: " + section);
            return node as RectTransform;
        }

        [Test]
        public void RendersAllThreeTrackButtons()
        {
            Build();
            RectTransform tracks = Section("Practice tracks");
            Assert.IsNotNull(tracks.Find("Button_1  TRACE C"));
            Assert.IsNotNull(tracks.Find("Button_2  COMPLETE C"));
            Assert.IsNotNull(tracks.Find("Button_3  REPAIR C"));
        }

        [Test]
        public void RendersAnswerFieldAndCheckAction()
        {
            Build();
            RectTransform work = Section("C reading and practice");
            Assert.IsNotNull(work.Find("Student C answer"));
            Assert.IsNotNull(work.Find("Button_CHECK"));
        }

        [Test]
        public void RendersReturnToLabNavigation()
        {
            Build();
            Assert.IsNotNull(Section("Code practice footer").Find("Button_RETURN TO STACK LAB"));
        }

        [Test]
        public void TrackButtonsDoNotOverlapInAnchorSpace()
        {
            Build();
            RectTransform tracks = Section("Practice tracks");
            RectTransform first = tracks.Find("Button_1  TRACE C") as RectTransform;
            RectTransform second = tracks.Find("Button_2  COMPLETE C") as RectTransform;
            RectTransform third = tracks.Find("Button_3  REPAIR C") as RectTransform;
            Assert.LessOrEqual(first.anchorMax.x, second.anchorMin.x);
            Assert.LessOrEqual(second.anchorMax.x, third.anchorMin.x);
            Assert.LessOrEqual(third.anchorMax.x, 1f);
        }

        [Test]
        public void MainPanelsDoNotOverlapInAnchorSpace()
        {
            Build();
            RectTransform work = Section("C reading and practice");
            RectTransform assistance = Section("Learning assistance");
            Assert.LessOrEqual(work.anchorMax.x, assistance.anchorMin.x);
            Assert.GreaterOrEqual(work.anchorMin.y, Section("Code practice footer").anchorMax.y);
            Assert.LessOrEqual(work.anchorMax.y, Section("Practice tracks").anchorMin.y);
        }

        [Test]
        public void RendersIndependentTrackWithoutMissingHelpArea()
        {
            session.SetAssistance(StackPracticeAssistance.Independent);
            session.SelectTrack(StackPracticeTrack.Repair);
            Build();
            RectTransform assistance = Section("Learning assistance");
            Assert.IsNotNull(assistance.Find("Button_GUIDED"));
            Assert.IsNotNull(assistance.Find("Button_INDEPENDENT"));
            // Transform.Find treats '/' as a path separator even when it appears in a
            // literal button name. Inspect direct child names without changing the UI.
            bool revealButtonPresent = false;
            foreach (Transform child in assistance)
            {
                if (child.name == "Button_REVEAL / REVIEW")
                {
                    revealButtonPresent = true;
                    break;
                }
            }
            Assert.IsTrue(revealButtonPresent, "Expected the REVEAL / REVIEW button in the help panel.");
        }
    }
}
