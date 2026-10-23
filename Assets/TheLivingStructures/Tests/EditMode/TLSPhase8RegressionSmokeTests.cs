using System;
using System.Reflection;
using NUnit.Framework;
using TLS.UI;
using UnityEngine;

namespace TLS.Tests
{
    public sealed class TLSPhase8RegressionSmokeTests
    {
        [Test] public void AllExistingMajorAcademyScreensRemainAvailable()
        {
            Type e=typeof(TLSAcademyApp).GetNestedType("ScreenMode",BindingFlags.NonPublic);
            Assert.IsNotNull(e);
            foreach(string name in new[]{"Academy","Stack","Arrays","Queues","PointerMemory","LinkedVisual",
                "Recursion","TreeVisual","Heap","Graph","Hash","Analysis","Debugging","Quality",
                "Phase6Functions","Phase7Dashboard","Phase7Exam","Phase7Reconstruct","Phase7TestDesign","Phase7Transfer"})
                Assert.IsTrue(Enum.IsDefined(e,name),name);
        }
        [Test] public void ExistingAcademyHasClickableModelAndFallbackDirectory()
        {
            Assert.IsNotNull(typeof(TLSPhase8Campus).GetMethod("TrySelect"));
            Assert.IsNotNull(typeof(TLSAcademyApp).GetMethod("RenderAcademy",BindingFlags.NonPublic|BindingFlags.Instance));
        }
        [Test] public void Phase8TreeDiagramPreservesModelIdentity()
        {
            Assert.IsNotNull(typeof(TLSPhase8TreeDiagram).GetMethod("Compute"));
            Assert.IsNotNull(typeof(TLSPhase8TreeDiagram).GetMethod("Draw"));
        }
    }
}
