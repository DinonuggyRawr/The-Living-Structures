#include <stdbool.h>
#include <stddef.h>
#include <limits.h>

typedef struct { int data[15]; size_t size; } TeachingHeap;
typedef struct { int n; unsigned char edge[9][9]; } TeachingGraph;
typedef struct { int key[7]; unsigned char state[7]; } TeachingHash;

bool heap_insert_min(TeachingHeap *heap, int value) {
    if (heap == NULL || heap->size >= 15) return false;
    size_t i = heap->size++;
    while (i > 0) {
        size_t parent = (i - 1) / 2;
        if (heap->data[parent] <= value) break;
        heap->data[i] = heap->data[parent];
        i = parent;
    }
    heap->data[i] = value;
    return true;
}

bool heap_extract_min(TeachingHeap *heap, int *out) {
    if (heap == NULL || out == NULL || heap->size == 0 || heap->size > 15) return false;
    *out = heap->data[0];
    int moved = heap->data[--heap->size];
    if (heap->size == 0) return true;
    size_t i = 0;
    while (2 * i + 1 < heap->size) {
        size_t child = 2 * i + 1;
        if (child + 1 < heap->size && heap->data[child + 1] < heap->data[child]) ++child;
        if (moved <= heap->data[child]) break;
        heap->data[i] = heap->data[child];
        i = child;
    }
    heap->data[i] = moved;
    return true;
}

size_t graph_bfs(const TeachingGraph *graph, int start, int *out, size_t capacity) {
    if (graph == NULL || graph->n < 1 || graph->n > 9 || start < 0 || start >= graph->n) return 0;
    bool visited[9] = {false};
    int queue[9]; int head = 0, tail = 0;
    int order[9]; size_t count = 0;
    visited[start] = true;
    queue[tail++] = start;
    while (head < tail) {
        int u = queue[head++];
        order[count++] = u;
        for (int v = 0; v < graph->n; ++v) {
            if (graph->edge[u][v] && !visited[v]) {
                visited[v] = true;
                queue[tail++] = v;
            }
        }
    }
    if (count > capacity || out == NULL) return 0;
    for (size_t i = 0; i < count; ++i) out[i] = order[i];
    return count;
}

static void graph_dfs_visit(const TeachingGraph *graph, int u, bool *seen, int *order, size_t *count) {
    seen[u] = true;
    order[(*count)++] = u;
    for (int v = 0; v < graph->n; ++v)
        if (graph->edge[u][v] && !seen[v]) graph_dfs_visit(graph, v, seen, order, count);
}
size_t graph_dfs(const TeachingGraph *graph, int start, int *out, size_t capacity) {
    if (graph == NULL || graph->n < 1 || graph->n > 9 || start < 0 || start >= graph->n) return 0;
    bool visited[9] = {false};
    int order[9]; size_t count = 0;
    graph_dfs_visit(graph, start, visited, order, &count);
    if (count > capacity || out == NULL) return 0;
    for (size_t i = 0; i < count; ++i) out[i] = order[i];
    return count;
}

static int hash_bucket(int key) { int value = key % 7; return value < 0 ? value + 7 : value; }
bool hash_insert_linear(TeachingHash *table, int key) {
    if (table == NULL) return false;
    int firstDeleted = -1, home = hash_bucket(key);
    for (int step = 0; step < 7; ++step) {
        int i = (home + step) % 7;
        if (table->state[i] == 1 && table->key[i] == key) return false;
        if (table->state[i] == 2 && firstDeleted < 0) firstDeleted = i;
        if (table->state[i] == 0) {
            int target = firstDeleted < 0 ? i : firstDeleted;
            table->state[target] = 1;
            table->key[target] = key;
            return true;
        }
    }
    if (firstDeleted < 0) return false;
    table->state[firstDeleted] = 1;
    table->key[firstDeleted] = key;
    return true;
}
bool hash_find_linear(const TeachingHash *table, int key) {
    if (table == NULL) return false;
    int home = hash_bucket(key);
    for (int step = 0; step < 7; ++step) {
        int i = (home + step) % 7;
        if (table->state[i] == 0) return false;
        if (table->state[i] == 1 && table->key[i] == key) return true;
    }
    return false;
}
bool hash_remove_linear(TeachingHash *table, int key) {
    if (table == NULL) return false;
    int home = hash_bucket(key);
    for (int step = 0; step < 7; ++step) {
        int i = (home + step) % 7;
        if (table->state[i] == 0) return false;
        if (table->state[i] == 1 && table->key[i] == key) {
            table->state[i] = 2;
            return true;
        }
    }
    return false;
}
