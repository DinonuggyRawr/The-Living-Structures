#include "stack.h"
#include <assert.h>
#include <limits.h>
#include <stdio.h>

int main(void) {
    Stack s;
    int x = -1;
    stack_init(&s);
    assert(s.count == 0);
    assert(!stack_pop(&s, &x));
    assert(!stack_peek(&s, &x));
    assert(x == -1);
    assert(!stack_push(NULL, 1));
    assert(!stack_pop(NULL, &x));
    assert(!stack_pop(&s, NULL));
    assert(!stack_peek(NULL, &x));
    assert(!stack_peek(&s, NULL));
    assert(stack_push(&s, INT_MIN));
    assert(stack_push(&s, 20));
    assert(stack_peek(&s, &x) && x == 20 && s.count == 2);
    assert(stack_pop(&s, &x) && x == 20 && s.count == 1);
    assert(s.data[1] == 20); /* raw value remains after pop */
    assert(stack_pop(&s, &x) && x == INT_MIN);
    assert(s.count == 0);
    for (int i = 0; i < CAPACITY; ++i)
        assert(stack_push(&s, i + 1));
    assert(s.count == CAPACITY);
    assert(!stack_push(&s, 999));
    assert(s.count == CAPACITY);
    for (int i = CAPACITY; i > 0; --i)
        assert(stack_pop(&s, &x) && x == i);
    assert(s.count == 0);
    assert(!stack_pop(&s, &x));
    puts("C11 stack edge tests: PASS");
    return 0;
}
