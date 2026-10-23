# Phase 7 Educational Design — evidence, not magic scores

The Living Structures remains a visual-first Unity Computing II learning system using real C11 instruction. The intended sequence for each topic is:

1. **Diagnose:** optional baseline question, no fake completion status.
2. **Watch and predict:** link to pre-existing graphical labs. Viewing updates a Phase 7 *viewed* flag only.
3. **Guided reconstruction:** optional C statement ordering and explanations. Hints/reveals are recorded as assisted exposure.
4. **Design an edge case:** submit test inputs and a predicted reference oracle; a useful case must distinguish a seeded bug.
5. **Practice independently:** attempt concept checks with visuals hidden on the assessment page and no in-question hints.
6. **Write/compile real C:** opt-in to finite WASM transfer cases. First-unseen results are separately tracked as compiler-backed evidence, not mastery.
7. **Revisit:** on-open review due at 1, 3 or 7 days after guided, unassisted or successful review events. No automatic notification.

Evidence legend: **untracked** (older historical labs and missing records), **viewed** (opened from Phase 7 directory), **guided** (answer reached with assistance), **independent reasoning** (first-unseen clean structured/concept answer), **compiler-tested independent** (clean first-unseen WASM pass), and **needs review** (recorded misconception). None is labelled certified mastery.

The diagnostic/final question banks are distinct, but finite and publicly inspectable in shipped code. They are not cheating-resistant standardized tests. Code reconstruction and test-design use known fixtures. Phase 8 student pilots should evaluate transfer outside the simulator by having students write unfamiliar C11 functions and tests in a normal editor without visual help.

The Academy's subject buildings remain visual hubs. In Phase 7 a curriculum directory maps their names to existing labs without walking-to-door mechanics; full building navigation and interiors would require a separate future design audit. Don't replace educational content with decorative expansion.
