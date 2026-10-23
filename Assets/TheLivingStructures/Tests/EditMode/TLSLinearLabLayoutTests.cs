using System;
using NUnit.Framework;
using TLS.Core;
using TLS.UI;
using UnityEngine;

namespace TLS.Tests
{
    public sealed class TLSLinearLabLayoutTests
    {
        [Test] public void ArrayAndQueueRenderEntryPointsExist()
        {
            Assert.IsNotNull(typeof(TLSLinearLabsScreen).GetMethod("RenderArray"));
            Assert.IsNotNull(typeof(TLSLinearLabsScreen).GetMethod("RenderQueue"));
        }
        [Test] public void LogicCanBeTestedWithoutMonoBehaviour()
        {
            Assert.IsFalse(typeof(MonoBehaviour).IsAssignableFrom(typeof(CArrayLabSession)));
            Assert.IsFalse(typeof(MonoBehaviour).IsAssignableFrom(typeof(CQueueLabSession)));
            Assert.IsFalse(typeof(MonoBehaviour).IsAssignableFrom(typeof(CArrayState)));
            Assert.IsFalse(typeof(MonoBehaviour).IsAssignableFrom(typeof(CQueueState)));
        }
        [TestCase(CArrayOperation.Append, 7)]
        [TestCase(CArrayOperation.Insert, 9)]
        [TestCase(CArrayOperation.Remove, 9)]
        [TestCase(CArrayOperation.Get, 6)]
        public void ArrayDisplayedCodeHasExpectedLineCount(CArrayOperation op, int expected)
        {
            Assert.AreEqual(expected, CArraySimulation.CodeFor(op).Length);
        }
        [TestCase(CQueueOperation.Enqueue, 8)]
        [TestCase(CQueueOperation.Dequeue, 8)]
        [TestCase(CQueueOperation.Peek, 6)]
        public void QueueDisplayedCodeHasExpectedLineCount(CQueueOperation op, int expected)
        {
            Assert.AreEqual(expected, CQueueSimulation.CodeFor(op).Length);
        }
        [Test] public void TraceCodeIndicesAlwaysWithinDisplayBounds()
        {
            foreach (CArrayOperation op in Enum.GetValues(typeof(CArrayOperation)))
            {
                var trace = CArraySimulation.Execute(new CArrayState(), op, 0, 3);
                foreach (var frame in trace.Frames)
                    Assert.That(frame.CodeLine, Is.InRange(0, CArraySimulation.CodeFor(op).Length - 1));
            }
            foreach (CQueueOperation op in Enum.GetValues(typeof(CQueueOperation)))
            {
                var trace = CQueueSimulation.Execute(new CQueueState(), op, 3);
                foreach (var frame in trace.Frames)
                    Assert.That(frame.CodeLine, Is.InRange(0, CQueueSimulation.CodeFor(op).Length - 1));
            }
        }
        [Test] public void LegacyStackUIStillExistsSeparately()
        {
            Assert.IsNotNull(typeof(TLSAcademyApp));
            Assert.IsNotNull(typeof(TLSFunctionLabScreen).GetMethod("Render"));
            Assert.IsNotNull(typeof(TLSWasmCompilerLabScreen).GetMethod("Render"));
        }
    }
}
