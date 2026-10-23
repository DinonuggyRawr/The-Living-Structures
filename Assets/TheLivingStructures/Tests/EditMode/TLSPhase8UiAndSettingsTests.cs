using NUnit.Framework;
using TLS.Core;
using TLS.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TLS.Tests
{
    public sealed class TLSPhase8UiAndSettingsTests
    {
        [Test] public void AccessibilityHasSeparateVersionedPreferenceKey()
        {Assert.AreEqual("tls_phase8_presentation_v1",TLSPhase8Accessibility.PreferenceKey);}
        [Test] public void GlobalAccessibilityButtonRendersWithoutChangingLabState()
        {
            var r=new GameObject("TestRoot",typeof(RectTransform));
            try
            {
                TLSPhase8Accessibility.Draw(r.transform,()=>{});
                Assert.IsNotNull(r.transform.Find("Button_Aa OPTIONS"));
                Assert.GreaterOrEqual(r.GetComponentsInChildren<Button>().Length,1);
            }
            finally{Object.DestroyImmediate(r);}
        }
        [Test] public void SourcePresentationDoesNotRewriteCompilerDraft()
        {
            var root=new GameObject("Phase7",typeof(RectTransform));
            try
            {
                var draft=new TLSPhase7FunctionSession();string original=draft.Draft;
                TLSPhase7Screens.Transfer(root.transform,draft,new TLSPhase7WasmSession(),new TLSPhase7ProgressBook(),()=>{},()=>{});
                TLSPhase8Accessibility.Apply(root.transform);
                InputField editor=root.transform.Find("Phase7 C transfer editor").GetComponentInChildren<InputField>();
                Assert.AreEqual(original,editor.text);
                Assert.AreEqual(original,draft.Draft);
                Assert.AreEqual(InputField.LineType.MultiLineNewline,editor.lineType);
                Assert.IsFalse(editor.textComponent.supportRichText);
            }
            finally{Object.DestroyImmediate(root);}
        }
    }
}
