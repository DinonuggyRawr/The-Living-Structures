using System;

namespace TLS.Core
{
    /// <summary>Version-labelled C11 whole-function learning prompts; no progress writes.</summary>
    public enum TLSLinearFunction
    {
        ArrayAppend = 0, ArrayInsert = 1, ArrayRemove = 2, ArrayGet = 3,
        QueueEnqueue = 4, QueueDequeue = 5, QueuePeek = 6
    }

    public static class TLSLinearFunctionCatalog
    {
        public const int Count = 7;
        public static bool IsArray(TLSLinearFunction function) { return (int)function >= 0 && (int)function <= 3; }
        public static bool IsQueue(TLSLinearFunction function) { return (int)function >= 4 && (int)function <= 6; }
        public static bool IsValid(TLSLinearFunction function) { return (int)function >= 0 && (int)function < Count; }
        public static string Name(TLSLinearFunction function)
        {
            switch (function)
            {
                case TLSLinearFunction.ArrayAppend: return "ARRAY APPEND";
                case TLSLinearFunction.ArrayInsert: return "ARRAY INSERT";
                case TLSLinearFunction.ArrayRemove: return "ARRAY REMOVE";
                case TLSLinearFunction.ArrayGet: return "ARRAY GET";
                case TLSLinearFunction.QueueEnqueue: return "QUEUE ENQUEUE";
                case TLSLinearFunction.QueueDequeue: return "QUEUE DEQUEUE";
                case TLSLinearFunction.QueuePeek: return "QUEUE PEEK";
                default: throw new ArgumentOutOfRangeException("function");
            }
        }
        public static string Goal(TLSLinearFunction function)
        {
            switch (function)
            {
                case TLSLinearFunction.ArrayAppend: return "Append only when space remains. Write data[count], then increase count.";
                case TLSLinearFunction.ArrayInsert: return "Validate 0 <= index <= count, shift cells RIGHT safely, then insert.";
                case TLSLinearFunction.ArrayRemove: return "Return the removed value, shift cells LEFT, and reduce count without erasing stale raw cells.";
                case TLSLinearFunction.ArrayGet: return "Read only active indices. Reject NULL pointers and invalid indices without changing state.";
                case TLSLinearFunction.QueueEnqueue: return "Compute physical tail as (head + count) % capacity; do not overwrite a full queue.";
                case TLSLinearFunction.QueueDequeue: return "Read the logical front, advance head modulo capacity, and decrease count.";
                case TLSLinearFunction.QueuePeek: return "Read the front without moving head or changing count.";
                default: throw new ArgumentOutOfRangeException("function");
            }
        }
        public static string Hint(TLSLinearFunction function)
        {
            switch (function)
            {
                case TLSLinearFunction.ArrayAppend: return "Guard NULL and full capacity before writing. The active boundary is a->count.";
                case TLSLinearFunction.ArrayInsert: return "Start the shift at count, move toward index, and copy data[i - 1] into data[i].";
                case TLSLinearFunction.ArrayRemove: return "Save *out before shifting. Walk forward from index; copy data[i + 1] into data[i].";
                case TLSLinearFunction.ArrayGet: return "Use index < 0 || index >= a->count; successful GET never mutates the array.";
                case TLSLinearFunction.QueueEnqueue: return "The FIFO tail may wrap even when head is not zero. Use modulo QUEUE_CAPACITY.";
                case TLSLinearFunction.QueueDequeue: return "Save the old front; then head = (head + 1) % QUEUE_CAPACITY.";
                case TLSLinearFunction.QueuePeek: return "PEEK shares DEQUEUE's checks but must not update head or count.";
                default: throw new ArgumentOutOfRangeException("function");
            }
        }
        public static string Starter(TLSLinearFunction function)
        {
            switch (function)
            {
                case TLSLinearFunction.ArrayAppend: return "bool array_append(TeachingArray *a, int value) {\n    /* TODO: validate, write, update count */\n    return false;\n}";
                case TLSLinearFunction.ArrayInsert: return "bool array_insert(TeachingArray *a, int index, int value) {\n    /* TODO: validate index, shift right, insert */\n    return false;\n}";
                case TLSLinearFunction.ArrayRemove: return "bool array_remove(TeachingArray *a, int index, int *out) {\n    /* TODO: validate, save output, shift left */\n    return false;\n}";
                case TLSLinearFunction.ArrayGet: return "bool array_get(const TeachingArray *a, int index, int *out) {\n    /* TODO: validate, return active element */\n    return false;\n}";
                case TLSLinearFunction.QueueEnqueue: return "bool queue_enqueue(TeachingQueue *q, int value) {\n    /* TODO: validate, wrap tail, write */\n    return false;\n}";
                case TLSLinearFunction.QueueDequeue: return "bool queue_dequeue(TeachingQueue *q, int *out) {\n    /* TODO: validate, return front, wrap head */\n    return false;\n}";
                case TLSLinearFunction.QueuePeek: return "bool queue_peek(const TeachingQueue *q, int *out) {\n    /* TODO: validate and read front without mutation */\n    return false;\n}";
                default: throw new ArgumentOutOfRangeException("function");
            }
        }
        public static string Reference(TLSLinearFunction function)
        {
            switch (function)
            {
                case TLSLinearFunction.ArrayAppend: return "bool array_append(TeachingArray *a, int value) {\n    if (a == NULL || a->count >= ARRAY_CAPACITY) return false;\n    a->data[a->count] = value;\n    a->count++;\n    return true;\n}";
                case TLSLinearFunction.ArrayInsert: return "bool array_insert(TeachingArray *a, int index, int value) {\n    if (a == NULL || a->count >= ARRAY_CAPACITY || index < 0 || index > a->count) return false;\n    for (int i = a->count; i > index; --i) a->data[i] = a->data[i - 1];\n    a->data[index] = value;\n    a->count++;\n    return true;\n}";
                case TLSLinearFunction.ArrayRemove: return "bool array_remove(TeachingArray *a, int index, int *out) {\n    if (a == NULL || out == NULL || index < 0 || index >= a->count) return false;\n    *out = a->data[index];\n    for (int i = index; i + 1 < a->count; ++i) a->data[i] = a->data[i + 1];\n    a->count--;\n    return true;\n}";
                case TLSLinearFunction.ArrayGet: return "bool array_get(const TeachingArray *a, int index, int *out) {\n    if (a == NULL || out == NULL || index < 0 || index >= a->count) return false;\n    *out = a->data[index];\n    return true;\n}";
                case TLSLinearFunction.QueueEnqueue: return "bool queue_enqueue(TeachingQueue *q, int value) {\n    if (q == NULL || q->count >= QUEUE_CAPACITY) return false;\n    int tail = (q->head + q->count) % QUEUE_CAPACITY;\n    q->data[tail] = value;\n    q->count++;\n    return true;\n}";
                case TLSLinearFunction.QueueDequeue: return "bool queue_dequeue(TeachingQueue *q, int *out) {\n    if (q == NULL || out == NULL || q->count == 0) return false;\n    *out = q->data[q->head];\n    q->head = (q->head + 1) % QUEUE_CAPACITY;\n    q->count--;\n    return true;\n}";
                case TLSLinearFunction.QueuePeek: return "bool queue_peek(const TeachingQueue *q, int *out) {\n    if (q == NULL || out == NULL || q->count == 0) return false;\n    *out = q->data[q->head];\n    return true;\n}";
                default: throw new ArgumentOutOfRangeException("function");
            }
        }
    }
}
