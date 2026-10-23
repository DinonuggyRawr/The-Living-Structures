using System;
using NUnit.Framework;
using TLS.Core;

namespace TLS.Tests
{
    public sealed class TLSLinearFunctionCatalogTests
    {
        [TestCase(TLSLinearFunction.ArrayAppend)]
        [TestCase(TLSLinearFunction.ArrayInsert)]
        [TestCase(TLSLinearFunction.ArrayRemove)]
        [TestCase(TLSLinearFunction.ArrayGet)]
        [TestCase(TLSLinearFunction.QueueEnqueue)]
        [TestCase(TLSLinearFunction.QueueDequeue)]
        [TestCase(TLSLinearFunction.QueuePeek)]
        public void EveryFunctionHasCompleteC11ReferenceAndStarter(TLSLinearFunction operation)
        {
            string refCode = TLSLinearFunctionCatalog.Reference(operation);
            Assert.That(refCode, Does.StartWith("bool "));
            Assert.That(refCode, Does.Contain("return true;"));
            Assert.That(refCode, Does.Contain("return false;"));
            Assert.That(refCode, Does.Not.Contain("new Node"));
            Assert.That(TLSLinearFunctionCatalog.Starter(operation), Does.Contain("TODO"));
            Assert.That(TLSLinearFunctionCatalog.Hint(operation).Length, Is.GreaterThan(30));
            Assert.That(TLSLinearFunctionCatalog.Goal(operation).Length, Is.GreaterThan(30));
        }
        [Test] public void CatalogCoversExactlyFourArraysAndThreeQueues()
        {
            Assert.AreEqual(7,TLSLinearFunctionCatalog.Count);
            for (int i=0;i<7;i++)
            {
                Assert.AreEqual(i<4,TLSLinearFunctionCatalog.IsArray((TLSLinearFunction)i));
                Assert.AreEqual(i>=4,TLSLinearFunctionCatalog.IsQueue((TLSLinearFunction)i));
            }
        }
        [Test] public void ArrayShiftingDirectionsDiffer()
        {
            Assert.That(TLSLinearFunctionCatalog.Reference(TLSLinearFunction.ArrayInsert),Does.Contain("i > index; --i"));
            Assert.That(TLSLinearFunctionCatalog.Reference(TLSLinearFunction.ArrayRemove),Does.Contain("i + 1 < a->count; ++i"));
        }
        [Test] public void QueueCodeExplicitlyUsesCircularModulo()
        {
            Assert.That(TLSLinearFunctionCatalog.Reference(TLSLinearFunction.QueueEnqueue),Does.Contain("% QUEUE_CAPACITY"));
            Assert.That(TLSLinearFunctionCatalog.Reference(TLSLinearFunction.QueueDequeue),Does.Contain("% QUEUE_CAPACITY"));
        }
        [Test] public void PeekCannotChangeHeadOrCount()
        {
            string body=TLSLinearFunctionCatalog.Reference(TLSLinearFunction.QueuePeek);
            Assert.That(body,Does.Not.Contain("q->head ="));
            Assert.That(body,Does.Not.Contain("q->count++"));
            Assert.That(body,Does.Not.Contain("q->count--"));
        }
        [Test] public void UnknownFunctionIsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(()=>TLSLinearFunctionCatalog.Reference((TLSLinearFunction)90));
            Assert.IsFalse(TLSLinearFunctionCatalog.IsValid((TLSLinearFunction)90));
        }
    }
}
