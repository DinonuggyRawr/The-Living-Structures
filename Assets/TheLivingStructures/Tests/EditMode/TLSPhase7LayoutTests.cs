using System;
using System.Reflection;
using NUnit.Framework;
using TLS.Core;
using TLS.UI;
using UnityEngine;
using UnityEngine.UI;
namespace TLS.Tests
{
    public sealed class TLSPhase7LayoutTests
    {
        private static GameObject Root(){return new GameObject("Phase7Root",typeof(RectTransform));}
        [Test] public void AllFivePhase7NavigationTabsRender()
        {
            var r=Root();try{TLSPhase7Screens.Menu(r.transform,0,()=>{},()=>{},()=>{},()=>{},()=>{});
                var bar=r.transform.Find("Phase7 navigation");Assert.IsNotNull(bar);Assert.AreEqual(5,bar.GetComponentsInChildren<Button>().Length);}
            finally{UnityEngine.Object.DestroyImmediate(r);}
        }
        [Test] public void DashboardKeepsSubjectsAndDetailsInSeparateColumns()
        {
            var r=Root();try{TLSPhase7Screens.Dashboard(r.transform,new TLSPhase7ProgressBook(),0,i=>{},i=>{},()=>{},()=>{},()=>{},()=>{},()=>{});
                var a=(RectTransform)r.transform.Find("Phase7 topic directory");var b=(RectTransform)r.transform.Find("Phase7 selected subject");
                Assert.IsNotNull(a);Assert.IsNotNull(b);Assert.Less(a.anchorMax.x,b.anchorMin.x);}
            finally{UnityEngine.Object.DestroyImmediate(r);}
        }
        [Test] public void DashboardHasValidTopicButtons()
        {
            var r=Root();try{TLSPhase7Screens.Dashboard(r.transform,new TLSPhase7ProgressBook(),0,i=>{},i=>{},()=>{},()=>{},()=>{},()=>{},()=>{});
                Assert.GreaterOrEqual(r.transform.Find("Phase7 topic directory").GetComponentsInChildren<Button>().Length,9);}
            finally{UnityEngine.Object.DestroyImmediate(r);}
        }
        [Test] public void AssessmentRendersThreeAnswersAndThreeModeTabs()
        {
            var r=Root();try{TLSPhase7Screens.ExamRoom(r.transform,new TLSPhase7Assessment(false),new TLSPhase7Assessment(true),
                new TLSPhase7ReviewSession(),0,i=>{},s=>{},()=>{},()=>{},new TLSPhase7ProgressBook());
                Assert.AreEqual(3,r.transform.Find("Phase7 exam tabs").GetComponentsInChildren<Button>().Length);
                Assert.AreEqual(3,r.transform.Find("Phase7 exam question").GetComponentsInChildren<Button>().Length);}
            finally{UnityEngine.Object.DestroyImmediate(r);}
        }
        [Test] public void ExamTabsDoNotOverlapQuestionPanel()
        {
            var r=Root();try{TLSPhase7Screens.ExamRoom(r.transform,new TLSPhase7Assessment(false),new TLSPhase7Assessment(true),
                new TLSPhase7ReviewSession(),0,i=>{},s=>{},()=>{},()=>{},new TLSPhase7ProgressBook());
                var a=(RectTransform)r.transform.Find("Phase7 exam tabs");var b=(RectTransform)r.transform.Find("Phase7 exam question");
                Assert.Less(b.anchorMax.y,a.anchorMin.y);}
            finally{UnityEngine.Object.DestroyImmediate(r);}
        }
        [Test] public void ReconstructionHasDistinctOrderInputAndCheckButton()
        {
            var r=Root();try{TLSPhase7Screens.Reconstruction(r.transform,new TLSPhase7ReconstructionSession(),"",s=>{},()=>{},()=>{},()=>{});
                Assert.IsNotNull(r.transform.Find("Phase7 reconstruction answer").Find("Order answer"));
                Assert.IsNotNull(r.transform.Find("Phase7 reconstruction answer").Find("Button_CHECK ORDER"));}
            finally{UnityEngine.Object.DestroyImmediate(r);}
        }
        [Test] public void TestDesignerRendersThreeUserFields()
        {
            var r=Root();try{TLSPhase7Screens.TestDesign(r.transform,new TLSPhase7TestDesignSession(),"1,2,3","3","2",
                s=>{},s=>{},s=>{},()=>{},()=>{});
                Assert.AreEqual(3,r.transform.Find("Phase7 test creator").GetComponentsInChildren<InputField>().Length);}
            finally{UnityEngine.Object.DestroyImmediate(r);}
        }
        [Test] public void CompilerTransferHasMultilineEditorAndTwoFunctionTabs()
        {
            var r=Root();try{TLSPhase7Screens.Transfer(r.transform,new TLSPhase7FunctionSession(),new TLSPhase7WasmSession(),new TLSPhase7ProgressBook(),()=>{},()=>{});
                Assert.AreEqual(2,r.transform.Find("Phase7 transfer function tabs").GetComponentsInChildren<Button>().Length);
                Assert.AreEqual(InputField.LineType.MultiLineNewline,r.transform.Find("Phase7 C transfer editor").GetComponentInChildren<InputField>().lineType);}
            finally{UnityEngine.Object.DestroyImmediate(r);}
        }
        [Test] public void TransferEditorPreservesStarterLineBreaksAndLiteralCText()
        {
            var r=Root();try
            {
                var session=new TLSPhase7FunctionSession();
                string original=session.Draft;
                TLSPhase7Screens.Transfer(r.transform,session,new TLSPhase7WasmSession(),
                    new TLSPhase7ProgressBook(),()=>{},()=>{});
                var field=r.transform.Find("Phase7 C transfer editor").GetComponentInChildren<InputField>();
                Assert.AreEqual(InputField.LineType.MultiLineNewline,field.lineType);
                StringAssert.Contains("\n",original);
                Assert.AreEqual(original,field.text,"Editor must retain C source newlines, not flatten to one line.");
                Assert.AreEqual(original,session.Draft,"Opening the editor must not change a student's draft.");
                Assert.IsFalse(field.textComponent.supportRichText,"C expressions must display as literal text.");
                Assert.GreaterOrEqual(field.textComponent.fontSize,19);
            }
            finally{UnityEngine.Object.DestroyImmediate(r);}
        }
        [Test] public void TransferEditorShowsReferenceAsMultilineWithoutChangingEvidence()
        {
            var r=Root();try
            {
                var session=new TLSPhase7FunctionSession();
                session.Reveal();
                string original=session.Draft;
                TLSPhase7Screens.Transfer(r.transform,session,new TLSPhase7WasmSession(),
                    new TLSPhase7ProgressBook(),()=>{},()=>{});
                var field=r.transform.Find("Phase7 C transfer editor").GetComponentInChildren<InputField>();
                StringAssert.Contains("\n",original);
                Assert.AreEqual(original,field.text);
                Assert.AreEqual(original,session.Draft);
                Assert.IsTrue(session.Exposed,"A shown reference must remain assisted evidence.");
            }
            finally{UnityEngine.Object.DestroyImmediate(r);}
        }
        [Test] public void CompilerFunctionTabsRemainBelowGlobalLearningNavigation()
        {
            var r=Root();try{
                TLSPhase7Screens.Menu(r.transform,4,()=>{},()=>{},()=>{},()=>{},()=>{});
                TLSPhase7Screens.Transfer(r.transform,new TLSPhase7FunctionSession(),new TLSPhase7WasmSession(),
                    new TLSPhase7ProgressBook(),()=>{},()=>{});
                var nav=(RectTransform)r.transform.Find("Phase7 navigation");
                var tabs=(RectTransform)r.transform.Find("Phase7 transfer function tabs");
                Assert.Less(tabs.anchorMax.y,nav.anchorMin.y);
            }finally{UnityEngine.Object.DestroyImmediate(r);}
        }
        [Test] public void Phase7AndLegacyModesCoexistInAcademy()
        {
            var type=typeof(TLSAcademyApp).GetNestedType("ScreenMode",BindingFlags.NonPublic);
            foreach(string name in new[]{"Stack","Arrays","Queues","LinkedVisual","Recursion","TreeVisual","Heap","Graph","Hash","Analysis","Debugging","Phase6Functions",
                "Phase7Dashboard","Phase7Exam","Phase7Reconstruct","Phase7TestDesign","Phase7Transfer"})
                Assert.IsTrue(Enum.IsDefined(type,name),name);
        }
        [Test] public void DirectPhase7PanelAnchorsStayWithinScreen()
        {
            var r=Root();try{TLSPhase7Screens.Dashboard(r.transform,new TLSPhase7ProgressBook(),4,i=>{},i=>{},()=>{},()=>{},()=>{},()=>{},()=>{});
                foreach(var t in r.GetComponentsInChildren<RectTransform>())
                {
                    Assert.GreaterOrEqual(t.anchorMin.x,-.001f,t.name);Assert.GreaterOrEqual(t.anchorMin.y,-.001f,t.name);
                    Assert.LessOrEqual(t.anchorMax.x,1.001f,t.name);Assert.LessOrEqual(t.anchorMax.y,1.001f,t.name);
                }}finally{UnityEngine.Object.DestroyImmediate(r);}
        }
    }
}
