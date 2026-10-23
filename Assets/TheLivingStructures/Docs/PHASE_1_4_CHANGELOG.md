# Changelog — Phase 1.4

- Added in-session complete-function practice for PUSH / POP / PEEK, with multiline source editing, hints, starter resets, reference review, and limited modeled behavioral tests.
- Added `StackFunctionEvaluator`: explicitly bounded C-subset AST parser/evaluator for fictional in-memory stack state, no native execution or disk/network access.
- Added `StackFunctionLabSession`: separates modeled passes, first-unseen-attempt results, hints and reveals from saved mastery; no PlayerPrefs writes.
- Added `TLSFunctionLabScreen` and WRITE C FUNCTIONS launcher in top bar of Stack Laboratory.
- Kept existing bottom controls and old Academy/Challenge/C PRACTICE UI intact; no scene rebuild, no changes to C11 examples, authoritative StackSimulation, or StudyProgress.
- Added three NUnit suites and detailed educational, security and acceptance documentation.
- Actual Unity compilation, NUnit execution, visual QA, and full compatibility regressions require user validation.
