using NUnit.Framework;
using TLS.Core;
using TLS.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TLS.Tests
{
    public sealed class TLSPhase8TreeDiagramTests
    {
        private static TLSTreeState ThreeNode(TLSTreeKind kind)
        {
            var s=new TLSTreeState(kind);int root=s.Add(8);s.Root=root;
            int a=s.Add(3,root),b=s.Add(10,root);
            if(kind==TLSTreeKind.General){s.Find(root).Children.Add(a);s.Find(root).Children.Add(b);}
            else{s.Find(root).Left=a;s.Find(root).Right=b;}
            Assert.IsTrue(s.Invariants());return s;
        }
        [TestCase(TLSTreeKind.General)]
        [TestCase(TLSTreeKind.Binary)]
        [TestCase(TLSTreeKind.BST)]
        public void DrawsEveryRealEdgeWithoutChangingTheTree(TLSTreeKind kind)
        {
            var s=ThreeNode(kind);var before=s.Copy();var layout=TLSPhase8TreeDiagram.Compute(s);
            Assert.IsTrue(layout.Valid);Assert.AreEqual(3,layout.Nodes.Count);
            Assert.AreEqual(2,layout.Edges.Count);Assert.IsTrue(s.Invariants());Assert.AreEqual(before.Count,s.Count);
            foreach(var e in layout.Edges)Assert.AreEqual(s.Root,e.From);
        }
        [Test] public void EmptyTreeDrawsNoFictitiousEdges()
        {var layout=TLSPhase8TreeDiagram.Compute(new TLSTreeState(TLSTreeKind.BST));Assert.IsTrue(layout.Valid);Assert.IsEmpty(layout.Nodes);Assert.IsEmpty(layout.Edges);}
        [Test] public void RootWithoutChildrenHasValidCenterCoordinate()
        {
            var s=new TLSTreeState(TLSTreeKind.Binary);s.Root=s.Add(2);
            var p=TLSPhase8TreeDiagram.Compute(s).Find(s.Root);
            Assert.That(p.X,Is.InRange(0f,1f));Assert.That(p.Y,Is.InRange(0f,1f));
        }
        [Test] public void ChildCoordinatesRespectRootDepthAndHierarchy()
        {
            var layout=TLSPhase8TreeDiagram.Compute(ThreeNode(TLSTreeKind.BST));
            Assert.Greater(layout.Find(0).Y,layout.Find(1).Y);
            Assert.Less(layout.Find(1).X,layout.Find(0).X);
            Assert.Greater(layout.Find(2).X,layout.Find(0).X);
        }
        [Test] public void InvalidIntermediateLinkIsNotDrawnOrExecuted()
        {
            var s=ThreeNode(TLSTreeKind.Binary);s.Find(s.Root).Left=-1;
            var layout=TLSPhase8TreeDiagram.Compute(s);
            Assert.IsFalse(layout.Valid);Assert.IsEmpty(layout.Edges);
        }
        [Test] public void GraphIncludesClippedViewportAndSelectorForRealNodes()
        {
            var root=new GameObject("GraphTest",typeof(RectTransform));
            try
            {
                TLSPhase8TreeDiagram.Draw(root.transform,ThreeNode(TLSTreeKind.BST),-1,()=>{});
                var viewport=root.transform.Find("Connected tree viewport");
                Assert.IsNotNull(viewport);Assert.IsNotNull(viewport.GetComponent<RectMask2D>());
                Assert.IsNotNull(viewport.Find("True parent child links"));
                Assert.AreEqual(3,viewport.GetComponentsInChildren<Button>().Length);
            }
            finally{Object.DestroyImmediate(root);}
        }
    }
}
