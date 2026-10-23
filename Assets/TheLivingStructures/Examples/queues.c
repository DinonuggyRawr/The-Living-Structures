#include "queues.h"
#include <stddef.h>

bool queue_enqueue(TeachingQueue *q, int value) {
    if (q == NULL || q->count >= QUEUE_CAPACITY)
        return false;
    int tail = (q->head + q->count) % QUEUE_CAPACITY;
    q->data[tail] = value;
    q->count++;
    return true;
}

bool queue_dequeue(TeachingQueue *q, int *out) {
    if (q == NULL || out == NULL || q->count == 0)
        return false;
    *out = q->data[q->head];
    q->head = (q->head + 1) % QUEUE_CAPACITY;
    q->count--;
    return true;
}

bool queue_peek(const TeachingQueue *q, int *out) {
    if (q == NULL || out == NULL || q->count == 0)
        return false;
    *out = q->data[q->head];
    return true;
}
