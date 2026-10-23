using System;
using NUnit.Framework;
using TLS.Core;
namespace TLS.Tests
{
    public sealed class TLSPhase7TestDesignTests
    {
        private static DateTime T=new DateTime(2026,10,21,10,0,0,DateTimeKind.Utc);
        [Test] public void LastCellSearchExposesOffByOneWithCorrectIndex()
        {var r=TLSPhase7TestDesign.Evaluate(0,"1,2,3",3,2);Assert.IsTrue(r.Valid);Assert.IsTrue(r.ExposesBug);Assert.IsTrue(r.CorrectOracle);}
        [Test] public void EarlierCellSearchDoesNotExposeTheBug()
        {var r=TLSPhase7TestDesign.Evaluate(0,"1,2,3",2,1);Assert.IsTrue(r.Valid);Assert.IsFalse(r.ExposesBug);}
        [Test] public void FaultyBinarySearchSkipsRightmostElement()
        {var r=TLSPhase7TestDesign.Evaluate(1,"1,3,5",5,2);Assert.IsTrue(r.Valid);Assert.IsTrue(r.ExposesBug);Assert.IsTrue(r.CorrectOracle);}
        [Test] public void BinarySearchRequiresSortedInput()
        {var r=TLSPhase7TestDesign.Evaluate(1,"5,1,3",1,1);Assert.IsFalse(r.Valid);}
        [Test] public void QueueHeadFourAndTwoItemsMustWrapModuloCapacity()
        {var r=TLSPhase7TestDesign.Evaluate(2,"5,7",4,1);Assert.IsTrue(r.Valid);Assert.IsTrue(r.ExposesBug);Assert.AreEqual("1",r.Reference);}
        [Test] public void QueueNoWrapDoesNotExposeFault()
        {var r=TLSPhase7TestDesign.Evaluate(2,"5",1,2);Assert.IsTrue(r.Valid);Assert.IsFalse(r.ExposesBug);}
        [TestCase("")][TestCase("1,2,3,4,5,6,7")][TestCase("1,NOPE")][TestCase("1001")]
        public void InvalidStudentTestInputsAreRejected(string input)
        {Assert.IsNull(TLSPhase7TestDesign.ParseValues(input));}
        [Test] public void WrongOracleDoesNotEarnTestDesignEvidence()
        {
            var p=new TLSPhase7ProgressBook();var s=new TLSPhase7TestDesignSession();s.SetGuided(false);
            Assert.IsFalse(s.Submit("1,2,3",3,0,p,T));Assert.AreEqual(0,p.Topic("testing").testDesign);
        }
        [Test] public void ValidUnassistedDiscriminatingTestEarnsDistinctEvidenceOnce()
        {
            var p=new TLSPhase7ProgressBook();var s=new TLSPhase7TestDesignSession();s.SetGuided(false);
            Assert.IsTrue(s.Submit("1,2,3",3,2,p,T));Assert.IsFalse(s.Submit("1,2,3",3,2,p,T));
            Assert.AreEqual(1,p.Topic("testing").testDesign);
        }
        [Test] public void HintThenIndependentDoesNotEarnTestDesignCredit()
        {
            var p=new TLSPhase7ProgressBook();var s=new TLSPhase7TestDesignSession();s.Hint();s.SetGuided(false);
            Assert.IsFalse(s.Submit("1,2,3",3,2,p,T));Assert.AreEqual(0,p.Topic("testing").testDesign);
        }
    }
}
