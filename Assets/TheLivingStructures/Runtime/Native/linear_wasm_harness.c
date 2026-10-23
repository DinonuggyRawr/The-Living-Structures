/* TLS 2.3 trusted C11 harness for seven separately compiled learner functions.
 * All expected values are derived from an independent reference oracle.
 * One test case per WASM invocation; no file/network preopens.
 * This finite suite is educational evidence, not a memory-safety guarantee.
 */
#include <stdbool.h>
#include <stddef.h>
#include <stdlib.h>
#include <limits.h>
#define ARRAY_CAPACITY 6
#define QUEUE_CAPACITY 5
typedef struct { int data[ARRAY_CAPACITY]; int count; } TeachingArray;
typedef struct { int data[QUEUE_CAPACITY]; int head; int count; } TeachingQueue;
bool array_append(TeachingArray *, int);
bool array_insert(TeachingArray *, int, int);
bool array_remove(TeachingArray *, int, int *);
bool array_get(const TeachingArray *, int, int *);
bool queue_enqueue(TeachingQueue *, int);
bool queue_dequeue(TeachingQueue *, int *);
bool queue_peek(const TeachingQueue *, int *);
#if !defined(TLS_SELECTED) || TLS_SELECTED < 0 || TLS_SELECTED > 6
#error TLS_SELECTED must be an operation from 0 through 6
#endif

