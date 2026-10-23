using System;
namespace TLS.Core
{
    public enum TLSPhase4Function { Insert=0, Search=1, Delete=2, Count=3, Height=4, Inorder=5 }
    public static class TLSPhase4FunctionCatalog
    {
        public const int Count = 6;
        public static bool Valid(TLSPhase4Function f) { return (int)f>=0 && (int)f<Count; }
        private static int Check(TLSPhase4Function f) { if(!Valid(f))throw new ArgumentOutOfRangeException("f");return (int)f; }
        public static string Name(TLSPhase4Function f)
        {
            switch (Check(f))
            {
                case 0: return "BST INSERT";
                case 1: return "BST SEARCH";
                case 2: return "BST DELETE";
                case 3: return "TREE COUNT";
                case 4: return "TREE HEIGHT";
                case 5: return "TREE INORDER";
                default: throw new ArgumentOutOfRangeException("f");
            }
        }
        public static string Goal(TLSPhase4Function f)
        {
            switch (Check(f))
            {
                case 0: return "Implement BST insertion with unique keys, NULL guard and malloc checking.";
                case 1: return "Search recursively using BST ordering, handling NULL safely.";
                case 2: return "Delete a key safely: handle leaf, one-child and two-child successor cases.";
                case 3: return "Count nodes in both subtrees recursively using a NULL base case.";
                case 4: return "Compute tree height in LEVELS: empty = 0; a leaf = 1.";
                case 5: return "Write inorder values into a bounded buffer, rejecting insufficient capacity without writes.";
                default: throw new ArgumentOutOfRangeException("f");
            }
        }
        public static string Hint(TLSPhase4Function f)
        {
            switch (Check(f))
            {
                case 0: return "Walk by pointer-to-pointer; reject duplicate; allocate only at NULL link.";
                case 1: return "For target smaller choose left, otherwise right; stop on NULL.";
                case 2: return "Use TreeNode **link; for two children copy inorder successor then free its original allocation.";
                case 3: return "return 1 + count(left) + count(right); base case is NULL.";
                case 4: return "1 + max(height(left), height(right)); NULL height = 0.";
                case 5: return "Check required count before writing. Visit left, node, right. Protect NULL and short buffers.";
                default: throw new ArgumentOutOfRangeException("f");
            }
        }
        public static string Starter(TLSPhase4Function f)
        {
            switch (Check(f))
            {
                case 0: return "bool bst_insert(TreeNode **root, int value) {\n    /* TODO: insert one unique key */\n    (void)root; (void)value; return false;\n}";
                case 1: return "bool bst_search(const TreeNode *root, int target) {\n    /* TODO: search */\n    (void)root; (void)target; return false;\n}";
                case 2: return "bool bst_delete(TreeNode **root, int target) {\n    /* TODO: unlink and free */\n    (void)root; (void)target; return false;\n}";
                case 3: return "size_t tree_count(const TreeNode *root) {\n    /* TODO: count recursively */\n    (void)root; return 0;\n}";
                case 4: return "size_t tree_height(const TreeNode *root) {\n    /* TODO: compute height in levels */\n    (void)root; return 0;\n}";
                case 5: return "size_t tree_inorder(const TreeNode *root, int *out, size_t capacity) {\n    /* TODO: enforce capacity and visit inorder */\n    (void)root; (void)out; (void)capacity; return 0;\n}";
                default: throw new ArgumentOutOfRangeException("f");
            }
        }
        public static string Reference(TLSPhase4Function f)
        {
            switch (Check(f))
            {
                case 0: return "bool bst_insert(TreeNode **root, int value) {\n    if (root == NULL) return false;\n    while (*root != NULL) {\n        if (value == (*root)->value) return false;\n        root = value < (*root)->value ? &(*root)->left : &(*root)->right;\n    }\n    TreeNode *node = malloc(sizeof *node);\n    if (node == NULL) return false;\n    node->value = value;\n    node->left = NULL;\n    node->right = NULL;\n    *root = node;\n    return true;\n}";
                case 1: return "bool bst_search(const TreeNode *root, int target) {\n    if (root == NULL) return false;\n    if (target == root->value) return true;\n    return bst_search(target < root->value ? root->left : root->right, target);\n}";
                case 2: return "bool bst_delete(TreeNode **root, int target) {\n    if (root == NULL) return false;\n    while (*root != NULL && (*root)->value != target)\n        root = target < (*root)->value ? &(*root)->left : &(*root)->right;\n    if (*root == NULL) return false;\n    TreeNode *victim = *root;\n    if (victim->left != NULL && victim->right != NULL) {\n        TreeNode **successor = &victim->right;\n        while ((*successor)->left != NULL) successor = &(*successor)->left;\n        victim->value = (*successor)->value;\n        root = successor;\n        victim = *root;\n    }\n    *root = victim->left != NULL ? victim->left : victim->right;\n    free(victim);\n    return true;\n}";
                case 3: return "size_t tree_count(const TreeNode *root) {\n    if (root == NULL) return 0;\n    return 1 + tree_count(root->left) + tree_count(root->right);\n}";
                case 4: return "size_t tree_height(const TreeNode *root) {\n    if (root == NULL) return 0;\n    size_t left = tree_height(root->left);\n    size_t right = tree_height(root->right);\n    return 1 + (left > right ? left : right);\n}";
                case 5: return "static size_t tls_student_count(const TreeNode *node) {\n    return node == NULL ? 0 : 1 + tls_student_count(node->left) + tls_student_count(node->right);\n}\nstatic void tls_inorder_fill(const TreeNode *node, int *out, size_t *index) {\n    if (node == NULL) return;\n    tls_inorder_fill(node->left, out, index);\n    out[(*index)++] = node->value;\n    tls_inorder_fill(node->right, out, index);\n}\nsize_t tree_inorder(const TreeNode *root, int *out, size_t capacity) {\n    size_t needed = tls_student_count(root);\n    if (needed > capacity || (needed > 0 && out == NULL)) return 0;\n    size_t written = 0;\n    tls_inorder_fill(root, out, &written);\n    return written;\n}";
                default: throw new ArgumentOutOfRangeException("f");
            }
        }
    }
}
