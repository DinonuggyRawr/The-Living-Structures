#include "stack.h"

void stack_init(Stack *stack) {
    if (stack == NULL)
        return;
    // Set all backing cells to 0, with no logical active elements.
    for (size_t i = 0; i < CAPACITY; ++i)
        stack->data[i] = 0;
    stack->count = 0;
}

bool stack_push(Stack *stack, int value) {
    if (stack == NULL || stack->count >= CAPACITY)
        return false;
    stack->data[stack->count] = value;
    stack->count++;
    return true;
}

bool stack_pop(Stack *stack, int *out_value) {
    if (stack == NULL || out_value == NULL || stack->count == 0)
        return false;
    stack->count--;
    *out_value = stack->data[stack->count];
    return true;
}

bool stack_peek(const Stack *stack, int *out_value) {
    if (stack == NULL || out_value == NULL || stack->count == 0)
        return false;
    *out_value = stack->data[stack->count - 1];
    return true;
}
