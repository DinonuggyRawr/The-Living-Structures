namespace TLS.Core
{
    public static class TLSPhase4WasmHarness
    {
        public const string Source = @"#include <stdbool.h>
#include <stddef.h>
#include <stdlib.h>
#include <limits.h>
#include <stdint.h>

/* Trusted test harness. Students provide exactly one selected function. */
typedef struct TreeNode { int value; struct TreeNode *left, *right; } TreeNode;
#ifndef TLS_SELECTED
#define TLS_SELECTED 0
#endif
#if TLS_SELECTED == 0
bool bst_insert(TreeNode **root, int value);
#elif TLS_SELECTED == 1
bool bst_search(const TreeNode *root, int target);
#elif TLS_SELECTED == 2
bool bst_delete(TreeNode **root, int target);
#elif TLS_SELECTED == 3
size_t tree_count(const TreeNode *root);
#elif TLS_SELECTED == 4
size_t tree_height(const TreeNode *root);
#elif TLS_SELECTED == 5
size_t tree_inorder(const TreeNode *root, int *out, size_t capacity);
#else
#error Unsupported selected tree function
#endif

static TreeNode *tls_new(int value) {
    TreeNode *n=malloc(sizeof *n);if(n==NULL)exit(90);
    n->value=value;n->left=NULL;n->right=NULL;return n;
}
static void tls_insert(TreeNode **root,int value) {
    while(*root!=NULL) root=value<(*root)->value?&(*root)->left:&(*root)->right;
    *root=tls_new(value);
}
static void tls_free(TreeNode *r) {
    if(r==NULL)return;
    tls_free(r->left);tls_free(r->right);free(r);
}
static size_t tls_count(const TreeNode *r) {
    return r==NULL?0:1+tls_count(r->left)+tls_count(r->right);
}
#if TLS_SELECTED == 4
static size_t tls_height(const TreeNode *r) {
    if(!r)return 0;
    size_t a=tls_height(r->left),b=tls_height(r->right);
    return 1+(a>b?a:b);
}
#endif
static int tls_valid(const TreeNode *r, long long lo,long long hi,size_t *visited) {
    if(r==NULL)return 1;
    if(++*visited>16 || r->value<=lo || r->value>=hi)return 0;
    return tls_valid(r->left,lo,r->value,visited)&&tls_valid(r->right,r->value,hi,visited);
}
static void tls_fill(const TreeNode *r,int *out,size_t *i) {
    if(!r)return;
    tls_fill(r->left,out,i);out[(*i)++]=r->value;tls_fill(r->right,out,i);
}
static int tls_matches(const TreeNode *r,const int *expected,size_t expectedCount) {
    size_t visited=0;
    if(!tls_valid(r,(long long)INT_MIN-1LL,(long long)INT_MAX+1LL,&visited))return 3;
    if(tls_count(r)!=expectedCount)return 3;
    int actual[16]={0};size_t i=0;tls_fill(r,actual,&i);
    if(i!=expectedCount)return 3;
    for(size_t k=0;k<i;k++)if(actual[k]!=expected[k])return 4;
    return 0;
}
int main(int argc,char **argv) {
    if(argc!=2)return 70;
    char *end=NULL;long parsed=strtol(argv[1],&end,10);
    if(!end || *end!='\0' || parsed<0 || parsed>7)return 71;
    int c=(int)parsed;
    const int all[8][8]={
        {0}, {10}, {10,5,15}, {10,5,15,3,7,12,18},
        {-1,0,1}, {0,INT_MIN,INT_MAX}, {INT_MAX}, {1,2,3,4,5,6,7,8}
    };
    const int lengths[8]={0,1,3,7,3,3,1,8};
    const int targetInsert[8]={42,10,6,10,-2,INT_MAX,INT_MIN,9};
    const int targetSearch[8]={42,10,12,8,0,INT_MIN,INT_MAX,6};
    const int targetDelete[8]={42,10,5,10,-1,INT_MIN,INT_MAX,5};
    (void)targetInsert; (void)targetSearch; (void)targetDelete;
    TreeNode *root=NULL;
    for(int k=0;k<lengths[c];k++)tls_insert(&root,all[c][k]);
    int before[16]={0};size_t preCount=0;tls_fill(root,before,&preCount);
    int expected[16]={0};for(size_t i=0;i<preCount;i++)expected[i]=before[i];
    size_t nextCount=preCount;
    int status=0;
#if TLS_SELECTED == 0
    int v=targetInsert[c],found=0;
    for(size_t i=0;i<preCount;i++)if(before[i]==v)found=1;
    bool ok=bst_insert(&root,v);
    if(ok==found){status=2;goto done;}
    if(!found){
        size_t where=0;while(where<preCount&&expected[where]<v)where++;
        for(size_t i=preCount;i>where;i--)expected[i]=expected[i-1];
        expected[where]=v;nextCount++;
    }
#elif TLS_SELECTED == 1
    int target=targetSearch[c];bool found=false;
    for(size_t i=0;i<preCount;i++)if(before[i]==target)found=true;
    if(bst_search(root,target)!=found){status=2;goto done;}
#elif TLS_SELECTED == 2
    int target=targetDelete[c];int where=-1;
    for(size_t i=0;i<preCount;i++)if(before[i]==target){where=(int)i;break;}
    if(bst_delete(&root,target)!=(where>=0)){status=2;goto done;}
    if(where>=0){for(size_t i=(size_t)where;i+1<preCount;i++)expected[i]=expected[i+1];nextCount--;}
#elif TLS_SELECTED == 3
    if(tree_count(root)!=preCount){status=2;goto done;}
#elif TLS_SELECTED == 4
    if(tree_height(root)!=tls_height(root)){status=2;goto done;}
#elif TLS_SELECTED == 5
    int output[18];for(size_t i=0;i<18;i++)output[i]=13579;
    size_t capacity=preCount;
    if(c==3)capacity=preCount-1; /* intentionally insufficient capacity */
    if(c==5)capacity=0;
    int *destination=(c==6?NULL:output);
    size_t actual=tree_inorder(root,destination,capacity);
    size_t expectedWritten=(destination==NULL||capacity<preCount)?0:preCount;
    if(actual!=expectedWritten){status=2;goto done;}
    if(expectedWritten){for(size_t i=0;i<preCount;i++)if(output[i]!=before[i]){status=4;goto done;}}
    else {for(size_t i=0;i<18;i++)if(output[i]!=13579){status=5;goto done;}}
#endif
    status=tls_matches(root,expected,nextCount);
done:
    /* If malformed student code creates a cycle the bounded Wasmtime process terminates.
       Do not traverse a potentially corrupted graph during cleanup after failed checks. */
    if(status==0)tls_free(root);
    return status;
}
";
    }
}
