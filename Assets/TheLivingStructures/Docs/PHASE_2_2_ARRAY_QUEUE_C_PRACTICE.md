# Phase 2.2 — Arrays and Circular Queues: C Practice

**The Living Structures — Unity educational learning simulator**

## Learning goal

Help Computing II learners translate a correct visual mental model into C11 code reasoning while fading scaffolding. This phase introduces 18 structured exercises: nine Arrays and nine Circular Queues (three each of TRACE C, COMPLETE C, and REPAIR C per laboratory). The separate modeled visual labs from Phase 2.1 remain available.

### Arrays

- Trace insertion (right shift) and the distinction between `count` and raw array memory.
- Trace removal with retained raw values and safe indexed GET output.
- Complete the append write, right-shift assignment, and left-shift assignment.
- Repair insert-at-end validation (`index == count` is permitted when not full), incorrect shift direction, and out-of-bounds removal guard (`index >= count` is invalid).

### Circular queues

- Trace FIFO removal and physical head/tail wraparound.
- Trace raw physical `data[0]` independently of FIFO logical order.
- Complete tail modulo, enqueue cell write, and dequeued head advance.
- Repair missing modulo, empty-queue guard, and an incorrect PEEK read.

## Assessment rules

- Both labs offer **Guided** and **Independent** practice. Hint and reveal are permitted only as documented; reveal remains available in Independent mode but forfeits first-try credit.
- A blank submission does not consume an attempt. A wrong nonblank submission, hint, reveal, or previous correct completion disqualifies *unseen first-attempt* status for that exercise in that session.
- Token-based C answer matching accepts whitespace changes while preserving C token boundaries, including `||`, `==`, `>=`, `->`, `++`, `--`, and `%`. It matches **one authored reference sequence**, not all semantically equivalent valid C.
- Scores belong only to the session; no file, PlayerPrefs, Academy mastery or course progress is written. Answering a one-line question is **not** compiler-backed programming mastery.
- Array and Queue code-practice answers are not executed; the prior Stack WASM compiler-backed lab is unmodified. A later milestone may add actual whole-function C coding and safe compiler-backed tests for Arrays/Queues.

## Fidelity and safety

Reference wording and expected values are grounded in the Phase 2.1 `arrays.c` / `queues.c` C11 examples and immutable Unity model operations. The additional `phase2_2_practice_smoke.c` verifies representative trace outputs by compiling real reference C. These questions do not execute students' C statements, and they never touch the user's files or network.

The `head + count` sum is safe within the deliberately small bounded queue state. C examples explicitly distinguish raw storage from logical elements and reject invalid indices/pointers before access. No undefined C behavior is claimed as predictable.

## Acceptance in Unity

1. Open the existing Academy scene, click **ENTER ARRAY LAB → C PRACTICE**.
2. Verify all TRACE C / COMPLETE C / REPAIR C tabs, text input, CHECK, GUIDED/INDEPENDENT, SHOW HINT, REVEAL / REVIEW, PREVIOUS/NEXT, and RETURN TO ARRAY LAB are visible and usable.
3. In Array TRACE task 1, choose Independent and enter `20`. Expected: correct first try, session 1/9 and independent 1/9. Trace task 2 expects raw slot `8`; task 3 expects output `7`.
4. Try Array COMPLETE right shift `a->data[i] = a->data[i - 1];` and the corresponding REPAIR task. Verify wrong direction fails with a useful explanation, and corrected submissions after mistakes do not gain unseen first-try credit.
5. Return to Academy, open **QUEUE LAB → C PRACTICE**. Queue TRACE task 1 expects `6` (FIFO), task 2 expects physical head `2`, task 3 expects raw slot `60` after wraparound.
6. Try Queue COMPLETE tail calculation `int tail = (q->head + q->count) % QUEUE_CAPACITY;` and Queue REPAIR empty guard with the `||` operator.
7. Verify switching tracks and modes clears visible answers without incorrectly restoring eligibility. Returning between practice and visual lab must not change the visual data structure.
8. Run the three new EditMode test suites, then the full green baseline regression suite. Verify layout at 1600×900 plus a smaller Game-view resolution; inspect the header `C PRACTICE` button for overlap.
9. Re-check original Stack trace, C practice, modeled whole functions, WASM reference tests, saved progress, Academy return, and no red Console errors. These are not automatically verified by authoring this patch.

## Preservation constraints

Only `TLSAcademyApp.cs` is replaced, with two new C# files, three new tests, C11 smoke source, and documentation added. Preserve all existing `.meta` files, Phase 2.1 labs and C sample source, Phase 1.5 one-click Zig/Wasmtime fixes, independent Stack learning progress, visual style, and C-only scope.
