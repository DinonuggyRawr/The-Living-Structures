using System;
namespace TLS.Core
{
    public enum TLSLinkedFunction { Prepend=0, Append=1, RemoveFirst=2, RecursiveCount=3, Reverse=4 }
    public static class TLSLinkedFunctionCatalog
    {
        public const int Count = 5;
        public static bool IsValid(TLSLinkedFunction f) { return (int)f>=0 && (int)f<Count; }
        private static int Check(TLSLinkedFunction f) { if(!IsValid(f)) throw new ArgumentOutOfRangeException("f"); return (int)f; }
        public static string Name(TLSLinkedFunction f)
        {
            switch (Check(f))
            {
                case 0: return "LIST PREPEND";
                case 1: return "LIST APPEND";
                case 2: return "LIST REMOVE FIRST";
                case 3: return "LIST COUNT RECURSIVE";
                case 4: return "LIST REVERSE";
                default: throw new ArgumentOutOfRangeException("f");
            }
        }
        public static string Goal(TLSLinkedFunction f)
        {
            switch (Check(f))
            {
                case 0: return "Safely allocate and prepend, updating the caller-owned head pointer.";
                case 1: return "Allocate then append, preserving existing nodes and a NULL-terminated tail.";
                case 2: return "Remove the first matching node and free it, handling head removal and absent targets.";
                case 3: return "Use a NULL base case to count a linear list recursively.";
                case 4: return "Reverse links in place without allocating new nodes; return the new head.";
                default: throw new ArgumentOutOfRangeException("f");
            }
        }
        public static string Hint(TLSLinkedFunction f)
        {
            switch (Check(f))
            {
                case 0: return "Guard Node **head. Check malloc, initialize fields, then assign *head.";
                case 1: return "Handle empty list separately and never dereference p->next when p is NULL.";
                case 2: return "A Node **link can point to head or an existing next field. Relink before free.";
                case 3: return "Base case returns zero; recursive case returns one plus count(next).";
                case 4: return "Save next before changing the arrow, then advance (prev, head).";
                default: throw new ArgumentOutOfRangeException("f");
            }
        }
        public static string Starter(TLSLinkedFunction f)
        {
            switch (Check(f))
            {
                case 0: return "bool list_prepend(Node **head, int value) {\n    /* TODO: allocate, initialize, link */\n    return false;\n}";
                case 1: return "bool list_append(Node **head, int value) {\n    /* TODO: allocate and link at end */\n    return false;\n}";
                case 2: return "bool list_remove_first(Node **head, int target) {\n    /* TODO: unlink and free first match */\n    return false;\n}";
                case 3: return "size_t list_count_recursive(const Node *head) {\n    /* TODO: base case and recursive return */\n    return 0;\n}";
                case 4: return "Node *list_reverse(Node *head) {\n    /* TODO: reverse links, return new head */\n    return head;\n}";
                default: throw new ArgumentOutOfRangeException("f");
            }
        }
        public static string Reference(TLSLinkedFunction f)
        {
            switch (Check(f))
            {
                case 0: return "bool list_prepend(Node **head, int value) {\n    if (head == NULL) return false;\n    Node *node = malloc(sizeof *node);\n    if (node == NULL) return false;\n    node->data = value;\n    node->next = *head;\n    *head = node;\n    return true;\n}";
                case 1: return "bool list_append(Node **head, int value) {\n    if (head == NULL) return false;\n    Node *node = malloc(sizeof *node);\n    if (node == NULL) return false;\n    node->data = value;\n    node->next = NULL;\n    if (*head == NULL) { *head = node; return true; }\n    Node *p = *head;\n    while (p->next != NULL) p = p->next;\n    p->next = node;\n    return true;\n}";
                case 2: return "bool list_remove_first(Node **head, int target) {\n    if (head == NULL) return false;\n    Node **link = head;\n    while (*link != NULL && (*link)->data != target) link = &(*link)->next;\n    if (*link == NULL) return false;\n    Node *old = *link;\n    *link = old->next;\n    free(old);\n    return true;\n}";
                case 3: return "size_t list_count_recursive(const Node *head) {\n    if (head == NULL) return 0;\n    return 1 + list_count_recursive(head->next);\n}";
                case 4: return "Node *list_reverse(Node *head) {\n    Node *prev = NULL;\n    while (head != NULL) {\n        Node *next = head->next;\n        head->next = prev;\n        prev = head;\n        head = next;\n    }\n    return prev;\n}";
                default: throw new ArgumentOutOfRangeException("f");
            }
        }
    }
}
