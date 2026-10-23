# Phase 1.5 Migration: Docker to WebAssembly

This source patch migrates only the optional native C practice checker. It does not alter stack model semantics, visual traces, practice prompts, saved Academy progress, the scene builder or the original C11 reference examples.

The new native host toolchain runs a local trusted Zig/Clang C frontend (`zig cc -target wasm32-wasi -std=c11`) to produce a `.wasm` module for each submitted full function. Wasmtime runs the same program eight times with one independent test index per invocation and explicit limits. No Docker daemon or container image is used. Zig's WASI libc supports the harness's `strtol`, `stdbool`, `stddef`, `limits` and process arguments.

Execution stages: frozen student source → private temporary C translation units → Zig wasm32-wasi compiler → WASM magic/size checks → eight isolated Wasmtime runs → classified results → temp cleanup. User C strings are *never command-line fragments*. The only process arguments derived from user choice are a fixed 0/1/2 function enum and fixed 0–7 test index. File paths are internally generated and quoted, with shell execution disabled.

Limits: source 5000 chars; compiler 75s wall timeout; WASM guest fuel 20M units per case, 16 MiB guest memory, 256 KiB WASM stack, 2s guest timeout, 6s host process timeout per case. Diagnostics are capped to 2800 characters in UI. Runner opt-in expires on restarting Unity. Results are hidden when the submitted code changes, and no score persists to Academy progress.

Threat model: This is a local tool for learners testing their own C code, not an adversarial execution service. **The compiler runs natively on the host** and must be trusted/patched; WASM memory and capability boundaries only apply at runtime. Host process limits are wall-clock timeouts, not OS job-object CPU/memory limits for the compiler. Cancellation kills the direct process, but descendant process guarantees are not proven. For public grading, use a dedicated hardened service/VM with compiler isolation, resource quotas and auditing.

Supported current distribution target: Windows x64 using project-local binaries. Future standalone redistribution must include dependency licenses, an updater/security patch story and OS-specific installers. The development patch does not bundle third-party executables.
