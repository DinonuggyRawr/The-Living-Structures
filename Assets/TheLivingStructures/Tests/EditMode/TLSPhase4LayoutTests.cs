using NUnit.Framework;
using TLS.Core;
using TLS.UI;
using UnityEngine;
using UnityEngine.UI;
namespace TLS.Tests
{
    public sealed class TLSPhase4LayoutTests
    {
        [Test] public void ThreeVisualAndCodingScreensExist(){Assert.IsNotNull(typeof(TLSPhase4VisualScreens).GetMethod("Tree"));Assert.IsNotNull(typeof(TLSPhase4VisualScreens).GetMethod("Recursion"));Assert.IsNotNull(typeof(TLSPhase4PracticeScreen).GetMethod("Render"));Assert.IsNotNull(typeof(TLSPhase4WasmScreen).GetMethod("Render"));}
        [Test] public void RecursionScreenHasVisibleSourceAndCallStack()
        {
            var root=new GameObject("root",typeof(RectTransform));
            try {TLSPhase4VisualScreens.Recursion(root.transform,new TLSRecursionLab(),()=>"4",x=>{},()=>"10",x=>{},()=>{},()=>{},()=>{});
                Assert.IsNotNull(root.transform.Find("Recursion C code"));Assert.IsNotNull(root.transform.Find("Recursion call stack"));}
            finally {Object.DestroyImmediate(root);}
        }
        [Test] public void TreeScreenHasThreeKindsAndOperations()
        {
            var root=new GameObject("root",typeof(RectTransform));
            try {TLSPhase4VisualScreens.Tree(root.transform,new TLSTreeSession(),()=>"8",()=>"8",x=>{},x=>{},()=>{},()=>{},()=>{});
                Assert.AreEqual(5,root.transform.Find("Tree mode tabs").GetComponentsInChildren<Button>().Length);
                Assert.AreEqual(8,root.transform.Find("Tree operation bar").GetComponentsInChildren<Button>().Length);}
            finally {Object.DestroyImmediate(root);}
        }
        [Test] public void TreeCodeGraphDoNotOverlap()
        {
            var root=new GameObject("root",typeof(RectTransform));
            try {TLSPhase4VisualScreens.Tree(root.transform,new TLSTreeSession(),()=>"8",()=>"8",x=>{},x=>{},()=>{},()=>{},()=>{});
                var a=(RectTransform)root.transform.Find("Tree C modeled algorithm");var b=(RectTransform)root.transform.Find("Tree topology and memory");
                Assert.Less(a.anchorMax.x,b.anchorMin.x);}
            finally {Object.DestroyImmediate(root);}
        }
        [Test] public void PracticeAndCompilerPanelsPresent()
        {
            var root=new GameObject("root",typeof(RectTransform));
            try {TLSPhase4PracticeScreen.Render(root.transform,new TLSPhase4PracticeSession(),()=>{},()=>{});
                Assert.AreEqual(4,root.transform.Find("Phase4 practice categories").GetComponentsInChildren<Button>().Length);}
            finally {Object.DestroyImmediate(root);}
        }
        [Test] public void SixWasmFunctionTabsRender()
        {
            var root=new GameObject("root",typeof(RectTransform));
            try {TLSPhase4WasmScreen.Render(root.transform,new TLSPhase4FunctionLabSession(),new TLSPhase4WasmLabSession(),()=>{},()=>{});
                Assert.AreEqual(6,root.transform.Find("Tree C11 function tabs").GetComponentsInChildren<Button>().Length);}
            finally {Object.DestroyImmediate(root);}
        }
    }
}
