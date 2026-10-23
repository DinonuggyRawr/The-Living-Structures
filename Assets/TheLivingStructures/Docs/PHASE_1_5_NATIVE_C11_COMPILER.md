# Phase 1.5 — Optional real C11 compilation and bounded case checking

The Living Structures uses Unity/C# for its interface and simulations, but teaches **C11**, not C++.

Phase 1.4's bounded interpreter was useful for guiding a small subset of C. It could not compile real student C or understand all valid C11 solutions. Phase 1.5 adds a distinct optional, container-based compiler screen.

## Architecture

`TLSAcademyApp` → `TLSNativeCompilerLabScreen` → `StackNativeLabSession` (opt-in, async, session-only) → `StackNativeDockerRunner` → local Docker Linux daemon → disposable `gcc:14-bookworm` container.

The student draft is sent via stdin with a fixed C11 prelude. A trusted, separately compiled harness checks eight deterministic cases. Its public source is included in `Runtime/Native/stack_native_harness.c`, and the byte-equivalent runtime string in `StackNativeHarness.cs`. Each case runs in its own process inside the same ephemeral container and has a 2-second time budget. The outer host waits at most 25 seconds. Compile options include C11 pedantic warnings-as-errors, stack protection, and undefined-behavior sanitizer. Those mechanisms may identify some errors but do not certify safety.

No test results modify the saved Academy progress or set mastery flags. Modeled first-attempt evidence remains separate. Hints/reveal in the function-writing lab are still available and still disqualify any existing *modeled* unseen-first-attempt metric; native reports do not grant a replacement independent-first-attempt metric.

## Threat model and notable limits

No arbitrary host shell execution is performed using student text. The only host-side executable intentionally launched is the `docker` CLI (and Docker CLI again for timeout cleanup). Student source is streamed through stdin. The Docker command line is static other than function selection and a generated container name. The fixed /bin/sh test script and stdin/base64 protocol have been smoke-tested locally without Docker, but execution inside Docker Desktop remains unverified. No host mounts, network connectivity or default runtime privileges are supplied to the container.

Docker may use a remote daemon if the user's Docker context points to one; verify the context manually before opting in. Docker containerization alone does not provide a hostile-code execution guarantee. Container runtime/image vulnerabilities and daemon access remain risks. Do not use this for untrusted public submissions or remote students. Multi-tenant production grading requires a dedicated security architecture and review.

Compilation diagnostics include only a bounded excerpt on-screen. A user-driven copy action exposes the fuller returned bounded diagnostic text. The checker cannot exhaust all C input possibilities. Compilation with a sanitizer, 8 passes, and a clean exit are insufficient to assert fully correct, memory-safe or portable C code. Do not overstate educational mastery.

## Suggested future follow-up

Evaluate a dedicated VM/microVM-backed runner for truly untrusted submissions, secure native execution instrumentation, teacher-authored hidden/variable cases, memory sanitizer and debugger integration, and competency transfer exams written outside the simulator. Do not require any of these for using the existing modeled Stack Lab.
