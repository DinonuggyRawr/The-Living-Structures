using NUnit.Framework;
using TLS.UI;
using UnityEngine;

namespace TLS.Tests
{
    public sealed class TLSPhase8CampusTests
    {
        [Test] public void FivePhysicalSceneBuildingsAreMapped(){Assert.AreEqual(5,TLSPhase8Campus.Hubs.Length);}
        [TestCase("STACK LAB / ACTIVE",0)]
        [TestCase("LINKED LISTS / LATER",1)]
        [TestCase("RECURSION / LATER",2)]
        [TestCase("TREES / LATER",3)]
        [TestCase("GRAPHS / LATER",4)]
        public void OriginalSceneNameResolvesToIntendedLab(string name,int expected)
        {Assert.AreEqual(expected,TLSPhase8Campus.ResolveSceneName(name));}
        [Test] public void UnknownOrDecorativeSceneObjectsDoNotBecomeLearningHubs()
        {Assert.AreEqual(-1,TLSPhase8Campus.ResolveSceneName("Academy Platform"));Assert.AreEqual(-1,TLSPhase8Campus.ResolveSceneName(null));}
        [Test] public void NestedBuildingGeometryMapsToExistingSceneRoot()
        {
            GameObject root=new GameObject("TREES / LATER");
            GameObject child=new GameObject("Roof strip");
            try{child.transform.SetParent(root.transform);Assert.AreEqual(3,TLSPhase8Campus.ResolveAncestor(child.transform));}
            finally{Object.DestroyImmediate(root);}
        }
        [Test] public void HubsExposeActualEducationAndDirectAction()
        {foreach(var hub in TLSPhase8Campus.Hubs){Assert.IsNotEmpty(hub.Title);Assert.IsNotEmpty(hub.Purpose);StringAssert.StartsWith("OPEN",hub.LabLabel);}}
    }
}
