/* TLS Phase 1.5 educational C11 reference test harness. TRUSTED test program.
 * Compiled in a dedicated Docker container, in a separate translation unit.
 * The student's sole function is compiled as submission.c; no host functions run.
 * Success on these finite tests does not prove general C correctness or memory safety.
 */
#include <stdbool.h>
#include <stddef.h>
#include <limits.h>
#include <stdlib.h>

#define CAPACITY 5
typedef struct { int data[CAPACITY]; size_t count; } Stack;
bool stack_push(Stack *stack, int value);
bool stack_pop(Stack *stack, int *out_value);
bool stack_peek(const Stack *stack, int *out_value);

#if !defined(TLS_SELECTED) || TLS_SELECTED < 0 || TLS_SELECTED > 2
#error Select PUSH(0), POP(1), or PEEK(2)
#endif

/* Eight intentionally independent cases, one per process invocation. */
static const int initial[8][5] = {
    {0,0,0,0,0}, {7,0,0,0,0}, {-3,21,0,0,0},
#if TLS_SELECTED == 0
    {1,2,3,4,0}, {10,-2,30,0,50}, {0,0,0,0,0},
    {-3,21,0,0,0}, {0,0,0,0,0}
#else
    {10,-2,30,0,50}, {-6,8,77,0,0},
    {7,0,0,0,0}, {-3,21,0,0,0}, {7,0,0,0,0}
#endif
};
static const size_t counts[8] = {
#if TLS_SELECTED == 0
    0,1,2,4,5,0,2,0
#else
    0,1,2,5,2,1,2,1
#endif
};
#if TLS_SELECTED == 0
static const int inputs[8] = {
    12,-9,0,99,20,INT_MIN,INT_MAX,1
};
#endif
static int test_case(int id)
{
    if (id < 0 || id >= 8) return 10;
    Stack s = {{0},0};
    int expected[CAPACITY];
    for (int j=0; j<CAPACITY; ++j) {s.data[j]=initial[id][j]; expected[j]=initial[id][j];}
    s.count=counts[id];
    const bool has_stack =
#if TLS_SELECTED == 0
        id != 7;
#else
        id != 5 && id != 7;
#endif
#if TLS_SELECTED != 0
    const bool has_output = id != 6 && id != 7;
#endif
    const bool should_succeed = has_stack &&
#if TLS_SELECTED == 0
        s.count < CAPACITY;
#else
        has_output && s.count > 0;
#endif
    size_t expected_count = s.count;
#if TLS_SELECTED != 0
    int out = 999, expected_out = 999;
#endif
#if TLS_SELECTED == 0
    if (should_succeed) {expected[expected_count]=inputs[id]; ++expected_count;}
    bool ok=stack_push(has_stack?&s:NULL,inputs[id]);
#elif TLS_SELECTED == 1
    if (should_succeed) {expected_out = expected[expected_count-1]; --expected_count;}
    bool ok=stack_pop(has_stack?&s:NULL,has_output?&out:NULL);
#else
    if (should_succeed) expected_out=expected[expected_count-1];
    bool ok=stack_peek(has_stack?&s:NULL,has_output?&out:NULL);
#endif
    if (ok != should_succeed) return 2;
    if (has_stack) {
        if (s.count != expected_count) return 3;
        for (int j=0; j<CAPACITY; ++j) if (s.data[j] != expected[j]) return 4;
    }
#if TLS_SELECTED != 0
    if (has_output && out != expected_out) return 5;
#endif
    return 0;
}
int main(int argc, char **argv)
{
    if (argc != 2) return 10;
    char *end=NULL;
    long which=strtol(argv[1],&end,10);
    if (!end || *end!='\0' || which < 0 || which > 7) return 10;
    return test_case((int)which);
}
