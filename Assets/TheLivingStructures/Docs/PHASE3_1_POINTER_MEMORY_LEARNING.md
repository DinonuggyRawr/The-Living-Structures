# Phase 3.1 — Pointer & Memory Foundations

## Purpose

Build an intuitive and technically honest model of pointer values, symbolic heap allocations,
`struct` fields, aliasing, lifetime, and reachability **before** starting real linked-list algorithms.
The simulator is a visual C teaching model, **not** a C interpreter, allocator, debugger, or
compiler-backed student-code executor. The pre-existing Zig/Wasmtime C11 exercises are preserved.

## The nine scenarios

| Lesson | Concept and misconception |
|---|---|
| NULL GUARD | A pointer set to `NULL` does not refer to a node; guarded access is skipped |
| MALLOC + FIELDS | `malloc` returns storage, not zero-initialized fields; `calloc` would be different |
| ALIASING | Copying a pointer does not duplicate the pointed-to object |
| LINK NODES | The `next` field points to another allocation; changing `head` changes only a pointer |
| DANGLING | `free` ends an allocation's lifetime; remaining aliases must never be dereferenced |
| SAFE CLEANUP | Releasing storage and explicitly clearing local aliases are distinct operations |
| LOST POINTER | Replacing the only reference can leave reachable memory lost (a potential leak) |
| MALLOC FAIL | `malloc` can return NULL; check before dereference; no node is allocated |
| INVALID ACCESS | A null-pointer dereference is undefined behavior; simulator blocks the step |

## Visual semantics

- `0x1000` and `0x1020` are invented **symbolic** lesson addresses, not actual addresses and not promises about allocator behavior.
- Stack pointer variables `head`, `node`, `alias`, `second` and two bounded symbolic heap blocks are separate objects.
- Newly allocated object fields are marked **UNINIT** until a `data` or `next` assignment.
- After `free`, both fields are **inaccessible**, even if the teaching model retains old metadata for rewind.
- Drawing a "dangling" arrow is a visual explanation of former aliasing. In ISO C, the pointer value to an object past its lifetime is indeterminate; the lab never performs a C comparison, print, or dereference of it.
- Reachability is relative to the small modeled local scope. "UNREACHABLE (LEAK?)" is educational, not a global leak detector.
- The linked-node scenario's second-allocation failure branch would release the first node; the displayed trace follows the successful branch.
- Source fragments assume `typedef struct Node { int data; struct Node *next; } Node;` and `<stdlib.h>` within a function. They are fragments, not complete standalone programs. The separately included C11 reference is compilable and releases owned allocations.
- The INVALID ACCESS scenario is intentionally unsafe C text, but is **never executed**, even under WASM. The simulator displays a blocked conceptual step without pretending undefined behavior is deterministic.

## Learning pattern

Select a scenario → predict before execution → step through C code and memory changes → inspect the explanation and safety indicators → rewind/replay → discuss why the operation is safe or unsafe. Prediction score is per-session formative feedback only; no hidden progression or saved mastery is written.

## Boundaries and next steps

Phase 3.1 is a **visual foundation**, not an interactive arbitrary-pointer sandbox, full linked list lab, recursion lesson, or compiler-backed function exercise. Phase 3.2 extends these models into interactive singly linked lists; later phases introduce repair challenges, full C11 functions, and independent tests. Keep all prior Phase 1–2 Academy, Stack, Array, Queue, structured C, WASM, bundled toolchain and progress behavior unchanged.
