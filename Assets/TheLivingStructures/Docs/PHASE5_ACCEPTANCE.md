# Phase 5 Unity acceptance / regression checklist

This is a development acceptance list, not a statement that Unity checks were already run.

## Install / compile
- Back up project, close Unity, verify TLSAcademyApp.cs SHA-256 against BASELINE_SHA256.txt.
- If source has local edits, merge DIFFS instead of overwriting source. Preserve existing .meta assets.
- Copy only ZIP's Assets/TheLivingStructures files into matching existing project folder; don't delete existing files.
- Open the same pre-existing Academy scene (no scene rebuild). Confirm zero red compiler errors.
- Run full EditMode suite, including six new Phase 5 suites; report any exact first failure.

## Academy directory / buildings
- Academy directory appears at left with FOUNDATIONS / STRUCTURES / ADVANCED tabs.
- Foundations: Stack, Array, Queue, Pointer; Structures: Linked List, Recursion, Tree/BST, Heap; Advanced: Graph, Hash.
- Click each existing launcher; test back navigation and preserved Stack Trace Challenge progress.
- Verify directory does not cover central 3D buildings, including at 1600x900 and 1280x720; compare SHOW LEARNING PATH/F0CUS ON CAMPUS.
- Buildings are currently conceptual landmarks (not physically clickable); directory is implemented now, later building interactions planned.

## Heap
- MIN INSERT 7, 4, 9, 2 -> root 2; after EXTRACT root 4; all parent/child edges correct.
- MAX switch resets state; insert 3, 8, 1 -> root 8.
- BUILD/HEAPIFY example -> 7 nodes, root 1 for min (9 for max), array/edges correspond.
- Full 15-cell heap rejects insertion without committing a new element; empty extraction rejects.
- Step forward/backward and FINISH; preview does not commit before final frame; RESET TRACE rolls back the traced operation.
- PREDICT MORE/FEWER/SAME displays outcome only on final trace frame.

## Graph
- Default sample edges: A-B, A-C, B-D, C-D, D-E, E-F.
- BFS from A = A,B,C,D,E,F; DFS from A = A,B,D,C,E,F (alphabetical neighbor scan).
- Switch DIRECTED: resets edges; LOAD SAMPLE -> directed arrows; BFS from F visits only F.
- Add/remove edge, add vertex, invalid vertex and max-nine bound; view adjacency list summary and explicit matrix model.
- Verify line connections and arrowheads reflect the exact stored adjacency data, including zero-node graph.

## Hash
- Separate chaining: keys 1,8,15,-6 all map to bucket 1; collision example displays 4 entries.
- Linear probing: insert 1 and 8, remove 1, find 8 still succeeds; deleted slot must show DELETED tombstone.
- Insert 15 into reused tombstone at index 1. Negative modulo and INT_MIN stay valid.
- Full probing table fails insertion safely; duplicate insertion does not increase count.

## Learning/C11
- Practice: 24 structured tasks (8 per domain): TRACE, COMPLETE, REPAIR, CHALLENGE; Guided and Independent accounting.
- New phase's result/attempt counters are session-only. Revealed references cannot earn first-try independent credit.
- WASM is disabled by default, uses existing Zig/Wasmtime and Phase 2.4 bundled toolchain checks, and does not download anything.
- Seven reference functions (Heap Insert/Extract, Graph BFS/DFS, Hash Insert/Find/Remove) should each pass 8/8 compiler-backed runs.
- Try incomplete C code: diagnostics should be visible and no pass count awarded.
- Edit a previously passing draft: old result must disappear until rerun. Check cancellation and a null-input case.

## Regressions
- Stack visual/C practice/8-case WASM, Array/Queue visual/structured/10-case WASM, Pointer, Linked List and Phase4 Recursion/BST remain usable.
- Unity progress/save keys, compiler files, Phase2.4 Windows staging, .meta, and scene objects are not replaced by this patch.
- This is not a standalone/clean-machine/security audit; that remains Phase 8.
