using NUnit.Framework;
using TLS.Core;
using TLS.UI;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
namespace TLS.Tests
{
    public sealed class TLSPhase5LayoutTests
    {
        private static GameObject Root(){return new GameObject("root",typeof(RectTransform));}
        [Test] public void HeapGraphAndHashScreensRender()
        {
            var r=Root();
            try{
                TLSPhase5VisualScreens.Heap(r.transform,new TLSHeapSession(),()=>"12",x=>{},()=>{},()=>{},()=>{});
                Assert.IsNotNull(r.transform.Find("Heap tree graph"));Assert.IsNotNull(r.transform.Find("Phase5 C code"));
            }finally{Object.DestroyImmediate(r);}
        }
        [Test] public void GraphEdgesAreActualTopologyGraphics()
        {
            var r=Root();try{TLSPhase5VisualScreens.Graph(r.transform,new TLSGraphSession(),()=>"A",()=>"B",x=>{},x=>{},()=>{},()=>{},()=>{});
                Assert.IsNotNull(r.GetComponentInChildren<TLSPhase5EdgeGraphic>());Assert.IsNotNull(r.transform.Find("Graph spatial topology"));}
            finally{Object.DestroyImmediate(r);}
        }
        [Test] public void HashBucketsRenderDistinctRows()
        {
            var r=Root();try{TLSPhase5VisualScreens.Hash(r.transform,new TLSHashSession(),()=>"1",x=>{},()=>{},()=>{},()=>{});
                Transform p=r.transform.Find("Hash bucket map");Assert.IsNotNull(p);for(int i=0;i<7;i++)Assert.IsNotNull(p.Find("Hash bucket "+i));}
            finally{Object.DestroyImmediate(r);}
        }
        [Test] public void CodeAndGraphPanelsDoNotOverlap()
        {
            var r=Root();try{TLSPhase5VisualScreens.Heap(r.transform,new TLSHeapSession(),()=>"1",x=>{},()=>{},()=>{},()=>{});
                RectTransform a=(RectTransform)r.transform.Find("Phase5 C code");RectTransform b=(RectTransform)r.transform.Find("Heap tree graph");Assert.Less(a.anchorMax.x,b.anchorMin.x);}
            finally{Object.DestroyImmediate(r);}
        }
        [Test] public void PracticeHasFourQuestionModes()
        {
            var r=Root();try{TLSPhase5PracticeScreen.Render(r.transform,new TLSPhase5PracticeSession(),()=>{},()=>{});
                Assert.AreEqual(4,r.transform.Find("Phase5 practice categories").GetComponentsInChildren<Button>().Length);}
            finally{Object.DestroyImmediate(r);}
        }
        [Test] public void CompilerHasSevenFunctionTabs()
        {
            var r=Root();try{TLSPhase5WasmScreen.Render(r.transform,new TLSPhase5FunctionLabSession(),new TLSPhase5WasmLabSession(),()=>{},()=>{});
                Assert.AreEqual(7,r.transform.Find("Phase5 C11 function tabs").GetComponentsInChildren<Button>().Length);}
            finally{Object.DestroyImmediate(r);}
        }
        [Test] public void AcademyHasNewNavigationModes()
        {
            var enumType=typeof(TLSAcademyApp).GetNestedType("ScreenMode",BindingFlags.NonPublic);
            Assert.IsNotNull(enumType);Assert.IsTrue(System.Enum.IsDefined(enumType,"Heap"));Assert.IsTrue(System.Enum.IsDefined(enumType,"Graph"));Assert.IsTrue(System.Enum.IsDefined(enumType,"Hash"));
        }
        [Test] public void AllPhase5PanelsStayInViewportAnchors()
        {
            var r=Root();try{TLSPhase5VisualScreens.Hash(r.transform,new TLSHashSession(),()=>"1",x=>{},()=>{},()=>{},()=>{});
                foreach(RectTransform rect in r.GetComponentsInChildren<RectTransform>()){
                    Assert.GreaterOrEqual(rect.anchorMin.x,-.001f,rect.name);Assert.LessOrEqual(rect.anchorMax.x,1.001f,rect.name);
                    Assert.GreaterOrEqual(rect.anchorMin.y,-.001f,rect.name);Assert.LessOrEqual(rect.anchorMax.y,1.001f,rect.name);
                }}finally{Object.DestroyImmediate(r);}
        }
    }
}
