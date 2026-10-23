using System;
using NUnit.Framework;
using TLS.Core;

namespace TLS.Tests
{
    public sealed class TLSPhase4TreeTests
    {
        private static void Do(TLSTreeSession s,TLSTreeAction action,int value=0,int parent=0)
        {s.Begin(action,value,parent);Assert.IsTrue(s.Trace.Success,s.Trace.Diagnostic);s.Finish();Assert.IsTrue(s.Current.Invariants());}
        private static TLSTreeSession Seed(params int[] values)
        {TLSTreeSession s=new TLSTreeSession();foreach(int n in values)Do(s,TLSTreeAction.Insert,n);return s;}
        private static void Values(TLSTreeState s,TLSTreeAction traversal,params int[] expected)
        {CollectionAssert.AreEqual(expected,s.Traverse(traversal));Assert.IsTrue(s.Invariants());}
        [TestCase(TLSTreeKind.General)] [TestCase(TLSTreeKind.Binary)] [TestCase(TLSTreeKind.BST)]
        public void EmptyTreeIsValid(TLSTreeKind kind)
        {var s=new TLSTreeState(kind);Assert.IsTrue(s.Invariants());Assert.AreEqual(0,s.Height);Assert.AreEqual(-1,s.Root);}
        [Test] public void BstInsertOrdersKeys()
        {var s=Seed(8,3,10,1,6);Values(s.Current,TLSTreeAction.Inorder,1,3,6,8,10);Assert.AreEqual(3,s.Current.Height);}
        [Test] public void BstDuplicateRejectedWithoutChanges()
        {var s=Seed(8,3);s.Begin(TLSTreeAction.Insert,3);Assert.IsFalse(s.Trace.Success);s.Finish();Assert.AreEqual(2,s.Current.Count);}
        [Test] public void BstFindFoundAndMissingNoMutation()
        {var s=Seed(8,3,10);s.Begin(TLSTreeAction.Search,10);Assert.IsTrue(s.Trace.Success);s.Finish();Assert.AreEqual(3,s.Current.Count);s.Begin(TLSTreeAction.Search,99);Assert.IsTrue(s.Trace.Success);s.Finish();Assert.AreEqual(3,s.Current.Count);}
        [Test] public void DeleteLeaf()
        {var s=Seed(8,3,10);Do(s,TLSTreeAction.Delete,3);Values(s.Current,TLSTreeAction.Inorder,8,10);}
        [Test] public void DeleteSingleRoot()
        {var s=Seed(8);Do(s,TLSTreeAction.Delete,8);Assert.AreEqual(-1,s.Current.Root);Assert.AreEqual(0,s.Current.Count);}
        [Test] public void DeleteRootWithOneChild()
        {var s=Seed(8,3);Do(s,TLSTreeAction.Delete,8);Values(s.Current,TLSTreeAction.Inorder,3);Assert.AreEqual(-1,s.Current.Find(s.Current.Root).Parent);}
        [Test] public void DeleteRootWithTwoChildren()
        {var s=Seed(8,3,10,9,11);Do(s,TLSTreeAction.Delete,8);Values(s.Current,TLSTreeAction.Inorder,3,9,10,11);Assert.AreEqual(9,s.Current.Find(s.Current.Root).Value);}
        [Test] public void DeleteSuccessorWithRightChild()
        {var s=Seed(20,10,30,25,27,40);Do(s,TLSTreeAction.Delete,20);Values(s.Current,TLSTreeAction.Inorder,10,25,27,30,40);Assert.AreEqual(25,s.Current.Find(s.Current.Root).Value);}
        [Test] public void DeleteAbsentLeavesTopologyUnchanged()
        {var s=Seed(8,3,10);s.Begin(TLSTreeAction.Delete,500);Assert.IsFalse(s.Trace.Success);s.Finish();Values(s.Current,TLSTreeAction.Inorder,3,8,10);}
        [Test] public void BstPreorderAndPostorder()
        {var s=Seed(8,3,10,1,6);Values(s.Current,TLSTreeAction.Preorder,8,3,1,6,10);Values(s.Current,TLSTreeAction.Postorder,1,6,3,10,8);}
        [Test] public void BstLevelOrder()
        {var s=Seed(8,3,10,1,6);Values(s.Current,TLSTreeAction.LevelOrder,8,3,10,1,6);}
        [Test] public void GeneralChildrenPreserveOrder()
        {var s=new TLSTreeSession();s.Select(TLSTreeKind.General);Do(s,TLSTreeAction.AddRoot,10);Do(s,TLSTreeAction.AddChild,20,10);Do(s,TLSTreeAction.AddChild,30,10);Do(s,TLSTreeAction.AddChild,40,20);Values(s.Current,TLSTreeAction.Preorder,10,20,40,30);}
        [Test] public void GeneralInorderRejected()
        {var s=new TLSTreeSession();s.Select(TLSTreeKind.General);Assert.Throws<InvalidOperationException>(()=>s.Current.Traverse(TLSTreeAction.Inorder));}
        [Test] public void GeneralHeightAndDepth()
        {var s=new TLSTreeSession();s.Select(TLSTreeKind.General);Do(s,TLSTreeAction.AddRoot,10);Do(s,TLSTreeAction.AddChild,20,10);Do(s,TLSTreeAction.AddChild,30,20);Assert.AreEqual(3,s.Current.Height);Assert.AreEqual(2,s.Current.Depth(s.Current.FindValue(30).Id));}
        [Test] public void BinaryManualSlots()
        {var s=new TLSTreeSession();s.Select(TLSTreeKind.Binary);Do(s,TLSTreeAction.AddRoot,10);Do(s,TLSTreeAction.AddLeft,20,10);Do(s,TLSTreeAction.AddRight,5,10);Values(s.Current,TLSTreeAction.Inorder,20,10,5);}
        [Test] public void BinaryIsNotRequiredToBeBst()
        {var s=new TLSTreeSession();s.Select(TLSTreeKind.Binary);Do(s,TLSTreeAction.AddRoot,10);Do(s,TLSTreeAction.AddLeft,20,10);Assert.IsTrue(s.Current.Invariants());}
        [Test] public void BinaryOccupiedSlotRejected()
        {var s=new TLSTreeSession();s.Select(TLSTreeKind.Binary);Do(s,TLSTreeAction.AddRoot,10);Do(s,TLSTreeAction.AddLeft,5,10);s.Begin(TLSTreeAction.AddLeft,3,10);Assert.IsFalse(s.Trace.Success);Assert.AreEqual(2,s.Current.Count);}
        [Test] public void GeneralWrongModeRejectsBinaryLinks()
        {var s=new TLSTreeSession();s.Select(TLSTreeKind.General);Do(s,TLSTreeAction.AddRoot,1);s.Begin(TLSTreeAction.AddLeft,2,1);Assert.IsFalse(s.Trace.Success);}
        [Test] public void BstRejectsManuallyPlacedChild()
        {var s=Seed(10);s.Begin(TLSTreeAction.AddLeft,5,10);Assert.IsFalse(s.Trace.Success);}
        [Test] public void RootCannotBeReplacedByAddRoot()
        {var s=new TLSTreeSession();s.Select(TLSTreeKind.General);Do(s,TLSTreeAction.AddRoot,1);s.Begin(TLSTreeAction.AddRoot,2);Assert.IsFalse(s.Trace.Success);}
        [Test] public void SwitchModesPreservesIndependentTopology()
        {var s=Seed(8);s.Select(TLSTreeKind.General);Do(s,TLSTreeAction.AddRoot,99);s.Select(TLSTreeKind.BST);Assert.AreEqual(8,s.Current.Find(s.Current.Root).Value);}
        [Test] public void FourteenNodesCanFitWithinLimit()
        {var s=new TLSTreeSession();for(int i=0;i<TLSTreeState.MaximumNodes;i++)Do(s,TLSTreeAction.Insert,i);Assert.AreEqual(15,s.Current.Count);}
        [Test] public void CapacityRejectsExtraNode()
        {var s=new TLSTreeSession();for(int i=0;i<TLSTreeState.MaximumNodes;i++)Do(s,TLSTreeAction.Insert,i);s.Begin(TLSTreeAction.Insert,16);Assert.IsFalse(s.Trace.Success);Assert.AreEqual(15,s.Current.Count);}
        [Test] public void MutationOnlyCommitsAtFinalFrame()
        {var s=new TLSTreeSession();s.Begin(TLSTreeAction.Insert,10);Assert.AreEqual(0,s.Current.Count);s.Next();Assert.AreEqual(0,s.Current.Count);s.Finish();Assert.AreEqual(1,s.Current.Count);}
        [Test] public void RewindBeforeLastFrameRestoresOriginal()
        {var s=Seed(5);s.Begin(TLSTreeAction.Insert,3);s.Finish();Assert.AreEqual(2,s.Current.Count);s.Previous();Assert.AreEqual(1,s.Current.Count);s.Finish();Assert.AreEqual(2,s.Current.Count);}
        [Test] public void ResetTreeIsSessionOnly()
        {var s=Seed(8,3);Do(s,TLSTreeAction.Reset);Assert.AreEqual(0,s.Current.Count);Assert.IsTrue(s.Current.Invariants());}
        [Test] public void PredictedCountReportsActualDifference()
        {var s=new TLSTreeSession();s.Begin(TLSTreeAction.Insert,20);s.Predict(1);StringAssert.Contains("Correct prediction",s.Prediction);}
        [Test] public void SymbolicAddressesDifferFromProcessPointers()
        {var s=new TLSTreeState(TLSTreeKind.BST);Assert.AreEqual("NULL",s.Address(-1));Assert.AreEqual("0x1000",s.Address(0));}
        [Test] public void CloneDoesNotShareNodes()
        {var s=Seed(8);var c=s.Current.Copy();c.Find(c.Root).Value=55;Assert.AreEqual(8,s.Current.Find(s.Current.Root).Value);}
        [Test] public void EveryFrameReferencesValidSourceLine()
        {var s=Seed(8,3);foreach(TLSTreeAction a in Enum.GetValues(typeof(TLSTreeAction))){s.Begin(a,6,8);foreach(var f in s.Trace.Frames)Assert.That(f.Line,Is.InRange(0,s.Trace.Code.Length-1));}}
    }
}
