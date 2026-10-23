using System;
namespace TLS.Core
{
    public enum TLSPhase5Function { HeapInsertMin, HeapExtractMin, GraphBfs, GraphDfs, HashInsert, HashFind, HashRemove }
    public static class TLSPhase5FunctionCatalog
    {
        public const int Count=7;
        public static bool Valid(TLSPhase5Function f){return (int)f>=0&&(int)f<Count;}
        public static string Name(TLSPhase5Function f){return Get(Names,f);}
        public static string Goal(TLSPhase5Function f){return Get(Goals,f);}
        public static string Hint(TLSPhase5Function f){return Get(Hints,f);}
        public static string Starter(TLSPhase5Function f){return Get(Starters,f);}
        public static string Reference(TLSPhase5Function f){return Get(References,f);}
        private static string Get(string[] values,TLSPhase5Function f){if(!Valid(f))throw new ArgumentOutOfRangeException("f");return values[(int)f];}
        private static readonly string[] Names = {
            "HEAP INSERT",
            "HEAP EXTRACT",
            "GRAPH BFS",
            "GRAPH DFS",
            "HASH INSERT",
            "HASH FIND",
            "HASH REMOVE"
        };
        private static readonly string[] Goals = {
            "Insert into an array-backed MIN heap; reject NULL or full without edits.",
            "Remove the MIN root, move last element and restore heap order.",
            "Traverse directed adjacency-matrix graph in FIFO neighbor order.",
            "Traverse directed adjacency-matrix graph in recursive depth-first order.",
            "Insert a unique integer using normalized modulo and tombstones.",
            "Find a key using linear probing through tombstones.",
            "Remove a key while preserving probe chains."
        };
        private static readonly string[] Hints = {
            "Guard full or NULL, place value, sift up.",
            "Guard empty and NULL output; sift the replacement down.",
            "Mark on enqueue; capacity must hold the entire result.",
            "Visit neighbors in increasing vertex index. Protect output capacity.",
            "Keep the first tombstone, continue searching for duplicates.",
            "Stop on EMPTY or after 7 slots, but never on DELETED.",
            "Mark DELETED, not EMPTY, when key is found."
        };
        private static readonly string[] Starters = {
            "bool heap_insert_min(TeachingHeap *heap, int value) {\n    /* TODO: implement C11 algorithm and handle boundary cases. */\n    return false;\n}",
            "bool heap_extract_min(TeachingHeap *heap, int *out) {\n    /* TODO: implement C11 algorithm and handle boundary cases. */\n    return false;\n}",
            "size_t graph_bfs(const TeachingGraph *graph, int start, int *out, size_t capacity) {\n    /* TODO: implement C11 algorithm and handle boundary cases. */\n    return 0;\n}",
            "size_t graph_dfs(const TeachingGraph *graph, int start, int *out, size_t capacity) {\n    /* TODO: implement C11 algorithm and handle boundary cases. */\n    return 0;\n}",
            "bool hash_insert_linear(TeachingHash *table, int key) {\n    /* TODO: implement C11 algorithm and handle boundary cases. */\n    return false;\n}",
            "bool hash_find_linear(const TeachingHash *table, int key) {\n    /* TODO: implement C11 algorithm and handle boundary cases. */\n    return false;\n}",
            "bool hash_remove_linear(TeachingHash *table, int key) {\n    /* TODO: implement C11 algorithm and handle boundary cases. */\n    return false;\n}"
        };
        private static readonly string[] References = {
            "bool heap_insert_min(TeachingHeap *heap, int value) {\n    if (heap == NULL || heap->size >= 15) return false;\n    size_t i = heap->size++;\n    while (i > 0) {\n        size_t parent = (i - 1) / 2;\n        if (heap->data[parent] <= value) break;\n        heap->data[i] = heap->data[parent];\n        i = parent;\n    }\n    heap->data[i] = value;\n    return true;\n}",
            "bool heap_extract_min(TeachingHeap *heap, int *out) {\n    if (heap == NULL || out == NULL || heap->size == 0 || heap->size > 15) return false;\n    *out = heap->data[0];\n    int moved = heap->data[--heap->size];\n    if (heap->size == 0) return true;\n    size_t i = 0;\n    while (2 * i + 1 < heap->size) {\n        size_t child = 2 * i + 1;\n        if (child + 1 < heap->size && heap->data[child + 1] < heap->data[child]) ++child;\n        if (moved <= heap->data[child]) break;\n        heap->data[i] = heap->data[child];\n        i = child;\n    }\n    heap->data[i] = moved;\n    return true;\n}",
            "size_t graph_bfs(const TeachingGraph *graph, int start, int *out, size_t capacity) {\n    if (graph == NULL || graph->n < 1 || graph->n > 9 || start < 0 || start >= graph->n) return 0;\n    bool visited[9] = {false};\n    int queue[9]; int head = 0, tail = 0;\n    int order[9]; size_t count = 0;\n    visited[start] = true;\n    queue[tail++] = start;\n    while (head < tail) {\n        int u = queue[head++];\n        order[count++] = u;\n        for (int v = 0; v < graph->n; ++v) {\n            if (graph->edge[u][v] && !visited[v]) {\n                visited[v] = true;\n                queue[tail++] = v;\n            }\n        }\n    }\n    if (count > capacity || out == NULL) return 0;\n    for (size_t i = 0; i < count; ++i) out[i] = order[i];\n    return count;\n}",
            "static void graph_dfs_visit(const TeachingGraph *graph, int u, bool *seen, int *order, size_t *count) {\n    seen[u] = true;\n    order[(*count)++] = u;\n    for (int v = 0; v < graph->n; ++v)\n        if (graph->edge[u][v] && !seen[v]) graph_dfs_visit(graph, v, seen, order, count);\n}\nsize_t graph_dfs(const TeachingGraph *graph, int start, int *out, size_t capacity) {\n    if (graph == NULL || graph->n < 1 || graph->n > 9 || start < 0 || start >= graph->n) return 0;\n    bool visited[9] = {false};\n    int order[9]; size_t count = 0;\n    graph_dfs_visit(graph, start, visited, order, &count);\n    if (count > capacity || out == NULL) return 0;\n    for (size_t i = 0; i < count; ++i) out[i] = order[i];\n    return count;\n}",
            "bool hash_insert_linear(TeachingHash *table, int key) {\n    if (table == NULL) return false;\n    int firstDeleted = -1, home = hash_bucket(key);\n    for (int step = 0; step < 7; ++step) {\n        int i = (home + step) % 7;\n        if (table->state[i] == 1 && table->key[i] == key) return false;\n        if (table->state[i] == 2 && firstDeleted < 0) firstDeleted = i;\n        if (table->state[i] == 0) {\n            int target = firstDeleted < 0 ? i : firstDeleted;\n            table->state[target] = 1;\n            table->key[target] = key;\n            return true;\n        }\n    }\n    if (firstDeleted < 0) return false;\n    table->state[firstDeleted] = 1;\n    table->key[firstDeleted] = key;\n    return true;\n}",
            "bool hash_find_linear(const TeachingHash *table, int key) {\n    if (table == NULL) return false;\n    int home = hash_bucket(key);\n    for (int step = 0; step < 7; ++step) {\n        int i = (home + step) % 7;\n        if (table->state[i] == 0) return false;\n        if (table->state[i] == 1 && table->key[i] == key) return true;\n    }\n    return false;\n}",
            "bool hash_remove_linear(TeachingHash *table, int key) {\n    if (table == NULL) return false;\n    int home = hash_bucket(key);\n    for (int step = 0; step < 7; ++step) {\n        int i = (home + step) % 7;\n        if (table->state[i] == 0) return false;\n        if (table->state[i] == 1 && table->key[i] == key) {\n            table->state[i] = 2;\n            return true;\n        }\n    }\n    return false;\n}"
        };
    }
}
