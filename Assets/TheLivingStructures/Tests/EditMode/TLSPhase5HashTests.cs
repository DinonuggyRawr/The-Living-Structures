using NUnit.Framework;
using TLS.Core;
namespace TLS.Tests
{
    public sealed class TLSPhase5HashTests
    {
        [Test] public void NormalizeNegativeMod7(){Assert.AreEqual(1,TLSHashState.Bucket(-6));Assert.AreEqual(0,TLSHashState.Bucket(0));Assert.AreEqual(6,TLSHashState.Bucket(-1));}
        [Test] public void NormalizeMinIntSafely(){int x=TLSHashState.Bucket(int.MinValue);Assert.GreaterOrEqual(x,0);Assert.Less(x,7);}
        [Test] public void ChainCollisionRetainsAllKeys()
        {var s=new TLSHashSession();foreach(int k in new[]{1,8,15}){s.Begin(TLSHashAction.Insert,k);s.Finish();}CollectionAssert.AreEqual(new[]{1,8,15},s.Current.Chains[1]);}
        [Test] public void ChainDuplicateRejectedWithoutMutation()
        {var s=new TLSHashSession();s.Begin(TLSHashAction.Insert,1);s.Finish();s.Begin(TLSHashAction.Insert,1);Assert.IsFalse(s.Trace.Success);s.Finish();Assert.AreEqual(1,s.Current.Count);}
        [Test] public void ChainingDeletionRemovesOnlyOneKey()
        {var s=new TLSHashSession();foreach(int k in new[]{1,8}){s.Begin(TLSHashAction.Insert,k);s.Finish();}s.Begin(TLSHashAction.Remove,1);s.Finish();CollectionAssert.AreEqual(new[]{8},s.Current.Chains[1]);}
        [Test] public void LinearProbingCollisionUsesNextSlot()
        {var s=new TLSHashSession();s.Select(TLSHashKind.LinearProbing);foreach(int k in new[]{1,8}){s.Begin(TLSHashAction.Insert,k);s.Finish();}Assert.AreEqual(1,s.Current.Values[1]);Assert.AreEqual(8,s.Current.Values[2]);}
        [Test] public void TombstoneKeepsLaterKeysFindable()
        {var s=new TLSHashSession();s.Select(TLSHashKind.LinearProbing);foreach(int k in new[]{1,8}){s.Begin(TLSHashAction.Insert,k);s.Finish();}s.Begin(TLSHashAction.Remove,1);s.Finish();Assert.AreEqual(TLSHashSlotKind.Deleted,s.Current.Kinds[1]);s.Begin(TLSHashAction.Find,8);StringAssert.Contains("FOUND 8",s.Trace.Output);}
        [Test] public void TombstoneIsReusedOnNextInsertion()
        {var s=new TLSHashSession();s.Select(TLSHashKind.LinearProbing);foreach(int k in new[]{1,8}){s.Begin(TLSHashAction.Insert,k);s.Finish();}s.Begin(TLSHashAction.Remove,1);s.Finish();s.Begin(TLSHashAction.Insert,15);s.Finish();Assert.AreEqual(15,s.Current.Values[1]);}
        [Test] public void FullProbingTableRejectsNewKey()
        {var s=new TLSHashSession();s.Select(TLSHashKind.LinearProbing);for(int i=0;i<7;i++){s.Begin(TLSHashAction.Insert,i);s.Finish();}s.Begin(TLSHashAction.Insert,100);Assert.IsFalse(s.Trace.Success);s.Finish();Assert.AreEqual(7,s.Current.Count);}
        [Test] public void FindDoesNotModifyCount()
        {var s=new TLSHashSession();s.Begin(TLSHashAction.Insert,7);s.Finish();s.Begin(TLSHashAction.Find,7);s.Finish();Assert.AreEqual(1,s.Current.Count);}
        [Test] public void CollisionExampleExplainsBothModes()
        {foreach(TLSHashKind kind in new[]{TLSHashKind.Chaining,TLSHashKind.LinearProbing}){var s=new TLSHashSession();s.Select(kind);s.Begin(TLSHashAction.CollisionExample);s.Finish();Assert.AreEqual(4,s.Current.Count);}}
        [Test] public void ChangingCollisionMethodDoesNotConvertDataSilently()
        {var s=new TLSHashSession();s.Begin(TLSHashAction.Insert,7);s.Finish();s.Select(TLSHashKind.LinearProbing);Assert.AreEqual(0,s.Current.Count);}
        [Test] public void FailedDeletionDoesNotCommitChanges()
        {var s=new TLSHashSession();s.Begin(TLSHashAction.Remove,404);Assert.IsFalse(s.Trace.Success);s.Finish();Assert.AreEqual(0,s.Current.Count);}
        [Test] public void LiveStateOnlyCommitsOnFinalFrame()
        {var s=new TLSHashSession();s.Begin(TLSHashAction.Insert,1);Assert.AreEqual(0,s.Current.Count);s.Finish();Assert.AreEqual(1,s.Current.Count);}
    }
}
