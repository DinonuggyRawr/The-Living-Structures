namespace TLS.Core
{
    public static class TLSPhase5WasmHarness
    {
        public const string Source = @"#include <stdbool.h>
#include <stddef.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <limits.h>

typedef struct { int data[15]; size_t size; } TeachingHeap;
typedef struct { int n; unsigned char edge[9][9]; } TeachingGraph;
typedef struct { int key[7]; unsigned char state[7]; } TeachingHash;
#ifndef TLS_SELECTED
#define TLS_SELECTED 0
#endif
#if TLS_SELECTED == 0
bool heap_insert_min(TeachingHeap *heap, int value);
#elif TLS_SELECTED == 1
bool heap_extract_min(TeachingHeap *heap, int *out);
#elif TLS_SELECTED == 2
size_t graph_bfs(const TeachingGraph *graph, int start, int *out, size_t capacity);
#elif TLS_SELECTED == 3
size_t graph_dfs(const TeachingGraph *graph, int start, int *out, size_t capacity);
#elif TLS_SELECTED == 4
bool hash_insert_linear(TeachingHash *table, int key);
#elif TLS_SELECTED == 5
bool hash_find_linear(const TeachingHash *table, int key);
#elif TLS_SELECTED == 6
bool hash_remove_linear(TeachingHash *table, int key);
#else
#error Unsupported C function index
#endif

/* Trusted test-only oracle, independent of the student's code. */
#if TLS_SELECTED < 2
static int higher(int a, int b) { return a < b; }
static bool __attribute__((unused)) oracle_heap_insert(TeachingHeap *h, int key) {
    if(!h || h->size>=15)return false;
    h->data[h->size++] = key;
    for(size_t i=h->size-1;i>0;) {
        size_t p=(i-1)/2;
        if(!higher(h->data[i],h->data[p]))break;
        int v=h->data[i];h->data[i]=h->data[p];h->data[p]=v;i=p;
    }
    return true;
}
static bool __attribute__((unused)) oracle_heap_extract(TeachingHeap *h, int *out) {
    if(!h || !out || h->size==0 || h->size>15)return false;
    *out = h->data[0];
    int last=h->data[h->size-1];h->size--;
    if(h->size==0)return true;
    h->data[0]=last;
    for(size_t i=0;2*i+1<h->size;) {
        size_t j=2*i+1;
        if(j+1<h->size&&h->data[j+1]<h->data[j])j++;
        if(h->data[i]<=h->data[j])break;
        int tmp=h->data[i];h->data[i]=h->data[j];h->data[j]=tmp;i=j;
    }
    return true;
}
static int valid_heap(const TeachingHeap *h) {
    if(h->size>15)return 0;
    for(size_t i=1;i<h->size;i++)if(h->data[(i-1)/2]>h->data[i])return 0;
    return 1;
}

static int __attribute__((unused)) same_heap(const TeachingHeap *a,const TeachingHeap *b) {
    return a->size==b->size&&valid_heap(a)&&memcmp(a->data,b->data,sizeof a->data)==0;
}

#endif
#if TLS_SELECTED == 2 || TLS_SELECTED == 3
static void graph_edge(TeachingGraph *g,int u,int v) { g->edge[u][v]=1; }
static void graph_setup(TeachingGraph *g,int c) {
    memset(g,0,sizeof *g);
    g->n=(c==0?0:c==1?1:c==7?9:6);
    if(g->n<2)return;
    graph_edge(g,0,1);graph_edge(g,0,2);
    if(c!=6)graph_edge(g,1,3);
    graph_edge(g,2,3);graph_edge(g,3,4);graph_edge(g,4,5);
    if(c==3){graph_edge(g,3,1);graph_edge(g,4,0);}
    if(c==5){graph_edge(g,2,0);graph_edge(g,5,5);}
    if(c==7){graph_edge(g,5,6);graph_edge(g,6,7);graph_edge(g,7,8);}
}
static void visit_dfs(const TeachingGraph *g,int u,int *out,size_t *n,bool *seen) {
    seen[u]=true;out[(*n)++]=u;
    for(int v=0;v<g->n;v++)if(g->edge[u][v]&&!seen[v])visit_dfs(g,v,out,n,seen);
}
static size_t __attribute__((unused)) oracle_graph(const TeachingGraph *g,int start,int *out,int dfs) {
    if(!g||g->n<1||start<0||start>=g->n)return 0;
    bool visited[9]={false}; size_t count=0;
    if(dfs)visit_dfs(g,start,out,&count,visited);
    else {
        int queue[9];int begin=0,end=0;queue[end++]=start;visited[start]=true;
        while(begin<end){int v=queue[begin++];out[count++]=v;
            for(int j=0;j<g->n;j++)if(g->edge[v][j]&&!visited[j]){visited[j]=true;queue[end++]=j;}}
    }
    return count;
}

#endif
#if TLS_SELECTED >= 4
static int hash_home(int key){int a=key%7;return a<0?a+7:a;}
static int oracle_lookup(const TeachingHash *h,int key) {
    int base=hash_home(key);
    for(int j=0;j<7;j++){int i=(base+j)%7;if(h->state[i]==0)return -1;if(h->state[i]==1&&h->key[i]==key)return i;}
    return -1;
}
static bool oracle_hash_insert(TeachingHash *h,int key) {
    if(!h || oracle_lookup(h,key)>=0)return false;
    int home=hash_home(key),target=-1;
    for(int j=0;j<7;j++){
        int i=(home+j)%7;if(h->state[i]!=1){target=i;break;}
    }
    if(target<0)return false;
    h->state[target]=1;h->key[target]=key;return true;
}
static bool __attribute__((unused)) oracle_hash_remove(TeachingHash *h,int key) {
    if(!h)return false;
    int at=oracle_lookup(h,key);if(at<0)return false;h->state[at]=2;return true;
}
static int same_hash(const TeachingHash *a,const TeachingHash *b) {
    return memcmp(a->state,b->state,sizeof a->state)==0&&memcmp(a->key,b->key,sizeof a->key)==0;
}
static void seed_hash(TeachingHash *h,int c) {
    memset(h,0,sizeof *h);
    const int common[]={1,8,15,22,-6,INT_MIN,INT_MAX};
    int n=(c==0||c==6?0:c==1?1:c==2?2:c==3?3:c==4?5:7);
    for(int i=0;i<n;i++)oracle_hash_insert(h,common[i]);
    if(c==5){oracle_hash_remove(h,8);oracle_hash_remove(h,22);}
    if(c==7){oracle_hash_remove(h,1);oracle_hash_insert(h,-13);}
}

#endif
int main(int argc,char **argv) {
    if(argc!=2)return 70;
    char *end=NULL;long idx=strtol(argv[1],&end,10);
    if(end==argv[1]||*end!='\0'||idx<0||idx>7)return 71;
    int c=(int)idx;
#if TLS_SELECTED < 2
    TeachingHeap actual={{0},0},expected={{0},0};
    const int inputs[]={8,3,5,14,0,10,2,7};
#if TLS_SELECTED == 0
    const int add[]={4,-3,6,1,INT_MIN,INT_MAX,4,9};
#endif
    for(int i=0;i<inputs[c];i++) {int x=(i*7+11)%37-19;oracle_heap_insert(&actual,x);}
    if(c==3){for(int i=actual.size;i<15;i++)oracle_heap_insert(&actual,i);}
    expected=actual;
    bool good,should;
#if TLS_SELECTED == 1
    int actualOut=12345,expectedOut=12345;
#endif
#if TLS_SELECTED == 0
    if(c==6){good=heap_insert_min(NULL,add[c]);should=false;}
    else{good=heap_insert_min(&actual,add[c]);should=oracle_heap_insert(&expected,add[c]);}
#else
    if(c==6){good=heap_extract_min(NULL,&actualOut);should=false;}
    else if(c==5){good=heap_extract_min(&actual,NULL);should=false;}
    else{good=heap_extract_min(&actual,&actualOut);should=oracle_heap_extract(&expected,&expectedOut);}
    if(actualOut!=expectedOut)return 4;
#endif
    if(good!=should)return 2;
    if(!same_heap(&actual,&expected))return 3;
#elif TLS_SELECTED < 4
    TeachingGraph g;graph_setup(&g,c);TeachingGraph original=g;
    int start=(c==0?0:c==4?5:c==6?9:0);
    int actual[9]={41,41,41,41,41,41,41,41,41};
    int expected[9]={41,41,41,41,41,41,41,41,41};
    size_t required=oracle_graph(&g,start,expected,TLS_SELECTED==3);
    size_t capacity=(c==2?1:9);
    int *buffer=c==5?NULL:actual;
    size_t got=0;
#if TLS_SELECTED == 2
    got=graph_bfs(c==6?NULL:&g,start,buffer,capacity);
#else
    got=graph_dfs(c==6?NULL:&g,start,buffer,capacity);
#endif
    if(c==6)required=0;
    if((required>capacity||buffer==NULL))required=0;
    if(got!=required)return 2;
    if(required>0)for(size_t i=0;i<required;i++)if(actual[i]!=expected[i])return 4;
    if(required==0)for(size_t i=0;i<9;i++)if(actual[i]!=41)return 5;
    if(memcmp(&g,&original,sizeof g)!=0)return 6;
#else
    TeachingHash actual,expected;seed_hash(&actual,c);expected=actual;
    int insertKeys[]={1,8,15,-6,22,8,INT_MIN,INT_MAX};
    int queries[]={15,1,8,-6,22,8,INT_MIN,INT_MAX};
    int key=TLS_SELECTED==4?insertKeys[c]:queries[c];
    bool got=false,want=false;
#if TLS_SELECTED == 4
    got=hash_insert_linear(c==6?NULL:&actual,key);
    want=c==6?false:oracle_hash_insert(&expected,key);
#elif TLS_SELECTED == 5
    got=hash_find_linear(c==6?NULL:&actual,key);
    want=c==6?false:oracle_lookup(&expected,key)>=0;
#else
    got=hash_remove_linear(c==6?NULL:&actual,key);
    want=c==6?false:oracle_hash_remove(&expected,key);
#endif
    if(got!=want)return 2;
    if(!same_hash(&actual,&expected))return 3;
#endif
    return 0;
}
";
    }
}
