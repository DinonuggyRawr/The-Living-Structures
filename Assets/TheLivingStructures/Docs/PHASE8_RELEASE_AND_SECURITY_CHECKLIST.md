# Phase 8 — Release-readiness and security checklist

This source patch is **NOT** a release-certified standalone application. It does not bundle Zig or Wasmtime binaries. The existing Phase 2.4 post-build step stages a locally installed compiler/runtime beside a Windows x64 Unity build. The Phase 8 post-build audit reports files and outstanding acceptance tasks; it never silently downloads anything.

## Manual acceptance gates

1. Back up the user’s project and save data. Apply baseline-verified source changes; preserve `.meta` identifiers.
2. Unity: compilation without Safe Mode, entire EditMode suite green; PlayMode tests and regressions across Phase 1–7.
3. Academy: click each of the five existing 3D building objects or use Previous/Next, enter each matching lab, use old four-group directory to reach every other topic; test UI pointer-over-UI guard and older Input System setting.
4. C editor: inspect C11 source linebreaks and literal code in Stack, Array, Queue, Linked List, Tree, Heap/Graph/Hash, Phase 6 and Phase 7. C source must not change or earn attempts merely from opening a field.
5. Tree graph: insert, traverse, and delete in General/Binary/BST modes. Edges must match the state’s actual parent/child references; verify focus, pan, zoom, clipping and empty-tree messages.
6. Accessibility: text levels 100/112/124%, high contrast, preference persistence; mouse and keyboard uGUI focus; check reduced-motion expectations (only new focus animations are suppressed). Verify at 1280×720, 1600×900, and 1920×1080. Screen-reader support is not established.
7. Windows x64 Build: verify `TLS_Toolchain_Windows/zig/zig.exe`, `zig/lib`, `wasmtime/wasmtime.exe`, `TLS_BUNDLE_MANIFEST_V1.txt`, and `TLS_PHASE8_RELEASE_READINESS.txt` beside executable.
8. Offline: run `SETUP/Test-TLSStandaloneOffline.ps1 -BuildFolder <path>`, disconnect internet, then run the standalone on a *clean Windows machine*, with no developer-installed compiler. Verify actual compiler-backed reference tests in representative labs, including Stack, Linear, Tree, and Phase 7 transfer.
9. Security: independent assessment of C compilation process limits and arbitrary student submissions. The **host-native Zig compiler is trusted**; Wasmtime fuel and memory limits do not sandbox Zig itself. Public hostile code submissions are not authorized by this patch.
10. Distribution: identify complete pinned binary artifacts, dependencies, license terms, notices/LLVM exceptions, redistribution conditions, plus Windows bundled libraries. The generated license inventory is a discovery aid, **not compliance approval**. Complete any needed attribution before distribution.
11. Education pilot: test novice usability, learning transfer without visual hints, incorrect answers/remediation, and evidence history across sessions. Passing finite C tests is not proof of general correctness or mastery.

## Scope limits

Five originally modeled Academy buildings are clickable; other subjects are accessible via the existing directory. Walking to doors, new buildings and interiors are **not** implemented. Existing Phase 1–7 labs and progress writes are not migrated. Tree pan/zoom affects only its presentation. The presentation preference key is `tls_phase8_presentation_v1`; it does not modify Phase 7 evidence or legacy save keys.
