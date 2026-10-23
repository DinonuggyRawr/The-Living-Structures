# Phase 1.2 — Visual-to-Code Learning Upgrade

**Status:** authored incremental source patch; Unity installation/compilation, EditMode tests, and Game-view visual acceptance are pending.

## Purpose

Improve the accepted basic Stack Laboratory without turning demonstrations into inflated claims of mastery. The final goal is still an independent student who can trace, write, debug, and test C without the visualization.

## What this patch changes

- The Academy defaults to a compact entrance card, letting students see more of the 3D environment. `SHOW LEARNING PATH` displays the detailed panels, and `FOCUS ON CITY` collapses them again. `ENTER STACK LAB`, camera rotation, and recorded trace practice remain available.
- The C pane is wider, with larger text, bounded automatic text fitting for long lines, fixed-snippet syntax coloring, and a cyan marker on the current executed statement.
- The memory panel uses the exact same `StackTrace.Frames` as the executable visual model. A code step highlights the written raw cell, including a same-value assignment; activating a cell by incrementing `count` is shown as a distinct change. POP's decrement occurs before its output read. The raw backing array remains visible even when a cell becomes logically inactive.
- Clicking PUSH/POP/PEEK first opens a **formative prediction**: increase / decrease / unchanged `count`. The learner may choose SKIP or CANCEL. Only after committing a prediction or skipping does the new trace start. Feedback appears upon reaching the final frame, with a cause tied to the branch (including overflow/underflow).
- The inspection pane gives step-specific WHY explanations. UI predictions are strictly **session-only**; no new save fields, progress claims, or mastery are added.
- The separate, independent three-question `TRACE CHALLENGE` is unchanged.

## Technical and teaching boundaries

- The existing `StackSimulation.Execute` and C11 `Examples/stack.c` are **not modified**. The visual deltas are derived from successive authoritative snapshots and the statement line/target cell. The execution model still assumes valid stack/output pointers; the shown C code explicitly checks them.
- Rendering C colored snippets is not a C parser. It only styles the shipped fixed source snippets. Student-authored C parsing/compilation, genuine source-level debugger, and automatic independent coding assessment remain future work.
- A raw storage slot is not a logical stack member until `count` includes it. An assignment is a write even if the new value equals the previous value.
- Static source checks and a passing GCC C11 reference test are **not** evidence of Unity compilation, running Unity NUnit, UI layout acceptance, or independent learner outcomes.

## Manual acceptance in Unity

1. BACK UP the Unity project. Verify the local `Assets/TheLivingStructures/Runtime/UI/TLSAcademyApp.cs` matches the SHA-256 baseline reported in the patch README. If modified locally, **merge the supplied diff** rather than replacing the file.
2. Copy/merge the exact contents under `Assets/TheLivingStructures` into the existing project; leave Unity-generated `.meta` files intact. Allow Unity to compile and look for Console errors. There is **no need to rebuild the scene**, because the existing scene references the unchanged `TLSAcademyApp` component class.
3. In the existing Phase 1.1 Academy scene, press Play. Verify compact Academy, SHOW LEARNING PATH / FOCUS ON CITY, rotation, and ENTER STACK LAB. Expand panels and verify progress values are still present.
4. With empty stack, click PUSH 12. Before any new trace appears, select INCREASES BY 1. Step through each frame. The assignment step should show `data[0] = 12` as **stored but inactive**, with count still 0. On the next step, count becomes 1 and index 0 becomes active. Complete the trace; the formative prediction should be marked correct.
5. PUSH 20, choose increase; FINISH. POP, choose decrease; step through count decrement and output read. Confirm raw former top may remain but becomes inactive. PEEK on the remaining stack, predict unchanged.
6. Fill the stack to 5, attempt a sixth PUSH and predict STAYS THE SAME. Ensure no out-of-bounds access, no mutation, and appropriate capacity-guard feedback. RESET; POP/PEEK on empty stack and predict STAYS THE SAME.
7. Try a wrong prediction and verify informative correction appears **only at the end of the trace**, not before. Try SKIP and CANCEL. Confirm CANCEL doesn't change the stack and SKIP doesn't grant credit.
8. Check the original TRACE CHALLENGE 3 questions; complete and RETURN TO STACK LAB, then BACK TO ACADEMY. Restart Play Mode to verify legacy trace-practice progress persists without being overwritten by formative predictions.
9. Run the existing and new Unity EditMode suites. Verify Game view in 1280×720, 1600×900, 1920×1080, and a narrow window; watch for truncated lines, modal overflow, overlap with the header, unreadable text, and inaccessible buttons. Also verify the `Input Handling = Both` project setting is unchanged.

**Acceptance is not yet claimed.** Record screenshot, Unity console errors, Test Runner results, and the behavior of the 3-question challenge separately.
