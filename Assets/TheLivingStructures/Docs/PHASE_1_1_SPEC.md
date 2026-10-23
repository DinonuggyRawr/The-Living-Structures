# The Living Structures — Phase 1.1 educational and technical specification

## Mission
Help Computing II students gain mental models of **C** data structures and eventually write, debug, and test the C code independently. The simulator is not an official certification or a substitute for hands-on programming.

## Target learning outcomes for this slice
1. Define last-in-first-out (LIFO) and distinguish `push`, `pop`, and `peek`.
2. Predict how an array-backed stack's `count` and `data` change line-by-line.
3. Explain that logically inactive backing array elements can retain old values after `pop`.
4. Explain why capacity and empty checks protect array bounds.
5. Trace unfamiliar stack operation sequences unaided.

**Not provided yet:** arbitrary C code compilation, pointer/heap visualization, C writing assessment, free-roaming character controller, mastery for C programming, saveable sandbox stacks, undo across operations, accessibility suite, or a complete Academy curriculum.

## Architecture
- `TLS.Core.StackSimulation`: pure deterministic operation model with immutable-per-frame snapshots; C-source snippet correspondence.
- `TLS.UI.TLSAcademyApp`: runtime uGUI interface, 3D hub/menu navigation, stack view, independent tracing checks.
- `TLS.Core.StackChallenge`: deterministic 3-question knowledge transfer check, answers calculated by the same operation model.
- `TLS.Core.StudyProgress`: phase- and project-namespaced PlayerPrefs, with separate completed and first-try fields. C implementation mastery is **never** awarded by this slice.
- `Editor/TLSPhase1SceneBuilder`: creates a primitive Unity scene with an actual 3D academy backdrop.
- `Examples/stack.h`, `stack.c`, `stack_demo.c`: independently compileable C11 analog with defensive API checks.

## Truthful progress design
- `Trace practice: COMPLETED` means all three questions were eventually solved, with retry allowed.
- `First-attempt trace pass: RECORDED` means all three were correct before receiving corrective feedback in one run.
- Neither equals `C coding mastery`, which requires independent code authoring and testing in later phases.
- Only progress metadata is stored; the simulated stack is **session-only**.

## Simulator semantics and limitations
- Values use a 32-bit signed integer approximation of commonly implemented C `int` (the standard permits other widths).
- `Stack stack = {{0}, 0}` / `stack_init` initializes all `data` cells to 0, with `count = 0`.
- `push` writes to `data[count]` **before** incrementing `count`.
- `pop` decrements `count` **before** reading `data[count]`; the old backing cell is not overwritten.
- `peek` reads `data[count - 1]` without modifying state.
- The C functions check for invalid pointers; Unity's modeled lab always passes valid stack/output pointers. Null pointer argument tests must be taught separately.
- One trace step is one salient C statement/guard. Some control flow and language runtime details are intentionally simplified. No actual C machine instructions are executing inside Unity.
- The view of `raw data` is the backing integer array in the model, **not** a process memory dump.

## Phase 1.1 user acceptance
1. In Unity 6.3 LTS, build the demo scene via Tools menu and press Play with no compiler errors.
2. See the academy backdrop and a clickable ENTER STACK LAB control.
3. PUSH(12), step through check/write/count/return, and verify correct row and code highlighting.
4. POP, verify the top value comes out and the removed cell is inactive while raw data retains its value.
5. PEEK when empty and POP when empty: failure trace, no state corruption.
6. PUSH six values: the sixth is rejected at capacity 5.
7. Step backward, forward, and FINISH with no unexpected stack changes.
8. RESET clears stack only; persisted tracing practice progress survives.
9. Trace three challenge sequences, verify practice-completed versus first-try-pass wording.
10. Exit lab and revisit; check academy and local progress status.
11. Run EditMode tests and record actual pass/fail/skipped results.
12. Inspect 1600x900, 1280x720, and 1920x1080 UI for clipping and sufficiently readable text.

## Known concerns and explicit future work
- Panel layout is a fixed percentage-layout prototype; extreme aspect ratios and small displays need responsive UI design.
- UI is pointer-friendly; keyboard and screen-reader accessibility require later work.
- A 3D presentation exists but full world-space interactions, character navigation and exploration are not yet implemented.
- Tests cover the pure stack model; PlayMode/UI testing requires user verification in Unity.
- `StandaloneInputModule` requires classic Input Manager support (Project Settings > Player > Active Input Handling: Both or Input Manager (Old)).
- The scene builder creates runtime materials for placeholder buildings; art assets and production scene polish are deferred.
