#if UNITY_EDITOR
using System;
using System.IO;
using System.Security.Cryptography;
using TLS.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TLS.Editor
{
    /// <summary>
    /// Phase 2.4 offline Windows BUILD prototype. A user-invoked Unity build stages
    /// local, previously verified tools next to the generated .exe; no downloads.
    /// Does not mutate Assets, the original toolchain, or any progress files.
    /// Windows-only, developer-side prototype; not a public distribution approval.
    /// </summary>
    public sealed class TLSOfflineBundlePostprocessor : IPostprocessBuildWithReport
    {
        public int callbackOrder { get { return 500; } }

        [MenuItem("Tools/The Living Structures/Check Offline Compiler Readiness")]
        private static void CheckCurrentToolchain()
        {
            string root = TLSBundledToolchain.RootFromUnityDataPath(Application.dataPath);
            TLSBundledToolchain.CheckResult check = TLSBundledToolchain.Check(root, false);
            EditorUtility.DisplayDialog("The Living Structures - C11 Toolchain", check.Message +
                "\n\nWindows builds will copy the project-local tools beside the executable. " +
                "The built app will require a bundle manifest; the Unity Editor does not.", "OK");
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.StandaloneWindows64) return;
            string exe = report.summary.outputPath;
            if (!exe.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                throw new BuildFailedException("Offline compiler bundling requires a Windows .exe build output.");
            string root = TLSBundledToolchain.RootFromUnityDataPath(Application.dataPath);
            string destination = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(exe)), TLSBundledToolchain.DirectoryName);
            try
            {
                StageBundle(root, destination);
                Debug.Log("TLS Phase 2.4: Offline Zig + Wasmtime staged beside Windows executable. " +
                    "Run clean-machine/offline acceptance tests before distribution.");
            }
            catch (Exception e)
            {
                throw new BuildFailedException("TLS offline compiler bundle failed: " + e.Message +
                    " The Windows build must not be presented as a ready-to-distribute offline release.");
            }
        }

        public static void StageBundle(string source, string destination)
        {
            TLSBundledToolchain.CheckResult sourceCheck = TLSBundledToolchain.Check(source, false);
            if (!sourceCheck.Ready) throw new InvalidOperationException(sourceCheck.Message);
            source = Path.GetFullPath(source);
            destination = Path.GetFullPath(destination);
            if (string.Equals(source.TrimEnd(Path.DirectorySeparatorChar),
                destination.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Build output cannot be the original toolchain folder.");
            // Reject build output inside the source tree; otherwise recursive
            // copying could replicate itself and consume the developer disk.
            if (destination.StartsWith(source.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Build output must be outside the toolchain source directory.");
            if (Directory.Exists(destination))
            {
                TLSBundledToolchain.CheckResult existing = TLSBundledToolchain.Check(destination, true);
                if (!existing.Ready) throw new InvalidOperationException(
                    "An incomplete or changed toolchain already exists beside the build. Remove it manually before rebuilding.");
                string srcZig = TLSBundledToolchain.Sha256(TLSBundledToolchain.ZigPath(source));
                string srcWasm = TLSBundledToolchain.Sha256(TLSBundledToolchain.WasmtimePath(source));
                if (srcZig != TLSBundledToolchain.Sha256(TLSBundledToolchain.ZigPath(destination)) ||
                    srcWasm != TLSBundledToolchain.Sha256(TLSBundledToolchain.WasmtimePath(destination)))
                    throw new InvalidOperationException("Build's existing compiler version differs. Remove old bundle manually.");
                return; // Verified previous stage; no silent overwrite.
            }
            string staging = destination + ".staging-" + Guid.NewGuid().ToString("N");
            try
            {
                Directory.CreateDirectory(staging);
                CopyTree(Path.Combine(source, "zig"), Path.Combine(staging, "zig"));
                CopyTree(Path.Combine(source, "wasmtime"), Path.Combine(staging, "wasmtime"));
                string manifest = TLSBundledToolchain.MakeManifest(
                    TLSBundledToolchain.Sha256(TLSBundledToolchain.ZigPath(staging)),
                    TLSBundledToolchain.Sha256(TLSBundledToolchain.WasmtimePath(staging)));
                File.WriteAllText(Path.Combine(staging, TLSBundledToolchain.ManifestName), manifest);
                File.WriteAllText(Path.Combine(staging, "BUNDLING_PROTOTYPE_README.txt"),
                    "The Living Structures Phase 2.4 offline packaging prototype.\r\n" +
                    "Zig and Wasmtime binaries were copied from the developer's local toolchain.\r\n" +
                    "The manifest detects accidental executable changes; it is not a digital signature.\r\n" +
                    "BEFORE PUBLIC DISTRIBUTION: review license/NOTICE files for ALL included dependencies,\r\n" +
                    "supply required attribution, test resource limits and hostile inputs, and validate\r\n" +
                    "offline Windows builds on a clean machine. No runtime downloads are performed.\r\n");
                if (!TLSBundledToolchain.Check(staging, true).Ready)
                    throw new IOException("Staged toolchain did not pass executable hash verification.");
                Directory.Move(staging, destination);
            }
            finally
            {
                if (Directory.Exists(staging)) Directory.Delete(staging, true);
            }
        }

        private static void CopyTree(string source, string destination)
        {
            if (!Directory.Exists(source)) throw new DirectoryNotFoundException("Toolchain directory missing: " + source);
            if ((File.GetAttributes(source) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("Toolchain contains a linked directory. Refusing to follow it: " + source);
            Directory.CreateDirectory(destination);
            foreach (string file in Directory.GetFiles(source))
            {
                if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)) continue;
                if ((File.GetAttributes(file) & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("Toolchain contains a linked file. Refusing to copy it: " + file);
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), false);
            }
            foreach (string folder in Directory.GetDirectories(source))
                CopyTree(folder, Path.Combine(destination, Path.GetFileName(folder)));
        }
    }
}
#endif
