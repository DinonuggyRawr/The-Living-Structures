using System;
using System.Reflection;
using NUnit.Framework;
using TLS.Core;
using TLS.UI;

namespace TLS.Tests
{
    public sealed class TLSPhase8PerformanceTests
    {
        [Test] public void AcademyBadgeAvoidsRepeatedHashCheckForSameBuild()
        {
            var badge = new TLSPhase8AcademyReadinessCache();
            int checks = 0;
            Func<string, bool, TLSBundledToolchain.CheckResult> check = (path, packaged) =>
            {
                checks++;
                return new TLSBundledToolchain.CheckResult { Ready = true, ManifestVerified = packaged, Root = path };
            };
            var first = badge.Get("C:/Demo/Tools", true, check);
            var second = badge.Get("C:/Demo/Tools", true, check);
            Assert.AreSame(first, second);
            Assert.AreEqual(1, checks, "Academy tab clicks should not rehash the executable bundle.");
        }

        [Test] public void NewToolchainLocationTriggersNewCheck()
        {
            var badge = new TLSPhase8AcademyReadinessCache();
            int checks = 0;
            Func<string, bool, TLSBundledToolchain.CheckResult> check = (path, packaged) =>
            {
                checks++;
                return new TLSBundledToolchain.CheckResult { Root = path };
            };
            badge.Get("C:/Tools/Old", true, check);
            badge.Get("C:/Tools/New", true, check);
            Assert.AreEqual(2, checks);
        }

        [Test] public void EditorAndPlayerChecksRemainSeparate()
        {
            var badge = new TLSPhase8AcademyReadinessCache();
            int checks = 0;
            Func<string, bool, TLSBundledToolchain.CheckResult> check = (path, packaged) =>
            {
                checks++;
                return new TLSBundledToolchain.CheckResult();
            };
            badge.Get("C:/Tools", false, check);
            badge.Get("C:/Tools", true, check);
            Assert.AreEqual(2, checks);
        }

        [Test] public void ManualInvalidationAllowsRecheckingStatus()
        {
            var badge = new TLSPhase8AcademyReadinessCache();
            int checks = 0;
            Func<string, bool, TLSBundledToolchain.CheckResult> check = (path, packaged) =>
            {
                checks++;
                return new TLSBundledToolchain.CheckResult();
            };
            badge.Get("C:/Tools", true, check);
            badge.Invalidate();
            badge.Get("C:/Tools", true, check);
            Assert.AreEqual(2, checks);
        }

        [Test] public void NullCheckerAndNullResultCannotSilentlyReportReady()
        {
            var badge = new TLSPhase8AcademyReadinessCache();
            Assert.Throws<ArgumentNullException>(() => badge.Get("C:/Tools", true, null));
            Assert.Throws<InvalidOperationException>(() => badge.Get("C:/Tools", true, (path, packaged) => null));
        }

        [Test] public void AcademyAppRetainsAllExistingScreenRoutes()
        {
            var modes = typeof(TLSAcademyApp).GetNestedType("ScreenMode", BindingFlags.NonPublic);
            Assert.IsNotNull(modes);
            foreach (string topic in new [] {"Academy", "Stack", "Arrays", "Queues", "LinkedVisual",
                "TreeVisual", "Heap", "Graph", "Hash", "Analysis", "Phase7Dashboard", "Phase7Transfer"})
                Assert.IsTrue(Enum.IsDefined(modes, topic), topic);
        }
    }
}
