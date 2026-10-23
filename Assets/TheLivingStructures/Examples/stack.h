#ifndef TLS_STACK_H
#define TLS_STACK_H

#include <stdbool.h>
#include <stddef.h>

#define CAPACITY 5

typedef struct {
    int data[CAPACITY];
    size_t count;
} Stack;

void stack_init(Stack *stack);
bool stack_push(Stack *stack, int value);
bool stack_pop(Stack *stack, int *out_value);
bool stack_peek(const Stack *stack, int *out_value);

#endif
