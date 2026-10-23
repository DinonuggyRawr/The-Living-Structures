// C11 reference for safe pointer fundamentals. The unsafe NULL dereference, use-after-free,
// and intentionally lost allocation scenarios are ONLY modeled; they are never executed.
#include <stdbool.h>
#include <stdlib.h>

typedef struct Node {
    int data;
    struct Node *next;
} Node;

bool tls_make_two_nodes(Node **out)
{
    if (out == NULL) return false;
    *out = NULL;
    Node *first = malloc(sizeof *first);
    if (first == NULL) return false;
    first->data = 10;
    first->next = NULL;

    Node *second = malloc(sizeof *second);
    if (second == NULL) {
        free(first);
        return false;
    }
    second->data = 20;
    second->next = first;
    *out = second;
    return true;
}

void tls_free_list(Node **head)
{
    if (head == NULL) return;
    Node *node = *head;
    while (node != NULL) {
        Node *next = node->next;
        free(node);
        node = next;
    }
    *head = NULL;
}

bool tls_alias_demo(int *observed)
{
    if (observed == NULL) return false;
    Node *head = malloc(sizeof *head);
    if (head == NULL) return false;
    head->data = 10;
    head->next = NULL;
    Node *alias = head;
    alias->data = 99;
    *observed = head->data;
    free(head);
    head = NULL;
    alias = NULL;
    (void)head;
    (void)alias;
    return true;
}

// Separate translation unit test entry point below; not production app logic.
#ifdef TLS_POINTER_TEST_MAIN
#include <assert.h>
int main(void)
{
    Node *head = NULL;
    assert(tls_make_two_nodes(&head));
    assert(head != NULL && head->data == 20);
    assert(head->next != NULL && head->next->data == 10);
    assert(head->next->next == NULL);
    tls_free_list(&head);
    assert(head == NULL);
    tls_free_list(&head); // safe on empty list
    int observed = 0;
    assert(tls_alias_demo(&observed) && observed == 99);
    assert(!tls_make_two_nodes(NULL));
    return 0;
}
#endif
