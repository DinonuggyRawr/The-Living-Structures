using NUnit.Framework;
using TLS.Core;
namespace TLS.Tests
{
    public sealed class TLSPhase5HeapTests
    {
        [Test] public void NewHeapIsEmptyAndValid(){var s=new TLSHeapSession();Assert.AreEqual(0,s.Current.Data.Count);Assert.IsTrue(s.Current.Valid());}
        [Test] public void StagedInsertDoesNotCommitUntilFinalStep()
        {var s=new TLSHeapSession();s.Begin(TLSHeapAction.Insert,5);Assert.AreEqual(0,s.Current.Data.Count);Assert.AreEqual(0,s.Visible.Data.Count);s.Finish();Assert.AreEqual(1,s.Current.Data.Count);}
        [Test] public void MinInsertSiftsToRoot()
        {var s=new TLSHeapSession();foreach(int v in new[]{6,3,9,1}){s.Begin(TLSHeapAction.Insert,v);s.Finish();}Assert.AreEqual(1,s.Current.Data[0]);Assert.IsTrue(s.Current.Valid());}
        [Test] public void MaxInsertSiftsToRoot()
        {var s=new TLSHeapSession();s.Select(TLSHeapKind.Max);foreach(int v in new[]{6,3,9,1}){s.Begin(TLSHeapAction.Insert,v);s.Finish();}Assert.AreEqual(9,s.Current.Data[0]);Assert.IsTrue(s.Current.Valid());}
        [Test] public void ExtractMinRestoresHeapPriority()
        {var s=new TLSHeapSession();foreach(int v in new[]{7,4,9,2,3,8}){s.Begin(TLSHeapAction.Insert,v);s.Finish();}s.Begin(TLSHeapAction.Extract);s.Finish();Assert.AreEqual(3,s.Current.Data[0]);Assert.IsTrue(s.Current.Valid());}
        [Test] public void ExtractMaxRestoresHeapPriority()
        {var s=new TLSHeapSession();s.Select(TLSHeapKind.Max);foreach(int v in new[]{7,4,9,2,3,8}){s.Begin(TLSHeapAction.Insert,v);s.Finish();}s.Begin(TLSHeapAction.Extract);s.Finish();Assert.AreEqual(8,s.Current.Data[0]);Assert.IsTrue(s.Current.Valid());}
        [Test] public void FullHeapRejectsAdditionalElementsAtomically()
        {var s=new TLSHeapSession();for(int i=0;i<15;i++){s.Begin(TLSHeapAction.Insert,i);s.Finish();}s.Begin(TLSHeapAction.Insert,100);Assert.IsFalse(s.Trace.Success);s.Finish();Assert.AreEqual(15,s.Current.Data.Count);Assert.IsTrue(s.Current.Valid());}
        [Test] public void EmptyExtractIsRejectedWithoutMutation()
        {var s=new TLSHeapSession();s.Begin(TLSHeapAction.Extract);Assert.IsFalse(s.Trace.Success);s.Finish();Assert.AreEqual(0,s.Current.Data.Count);}
        [Test] public void BottomUpHeapifyProducesValidArray()
        {var s=new TLSHeapSession();s.Begin(TLSHeapAction.BuildExample);s.Finish();Assert.AreEqual(7,s.Current.Data.Count);Assert.IsTrue(s.Current.Valid());Assert.AreEqual(1,s.Current.Data[0]);}
        [Test] public void BuildMaxHeapAlsoWorks()
        {var s=new TLSHeapSession();s.Select(TLSHeapKind.Max);s.Begin(TLSHeapAction.BuildExample);s.Finish();Assert.AreEqual(9,s.Current.Data[0]);Assert.IsTrue(s.Current.Valid());}
        [Test] public void RewindingAfterFinishRestoresOldLiveState()
        {var s=new TLSHeapSession();s.Begin(TLSHeapAction.Insert,4);s.Finish();s.Previous();Assert.AreEqual(0,s.Current.Data.Count);s.Finish();Assert.AreEqual(1,s.Current.Data.Count);}
        [Test] public void ResetTraceBeforeFinishDiscardsPreview()
        {var s=new TLSHeapSession();s.Begin(TLSHeapAction.Insert,4);s.ResetTrace();Assert.IsNull(s.Trace);Assert.AreEqual(0,s.Current.Data.Count);}
        [Test] public void PriorityComparisonHandlesNegativeAndExtremeValues()
        {var s=new TLSHeapSession();foreach(int v in new[]{int.MaxValue,0,int.MinValue}){s.Begin(TLSHeapAction.Insert,v);s.Finish();}Assert.AreEqual(int.MinValue,s.Current.Data[0]);}
        [Test] public void ResetClearsHeap()
        {var s=new TLSHeapSession();s.Begin(TLSHeapAction.Insert,4);s.Finish();s.Begin(TLSHeapAction.Reset);s.Finish();Assert.AreEqual(0,s.Current.Data.Count);}
        [Test] public void PredictionIsOnlyFormative()
        {var s=new TLSHeapSession();s.Begin(TLSHeapAction.Insert,4);s.Predict(1);Assert.IsTrue(s.Predicted);s.Finish();StringAssert.Contains("Actual: MORE",s.Prediction);}
    }
}
