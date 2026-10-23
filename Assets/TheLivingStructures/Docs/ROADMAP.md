# The Living Structures — incremental development roadmap (proposed)

**Approved:** standalone Unity project, hybrid virtual academy with specialized 2D laboratories, instructional language C only, visual-first teaching, independent C programming as the outcome.

## Phase 1 — foundation
- **1.1 (this package):** first 3D academy shell, array-backed C Stack lab, trace-driven visuals, first-try independent tracing assessment, pure model unit tests.
- **1.2:** responsive workspace, accessibility groundwork, interactive tutorial and glossary, stronger PlayMode UI tests, structured learning objectives.
- **1.3:** generalized discrete event execution trace architecture, memory-object identity/lifetime model, code-line mapping and safely reversible visual inspection.
- **1.4:** first real guided code-completion tasks and independent authoring path, designed around C rather than a fake all-purpose C parser. Evaluate a sandboxed C compiler process, *outside* the Unity runtime, with resource limits and runtime diagnostics before enabling untrusted code.

## Later curriculum phases
2. Arrays, queues, and stack implementation alternatives: indexing, bounds, circular queues, complexity.
3. Linked lists, memory, dynamic allocation and pointer safety: singly, doubly, circular, malloc/free, invalid accesses.
4. Recursion and trees: stack frames, return values, traversals, binary trees, BST operations.
5. Heaps, graphs, and hash tables: priority queues, BFS/DFS, collision resolution.
6. Runtime operation counts versus asymptotic bounds, documentation, automated tests, boundary conditions, memory management.
7. Independent exams, transfer problems, scaffolding removal, evidence-based C coding mastery.
8. Accessibility, accurate content review, usability pilots, performance and Windows standalone release.

## Quality gates on each milestone
- Verify C and C# semantic alignment, with explicit limits for illustrative models.
- Compile in Unity, execute real EditMode tests and PlayMode checks; report actual pass/fail/skipped counts.
- Test multi-resolution UI and input, non-overlap, transitions, and persistence behavior.
- Check progress represents actual work (viewed vs guided vs independent vs coding mastery).
- Maintain versioned project-only save keys and avoid modifying other projects.
- Document provenance, source hashes, limitations, and user-verified acceptance.
