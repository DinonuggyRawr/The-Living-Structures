# Phase 2.1 — Arrays and Circular Queues (C11)

## Instructional intent

After completing introductory stack lessons, learners compare contiguous array indexing and FIFO ring-buffer traversal. Every operation has one authoritative deterministic timeline that drives the source-line highlight, physical memory, logical membership and explanation.

### Array invariants

`0 <= count <= 6`. Only `data[0]` through `data[count - 1]` belong to the logical sequence. `data[count]` may hold a recently written pending value before the count update, or an inactive stale value after removal. Both are physically allocated slots but not logically part of the array. For valid `index` in `[0,count]`, `insert` shifts existing values **right-to-left** to preserve source cells; removal of valid `[0,count-1]` shifts **left-to-right**. Reads and invalid operations do not write or alter count. The C source uses `int` indices, and explicitly checks negative values before indexing.

### Circular queue invariants

`0 <= count <= 5` and `0 <= head < 5`. Logical FIFO index `i` maps to physical slot `(head + i) % 5`; when not full, the next tail slot is `(head + count) % 5`. Enqueue stores at tail before increasing count. Dequeue reads the oldest value at `head`, then advances head modulo 5 and decrements count. The intermediate snapshot after moving head but before decreasing count is **not** a stable completed FIFO state; trace text calls that out. After removing the last element, `head` is left where it lands, rather than forcibly reset to zero. No stale inactive cell is automatically erased.

### Prediction scaffolding

Before tracing, students can predict whether `count` increases by 1, decreases by 1, or stays unchanged. Bounds/full/empty rejections, GET and PEEK keep count unchanged. Feedback is delayed until final step; skip is available and grants no success credit. Prediction counts are in-session and are not independently authored code or mastery certificates.

### C consistency and verification

The exact line-traced C snippets are reproduced in `Examples/arrays.c` and `Examples/queues.c`, with headers describing real C structs and operation signatures. Those separate C reference functions can be compiled externally; student-entered arbitrary C is **not** compiled by either of these new labs. The existing opt-in Stack WASM C11 compiler remains limited to stack functions. All new data models can run in Unity EditMode tests without camera or UI.

### Recommended student lab questions

1. Why must array insertion shift right from the highest occupied index, rather than from the insertion index upward?
2. Why can an inactive array or queue slot retain a nonzero value after removal?
3. How does the circular queue find its next tail when the physical last slot was just used?
4. How can two queues with different `head` values represent the same logical FIFO sequence?
5. Why is array insertion worst-case O(n), while ring enqueue and dequeue can be O(1) with bounded capacity?
6. Which C checks prevent out-of-bounds reads, writes, and underflow?

### Post-install smoke checklist

- No new Unity Console compiler errors.
- Array and Queue entrances from Academy; Stack Lab and all old learning tools still open.
- Prediction, cancel, skip and finish logic, invalid integer feedback.
- Full array overflow, negative/too-large index rejection, shifts and retained raw cells.
- Queue full/empty bounds, wraparound after 5 enqueues/2 dequeues/2 enqueues, FIFO order, head and tail readouts.
- Earlier trace frames visible without breaking later committed state.
- Existing saved Stack Trace Challenge retained; prior wasm source and toolchain untouched.
- New EditMode suites and full old suite green, visual check at primary and smaller Game resolutions.
