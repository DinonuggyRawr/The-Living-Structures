using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TLS.Core
{
    /// <summary>
    /// Phase 2.4 Windows toolchain discovery and accidental-corruption checks.
    /// The Editor uses project-local development tools; the Windows player requires
    /// a staged manifest beside the executable. No download, PATH search, registry
    /// access, student-controlled tool paths, or subprocesses occur in this class.
    /// Manifest hashes are not signatures: this is NOT a hostile-tampering boundary.
    /// </summary>
    public static class TLSBundledToolchain
    {
        public const string DirectoryName = "TLS_Toolchain_Windows";
        public const string ManifestName = "TLS_BUNDLE_MANIFEST_V1.txt";
        public const string ManifestHeader = "TLS_BUNDLE_V1";
        public const string ZigRelative = "zig/zig.exe";
        public const string WasmtimeRelative = "wasmtime/wasmtime.exe";

        public sealed class CheckResult
        {
            public bool Ready;
            public bool ManifestVerified;
            public string Message = "Not checked.";
            public string Root = "";
        }

        public static string RootFromUnityDataPath(string dataPath)
        {
            if (string.IsNullOrWhiteSpace(dataPath))
                throw new ArgumentException("Unity Application.dataPath is required.", "dataPath");
            // Editor: <Project>/Assets -> <Project>/TLS_Toolchain_Windows.
            // Windows player: <Build>/<Product>_Data -> <Build>/TLS_Toolchain_Windows.
            return Path.GetFullPath(Path.Combine(dataPath, "..", DirectoryName));
        }

        public static string ZigPath(string root) { return Path.Combine(root ?? "", "zig", "zig.exe"); }
        public static string WasmtimePath(string root) { return Path.Combine(root ?? "", "wasmtime", "wasmtime.exe"); }

        public static string Sha256(string path)
        {
            using (SHA256 hasher = SHA256.Create())
            using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                return BitConverter.ToString(hasher.ComputeHash(file)).Replace("-", "").ToLowerInvariant();
        }

        public static string MakeManifest(string zigSha256, string wasmtimeSha256)
        {
            if (!IsDigest(zigSha256) || !IsDigest(wasmtimeSha256))
                throw new ArgumentException("Manifest requires two SHA-256 digests.");
            return ManifestHeader + "\nzig_sha256=" + zigSha256.ToLowerInvariant() +
                "\nwasmtime_sha256=" + wasmtimeSha256.ToLowerInvariant() + "\n";
        }

        private static bool IsDigest(string value)
        {
            if (value == null || value.Length != 64) return false;
            foreach (char c in value)
                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
                    return false;
            return true;
        }

        private static bool ReadManifest(string root, out string zig, out string wasmtime)
        {
            zig = null; wasmtime = null;
            try
            {
                string[] lines = File.ReadAllLines(Path.Combine(root, ManifestName));
                if (lines.Length != 3 || lines[0] != ManifestHeader ||
                    !lines[1].StartsWith("zig_sha256=", StringComparison.Ordinal) ||
                    !lines[2].StartsWith("wasmtime_sha256=", StringComparison.Ordinal)) return false;
                zig = lines[1].Substring("zig_sha256=".Length);
                wasmtime = lines[2].Substring("wasmtime_sha256=".Length);
                return IsDigest(zig) && IsDigest(wasmtime);
            }
            catch (IOException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
        }

        public static CheckResult Check(string root, bool requirePackagedManifest)
        {
            CheckResult result = new CheckResult { Root = root ?? "" };
            try
            {
                if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
                {
                    result.Message = requirePackagedManifest ?
                        "Offline C compiler bundle missing beside the Windows application. Visual lessons still work." :
                        "Project-local Zig/Wasmtime tools missing. Use the Unity toolchain setup menu.";
                    return result;
                }
                if (!File.Exists(ZigPath(root)) || !Directory.Exists(Path.Combine(root, "zig", "lib")) ||
                    !File.Exists(WasmtimePath(root)))
                {
                    result.Message = "Incomplete Zig/Wasmtime toolchain. C compiler tests are disabled; visual lessons still work.";
                    return result;
                }
                string manifest = Path.Combine(root, ManifestName);
                if (!File.Exists(manifest))
                {
                    if (requirePackagedManifest)
                    {
                        result.Message = "Offline bundle manifest is missing. C compiler tests are disabled.";
                        return result;
                    }
                    result.Ready = true;
                    result.Message = "Development Zig/Wasmtime tools found (unmanifested). Ready for opt-in C tests; packaging verification pending.";
                    return result;
                }
                string zigDigest, wasmtimeDigest;
                if (!ReadManifest(root, out zigDigest, out wasmtimeDigest))
                {
                    result.Message = "Offline toolchain manifest is malformed. C compiler tests are disabled.";
                    return result;
                }
                if (!string.Equals(Sha256(ZigPath(root)), zigDigest, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(Sha256(WasmtimePath(root)), wasmtimeDigest, StringComparison.OrdinalIgnoreCase))
                {
                    result.Message = "Offline toolchain checksum mismatch. C compiler tests are disabled; do not run these binaries.";
                    return result;
                }
                result.Ready = true;
                result.ManifestVerified = true;
                result.Message = "Offline C11 toolchain files verified: Zig and Wasmtime ready for opt-in tests.";
                return result;
            }
            catch (IOException) { result.Message = "Unable to read compiler tools; C tests disabled."; return result; }
            catch (UnauthorizedAccessException) { result.Message = "No permission to read compiler tools; C tests disabled."; return result; }
        }
    }
}
