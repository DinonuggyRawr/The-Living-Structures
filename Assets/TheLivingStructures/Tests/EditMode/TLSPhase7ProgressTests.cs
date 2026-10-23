using System;
using NUnit.Framework;
using TLS.Core;
namespace TLS.Tests
{
    public sealed class TLSPhase7ProgressTests
    {
        private static DateTime T=new DateTime(2026,10,21,14,0,0,DateTimeKind.Utc);
        [Test] public void SeparateSaveKeyDoesNotCollideWithExistingAcademyKey()
        {Assert.AreEqual("tls_phase7_learning_evidence_v1",TLSPhase7ProgressBook.SaveKey);}
        [Test] public void FreshProgressIsUntrackedAndDoesNotInventMastery()
        {
            var p=new TLSPhase7ProgressBook();Assert.AreEqual(-1,p.Data.diagnosticCorrect);
            foreach(var e in p.Data.topics){Assert.AreEqual(0,e.views);Assert.AreEqual(0,e.independent);Assert.AreEqual(0,e.compilerCases);}
        }
        [Test] public void AViewDoesNotAwardGuidedIndependentOrCompilerEvidence()
        {
            var p=new TLSPhase7ProgressBook();p.MarkViewed("array");p.MarkViewed("array");
            var t=p.Topic("array");Assert.AreEqual(1,t.views);Assert.AreEqual(0,t.guided);Assert.AreEqual(0,t.independent);Assert.AreEqual(0,t.compilerCases);
        }
        [Test] public void GuidedEvidenceIsNotAnIndependentAttempt()
        {
            var p=new TLSPhase7ProgressBook();Assert.IsFalse(p.RecordAttempt("exercise1","array",true,true,T));
            Assert.AreEqual(1,p.Topic("array").guided);Assert.AreEqual(0,p.Topic("array").independent);
        }
        [Test] public void IndependentFirstPassGetsCreditExactlyOnce()
        {
            var p=new TLSPhase7ProgressBook();Assert.IsTrue(p.RecordAttempt("first","stack",true,false,T));
            Assert.IsFalse(p.RecordAttempt("first","stack",true,false,T));Assert.AreEqual(1,p.Topic("stack").independent);
        }
        [Test] public void IncorrectFirstAttemptBlocksUnseenEvidenceOnRetry()
        {
            var p=new TLSPhase7ProgressBook();p.RecordAttempt("err","queue",false,false,T);
            Assert.IsFalse(p.RecordAttempt("err","queue",true,false,T));Assert.AreEqual(0,p.Topic("queue").independent);
        }
        [Test] public void CompilerCaseCannotBeAwardedWithoutCompletedRealTests()
        {
            var p=new TLSPhase7ProgressBook();
            Assert.IsFalse(p.RecordCompilerCase("x","array",false,true,true,false,false,T));
            Assert.IsFalse(p.RecordCompilerCase("x","array",true,false,true,false,false,T));
            Assert.IsFalse(p.RecordCompilerCase("x","array",true,true,false,false,false,T));
            Assert.AreEqual(0,p.Topic("array").compilerCases);
        }
        [Test] public void CorrectCleanC11CaseRecordsCompilerEvidenceOnce()
        {
            var p=new TLSPhase7ProgressBook();
            Assert.IsTrue(p.RecordCompilerCase("remove","array",true,true,true,false,false,T));
            Assert.IsFalse(p.RecordCompilerCase("remove","array",true,true,true,false,true,T));
            Assert.AreEqual(1,p.Topic("array").compilerCases);
        }
        [Test] public void AssistedCompilerCaseDoesNotGetIndependentCredit()
        {
            var p=new TLSPhase7ProgressBook();Assert.IsFalse(p.RecordCompilerCase("remove","array",true,true,true,true,false,T));
            Assert.AreEqual(0,p.Topic("array").compilerCases);Assert.AreEqual(1,p.Topic("array").guided);
        }
        [Test] public void ProgressRoundTripPreservesUniqueEvidenceAndReviewTicks()
        {
            var p=new TLSPhase7ProgressBook();p.RecordAttempt("fresh","bst",true,false,T);
            string json=p.ExportJson();TLSPhase7ProgressBook clone;
            Assert.IsTrue(TLSPhase7ProgressBook.TryParse(json,out clone));
            Assert.AreEqual(1,clone.Topic("bst").independent);Assert.IsTrue(clone.Seen("fresh"));
            Assert.IsTrue(clone.ReviewDue("bst",T.AddDays(4)));
        }
        [Test] public void CorruptOrUnknownVersionIsRefusedWithoutOverwritingPriorData()
        {
            TLSPhase7ProgressBook ignored;
            Assert.IsFalse(TLSPhase7ProgressBook.TryParse("broken json",out ignored));
            var p=TLSPhase7ProgressBook.Fresh();p.version=99;
            Assert.IsFalse(TLSPhase7ProgressBook.Valid(p));
        }
        [Test] public void DuplicateAttemptIdsAreInvalid()
        {
            var p=TLSPhase7ProgressBook.Fresh();p.attemptedIds=new[]{"a","a"};Assert.IsFalse(TLSPhase7ProgressBook.Valid(p));
        }
        [Test] public void IndependentRecordMustHaveCorrespondingAttemptId()
        {
            var p=TLSPhase7ProgressBook.Fresh();p.independentIds=new[]{"unseen"};
            Assert.IsFalse(TLSPhase7ProgressBook.Valid(p));
        }
        [Test] public void ReviewIsDueOnlyAfterPracticeAndTimeHasElapsed()
        {
            var p=new TLSPhase7ProgressBook();Assert.IsFalse(p.ReviewDue("list",T.AddYears(1)));
            p.RecordAttempt("guided-list","list",true,true,T);
            Assert.IsFalse(p.ReviewDue("list",T.AddHours(23)));
            Assert.IsTrue(p.ReviewDue("list",T.AddDays(2)));
        }
        [Test] public void UnknownTopicDoesNotSilentlyWriteIntoAnotherTopic()
        {Assert.Throws<ArgumentOutOfRangeException>(()=>new TLSPhase7ProgressBook().MarkViewed("unknown"));}
        [Test] public void NonUtcTimestampsAreRejected()
        {Assert.Throws<ArgumentException>(()=>new TLSPhase7ProgressBook().RecordAttempt("x","stack",true,false,DateTime.Now));}
    }
}