#if TLS_SELECTED < 4
static void setup_array(TeachingArray *a, int count) {
    a->count = count;
    for (int i=0; i<ARRAY_CAPACITY; ++i) a->data[i] = 100 + 7*i;
}
#endif
#if TLS_SELECTED >= 4
static void setup_queue(TeachingQueue *q, int head, int count) {
    q->head = head; q->count = count;
    for (int i=0; i<QUEUE_CAPACITY; ++i) q->data[i] = 200 + 7*i;
}
#endif
#if TLS_SELECTED < 4
static int run_array(int id) {
#if TLS_SELECTED == 0
    static const int appendCounts[10] = {0,1,3,5,6,0,2,2,4,2};
#endif
#if TLS_SELECTED == 1
    static const int insertCounts[10] = {0,3,3,3,5,6,3,3,0,2};
    static const int insertIndices[10] = {0,1,0,3,2,1,-1,4,1,1};
#endif
#if TLS_SELECTED == 2
    static const int removeCounts[10] = {0,1,3,3,3,6,3,3,3,2};
    static const int removeIndices[10] = {0,0,1,0,2,3,-1,3,1,0};
#endif
#if TLS_SELECTED == 3
    static const int getCounts[10] = {0,1,3,6,3,3,3,3,3,2};
    static const int getIndices[10] = {0,0,1,5,0,2,-1,3,1,0};
#endif
#if TLS_SELECTED == 0 || TLS_SELECTED == 1
    static const int values[10] = {42,-4,0,99,77,INT_MIN,INT_MAX,-123,35,8};
#endif
    TeachingArray actual, expected;
#if TLS_SELECTED == 0
    const int count = appendCounts[id];
#elif TLS_SELECTED == 1
    const int count = insertCounts[id];
#elif TLS_SELECTED == 2
    const int count = removeCounts[id];
#else
    const int count = getCounts[id];
#endif
    setup_array(&actual,count); setup_array(&expected,count);
    const bool has_obj = id != 9;
#if TLS_SELECTED == 2 || TLS_SELECTED == 3
    const bool has_output = id != 8;
    int out = 987654, expected_out = 987654;
#endif
    bool expected_ok = false, actual_ok = false;
#if TLS_SELECTED == 0
    const int v = values[id];
    expected_ok = has_obj && count < ARRAY_CAPACITY;
    if (expected_ok) { expected.data[count] = v; expected.count++; }
    actual_ok = array_append(has_obj ? &actual : NULL,v);
#elif TLS_SELECTED == 1
    const int ix = insertIndices[id], v = values[id];
    expected_ok = has_obj && count < ARRAY_CAPACITY && ix >= 0 && ix <= count;
    if (expected_ok) {
        for (int i=count; i>ix; --i) expected.data[i] = expected.data[i-1];
        expected.data[ix] = v; expected.count++;
    }
    actual_ok = array_insert(has_obj ? &actual : NULL,ix,v);
#elif TLS_SELECTED == 2
    const int ix = removeIndices[id];
    expected_ok = has_obj && has_output && ix >= 0 && ix < count;
    if (expected_ok) {
        expected_out = expected.data[ix];
        for (int i=ix; i+1<count; ++i) expected.data[i] = expected.data[i+1];
        expected.count--;
    }
    actual_ok = array_remove(has_obj ? &actual : NULL,ix,has_output ? &out : NULL);
#else
    const int ix = getIndices[id];
    expected_ok = has_obj && has_output && ix >= 0 && ix < count;
    if (expected_ok) expected_out = expected.data[ix];
    actual_ok = array_get(has_obj ? &actual : NULL,ix,has_output ? &out : NULL);
#endif
    if (actual_ok != expected_ok) return 2;
    if (has_obj) {
        if (actual.count != expected.count) return 3;
        for (int i=0; i<ARRAY_CAPACITY; ++i) if (actual.data[i] != expected.data[i]) return 4;
    }
#if TLS_SELECTED == 2 || TLS_SELECTED == 3
    if (has_output && out != expected_out) return 5;
#endif
    return 0;
}
#endif
#if TLS_SELECTED >= 4
static int run_queue(int id) {
    static const int heads[10] = {0,0,3,1,4,4,3,3,0,2};
#if TLS_SELECTED == 4
    static const int counts[10] = {0,1,2,4,5,0,1,1,4,2};
#else
    static const int counts[10] = {0,1,3,5,1,2,0,3,2,1};
#endif
#if TLS_SELECTED == 4
    static const int inputs[10] = {42,-4,0,99,77,INT_MIN,INT_MIN,INT_MAX,35,8};
#endif
    TeachingQueue actual, expected;
    setup_queue(&actual,heads[id],counts[id]);
    setup_queue(&expected,heads[id],counts[id]);
    const bool has_obj = id != 9;
#if TLS_SELECTED == 5 || TLS_SELECTED == 6
    const bool has_output = id != 7;
    int out = 987654, expected_out = 987654;
#endif
    bool expected_ok = false, actual_ok = false;
#if TLS_SELECTED == 4
    expected_ok = has_obj && expected.count < QUEUE_CAPACITY;
    if (expected_ok) {
        int tail = (expected.head + expected.count) % QUEUE_CAPACITY;
        expected.data[tail] = inputs[id]; expected.count++;
    }
    actual_ok = queue_enqueue(has_obj ? &actual : NULL,inputs[id]);
#elif TLS_SELECTED == 5
    expected_ok = has_obj && has_output && expected.count > 0;
    if (expected_ok) {
        expected_out = expected.data[expected.head];
        expected.head = (expected.head + 1) % QUEUE_CAPACITY;
        expected.count--;
    }
    actual_ok = queue_dequeue(has_obj ? &actual : NULL,has_output ? &out : NULL);
#else
    expected_ok = has_obj && has_output && expected.count > 0;
    if (expected_ok) expected_out = expected.data[expected.head];
    actual_ok = queue_peek(has_obj ? &actual : NULL,has_output ? &out : NULL);
#endif
    if (actual_ok != expected_ok) return 2;
    if (has_obj) {
        if (actual.count != expected.count || actual.head != expected.head) return 3;
        for (int i=0; i<QUEUE_CAPACITY; ++i) if (actual.data[i] != expected.data[i]) return 4;
    }
#if TLS_SELECTED == 5 || TLS_SELECTED == 6
    if (has_output && out != expected_out) return 5;
#endif
    return 0;
}
#endif
int main(int argc, char **argv) {
    if (argc != 2) return 10;
    char *end = NULL;
    long case_id = strtol(argv[1], &end, 10);
    if (end == NULL || *end != '\0' || case_id < 0 || case_id >= 10) return 10;
#if TLS_SELECTED < 4
    return run_array((int)case_id);
#else
    return run_queue((int)case_id);
#endif
}
