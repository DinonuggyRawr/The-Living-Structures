#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using TLS.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TLS.Editor
{
    /// <summary>
    /// Distribution audit AFTER existing Phase 2.4 tool staging (callback 500).
    /// Reports observed file readiness, not proof of safe code execution or legal clearance.
    /// Never downloads tools or alters existing student progress.
    /// </summary>
    public sealed class TLSPhase8ReleaseAudit : IPostprocessBuildWithReport
    {
        public int callbackOrder { get { return 650; } }
        [MenuItem("Tools/The Living Structures/Audit Local Offline Compiler Bundle")]
        private static void AuditProject()
        {
            string root=TLSBundledToolchain.RootFromUnityDataPath(Application.dataPath);
            TLSBundledToolchain.CheckResult status=TLSBundledToolchain.Check(root,false);
            EditorUtility.DisplayDialog("The Living Structures — local bundle",status.Message+"\n\n"+
                "A green local check does not authorize redistribution. Build a Windows player, check its license/NOTICE audit, and run offline clean-machine tests.","OK");
        }
        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform!=BuildTarget.StandaloneWindows64) return;
            string output=Path.GetDirectoryName(Path.GetFullPath(report.summary.outputPath));
            string bundle=Path.Combine(output,TLSBundledToolchain.DirectoryName);
            TLSBundledToolchain.CheckResult status=TLSBundledToolchain.Check(bundle,true);
            string reportFile=Path.Combine(output,"TLS_PHASE8_RELEASE_READINESS.txt");
            int noticeCount=0;
            var audit=new StringBuilder();
            audit.AppendLine("THE LIVING STRUCTURES — PHASE 8 RELEASE REVIEW");
            audit.AppendLine("GENERATED: "+DateTime.UtcNow.ToString("u")+" UTC");
            audit.AppendLine("PUBLIC RELEASE STATUS: NOT VERIFIED / DO NOT DISTRIBUTE YET");
            audit.AppendLine("Bundled Zig/Wasmtime executables: "+(status.Ready&&status.ManifestVerified?"HASHES MATCH":"MISSING/FAILED"));
            audit.AppendLine("Bundle detail: "+status.Message);
            audit.AppendLine("\nDISCOVERED POTENTIAL NOTICE/LICENSING FILES (manual review required):");
            if(Directory.Exists(bundle))
            {
                foreach(string file in Directory.GetFiles(bundle,"*",SearchOption.AllDirectories))
                {
                    string name=Path.GetFileName(file).ToUpperInvariant();
                    if(name.StartsWith("LICENSE")||name.StartsWith("NOTICE")||name.StartsWith("COPYING")||name.StartsWith("THIRD_PARTY"))
                    { audit.AppendLine(file.Substring(output.Length).TrimStart(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar));noticeCount++; }
                }
            }
            if(noticeCount==0)audit.AppendLine("NONE FOUND — redistribution notice review is blocked.");
            audit.AppendLine("\nOPEN ACCEPTANCE ITEMS:");
            audit.AppendLine("[ ] Verify redistribution terms of exact pinned Zig, LLVM and Wasmtime artifacts, all bundled libraries and transitive dependencies.");
            audit.AppendLine("[ ] Include correct license and attribution files in student package (inventory above is not an approval).");
            audit.AppendLine("[ ] Validate Windows standalone startup and all C11 labs without internet on a clean machine without Zig/Wasmtime installed.");
            audit.AppendLine("[ ] Run hostile-input process/resource audits; Zig is native host software, not contained by Wasmtime.");
            audit.AppendLine("[ ] Run Unity EditMode/PlayMode regressions, save migration, UI sizes, keyboard, contrast and screen-reader review.");
            audit.AppendLine("[ ] Confirm user pilot outcomes and privacy/safe-storage policy before public release.");
            audit.AppendLine("File hashes detect accidental changes, not malicious replacement or full binary dependency integrity.");
            File.WriteAllText(reportFile,audit.ToString());
            if(!status.Ready)throw new BuildFailedException("Phase 8: bundled offline compiler verification failed; see "+reportFile);
            Debug.LogWarning("TLS Phase 8 audit: source and bundle appear staged, but release checks remain open: "+reportFile);
        }
    }
}
#endif
