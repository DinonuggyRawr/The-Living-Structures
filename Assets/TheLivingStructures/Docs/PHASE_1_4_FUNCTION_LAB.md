# Phase 1.4 — Complete C Function Lab (Bounded C11 Subset)

## Educational intent
Students write **complete** stack_push(), stack_pop(), and stack_peek() functions, not isolated fill-in statements. Guided mode provides algorithm hints. Independent mode hides hints, permits a fresh first-unseen attempt, and tracks it *within this one session only*. A modeled test pass is not compiler-verified C implementation mastery.

## Where it appears
Academy > Stack Laboratory > **WRITE C FUNCTIONS** in the top bar. This leaves the prior Stack Lab footer buttons (TRACE CHALLENGE and C PRACTICE) in their original placements. Tabs: WRITE PUSH, WRITE POP, WRITE PEEK. Editor is multiline. Use **RUN MODELED TESTS**; results include passed case count and the first observed failure. **SHOW REFERENCE** replaces the draft with a known reference; hint/reveal/wrong submission prevents first-unseen-attempt credit. Restore starter does not erase prior attempt history. All new state is in-memory only.

## Supported and explicitly unsupported

Supported C11-inspired subset: one complete, exact provided function signature, block braces, `if`/`else`, `return`, simple initialized `int` / `size_t` / `bool` locals, `stack`/`out_value` pointer comparisons against `NULL`, integer and boolean literals, unary `!`, unary `-`, `*out_value`, `stack->count`, `stack->data[index]`, assignment, `+=`, `-=`, postfix `++`/`--`, `+`, `-`, `*`, `/`, `%`, comparisons, `&&`, `||`, parentheses. Supports short-circuit evaluation, proper bounds, pointer guards, multiple valid bodies, and limited local variables. Simple integer arithmetic uses signed 32-bit values with overflow diagnosed, not undefined-behavior emulation.

Not supported: preprocessor `#include`/`#define`, macros, arbitrary C types, arbitrary identifiers, pointer arithmetic, for/while/do loops, function calls, casts, dynamic allocation, general C memory layout, `sizeof`, structs other than the prescribed `Stack` shape, arbitrary expressions, full C type checking, source compilation, debugger integration, file system/shell/network calls, or a C standard conformance guarantee. A **valid C program outside the subset can be rejected**; an accepted subset program is not guaranteed to compile as C, because native C type checking is not performed. The provided function signatures must match exactly apart from whitespace/comments.

The evaluator never launches a compiler, OS process, or user code. It parses an AST and evaluates only approved statements in **an isolated fictional in-memory model**, with a 5000-character, 1000-token, 250-step, and 48-depth limit. No persistent save keys are created or changed, and nothing writes to Academy mastery.

## Modeled tests

All three functions receive **eight deterministic tests**. For PUSH: empty, one element, partly full, last free slot, full, minimum signed value, maximum signed value, null stack. For POP and PEEK: empty, one element, two elements, full, inactive-cell preservation, null stack, null out pointer, both null. The checker compares function return value, logical count, output-pointer value, and all five backing cells; for rejected operations, it verifies unchanged state. Only the **first failing case** is shown to the learner after each run. Some test inputs are hidden until evaluation; this is diagnostic practice rather than a secure high-stakes examination.

The supplied reference C11 functions in `Examples/stack.c` were independently compiled and ran smoke/edge tests outside Unity. That validates the references, **not the learner's editor source**. No arbitrary submitted C is compiled or executed in this patch.

## Functional acceptance checklist

1. Back up project, stop Play, preserve `.meta` files; confirm old `TLSAcademyApp.cs` SHA256 against `BASELINE_SHA256.txt` before replacing.
2. Reopen existing Academy scene; confirm no C# compile errors. Do **not** rebuild the scene.
3. Stack Lab: original PUSH, POP, PEEK, prediction, TRACE CHALLENGE, and C PRACTICE still work and their buttons do not overlap the new header control.
4. Click **WRITE C FUNCTIONS**; editor accepts multiline input, tab changes preserve drafts, and all controls fit the Game view (test 1600×900, 1280×720, and your preferred size).
5. Starter function fails modeled tests; **SHOW REFERENCE** inserts reference; reference passes **8/8** for PUSH, POP, and PEEK. A wrong `count` guard or off-by-one array index should fail with the first failing case.
6. Independent first try with correct function should record **one modeled first-attempt pass** in-session; hints/reveal, wrong first submission, or Guided attempt must **not** count as first-unseen success.
7. Return to Stack Lab and Academy. Old Trace Challenge saved progress remains; new whole-function attempts must *not* be persisted or award C coding mastery.
8. Run EditMode suites: `StackFunctionEvaluatorTests`, `StackFunctionLabSessionTests`, `StackFunctionLabLayoutTests`, and all previous Phase 1.1–1.3 suites. Report actual results rather than counting written methods as passed tests.

## Future compiler-backed milestone
Before awarding independent C function-implementation mastery, add a genuine secure C11 compilation/testing backend with isolation, resource controls, memory-safety diagnostics, compiler feedback, and unseen tests. This bounded teaching engine is not a substitute for such a backend.
