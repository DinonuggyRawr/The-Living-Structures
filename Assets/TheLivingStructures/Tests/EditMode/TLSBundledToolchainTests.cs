using System;
using System.IO;
using NUnit.Framework;
using TLS.Core;

namespace TLS.Tests
{
    public sealed class TLSBundledToolchainTests
    {
        private string temp;
        private string folder;

        [SetUp] public void Before()
        {
            temp = Path.Combine(Path.GetTempPath(), "tls-2-4-tests-" + Guid.NewGuid().ToString("N"));
            folder = Path.Combine(temp, TLSBundledToolchain.DirectoryName);
            Directory.CreateDirectory(Path.Combine(folder, "zig", "lib"));
            Directory.CreateDirectory(Path.Combine(folder, "wasmtime"));
            File.WriteAllText(TLSBundledToolchain.ZigPath(folder), "fake-tool-in-test-only");
            File.WriteAllText(TLSBundledToolchain.WasmtimePath(folder), "fake-runtime-in-test-only");
        }
        [TearDown] public void After() { if (Directory.Exists(temp)) Directory.Delete(temp, true); }

        private void Manifest()
        {
            File.WriteAllText(Path.Combine(folder, TLSBundledToolchain.ManifestName),
                TLSBundledToolchain.MakeManifest(TLSBundledToolchain.Sha256(TLSBundledToolchain.ZigPath(folder)),
                TLSBundledToolchain.Sha256(TLSBundledToolchain.WasmtimePath(folder))));
        }
        [Test] public void EditorFindsProjectLocalToolsWithoutManifest()
        {
            var result = TLSBundledToolchain.Check(folder, false);
            Assert.IsTrue(result.Ready);
            Assert.IsFalse(result.ManifestVerified);
        }
        [Test] public void StandaloneRefusesUnmanifestedTools()
        {
            Assert.IsFalse(TLSBundledToolchain.Check(folder, true).Ready);
        }
        [Test] public void ValidManifestVerifiesBothExecutableHashes()
        {
            Manifest();var result = TLSBundledToolchain.Check(folder, true);
            Assert.IsTrue(result.Ready); Assert.IsTrue(result.ManifestVerified);
        }
        [Test] public void CorruptZigFailsClosed()
        {
            Manifest();File.AppendAllText(TLSBundledToolchain.ZigPath(folder), "CORRUPT");
            Assert.IsFalse(TLSBundledToolchain.Check(folder, true).Ready);
        }
        [Test] public void CorruptWasmtimeFailsClosed()
        {
            Manifest();File.AppendAllText(TLSBundledToolchain.WasmtimePath(folder), "CORRUPT");
            Assert.IsFalse(TLSBundledToolchain.Check(folder, true).Ready);
        }
        [Test] public void InvalidManifestFailsClosedEvenInEditor()
        {
            File.WriteAllText(Path.Combine(folder, TLSBundledToolchain.ManifestName), "invalid");
            Assert.IsFalse(TLSBundledToolchain.Check(folder, false).Ready);
        }
        [Test] public void MissingLibraryDirectoryIsIncomplete()
        {
            Directory.Delete(Path.Combine(folder, "zig", "lib"));
            Assert.IsFalse(TLSBundledToolchain.Check(folder, false).Ready);
        }
        [Test] public void MissingExecutableDoesNotPass()
        {
            File.Delete(TLSBundledToolchain.WasmtimePath(folder));
            Assert.IsFalse(TLSBundledToolchain.Check(folder, false).Ready);
        }
        [Test] public void DataPathResolvesSiblingForEditorAndPlayer()
        {
            Assert.AreEqual(Path.Combine(temp, TLSBundledToolchain.DirectoryName),
                TLSBundledToolchain.RootFromUnityDataPath(Path.Combine(temp, "Assets")));
            Assert.AreEqual(Path.Combine(temp, TLSBundledToolchain.DirectoryName),
                TLSBundledToolchain.RootFromUnityDataPath(Path.Combine(temp, "TheLivingStructures_Data")));
        }
        [Test] public void ManifestRejectsInvalidDigests()
        {
            Assert.Throws<ArgumentException>(() => TLSBundledToolchain.MakeManifest("x",new string('f',64)));
        }
        [Test] public void MissingRootHasActionableStatus()
        {
            Assert.IsFalse(TLSBundledToolchain.Check(Path.Combine(temp,"not-there"),true).Ready);
            Assert.That(TLSBundledToolchain.Check(Path.Combine(temp,"not-there"),true).Message,Does.Contain("missing"));
        }
        [Test] public void PinnedNamesMatchExistingRunnerLayout()
        {
            Assert.That(TLSBundledToolchain.ZigPath(folder),Does.EndWith(Path.Combine("zig","zig.exe")));
            Assert.That(TLSBundledToolchain.WasmtimePath(folder),Does.EndWith(Path.Combine("wasmtime","wasmtime.exe")));
        }
    }
}
