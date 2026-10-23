namespace TLS.Core
{
    public static class TLSLinkedWasmHarness
    {
        public const string Source = @"#include <stdbool.h>
#include <stddef.h>
#include <stdlib.h>
#include <limits.h>
#include <string.h>

typedef struct Node { int data; struct Node *next; } Node;
#ifndef TLS_SELECTED
#define TLS_SELECTED 0
#endif
#if TLS_SELECTED == 0
bool list_prepend(Node **head, int value);
#elif TLS_SELECTED == 1
bool list_append(Node **head, int value);
#elif TLS_SELECTED == 2
bool list_remove_first(Node **head, int target);
#elif TLS_SELECTED == 3
size_t list_count_recursive(const Node *head);
#elif TLS_SELECTED == 4
Node *list_reverse(Node *head);
#else
#error invalid TLS_SELECTED
#endif

/* Trusted harness uses prefixed helpers, deliberately not student function names. */
static Node *tls_make(const int *src, int n) {
    Node *head = NULL, **tail = &head;
    for (int i = 0; i < n; ++i) {
        Node *p = (Node *)malloc(sizeof *p);
        if (!p) exit(90);
        p->data = src[i]; p->next = NULL;
        *tail = p; tail = &p->next;
    }
    return head;
}
static int tls_match(const Node *head, const int *values, int n) {
    const Node *slow = head, *fast = head;
    for (int i=0;i<n;i++) {
        if (!head || head->data != values[i]) return 3;
        head = head->next;
        if (fast && fast->next) fast = fast->next->next; else fast = NULL;
        if (slow) slow = slow->next;
        if (fast && slow == fast) return 4; /* cyclic submission */
    }
    return head == NULL ? 0 : 3;
}
static void tls_destroy(Node *head) {
    for (int i=0;head && i<32;i++) {
        Node *next = head->next; free(head);head=next;
    }
}
int main(int argc, char **argv) {
    if (argc!=2) return 70;
    char *end = NULL;
    long x = strtol(argv[1], &end, 10);
    if (!end || *end != '\0' || x<0 || x>9) return 71;
    int c=(int)x;
    const int templates[10][7] = {
        {0}, {7}, {4,9}, {1,2,3}, {-1,0,2,3}, {9,9,9}, {INT_MIN,5}, {INT_MAX}, {6,5,4,3,2,1}, {11,12,13,14,15,16,17}
    };
    const int sizes[10] = {0,1,2,3,4,3,2,1,6,7};
    const int additions[10] = {21,-7,0,99,INT_MIN,9,INT_MAX,22,-11,42};
    Node *head = tls_make(templates[c],sizes[c]);
    int expected[10];
    int n = sizes[c];
    memcpy(expected,templates[c],sizeof templates[c]);
    int output=0;
    (void)output; (void)additions;
#if TLS_SELECTED == 0
    if (c==9) {output = list_prepend(NULL, 88); if (output!=0) return 2;}
    else {
        output = list_prepend(&head,additions[c]);
        if (!output) return 2;
        memmove(expected+1,expected,(size_t)n*sizeof(int)); expected[0]=additions[c]; n++;
    }
#elif TLS_SELECTED == 1
    if (c==9) {output=list_append(NULL, 88); if (output!=0) return 2;}
    else {
        output=list_append(&head,additions[c]); if (!output) return 2;
        expected[n++]=additions[c];
    }
#elif TLS_SELECTED == 2
    int target = c==0 ? 3 : c==1 ? 7 : c==2 ? 9 : c==3 ? 2 :
                 c==4 ? -1 : c==5 ? 9 : c==6 ? INT_MIN : c==7 ? 666 :
                 c==8 ? 4 : 0;
    if (c==9) {output=list_remove_first(NULL,0);if (output!=0) return 2;}
    else {
        int where=-1;
        for(int i=0;i<n;i++)if(expected[i]==target){where=i;break;}
        output=list_remove_first(&head,target);
        if (output!=(where>=0))return 2;
        if(where>=0){memmove(expected+where,expected+where+1,(size_t)(n-where-1)*sizeof(int));n--;}
    }
#elif TLS_SELECTED == 3
    if(list_count_recursive(head)!=(size_t)n) return 2;
#elif TLS_SELECTED == 4
    Node *original[8]; int k=0;
    for(Node *p=head;p;p=p->next)original[k++]=p;
    head=list_reverse(head);
    for(int i=0;i<n/2;i++){int v=expected[i];expected[i]=expected[n-1-i];expected[n-1-i]=v;}
    Node *p=head;
    for(int i=n-1;i>=0;i--){if(p!=original[i])return 4; p=p->next;}
    if(p)return 4;
#endif
    int code=tls_match(head,expected,n);
    if(code==0)tls_destroy(head);
    return code;
}
";
    }
}
