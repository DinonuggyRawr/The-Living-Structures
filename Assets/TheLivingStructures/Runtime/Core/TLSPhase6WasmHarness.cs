namespace TLS.Core
{
    public static class TLSPhase6WasmHarness
    {
        public const string Source = @"#include <stdbool.h>
#include <stddef.h>
#include <limits.h>
#include <string.h>
#include <stdlib.h>

#ifndef TLS_SELECTED
#error TLS_SELECTED missing
#endif

#if TLS_SELECTED == 0
extern int linear_search(const int *, size_t, int);
#elif TLS_SELECTED == 1
extern int binary_search(const int *, size_t, int);
#elif TLS_SELECTED == 2
extern bool insertion_sort(int *, size_t);
#else
#error invalid selection
#endif

static int check(int num) {
#if TLS_SELECTED == 0
    const int a0[] = {0};
    const int a1[] = {17};
    const int a2[] = {2, 5, 8};
    const int a3[] = {2, 5, 8};
    const int a4[] = {5, 5, 8};
    const int a5[] = {-6, -1, 4};
    const int a6[] = {INT_MIN, 0, INT_MAX};
    const int a7[] = {7, 8, 9, 10, 11, 12};
    const int *arr[] = {NULL, a1, a2, a3, a4, a5, a6, a7};
    const size_t count[] = {0,1,3,3,3,3,3,6};
    const int target[] = {3,17,2,8,5,-1,INT_MAX,13};
    const int expected[] = {-1,0,0,2,0,1,2,-1};
    (void)a0;
    int found = linear_search(arr[num], count[num], target[num]);
    return found == expected[num] ? 0 : 2;
#elif TLS_SELECTED == 1
    const int a1[] = {17};
    const int a2[] = {2, 5, 8};
    const int a3[] = {2, 5, 8};
    const int a4[] = {-9, -3, 1, 6};
    const int a5[] = {-9, -3, 1, 6};
    const int a6[] = {INT_MIN,0,INT_MAX};
    const int a7[] = {1,3,5,7,9,11,13,15};
    const int *arr[] = {NULL,a1,a2,a3,a4,a5,a6,a7};
    const size_t count[] = {0,1,3,3,4,4,3,8};
    const int target[] = {4,17,2,8,-9,6,INT_MAX,8};
    const int expected[] = {-1,0,0,2,0,3,2,-1};
    int found = binary_search(arr[num],count[num],target[num]);
    return found == expected[num] ? 0 : 2;
#elif TLS_SELECTED == 2
    const int input[][8] = {
        {0}, {5}, {1,2,3}, {3,2,1}, {2,2,1}, {-4,3,0,-7}, {INT_MAX,INT_MIN,0}, {7,6,5,4,3,2,1,0}
    };
    const int expected[][8] = {
        {0}, {5}, {1,2,3}, {1,2,3}, {1,2,2}, {-7,-4,0,3}, {INT_MIN,0,INT_MAX}, {0,1,2,3,4,5,6,7}
    };
    const size_t count[] = {0,1,3,3,3,4,3,8};
    struct Guarded { int start; int a[8]; int end; } g = {0x12345678,{0},0x12345678};
    for (size_t i=0;i<8;i++) g.a[i]=input[num][i];
    int before[8];memcpy(before,g.a,sizeof before);
    bool ok=insertion_sort(num==0?NULL:g.a,count[num]);
    if(!ok)return 2;
    if(g.start!=0x12345678||g.end!=0x12345678)return 3;
    for(size_t i=0;i<count[num];i++)if(g.a[i]!=expected[num][i])return 4;
    for(size_t i=count[num];i<8;i++)if(g.a[i]!=before[i])return 5;
    return 0;
#endif
}

int main(int argc,char **argv) {
    if(argc!=2)return 6;
    char *end=NULL;long num=strtol(argv[1],&end,10);
    if(end==argv[1]||*end!='\0'||num<0||num>=8)return 7;
    return check((int)num);
}
";
    }
}
