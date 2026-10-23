#include "arrays.h"
#include <stddef.h>

bool array_append(TeachingArray *a, int value) {
    if (a == NULL || a->count >= ARRAY_CAPACITY)
        return false;
    a->data[a->count] = value;
    a->count++;
    return true;
}

bool array_insert(TeachingArray *a, int index, int value) {
    if (a == NULL || a->count >= ARRAY_CAPACITY ||
        index < 0 || index > a->count) return false;
    for (int i = a->count; i > index; --i)
        a->data[i] = a->data[i - 1];
    a->data[index] = value;
    a->count++;
    return true;
}

bool array_remove(TeachingArray *a, int index, int *out) {
    if (a == NULL || out == NULL ||
        index < 0 || index >= a->count) return false;
    *out = a->data[index];
    for (int i = index; i + 1 < a->count; ++i)
        a->data[i] = a->data[i + 1];
    a->count--;
    return true;
}

bool array_get(const TeachingArray *a, int index, int *out) {
    if (a == NULL || out == NULL ||
        index < 0 || index >= a->count) return false;
    *out = a->data[index];
    return true;
}
