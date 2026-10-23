using NUnit.Framework;
using TLS.Core;
using TLS.UI;
using UnityEngine;
using UnityEngine.UI;
namespace TLS.Tests
{
    public sealed class TLSLinkedLayoutTests
    {
        [Test] public void ThreeScreensHaveRenderingEntrypoints(){Assert.IsNotNull(typeof(TLSLinkedListScreen).GetMethod("Render"));Assert.IsNotNull(typeof(TLSLinkedCodingScreen).GetMethod("Render"));Assert.IsNotNull(typeof(TLSLinkedWasmScreen).GetMethod("Render"));}
        [Test] public void ListLabHasThreeKindsNineOperationsAndTwoPracticeLaunchers()
        {
            var root=new GameObject("test",typeof(RectTransform));
            try
            {
                var s=new TLSLinkedLabSession();
                TLSLinkedListScreen.Render(root.transform,s,()=>{},()=>{},()=>{},()=>"11",()=>"0",x=>{},x=>{});
                Assert.AreEqual(5,root.transform.Find("Linked kinds and navigation").GetComponentsInChildren<Button>().Length);
                Assert.AreEqual(9,root.transform.Find("Linked operations").GetComponentsInChildren<Button>().Length);
                Assert.IsNotNull(root.transform.Find("Linked C trace"));Assert.IsNotNull(root.transform.Find("Linked memory visualization"));
            }
            finally {Object.DestroyImmediate(root);}
        }
        [Test] public void ListCodeGraphAndControlsAreNonoverlapping()
        {
            var root=new GameObject("test",typeof(RectTransform));
            try
            {
                TLSLinkedListScreen.Render(root.transform,new TLSLinkedLabSession(),()=>{},()=>{},()=>{},()=>"0",()=>"0",x=>{},x=>{});
                var code=(RectTransform)root.transform.Find("Linked C trace");var graph=(RectTransform)root.transform.Find("Linked memory visualization");
                var controls=(RectTransform)root.transform.Find("Linked control ribbon");
                Assert.Less(code.anchorMax.x,graph.anchorMin.x);Assert.Less(controls.anchorMax.y,code.anchorMin.y);
            }
            finally {Object.DestroyImmediate(root);}
        }
        [Test] public void PracticeChallengeCategoriesVisible()
        {
            var root=new GameObject("test",typeof(RectTransform));
            try {TLSLinkedCodingScreen.Render(root.transform,new TLSLinkedCodingSession(),()=>{},()=>{});
                Assert.AreEqual(4,root.transform.Find("Linked practice categories").GetComponentsInChildren<Button>().Length);
                Assert.IsNotNull(root.transform.Find("Linked C practice sheet"));}
            finally {Object.DestroyImmediate(root);}
        }
        [Test] public void FunctionLabDisplaysFiveFunctionSelectorsAndResults()
        {
            var root=new GameObject("test",typeof(RectTransform));
            try {TLSLinkedWasmScreen.Render(root.transform,new TLSLinkedFunctionLabSession(),new TLSLinkedWasmLabSession(),()=>{},()=>{});
                Assert.AreEqual(5,root.transform.Find("Linked C11 function tabs").GetComponentsInChildren<Button>().Length);
                Assert.IsNotNull(root.transform.Find("Linked C11 results"));}
            finally {Object.DestroyImmediate(root);}
        }
    }
}
