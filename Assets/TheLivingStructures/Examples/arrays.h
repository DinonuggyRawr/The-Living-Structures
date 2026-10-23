#ifndef TLS_TEACHING_ARRAYS_H
#define TLS_TEACHING_ARRAYS_H
#include <stdbool.h>
#define ARRAY_CAPACITY 6

typedef struct {
    int data[ARRAY_CAPACITY];
    int count;
} TeachingArray;

bool array_append(TeachingArray *a, int value);
bool array_insert(TeachingArray *a, int index, int value);
bool array_remove(TeachingArray *a, int index, int *out);
bool array_get(const TeachingArray *a, int index, int *out);
#endif
