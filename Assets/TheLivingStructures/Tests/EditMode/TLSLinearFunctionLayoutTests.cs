using NUnit.Framework;
using TLS.Core;
using TLS.UI;
using UnityEngine;

namespace TLS.Tests
{
    public sealed class TLSLinearFunctionLayoutTests
    {
        [TestCase(true)] [TestCase(false)]
        public void WholeFunctionScreenShowsAllNeededControls(bool array)
        {
            var root=new GameObject("linear function root",typeof(RectTransform));
            try
            {
                TLSLinearFunctionScreen.Render(root.transform,new TLSLinearFunctionLabSession(array),
                    new TLSLinearWasmLabSession(),()=>{},()=>{});
                Assert.IsNotNull(FindByName(root.transform,"Button_ENABLE WASM RUNNER"));
                Assert.IsNotNull(FindByName(root.transform,"Button_GUIDED"));
                Assert.IsNotNull(FindByName(root.transform,"Button_INDEPENDENT"));
                Assert.IsNotNull(FindByName(root.transform,"Button_SHOW HINT"));
                Assert.IsNotNull(FindByName(root.transform,"Button_SHOW REFERENCE"));
                Assert.IsNotNull(FindByName(root.transform,"Button_RUN C11 WASM TESTS"));
                Assert.IsNotNull(FindByName(root.transform,"Linear student C editor"));
                Assert.IsNotNull(FindByName(root.transform,"Button_RETURN TO "+(array?"ARRAY":"QUEUE")+" LAB"));
            }
            finally {Object.DestroyImmediate(root);}
        }
        [Test] public void PreviousAcademyAndLabScreenTypesArePreserved()
        {
            Assert.IsNotNull(typeof(TLSLinearLabsScreen).GetMethod("RenderArray"));
            Assert.IsNotNull(typeof(TLSLinearLabsScreen).GetMethod("RenderQueue"));
            Assert.IsNotNull(typeof(TLSLinearPracticeScreen).GetMethod("Render"));
            Assert.IsNotNull(typeof(TLSWasmCompilerLabScreen).GetMethod("Render"));
        }
        private static Transform FindByName(Transform parent,string name)
        {
            if (parent.name==name) return parent;
            for (int i=0;i<parent.childCount;i++)
            {
                Transform result=FindByName(parent.GetChild(i),name);
                if (result!=null) return result;
            }
            return null;
        }
    }
}
