#include "stack.h"
#include <assert.h>
#include <stdio.h>

int main(void) {
    Stack stack;
    int value = -1;
    stack_init(&stack);
    assert(!stack_pop(&stack, &value));
    assert(stack_push(&stack, 4));
    assert(stack_push(&stack, 9));
    assert(stack_pop(&stack, &value) && value == 9);
    assert(stack_push(&stack, 7));
    assert(stack_peek(&stack, &value) && value == 7);
    assert(stack.count == 2);
    puts("C11 stack demo: PASS");
    return 0;
}
