# Phase 6 educational contract

## Why operations are counted

A comparison count, array read, write, visit, or allocation is a **defined event in a simplified deterministic algorithm model**. It is neither instruction count nor an actual measured elapsed time.

- Array access: one read plus one visited cell for in-bounds `a[n/2]`; `O(1)` with respect to n, assuming ordinary contiguous random access.
- Linear search: absent target checked against each active element (worst-case `O(n)`); setup data construction excluded.
- Binary search: absent target on an already ascending-sorted array (`O(log n)`); input sorting cost explicitly excluded.
- Merge sort: reverse input, recursive merges, recorded comparisons/copy reads/writes and one O(n) temporary-array allocation; `O(n log n)` time / `O(n)` extra storage.
- Insertion sort: reverse input and increasing sorted prefix; `O(n²)` worst-case comparisons and element shifts / `O(1)` extra storage.

Different primitives can have different costs on real hardware. Total event count should not be treated as elapsed runtime or a fair cross-implementation clock metric.

## Debugging methodology

Five deliberately faulty algorithms are run on eight deterministic fixtures each. The learner sees reference vs bug output and identifies the misconception. Faults and reference implementations are bounded educational models: not a universal dynamic C analyzer. Case design covers empty/single, first/last, index zero shifting, circular queue physical wraparound and hash tombstones.

## Coding assessment

The real C11 practice tests compile complete learner functions with the installed opt-in Zig and run separate WASM processes via Wasmtime under existing restrictions. Eight finite cases per function can still miss bugs; guided or reference-based passes are never called independent mastery. Results are session-only and no Academy progress is written.

## Code quality

Readable formatting does not guarantee correct behavior. Students compare semantics, contracts, memory and boundary safety, maintainability, unit-test coverage and honest asymptotic claims. The current multiple-choice reviews are practice rather than a human evaluation of independently produced C documentation.
