#ifndef TLS_TEACHING_QUEUES_H
#define TLS_TEACHING_QUEUES_H
#include <stdbool.h>
#define QUEUE_CAPACITY 5

typedef struct {
    int data[QUEUE_CAPACITY];
    int head;
    int count;
} TeachingQueue;

bool queue_enqueue(TeachingQueue *q, int value);
bool queue_dequeue(TeachingQueue *q, int *out);
bool queue_peek(const TeachingQueue *q, int *out);
#endif
