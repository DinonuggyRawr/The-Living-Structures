// Standalone C11 reference regressions; not executed by Unity or the WASM Stack lab.
#include "arrays.h"
#include "queues.h"
#include <assert.h>
#include <limits.h>
#include <stddef.h>
#include <stdio.h>

static void array_tests(void)
{
    TeachingArray a = {{0}, 0};
    int out = -99;
    assert(!array_get(&a, 0, &out));
    assert(!array_remove(&a, 0, &out));
    assert(!array_append(NULL, 3));
    assert(!array_insert(NULL, 0, 3));
    assert(!array_remove(NULL, 0, &out));
    assert(!array_get(NULL, 0, &out));
    assert(!array_get(&a, 0, NULL));
    assert(!array_remove(&a, 0, NULL));
    assert(!array_insert(&a, -1, 4));
    assert(!array_insert(&a, 1, 4));
    assert(array_append(&a, INT_MIN));
    assert(array_append(&a, 20));
    assert(array_append(&a, INT_MAX));
    assert(a.count == 3);
    assert(array_insert(&a, 1, 15));
    assert(a.count == 4 && a.data[0] == INT_MIN && a.data[1] == 15);
    assert(a.data[2] == 20 && a.data[3] == INT_MAX);
    assert(array_get(&a, 2, &out) && out == 20 && a.count == 4);
    assert(array_remove(&a, 1, &out) && out == 15);
    assert(a.count == 3 && a.data[1] == 20 && a.data[2] == INT_MAX);
    // No zeroing: the physically retained value is inactive.
    assert(a.data[3] == INT_MAX);
    assert(array_insert(&a, 0, 5));
    assert(array_insert(&a, a.count, 100));
    assert(array_append(&a, 101));
    assert(a.count == ARRAY_CAPACITY);
    assert(!array_append(&a, 102));
    assert(!array_insert(&a, 0, 103));
    assert(!array_get(&a, a.count, &out));
    assert(!array_remove(&a, -1, &out));
    assert(a.count == ARRAY_CAPACITY);
}

static void queue_tests(void)
{
    TeachingQueue q = {{0}, 0, 0};
    int out = -99;
    assert(!queue_dequeue(&q, &out));
    assert(!queue_peek(&q, &out));
    assert(!queue_enqueue(NULL, 42));
    assert(!queue_dequeue(NULL, &out));
    assert(!queue_peek(NULL, &out));
    assert(!queue_dequeue(&q, NULL));
    assert(!queue_peek(&q, NULL));
    for (int i = 1; i <= 5; i++) assert(queue_enqueue(&q, i * 10));
    assert(q.count == QUEUE_CAPACITY && q.head == 0);
    assert(!queue_enqueue(&q, 60));
    assert(queue_peek(&q, &out) && out == 10 && q.count == 5);
    assert(queue_dequeue(&q, &out) && out == 10);
    assert(queue_dequeue(&q, &out) && out == 20);
    assert(q.head == 2 && q.count == 3);
    assert(queue_enqueue(&q, 60));
    assert(queue_enqueue(&q, 70));
    assert(q.count == 5 && q.head == 2);
    assert(q.data[0] == 60 && q.data[1] == 70);
    const int expected[] = {30, 40, 50, 60, 70};
    for (int i = 0; i < 5; i++) assert(queue_dequeue(&q, &out) && out == expected[i]);
    assert(q.count == 0 && q.head == 2);
    assert(q.data[1] == 70); // stale physical cells are allowed
    assert(!queue_dequeue(&q, &out));
    assert(queue_enqueue(&q, INT_MIN));
    assert(queue_peek(&q, &out) && out == INT_MIN);
    assert(queue_dequeue(&q, &out) && out == INT_MIN);
}
int main(void)
{
    array_tests(); queue_tests();
    puts("PASS: C11 arrays and circular queue reference behavior");
    return 0;
}
