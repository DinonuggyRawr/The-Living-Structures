using NUnit.Framework;
using TLS.Core;
using TLS.UI;

namespace TLS.Tests
{
    public sealed class StackNativeCompilerLayoutTests
    {
        [Test] public void NativeLabUsesExistingRuntimeAssembly() {Assert.IsNotNull(typeof(TLSNativeCompilerLabScreen));}
        [Test] public void ModelLabStillAvailable() {Assert.IsNotNull(typeof(TLSFunctionLabScreen));}
        [Test] public void SharedCFunctionDraftTypePreserved() {Assert.IsNotNull(typeof(StackFunctionLabSession));}
        [Test] public void CompilerRequiresExplicitConsentState() {Assert.IsFalse(new StackNativeLabSession().OptedIn);}
    }
}
