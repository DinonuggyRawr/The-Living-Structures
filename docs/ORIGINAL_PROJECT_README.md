# The Living Structures — Phase 8 Focused UI Responsiveness / Diagnostics Patch

**Status:** Source-authored and ZIP-validated; **not compiled or run in Unity here**. Do not merge into a release candidate before local testing. The v1.0.0-rc1 private QA package remains an older, unchanged build until a new Windows executable is built and separately validated.

## Why this patch exists

The reported Windows standalone build sometimes takes **3+ seconds to react to nearly every button**. Inspection of the accepted Phase 8 source identified:

1. The Academy's `RenderAcademy()` calls `TLSBundledToolchain.Check()` on every redraw. In a packaged build, that reads and computes SHA-256 for BOTH large compiler executables. This is appropriate at compiler authorization time but unnecessary for merely displaying the Academy's readiness badge.
2. The centralized `TLSAcademyApp.Render()` destroys and recreates the entire uGUI page in response to almost every button click. Many controls and labels are then traversed by `TLSPhase8Accessibility.Apply()`. This can generate main-thread stalls and Canvas/GC work. This patch does **not** claim to solve the architectural rebuild issue: measured data is needed before a larger caching/pooling refactor.
3. The release-packaging PowerShell script and OneDrive synchronization may also have competed for disk/CPU. Finish the script first and compare a local non-OneDrive copy before attributing all latency to Unity.

## What changes

Only TWO existing scripts are replaced:

- `Assets/TheLivingStructures/Runtime/UI/TLSAcademyApp.cs` — Academy informational badge caches its integrity status for this app instance; screen rebuilding receives low-overhead Unity Profiler samples (`TLS.UI.FullRender`, `TLS.UI.ClearOldScreen`, `TLS.UI.BuildScreen`, `TLS.UI.AccessibilityAndExit`). A throttled diagnostic warning is logged only when a synchronous redraw takes >=250 ms in Play Mode. Messages show clear/build/styling times.
- `Assets/TheLivingStructures/Runtime/UI/TLSPhase8Accessibility.cs` — avoids traversing the whole UI hierarchy when text scaling, high contrast and reduced motion are all at their unmodified defaults. If any setting is active, the existing accessibility styling continues unchanged.

Two files are added:

- `Assets/TheLivingStructures/Runtime/UI/TLSPhase8AcademyReadinessCache.cs` — display-only cache (never used by C compilers) with explicit invalidation and path/mode change behavior.
- `Assets/TheLivingStructures/Tests/EditMode/TLSPhase8PerformanceTests.cs` — six authored NUnit tests for cache behavior, null guards, and route preservation.

**Security boundary:** ALL existing `TLSBundledToolchain.Check()` calls within actual C11/WASM runners remain intact and run before compilation. The badge cache MUST NEVER authorize C11 compilation, and its display may become stale after an out-of-band toolchain change until the app restarts. No changes to WASM timeouts, memory/fuel, file accesses, saves, scoring, C code, or Phase 1–7 data.

## Before installing: isolate background load

1. Finish or stop the **release-candidate packager**; do not compare while it is hashing/ZIP-compressing files.
2. Close Unity, wait for OneDrive to finish syncing, and make a **complete local copy** of the tested Windows build under `C:\TLS_Local_Test` (not the Unity project; copy the EXE, `_Data`, DLLs, `TLS_Toolchain_Windows`, manifest, etc.). Do not run the EXE from inside a compressed archive.
3. Test the local EXE **before the patch**. Compare Academy tabs, Stack PUSH/POP, and Phase 7 navigation. If the 3+ second delay disappears, investigate background I/O / sync before making further UI changes.

## Installation

1. Back up and **close Unity**. Extract this ZIP.
2. Compare local versions of both files with `BASELINE_SHA256.txt`. The Academy SHA expected is `cb78dc56ed10fafb2ba054923a866ba3ada2319af4f526e3afead17606101041`. The Phase 8 Exit Cancel/EditMode hotfix must already be present. If any hash differs, use the enclosed `DIFFS` to **merge**, do not blindly overwrite customized scripts.
3. Copy the ZIP's `Assets/TheLivingStructures` into your Unity project, retaining existing `.meta` files.
4. Reopen the same Academy scene. Run **TLSPhase8PerformanceTests** and your full Unity EditMode suite. Confirm all tests green, no new compiler errors, and no UI clipping or exit-dialog regressions.
5. In the Unity Editor, enable **Window > Analysis > Profiler > CPU Usage**. Capture a few button clicks and inspect `TLS.UI.BuildScreen`, `TLS.UI.AccessibilityAndExit`, `Canvas.BuildBatch`, `GC.Collect` and total main-thread frame time. For a Windows development build enable **Development Build + Autoconnect Profiler** in Build Profiles and repeat in the actual player. Leave Deep Profiling OFF for the first pass (it adds overhead).
6. Rebuild the Windows x64 standalone to test this patch. A previous build or v1.0 private QA ZIP **does not automatically update**. Retest standalone Zig/Wasmtime hashes, reference compiler cases, Exit App, accessibility modes, and offline clean-machine behavior before making a new candidate.

## How to see slow redraw warnings

Warnings above 250ms emit `TLS UI redraw <screen>: <total>ms (clear ..., build ..., styling ...)`. These are throttled to at most one per five seconds, *only in Play Mode*. The warnings exclude Unity's post-return Canvas batching, GPU frame presentation and unrelated OS scheduling. A low build time does not prove that input-to-display latency is low. In the Editor inspect Console; in the player inspect Unity's `Player.log` under `%USERPROFILE%\AppData\LocalLow\<CompanyName>\<ProductName>\`.

If the profiler shows heavy `Canvas.BuildBatch` / UI rebuilds while `TLS.UI.BuildScreen` itself is modest, a **separate, tested UI caching/pooling refactor** will be required. Avoid disabling C11 security verification or timeouts to make the UI feel faster.

## Validation and limitations

Source diff, lexical/delimiter checks, structural assertions, and ZIP/hash integrity are performed in the authoring environment; actual Unity compilation, PlayMode, profiler timing, user-machine performance and standalone packaging remain unverified. No specific speedup is promised. Keep older QA release candidates private and unmodified.
