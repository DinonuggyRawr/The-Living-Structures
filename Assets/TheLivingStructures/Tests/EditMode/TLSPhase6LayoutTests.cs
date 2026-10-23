using System;
using System.Reflection;
using NUnit.Framework;
using TLS.Core;
using TLS.UI;
using UnityEngine;
using UnityEngine.UI;
namespace TLS.Tests
{
    public sealed class TLSPhase6LayoutTests
    {
        private static GameObject Root(){return new GameObject("root",typeof(RectTransform));}
        [Test] public void BigOLabRendersFiveAlgorithmButtonsAndTraceGraph()
        {
            var r=Root();try{TLSPhase6Screens.Analysis(r.transform,new TLSAnalysisSession(),()=>"12",x=>{},()=>{},()=>{},()=>{},()=>{});
                Assert.AreEqual(5,r.transform.Find("Complexity algorithm tabs").GetComponentsInChildren<Button>().Length);
                Assert.IsNotNull(r.transform.Find("Phase6 operation growth chart"));}
            finally{UnityEngine.Object.DestroyImmediate(r);}
        }
        [Test] public void DebugLabShowsEightReferenceVersusMutantResults()
        {
            var r=Root();try{TLSPhase6Screens.Debugging(r.transform,new TLSDebugSession(),()=>"",x=>{},()=>{},()=>{},()=>{},()=>{});
                Transform p=r.transform.Find("Phase6 real boundary cases");Assert.IsNotNull(p);
                for(int i=0;i<8;i++)Assert.IsNotNull(p.Find("Boundary test row "+i));}
            finally{UnityEngine.Object.DestroyImmediate(r);}
        }
        [Test] public void QualityWorkshopHasTwoSeparateChoicePanels()
        {
            var r=Root();try{TLSPhase6Screens.Quality(r.transform,new TLSQualitySession(),()=>{},()=>{},()=>{},()=>{});
                Assert.IsNotNull(r.transform.Find("C candidate A"));Assert.IsNotNull(r.transform.Find("C candidate B"));}
            finally{UnityEngine.Object.DestroyImmediate(r);}
        }
        [Test] public void ThreeCompilerTabsRenderAndMultilineEditorIsPresent()
        {
            var r=Root();try{TLSPhase6WasmScreen.Render(r.transform,new TLSPhase6FunctionSession(),new TLSPhase6WasmLabSession(),()=>{},()=>{});
                Assert.AreEqual(3,r.transform.Find("Phase6 C11 function tabs").GetComponentsInChildren<Button>().Length);
                var input=r.GetComponentInChildren<InputField>();Assert.IsNotNull(input);Assert.AreEqual(InputField.LineType.MultiLineNewline,input.lineType);}
            finally{UnityEngine.Object.DestroyImmediate(r);}
        }
        [Test] public void NewScreensAreReachableThroughAcademyModes()
        {
            var type=typeof(TLSAcademyApp).GetNestedType("ScreenMode",BindingFlags.NonPublic);
            foreach(string name in new[]{"Analysis","Debugging","Quality","Phase6Functions"})Assert.IsTrue(Enum.IsDefined(type,name),name);
        }
        [Test] public void AnalysisChartAndTraceDoNotOverlapHorizontally()
        {
            var r=Root();try{TLSPhase6Screens.Analysis(r.transform,new TLSAnalysisSession(),()=>"",x=>{},()=>{},()=>{},()=>{},()=>{});
                var a=(RectTransform)r.transform.Find("Phase6 operation growth chart");
                var b=(RectTransform)r.transform.Find("Phase6 operation timeline");Assert.Less(a.anchorMax.x,b.anchorMin.x);}
            finally{UnityEngine.Object.DestroyImmediate(r);}
        }
        [Test] public void EveryDirectPanelHasNormalViewportAnchors()
        {
            var r=Root();try{TLSPhase6Screens.Analysis(r.transform,new TLSAnalysisSession(),()=>"",x=>{},()=>{},()=>{},()=>{},()=>{});
                foreach(RectTransform rect in r.GetComponentsInChildren<RectTransform>()){
                    Assert.GreaterOrEqual(rect.anchorMin.x,-.001f,rect.name);Assert.LessOrEqual(rect.anchorMax.x,1.001f,rect.name);
                    Assert.GreaterOrEqual(rect.anchorMin.y,-.001f,rect.name);Assert.LessOrEqual(rect.anchorMax.y,1.001f,rect.name);
                }}finally{UnityEngine.Object.DestroyImmediate(r);}
        }
        [Test] public void NoNewMasteryWritePathIsPresent()
        {Assert.IsFalse(typeof(TLSAnalysisSession).GetProperty("Save")!=null);Assert.AreEqual(0,new TLSQualitySession().Independent);}
    }
}
