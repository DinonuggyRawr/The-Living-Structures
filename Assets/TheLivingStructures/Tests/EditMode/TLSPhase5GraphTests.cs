using NUnit.Framework;
using TLS.Core;
namespace TLS.Tests
{
    public sealed class TLSPhase5GraphTests
    {
        [Test] public void DefaultDemoContainsSixVertices()
        {var s=new TLSGraphSession();Assert.AreEqual(6,s.Current.Vertices);Assert.AreEqual(6,s.Current.EdgeCount);Assert.IsNull(s.Trace);}
        [Test] public void DemoBfsUsesAlphabeticNeighbors()
        {var s=new TLSGraphSession();s.Begin(TLSGraphAction.BFS,"A");StringAssert.Contains("A -> B -> C -> D -> E -> F",s.Trace.Output);}
        [Test] public void DemoDfsUsesStackLikeDepthFirstOrder()
        {var s=new TLSGraphSession();s.Begin(TLSGraphAction.DFS,"A");StringAssert.Contains("A -> B -> D -> C -> E -> F",s.Trace.Output);}
        [Test] public void UndirectedEdgesRemainSymmetric()
        {var s=new TLSGraphSession();s.Begin(TLSGraphAction.AddEdge,"A","F");s.Finish();Assert.IsTrue(s.Current.Edges[0,5]);Assert.IsTrue(s.Current.Edges[5,0]);}
        [Test] public void DirectedEdgesHaveOneDirectionOnly()
        {var s=new TLSGraphSession();s.SetDirected(true);s.Begin(TLSGraphAction.Example);s.Finish();Assert.IsTrue(s.Current.Edges[0,1]);Assert.IsFalse(s.Current.Edges[1,0]);}
        [Test] public void ChangingDirectionResetsGraphInsteadOfRewritingEdges()
        {var s=new TLSGraphSession();s.SetDirected(true);Assert.AreEqual(0,s.Current.Vertices);Assert.AreEqual(0,s.Current.EdgeCount);}
        [Test] public void DisconnectedVerticesAreNotVisited()
        {var s=new TLSGraphSession();s.Begin(TLSGraphAction.Clear);s.Finish();s.Begin(TLSGraphAction.AddVertex);s.Finish();s.Begin(TLSGraphAction.AddVertex);s.Finish();s.Begin(TLSGraphAction.BFS,"A");StringAssert.Contains("A. Disconnected",s.Trace.Output);}
        [Test] public void InvalidVertexIsRejectedAtomically()
        {var s=new TLSGraphSession();int count=s.Current.EdgeCount;s.Begin(TLSGraphAction.AddEdge,"Z","A");Assert.IsFalse(s.Trace.Success);s.Finish();Assert.AreEqual(count,s.Current.EdgeCount);}
        [Test] public void SelfLoopIsRejectedInBeginnerModel()
        {var s=new TLSGraphSession();s.Begin(TLSGraphAction.AddEdge,"A","A");Assert.IsFalse(s.Trace.Success);}
        [Test] public void EdgeRemovalLeavesOthersUnchanged()
        {var s=new TLSGraphSession();s.Begin(TLSGraphAction.RemoveEdge,"A","B");s.Finish();Assert.IsFalse(s.Current.Edges[0,1]);Assert.IsTrue(s.Current.Edges[0,2]);}
        [Test] public void GraphLabelParsingIsCaseInsensitive()
        {Assert.AreEqual(2,TLSGraphSession.Id("c"));Assert.AreEqual(-1,TLSGraphSession.Id("AA"));}
        [Test] public void GraphCapacityRejectsTenthVertex()
        {var s=new TLSGraphSession();for(int i=6;i<9;i++){s.Begin(TLSGraphAction.AddVertex);s.Finish();}s.Begin(TLSGraphAction.AddVertex);Assert.IsFalse(s.Trace.Success);}
        [Test] public void GraphEdgesDoNotChangeBeforeFinish()
        {var s=new TLSGraphSession();s.Begin(TLSGraphAction.AddEdge,"A","F");Assert.IsFalse(s.Current.Edges[0,5]);s.Finish();Assert.IsTrue(s.Current.Edges[0,5]);}
        [Test] public void DirectedSampleReachabilityIsNotUndirected()
        {var s=new TLSGraphSession();s.SetDirected(true);s.Begin(TLSGraphAction.Example);s.Finish();s.Begin(TLSGraphAction.BFS,"F");StringAssert.Contains("F. Disconnected",s.Trace.Output);}
    }
}
