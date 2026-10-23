#include <stdbool.h>
#include <stddef.h>
#include <stdlib.h>
typedef struct Node { int data; struct Node *next; } Node;

bool list_prepend(Node **head, int value) {
    if (head == NULL) return false;
    Node *node = malloc(sizeof *node);
    if (node == NULL) return false;
    node->data = value;
    node->next = *head;
    *head = node;
    return true;
}
bool list_append(Node **head, int value) {
    if (head == NULL) return false;
    Node *node = malloc(sizeof *node);
    if (node == NULL) return false;
    node->data = value;
    node->next = NULL;
    if (*head == NULL) { *head = node; return true; }
    Node *p = *head;
    while (p->next != NULL) p = p->next;
    p->next = node;
    return true;
}
bool list_remove_first(Node **head, int target) {
    if (head == NULL) return false;
    Node **link = head;
    while (*link != NULL && (*link)->data != target) link = &(*link)->next;
    if (*link == NULL) return false;
    Node *old = *link;
    *link = old->next;
    free(old);
    return true;
}
size_t list_count_recursive(const Node *head) {
    if (head == NULL) return 0;
    return 1 + list_count_recursive(head->next);
}
Node *list_reverse(Node *head) {
    Node *prev = NULL;
    while (head != NULL) {
        Node *next = head->next;
        head->next = prev;
        prev = head;
        head = next;
    }
    return prev;
}
