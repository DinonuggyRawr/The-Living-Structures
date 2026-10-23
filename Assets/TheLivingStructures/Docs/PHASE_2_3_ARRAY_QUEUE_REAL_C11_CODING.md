# Phase 2.3 — Whole-Function Arrays and Queues in real C11

## Goal
Students first watch and explain a visual operation (Phase 2.1), then trace/complete/repair a statement (Phase 2.2), then implement its entire C11 function here using the same project-local Zig/Wasmtime workflow as the accepted Stack Lab.

## Functions and accepted C11 signatures

Array (`TeachingArray` has `int data[6]` and `int count`):
- `bool array_append(TeachingArray *a, int value)`
- `bool array_insert(TeachingArray *a, int index, int value)`
- `bool array_remove(TeachingArray *a, int index, int *out)`
- `bool array_get(const TeachingArray *a, int index, int *out)`

Queue (`TeachingQueue` has `int data[5]`, `int head`, `int count`):
- `bool queue_enqueue(TeachingQueue *q, int value)`
- `bool queue_dequeue(TeachingQueue *q, int *out)`
- `bool queue_peek(const TeachingQueue *q, int *out)`

The same conditional protections, raw-cell retention, and modulo wraparound semantics as `Examples/arrays.c` and `Examples/queues.c` are assessed. `NULL`, out-of-range indices, full/empty states and stale data are relevant. Only the selected function is compiled and linked to a trusted harness, so students do not need to implement all seven functions at once.

## Workflow

1. Enter the Academy and choose Array Lab or Queue Lab.
2. Select WRITE C FUNCTIONS in the lab header.
3. Choose a function tab. Use Guided mode for coaching or Independent to try without aid.
4. ENABLE WASM RUNNER intentionally. No compiler tool is installed automatically by opening the lab. If the previously verified Zig/Wasmtime project-local toolchain isn't installed, use the existing Unity Editor setup menu.
5. Write a complete C function (maximum 5000 characters) and select RUN C11 WASM TESTS. Each run compiles for wasm32-wasi with warnings treated as errors, then starts ten separate Wasmtime runs subject to memory, stack, fuel, execution timeout and no directory preopens.
6. Inspect the per-case result. The first failed case and compiler diagnostics help with diagnosis. Debug and retest.
7. Guided SHOW HINT, SHOW REFERENCE, or an earlier failed submission excludes that task from unseen independent first-attempt credit for the **current in-memory session**. A passing result from a revealed reference is assisted practice, not implementation mastery.

## Explanatory boundaries

- Ten finite cases are **not** exhaustive proof of C correctness or memory safety.
- Compile/runtime tests are genuine C11-to-WASM, while visual animation is driven by the original deterministic teaching model; do **not** claim compiled learner code drives the 2D animation.
- Zig is a trusted native host compiler; Wasmtime executes the student-compiled WASM. This workflow isn't hardened for hostile or anonymous code submissions.
- Phase 2.3 reuses the existing project-local Zig/Wasmtime setup. It **does not** yet bundle the tools for final standalone Windows distribution; that's Phase 2.4 and later release/security work.
- Session results are ephemeral, not saved progress or certified independent C mastery.
- No changes to `StudyProgress`, legacy save keys, other labs, or assets other than the targeted UI entrypoint.

## Test matrix

Each function has ten dedicated test cases with the following common invariants:
- `bool` return indicates success/failure.
- The logical count must agree with expected behavior, and the Queue head must only move on successful dequeue.
- Every raw backing cell must match (catch unintentional writes and stale-slot clearing).
- `out` must match for successful reads and remain unchanged after failure.
- Invalid/null/full/empty cases must not modify the structure or output.
- Traps, compilation errors, missing toolchain, cancelled runs and stale edited drafts can never be interpreted as a passing current implementation.

## Installation/regression

The only replaced existing script is `Assets/TheLivingStructures/Runtime/UI/TLSAcademyApp.cs`, based on the accepted Phase 2.2 SHA256 given in `BASELINE_SHA256.txt`. The other source/test files are new. Do not overwrite a customized app script; merge `DIFFS/TLSAcademyApp.cs.diff`. Preserve existing Unity `.meta` files and do not rebuild the Academy scene.

Run the new EditMode suites, then the entire project suite, then visual inspections at your normal Game view and a narrower resolution. Smoke test each of seven reference functions (expect 10/10), try a deliberately faulty function, navigate back to original Stack WASM lab, and verify its 8/8 reference remains intact. Unity compilation and Zig/Wasmtime integration must be user-verified.
