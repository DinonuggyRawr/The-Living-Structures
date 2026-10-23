using NUnit.Framework;
using TLS.Core;
using TLS.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TLS.Tests
{
    public sealed class TLSPointerMemoryLayoutTests
    {
        [Test] public void PointerLabHasAccessibleRenderEntry()
        {
            Assert.IsNotNull(typeof(TLSPointerMemoryScreen).GetMethod("Render"));
        }
        [Test] public void AllLessonChoicesAndStepButtonsExist()
        {
            GameObject root=new GameObject("root",typeof(RectTransform));
            try
            {
                TLSPointerMemoryScreen.Render(root.transform,new TLSPointerMemorySession(),()=>{});
                Transform choices=root.transform.Find("Pointer lesson choices");Assert.IsNotNull(choices);
                Assert.AreEqual(9,choices.GetComponentsInChildren<Button>().Length);
                Transform controls=root.transform.Find("Pointer trace controls");Assert.IsNotNull(controls);
                Assert.IsNotNull(controls.Find("Button_NEXT STEP >"));
                Assert.IsNotNull(controls.Find("Button_FINISH"));
                Assert.IsNotNull(controls.Find("Button_RESET TRACE"));
            }
            finally{Object.DestroyImmediate(root);}
        }
        [Test] public void CodeMemoryAndExplanationPanelsAreDistinct()
        {
            GameObject root=new GameObject("root",typeof(RectTransform));
            try
            {
                TLSPointerMemoryScreen.Render(root.transform,new TLSPointerMemorySession(),()=>{});
                var code=(RectTransform)root.transform.Find("Pointer C code");
                var memory=(RectTransform)root.transform.Find("Pointer memory viewport");
                var explain=(RectTransform)root.transform.Find("Pointer explanation");
                Assert.Less(code.anchorMax.x,memory.anchorMin.x);
                Assert.Less(memory.anchorMax.x,explain.anchorMin.x);
                Assert.AreEqual(2,memory.Find("Heap_object1")!=null && memory.Find("Heap_object2")!=null?2:0);
            }
            finally{Object.DestroyImmediate(root);}
        }
        [Test] public void ScenarioChoiceDoesNotOverlapHeaderOrCode()
        {
            GameObject root=new GameObject("root",typeof(RectTransform));
            try
            {
                TLSPointerMemoryScreen.Render(root.transform,new TLSPointerMemorySession(),()=>{});
                var choices=(RectTransform)root.transform.Find("Pointer lesson choices");
                var code=(RectTransform)root.transform.Find("Pointer C code");
                Assert.Greater(choices.anchorMin.y,code.anchorMax.y);
                Assert.Less(choices.anchorMax.y,0.91f);
            }
            finally{Object.DestroyImmediate(root);}
        }
        [Test] public void InvalidDereferenceShowsSafetyWarning()
        {
            var s=new TLSPointerMemorySession();s.Select(TLSPointerLessonId.InvalidDereference);s.Finish();
            GameObject root=new GameObject("root",typeof(RectTransform));
            try
            {
                TLSPointerMemoryScreen.Render(root.transform,s,()=>{});
                var explanation=root.transform.Find("Pointer explanation");
                var safety=explanation.Find("Safety").GetComponent<Text>();
                StringAssert.Contains("BLOCKED",safety.text);
            }
            finally{Object.DestroyImmediate(root);}
        }
    }
}
