using System;
using NUnit.Framework;
using TLS.Core;
namespace TLS.Tests
{
    public sealed class TLSPhase7ReconstructionTests
    {
        private static DateTime Now=new DateTime(2026,10,21,10,0,0,DateTimeKind.Utc);
        [Test] public void FivePuzzlesRequireCompleteStatementPermutations()
        {Assert.AreEqual(5,TLSPhase7ReconstructionCatalog.Cards.Length);foreach(var c in TLSPhase7ReconstructionCatalog.Cards)Assert.AreEqual(c.Shuffled.Length,c.CorrectOrder.Length);}
        [TestCase(0)][TestCase(1)][TestCase(2)][TestCase(3)][TestCase(4)]
        public void EachAuthoredCorrectReconstructionIsAccepted(int i)
        {
            var p=new TLSPhase7ProgressBook();var s=new TLSPhase7ReconstructionSession();s.Select(i);s.SetGuided(false);
            Assert.IsTrue(s.Submit(s.Current.CorrectText,p,Now));Assert.IsTrue(p.Independent("parsons:"+s.Current.Id));
        }
        [Test] public void IncorrectFirstPermutationBlocksCreditEvenIfRetried()
        {
            var p=new TLSPhase7ProgressBook();var s=new TLSPhase7ReconstructionSession();s.SetGuided(false);
            Assert.IsFalse(s.Submit("1,2,3",p,Now));Assert.IsTrue(s.Submit("3,2,1",p,Now));Assert.AreEqual(0,p.Topic("stack").independent);
        }
        [Test] public void GuidanceAndRevealPrecludeCleanEvidence()
        {
            var p=new TLSPhase7ProgressBook();var s=new TLSPhase7ReconstructionSession();s.Hint();s.SetGuided(false);
            Assert.IsTrue(s.Submit(s.Current.CorrectText,p,Now));Assert.AreEqual(0,p.Topic("stack").independent);
        }
        [TestCase("1,1,2")][TestCase("1,2")][TestCase("4,2,1")][TestCase("1,x,2")][TestCase("")]
        public void InvalidPermutationIsRejectedWithoutConsumingTry(string permutation)
        {
            Assert.IsNull(TLSPhase7ReconstructionSession.Parse(permutation,3));
        }
        [Test] public void InvalidOrderDoesNotMarkQuestionAttempted()
        {
            var p=new TLSPhase7ProgressBook();var s=new TLSPhase7ReconstructionSession();s.SetGuided(false);
            Assert.IsFalse(s.Submit("2,2,2",p,Now));Assert.IsFalse(p.Seen("parsons:"+s.Current.Id));
        }
    }
}
