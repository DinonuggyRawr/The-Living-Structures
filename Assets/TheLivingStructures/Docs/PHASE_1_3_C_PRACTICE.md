# Phase 1.3 — Guided C Practice: Educational and Unity Acceptance Guide

## Learning outcome

Students move beyond watching StackSimulation: they trace C output and memory, supply exact one-line C statements, and diagnose representative C bounds/pointer/index bugs. They can elect to practice with hints and later try unseen structured tasks. The aim remains eventually implementing full C functions independently, which is **not** assessed in this phase.

## How to open

Start the existing Academy scene → **ENTER STACK LAB** → **C PRACTICE** (bottom right, next to **TRACE CHALLENGE**). Return using **RETURN TO STACK LAB** at the bottom or **BACK TO ACADEMY** at the top. The old Stack Lab and Trace Challenge continue to work independently.

## Three tracks and expected examples

**TRACE C:** Each numeric answer is derived through the authoritative `StackSimulation` trace rather than hard-coded into the UI.

| Exercise | Expected | Main concept |
|---|---|---|
| TR-01 | `9` | `stack_pop()` writes the LIFO top into `out` through `*out_value` |
| TR-02 | `4` | After POP, data[0] retains its initialized raw value while `count` becomes 0 |
| TR-03 | `2` | Three pushes and one pop leave two active stack elements |

**COMPLETE C:** Fill one missing source statement (C11 syntax).

| Exercise | Reference statement | Main concept |
|---|---|---|
| CO-01 | `stack->data[stack->count] = value;` | Write raw cell before increasing count |
| CO-02 | `stack->count++;` | Increase logical active-element count |
| CO-03 | `*out_value = stack->data[stack->count];` | POP reads old top after decrement |

**REPAIR C:** Replace exactly the buggy line, not the whole function.

| Exercise | Reference repair | Main concept |
|---|---|---|
| RE-01 | `if (stack == NULL || stack->count >= CAPACITY)` | Reject the full-capacity boundary |
| RE-02 | `if (stack == NULL || out_value == NULL || stack->count == 0)` | NULL guards and zero check for `size_t` |
| RE-03 | `*out_value = stack->data[stack->count - 1];` | Read active top, not next free slot |

The reference statements for six exercises have been compiled and behaviorally checked under C11 after substituting into their shown function contexts. The app's text matcher compares normalized *C token sequences* against these references. It does **not** execute or semantically validate student-authored C, so syntactically different but valid equivalent statements may be rejected.

## Fading the scaffolding

- **GUIDED:** Students may tap SHOW HINT; the hint is specific to the current mistake. They may inspect REVEAL / REVIEW at any time. Such exposure forfeits a future *unseen first-try* label for that exercise in this session, even if they later switch to INDEPENDENT.
- **INDEPENDENT:** Hints are disabled, and the reference remains hidden before submission. A correct first submission with no earlier hint/reveal on that task earns an `unseen first-attempt` **structured practice** count for the session. A wrong first submission still permits retry, but cannot earn that label later.
- **REFERENCE REVEAL:** Always optional. It forfeits unseen-first-attempt status for that exercise. No assessment penalties or permanent results.
- **Progress:** The bottom strip shows completed structured answers and first-try structured answers in **this running Unity session only**. Neither is independent C implementation mastery and neither updates `StudyProgress`, `PlayerPrefs`, or save files.

## Unity acceptance checks

1. Existing Academy scene loads with no Console errors or warnings introduced by this patch. Preserve Phase 1.2 Academy collapsible guide.
2. Existing Stack PUSH 12 works, optional prediction shows, and step-specific raw-cell/count highlights remain accurate.
3. The bottom control bar displays **TRACE CHALLENGE** and **C PRACTICE** as separate clickable buttons without collision/truncation.
4. Open C PRACTICE; switch TRACE / COMPLETE / REPAIR. Confirm there are exactly three exercises in each.
5. In TRACE, answer TR-01 = 9, TR-02 = 4, TR-03 = 2, and confirm feedback/reference reveal only after correct check or explicit reveal.
6. In COMPLETE, try a wrong `stack->count--;` in CO-02 and check diagnostic; then answer `stack->count++;` and confirm success. Repeat with CO-01 and CO-03.
7. In REPAIR, try the buggy `>` in RE-01 (must fail); change to `>=` (must pass). Test `== 0` in RE-02 and `count - 1` in RE-03.
8. In GUIDED, show a hint, switch to INDEPENDENT and correctly answer that same task. Its unseen-first-try counter must **not** increase.
9. On a task never opened with a hint or reveal, select INDEPENDENT and submit the correct reference answer on the first try; its unseen-first-try count must increase **once**, not on repeats.
10. Reveal a reference answer, then submit it correctly; count it as guided practice but not as an unseen independent attempt.
11. Return to Stack Lab, perform PUSH/POP/PEEK and check existing 3-question TRACE CHALLENGE remains reachable and previous trace-progress state remains unchanged.
12. Return to Academy, confirm camera and 3D scene work, and reenter both lab screens. No unexpected reset of the StackSimulation state.
13. Run the three added EditMode NUnit suites and existing Phase 1.1 and Phase 1.2 tests. Validate PlayMode, Game view minimum resolution, and UI readability separately.

## Remaining education work after Phase 1.3

A genuine C implementation assessment requires a secure, isolated C compiler/test environment; syntax and semantics validation; whole functions rather than single-statement matching; robust diagnostics; unseen test suites; and clear handling of undefined behavior and memory safety. That is future work and should not be falsely credited as achieved here.
