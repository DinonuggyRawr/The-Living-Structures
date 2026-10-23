using System;
using TLS.Core;

namespace TLS.UI
{
    /// <summary>
    /// Session-only cache for the Academy's informational compiler badge.
    /// The real C11 runners NEVER use this cache for authorization or integrity checks;
    /// they still call TLSBundledToolchain.Check before starting compilation.
    /// </summary>
    public sealed class TLSPhase8AcademyReadinessCache
    {
        private bool cached;
        private string root;
        private bool requirePackagedManifest;
        private TLSBundledToolchain.CheckResult result;

        public TLSBundledToolchain.CheckResult Get(string toolchainRoot, bool requireManifest,
            Func<string, bool, TLSBundledToolchain.CheckResult> check)
        {
            if (check == null) throw new ArgumentNullException("check");
            if (cached && string.Equals(root, toolchainRoot, StringComparison.Ordinal) &&
                requirePackagedManifest == requireManifest)
                return result;

            TLSBundledToolchain.CheckResult checkedResult = check(toolchainRoot, requireManifest);
            if (checkedResult == null)
                throw new InvalidOperationException("Toolchain check returned no status.");
            root = toolchainRoot;
            requirePackagedManifest = requireManifest;
            result = checkedResult;
            cached = true;
            return result;
        }

        public void Invalidate()
        {
            cached = false;
            root = null;
            result = null;
        }
    }
}
