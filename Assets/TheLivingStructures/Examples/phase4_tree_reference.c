#include <stdbool.h>
#include <stddef.h>
#include <stdlib.h>

typedef struct TreeNode { int value; struct TreeNode *left, *right; } TreeNode;

bool bst_insert(TreeNode **root, int value) {
    if (root == NULL) return false;
    while (*root != NULL) {
        if (value == (*root)->value) return false;
        root = value < (*root)->value ? &(*root)->left : &(*root)->right;
    }
    TreeNode *node = malloc(sizeof *node);
    if (node == NULL) return false;
    node->value = value;
    node->left = NULL;
    node->right = NULL;
    *root = node;
    return true;
}

bool bst_search(const TreeNode *root, int target) {
    if (root == NULL) return false;
    if (target == root->value) return true;
    return bst_search(target < root->value ? root->left : root->right, target);
}

bool bst_delete(TreeNode **root, int target) {
    if (root == NULL) return false;
    while (*root != NULL && (*root)->value != target)
        root = target < (*root)->value ? &(*root)->left : &(*root)->right;
    if (*root == NULL) return false;
    TreeNode *victim = *root;
    if (victim->left != NULL && victim->right != NULL) {
        TreeNode **successor = &victim->right;
        while ((*successor)->left != NULL) successor = &(*successor)->left;
        victim->value = (*successor)->value;
        root = successor;
        victim = *root;
    }
    *root = victim->left != NULL ? victim->left : victim->right;
    free(victim);
    return true;
}

size_t tree_count(const TreeNode *root) {
    if (root == NULL) return 0;
    return 1 + tree_count(root->left) + tree_count(root->right);
}

size_t tree_height(const TreeNode *root) {
    if (root == NULL) return 0;
    size_t left = tree_height(root->left);
    size_t right = tree_height(root->right);
    return 1 + (left > right ? left : right);
}

static size_t tls_student_count(const TreeNode *node) {
    return node == NULL ? 0 : 1 + tls_student_count(node->left) + tls_student_count(node->right);
}
static void tls_inorder_fill(const TreeNode *node, int *out, size_t *index) {
    if (node == NULL) return;
    tls_inorder_fill(node->left, out, index);
    out[(*index)++] = node->value;
    tls_inorder_fill(node->right, out, index);
}
size_t tree_inorder(const TreeNode *root, int *out, size_t capacity) {
    size_t needed = tls_student_count(root);
    if (needed > capacity || (needed > 0 && out == NULL)) return 0;
    size_t written = 0;
    tls_inorder_fill(root, out, &written);
    return written;
}
