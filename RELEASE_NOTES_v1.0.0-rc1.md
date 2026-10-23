# The Living Structures — v1.0.0-rc1 (draft)

**Status: release candidate / public binary redistribution pending final license and native compiler security review.**

The Living Structures is a visual Computing II academy that combines interactive data-structure models with C11 practice and compiler-backed WebAssembly tests.

### Included subjects

Stacks, arrays, circular queues, pointers and memory, linked lists, recursion, general and binary trees, BSTs, heaps, graphs, hash tables, algorithmic complexity, testing/debugging, and learning-path assessments.

### Quality improvements

Interactive Academy navigation, connected tree views, multiline C editing, readable/accessibility options, Exit App confirmation, and a nonblocking compiler-status check for responsive buttons.

### Functional testing reported by the developer

- Unity full EditMode suite reported green after the Exit App hotfix.
- Windows standalone reportedly launched and compiled C11 exercises, including linked-list prepend (10/10 demonstrated).
- Clean-machine operation with no internet was explicitly confirmed.
- The latest UI responsiveness patch reportedly fixed the several-second button delay. Formal quantitative performance profiling not supplied.

### Important limitations

- A Windows player requires its complete `_Data` folder, Unity runtime and bundled compiler directories; the `.exe` alone does not work.
- Source repository clone alone does not provide the native Zig/Wasmtime binaries.
- Native Zig executes locally with normal user permissions; this is not an adversarial-source sandbox. Wasmtime limits apply after compilation.
- Passing finite C11 tests doesn't prove total correctness or transferable mastery. 
- Binary license/notice inventory and full version-specific security and Unity terms review remain open. **Do not remove this caveat or mark GA until reviewed.**
