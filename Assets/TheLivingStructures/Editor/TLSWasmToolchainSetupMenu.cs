#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TLS.EditorTools
{
    /// <summary>
    /// Explicit one-click Windows development toolchain onboarding.
    /// No downloads or process starts on import, domain reload, or Play Mode.
    /// A separate window displays installer output; Unity project data is untouched.
    /// </summary>
    public static class TLSWasmToolchainSetupMenu
    {
        private const string MenuPath = "Tools/The Living Structures/Install or Verify C11 WASM Tools...";

        [MenuItem(MenuPath)]
        public static void InstallOrVerify()
        {
            if (Application.platform != RuntimePlatform.WindowsEditor)
            {
                EditorUtility.DisplayDialog("Windows only", "The one-click installer currently supports Windows x64. Modeled labs continue to work without compiler tools.", "OK");
                return;
            }
            var project = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var script = Path.Combine(Application.dataPath, "TheLivingStructures", "Editor", "Toolchain", "Install-TLS-WasmTools.ps1");
            if (!File.Exists(script))
            {
                EditorUtility.DisplayDialog("Installer missing", "The Phase 1.5 toolchain script was not found inside Assets/TheLivingStructures/Editor/Toolchain.", "OK");
                return;
            }
            bool exists = File.Exists(Path.Combine(project, "TLS_Toolchain_Windows", "zig", "zig.exe"))
                       && File.Exists(Path.Combine(project, "TLS_Toolchain_Windows", "wasmtime", "wasmtime.exe"));
            string message = exists
                ? "Check your existing local Zig/Wasmtime compiler and runtime using a real C-to-WASM smoke test. No downloads or replacement will occur."
                : "Download pinned Zig 0.17.0 and Wasmtime v50.0.0 from their official releases (roughly 110 MB), verify SHA-256, and install them beside this Unity project. The installer then runs a small C-to-WASM test. Native compiler tools will execute locally; only use your own learning code. No Docker or global PATH changes.";
            if (!EditorUtility.DisplayDialog("The Living Structures — C11 WebAssembly Tools", message, "Continue", "Cancel"))
                return;
            try
            {
                var info = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-NoProfile -NoExit -ExecutionPolicy Bypass -File " + Quoted(script)
                              + " -ProjectRoot " + Quoted(project),
                    WorkingDirectory = project,
                    UseShellExecute = true
                };
                Process.Start(info);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("TLS WebAssembly toolchain setup could not start: " + ex.Message);
                EditorUtility.DisplayDialog("Setup could not start", ex.Message, "OK");
            }
        }

        private static string Quoted(string path)
        {
            if (path.IndexOf('"') >= 0) throw new ArgumentException("Unexpected quote in path.");
            return "\"" + path + "\"";
        }
    }
}
#endif
