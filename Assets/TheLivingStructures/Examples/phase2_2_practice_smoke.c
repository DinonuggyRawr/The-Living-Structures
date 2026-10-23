/* Phase 2.2 practice trace/reference verification against actual C11 implementations. */
#include "arrays.h"
#include "queues.h"
#include <assert.h>
#include <stdio.h>

static void array_traces(void) {
    TeachingArray a = {{0}, 0};
    int out = -1;
    assert(array_append(&a, 10));
    assert(array_append(&a, 20));
    assert(array_insert(&a, 1, 15));
    assert(a.data[2] == 20);

    TeachingArray b = {{0}, 0};
    assert(array_append(&b, 4));
    assert(array_append(&b, 8));
    assert(array_remove(&b, 0, &out) && out == 4);
    assert(b.count == 1 && b.data[0] == 8 && b.data[1] == 8);

    TeachingArray c = {{0}, 0};
    assert(array_append(&c, 3));
    assert(array_append(&c, 7));
    out = -1;
    assert(array_get(&c, 1, &out) && out == 7);
}
static void queue_traces(void) {
    TeachingQueue q = {{0}, 0, 0};
    int out = -1;
    assert(queue_enqueue(&q, 6));
    assert(queue_enqueue(&q, 9));
    assert(queue_dequeue(&q, &out) && out == 6);

    TeachingQueue w = {{0}, 0, 0};
    for (int n = 10; n <= 50; n += 10) assert(queue_enqueue(&w, n));
    assert(queue_dequeue(&w, &out));
    assert(queue_dequeue(&w, &out));
    assert(queue_enqueue(&w, 60));
    assert(queue_enqueue(&w, 70));
    assert(w.head == 2 && w.count == 5 && w.data[0] == 60);
    assert(w.data[w.head] == 30);
}
int main(void) {
    array_traces();
    queue_traces();
    puts("Phase 2.2 C11 trace cases PASS (arrays, queue, wraparound, stale memory)");
    return 0;
}
