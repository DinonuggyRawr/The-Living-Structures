# Phase 3 integrated C pointer and linked-list learning scope

## Learning ladder

1. **Learn:** Phase 3.1 pointers, malloc/free, aliasing, dangling pointers.
2. **Watch:** Phase 3 visual list algorithms with symbolic head/tail/next/prev, allocated and freed nodes, and bounded traversal.
3. **Experiment:** Alter values and indices; perform prepend, append, insert, remove, search, reverse, clear. Circular traversal stops at head; recursive NULL-based functions are not usable on rings without adapting base cases.
4. **Guided Practice:** Five each of tracing, C statement completion, and bug repair; show targeted misconception feedback and hints.
5. **Independent Practice:** Four short structured challenges and five actual C11 singly-linked function problems in WASM. Independent evidence is first-unseen attempt only; grades are session-only and distinguish modeled versus real C tests.
6. **Master:** A future independent project should test novel C coding scenarios, detailed pointer/memory invariants, style, documentation, compiler warnings, and resource safety; this prototype **does not** award mastery.

## Technical limits

The linked-list renderer uses a bounded deterministic heap of up to eight symbolic slots. It shows explicit node fields and a readable HEAD -> values -> NULL/HEAD chain, rather than process addresses. It can rewind a historical trace after committing an operation, but **rewinding the picture does not undo the committed data structure**. Calling RESET TRACE clears the trace, not the list; switching list kind resets that mode's list.

The five compiler-backed functions target only:

`typedef struct Node { int data; struct Node *next; } Node;`

No `Node.prev` field exists in compiler-backed functions. Doubly/circular exercises are visualization and structured C concept practice, not yet compiled full-function implementations.

The reference C snippets on visual mutation traces are phase-oriented explanatory fragments. Only the dedicated function-lab source is actually passed to Zig/Wasmtime. Non-C behaviors (allocator failure, UB) remain safely conceptual where stated.

## Critical misconceptions to surface

- NULL is a sentinel value, not an allocated node.
- Relink before free; never read `old->next` after `free(old)`.
- A `Node **head` lets a function change its caller's head pointer.
- `tail->next=head` describes a circular ring, not a NULL-terminated list.
- Reverse links, not data values, for in-place list reversal.
- `prev` and `next` must agree for a doubly linked list.
- Circular recursion needs an explicit stopping boundary; `if (node==NULL)` is insufficient.
- Incorrect pointers can trigger undefined behavior; visual model does not execute it.
- Passing 10 finite tests does not prove unrestricted correct C behavior.
