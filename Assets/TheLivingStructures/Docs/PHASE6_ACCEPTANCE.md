# Phase 6 — Unity acceptance checklist

This package is **not accepted until actual Unity tests and the user functional verification pass**.

- [ ] Unity 6 compiles without new errors (check Console and `TLS.EditModeTests.dll`).
- [ ] Full existing EditMode suite remains green (report actual test count rather than inferring it).
- [ ] All five Phase6 test suites execute and pass.
- [ ] Academy Advanced directory opens Analysis and Debugging without clipping, retaining Heap/Graph/Hash entrances.
- [ ] Analysis: linear miss n=8 has eight comparisons; binary miss n=8 has four comparisons; insertion reverse n=8 has 28 comparisons; array-index stays constant as n rises; merge-sort count grows nlogn and produces ascending result.
- [ ] Analysis: prediction is not graded/revealed until FINISH; rewind does not mutate a finished trace. Larger n view remains readable.
- [ ] Debugging: reference-vs-seeded result differs on target edge cases; A–E diagnosis works; hints/reveals and wrong first tries prevent unseen credit.
- [ ] Code Quality: all 12 cards render without clipped C text, A/B choices, next/back and guided/independent behavior function.
- [ ] WASM: real compiler-backed reference `linear_search`, `binary_search`, `insertion_sort` each PASS 8/8 (24 bounded WASM cases); bad C fails; stale result disappears when draft edited.
- [ ] Stack / Array / Queue / Pointer / Linked List / Recursion / Tree / Heap / Graph / Hash older labs still render and operate.
- [ ] Existing progress/save data untouched by Phase 6; local compiler discovery and bundling still work.
- [ ] PlayMode and UI at common game resolutions, keyboard entry, text readability and tab navigation tested.

Deferred to Phase 8: security hardening, complete offline clean-machine standalone test, redistribution compliance, accessibility and full performance/regression QA.
