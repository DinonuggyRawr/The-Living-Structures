# Experimental bulk Phase 3 acceptance checklist

## Unity first

[ ] Unity Editor recompiles without errors after copy (no scene rebuild)
[ ] Full EditMode suite green, including new TLSLinkedListModel/Practice/Wasm/Layout tests
[ ] Academy labs still accessible: Stack, Array, Queue, Pointer Memory, new Linked Lists
[ ] Existing Stack, Array and Queue WASM still pass reference smoke tests
[ ] Prior Academy progress/save retained
[ ] No header/footer overlap at 1600x900, 1920x1080, reduced 1280x720 (inspect text clipping)
[ ] MIN/X and back navigation remain visible; no input traps

## Visual list models

[ ] Singly append, indexed insert/remove, reverse and search sequences correct
[ ] Doubly forward/back references update on head/middle/tail removal
[ ] Circular tail.next=head and bounded traversal/search
[ ] Circular NULL-based recursive trace blocked with clear explanation
[ ] malloc symbolic heap bounded; freed node cannot appear reachable; reused freed slots behave coherently
[ ] Clear step-by-step preserves valid live traversal; no freed pointer dereference
[ ] Prediction feedback appears after final step, not before
[ ] Step rewind does not secretly mutate committed list state

## Student C and tests

[ ] 19 structured questions render; wrong and corrected answers behave correctly
[ ] Hint/reveal/failed first tries cannot later earn unseen first credit
[ ] Five supplied C11 linked-list references pass 10/10 real WebAssembly cases each
[ ] Invalid C yields compiler diagnostics; stale passes hidden when source changes
[ ] No saved C mastery awarded from finite tests or formative tasks
[ ] Explicit WASM opt-in, cancellation, resource limits and no directory grants retained
[ ] Windows offline standalone bundle and clean-machine license/security verification deferred to release testing

**Experimental acceptance:** report which checks you actually performed. Partial success does not establish all Phase 3 work is production-complete.
