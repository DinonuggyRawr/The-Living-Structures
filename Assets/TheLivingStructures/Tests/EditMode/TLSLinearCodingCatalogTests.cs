using System;
using System.Collections.Generic;
using NUnit.Framework;
using TLS.Core;

namespace TLS.Tests
{
    public sealed class TLSLinearCodingCatalogTests
    {
        [Test] public void EveryTrackContainsThreeTasksPerDomainWithUniqueIds()
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (LinearPracticeDomain domain in Enum.GetValues(typeof(LinearPracticeDomain)))
            foreach (LinearPracticeTrack track in Enum.GetValues(typeof(LinearPracticeTrack)))
            for (int i = 0; i < LinearPracticeCatalog.TasksPerTrack; i++)
            {
                LinearPracticeTask t = LinearPracticeCatalog.Get(domain, track, i);
                Assert.IsTrue(seen.Add(t.Id)); Assert.AreEqual(track, t.Track); Assert.AreEqual(domain, t.Domain);
                Assert.IsNotEmpty(t.Source); Assert.IsNotEmpty(t.Question); Assert.IsNotEmpty(t.Reference);
                Assert.IsNotEmpty(t.Hint); Assert.IsNotEmpty(t.Reasoning); Assert.IsNotEmpty(t.Pitfall);
                Assert.IsTrue(LinearPracticeCatalog.MatchesReference(t, t.Reference), t.Id);
            }
            Assert.AreEqual(18, seen.Count);
        }
        [TestCase(LinearPracticeDomain.Array)] [TestCase(LinearPracticeDomain.Queue)]
        public void EachDomainHasNineDistinctExercises(LinearPracticeDomain domain)
        {
            var seen = new HashSet<string>();
            foreach (LinearPracticeTrack track in Enum.GetValues(typeof(LinearPracticeTrack)))
            for (int i = 0; i < 3; i++) seen.Add(LinearPracticeCatalog.Get(domain, track, i).Id);
            Assert.AreEqual(9, seen.Count);
        }
        [Test] public void RejectInvalidIndexesAndDomainTrackValues()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => LinearPracticeCatalog.Get(LinearPracticeDomain.Array, LinearPracticeTrack.Trace, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => LinearPracticeCatalog.Get(LinearPracticeDomain.Queue, LinearPracticeTrack.Repair, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => LinearPracticeCatalog.Get((LinearPracticeDomain)123, LinearPracticeTrack.Trace, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => LinearPracticeCatalog.Get(LinearPracticeDomain.Array, (LinearPracticeTrack)123, 0));
        }
        [TestCase(LinearPracticeDomain.Array, 0, 20)]
        [TestCase(LinearPracticeDomain.Array, 1, 8)]
        [TestCase(LinearPracticeDomain.Array, 2, 7)]
        [TestCase(LinearPracticeDomain.Queue, 0, 6)]
        [TestCase(LinearPracticeDomain.Queue, 1, 2)]
        [TestCase(LinearPracticeDomain.Queue, 2, 60)]
        public void TraceNumbersMatchModeledCOperations(LinearPracticeDomain domain, int index, int expected)
        {
            LinearPracticeTask task = LinearPracticeCatalog.Get(domain, LinearPracticeTrack.Trace, index);
            Assert.IsTrue(task.IntegerAnswer);
            Assert.AreEqual(expected.ToString(), task.Reference);
            Assert.IsTrue(LinearPracticeCatalog.MatchesReference(task, " " + expected + " "));
            Assert.IsFalse(LinearPracticeCatalog.MatchesReference(task, (expected + 1).ToString()));
        }
        [TestCase(LinearPracticeDomain.Array, LinearPracticeTrack.Complete, 0)]
        [TestCase(LinearPracticeDomain.Array, LinearPracticeTrack.Complete, 1)]
        [TestCase(LinearPracticeDomain.Array, LinearPracticeTrack.Complete, 2)]
        [TestCase(LinearPracticeDomain.Array, LinearPracticeTrack.Repair, 0)]
        [TestCase(LinearPracticeDomain.Array, LinearPracticeTrack.Repair, 1)]
        [TestCase(LinearPracticeDomain.Array, LinearPracticeTrack.Repair, 2)]
        [TestCase(LinearPracticeDomain.Queue, LinearPracticeTrack.Complete, 0)]
        [TestCase(LinearPracticeDomain.Queue, LinearPracticeTrack.Complete, 1)]
        [TestCase(LinearPracticeDomain.Queue, LinearPracticeTrack.Complete, 2)]
        [TestCase(LinearPracticeDomain.Queue, LinearPracticeTrack.Repair, 0)]
        [TestCase(LinearPracticeDomain.Queue, LinearPracticeTrack.Repair, 1)]
        [TestCase(LinearPracticeDomain.Queue, LinearPracticeTrack.Repair, 2)]
        public void ReferenceTokenGradingAllowsWhitespaceAndRejectsExtraText(LinearPracticeDomain domain, LinearPracticeTrack track, int index)
        {
            LinearPracticeTask task = LinearPracticeCatalog.Get(domain, track, index);
            Assert.IsFalse(task.IntegerAnswer);
            Assert.IsTrue(LinearPracticeCatalog.MatchesReference(task, " \n" + task.Reference + "\t"));
            Assert.IsFalse(LinearPracticeCatalog.MatchesReference(task, task.Reference + " return true;"));
        }
        [Test] public void MultiCharacterOperatorsMustRemainAtomic()
        {
            var task = LinearPracticeCatalog.Get(LinearPracticeDomain.Queue, LinearPracticeTrack.Repair, 1);
            Assert.IsTrue(LinearPracticeCatalog.MatchesReference(task, task.Reference));
            Assert.IsFalse(LinearPracticeCatalog.MatchesReference(task, task.Reference.Replace("||", "| |")));
            Assert.IsFalse(LinearPracticeCatalog.MatchesReference(task, task.Reference.Replace("==", "= =")));
        }
        [Test] public void ShiftDirectionIsNotInterchangeable()
        {
            var insert = LinearPracticeCatalog.Get(LinearPracticeDomain.Array, LinearPracticeTrack.Complete, 1);
            var remove = LinearPracticeCatalog.Get(LinearPracticeDomain.Array, LinearPracticeTrack.Complete, 2);
            Assert.IsFalse(LinearPracticeCatalog.MatchesReference(insert, remove.Reference));
            Assert.IsFalse(LinearPracticeCatalog.MatchesReference(remove, insert.Reference));
        }
        [Test] public void QueueTailRequiresModulo()
        {
            var task = LinearPracticeCatalog.Get(LinearPracticeDomain.Queue, LinearPracticeTrack.Complete, 0);
            Assert.IsFalse(LinearPracticeCatalog.MatchesReference(task, "int tail = q->head + q->count;"));
        }
        [Test] public void MalformedOrLongAnswersNeverMatch()
        {
            var task = LinearPracticeCatalog.Get(LinearPracticeDomain.Array, LinearPracticeTrack.Complete, 0);
            Assert.IsFalse(LinearPracticeCatalog.MatchesReference(task, ""));
            Assert.IsFalse(LinearPracticeCatalog.MatchesReference(task, "a->data[0] = value; // extra"));
            Assert.IsFalse(LinearPracticeCatalog.MatchesReference(task, new string('x', 513)));
        }
    }
}
