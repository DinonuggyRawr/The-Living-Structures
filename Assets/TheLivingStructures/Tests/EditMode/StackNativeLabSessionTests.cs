using NUnit.Framework;
using TLS.Core;

namespace TLS.Tests
{
    public sealed class StackNativeLabSessionTests
    {
        [Test] public void NativeRunnerStartsDisabled()
        { Assert.IsFalse(new StackNativeLabSession().OptedIn); }
        [Test] public void NativeRunWithoutOptInDoesNotStartProcess()
        {
            var s=new StackNativeLabSession();
            Assert.IsFalse(s.Start(StackFunctionEvaluator.Function.Push,"bool x(void){return false;}"));
            Assert.IsFalse(s.Busy);Assert.IsNull(s.Latest);
        }
        [Test] public void OptInCanBeRevokedWithoutAffectingModeledLab()
        {
            var n=new StackNativeLabSession();var m=new StackFunctionLabSession();
            string before=m.Draft;n.Enable();n.Disable();
            Assert.IsFalse(n.OptedIn);Assert.AreEqual(before,m.Draft);
        }
        [Test] public void NativeResultsNeverShareModeledCredit()
        {
            var n=new StackNativeLabSession();var m=new StackFunctionLabSession();
            Assert.AreEqual(0,n.CompletedRuns);Assert.AreEqual(0,m.PassedCount);
            Assert.AreEqual(0,m.IndependentFirstAttemptCount);
        }
        [Test] public void PollBeforeRunIsNoOp()
        { Assert.IsFalse(new StackNativeLabSession().Poll()); }
    }
}
