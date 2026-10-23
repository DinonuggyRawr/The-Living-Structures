#include <stdbool.h>
#include <stddef.h>
#include <stdlib.h>
#include <string.h>
#include <limits.h>
#ifndef TLS_SELECTED
#define TLS_SELECTED 0
#endif
#if TLS_SELECTED == 0
bool shift_left_remove(int *data, size_t *count, size_t capacity, size_t index);
#else
int count_target_recursive(const int *data, size_t count, int target);
#endif
static int check(int k) {
#if TLS_SELECTED == 0
    const int input[8][8] = {
        {0}, {0}, {9,8,7,6}, {4,5,6,7}, {1,2,3,4,5}, {2,2,2,5},
        {INT_MIN,9,INT_MAX,12,0,6}, {3,5,7,9}
    };
    const size_t counts[8] = {0,1,4,4,5,4,6,4};
    const size_t capacities[8] = {8,8,8,8,8,8,6,8};
    const size_t indexes[8] = {0,0,0,1,4,1,2,4};
    const bool expectedReturn[8] = {false,true,true,true,true,true,true,false};
    const size_t afterCount[8] = {0,0,3,3,4,3,5,4};
    const int after[8][8] = {
        {0}, {0}, {8,7,6,6}, {4,6,7,7}, {1,2,3,4,5}, {2,2,5,5},
        {INT_MIN,9,12,0,6,6}, {3,5,7,9}
    };
    // An invalid NULL argument check is an additional boundary in case 0.
    if(k==0){size_t n=0;return shift_left_remove(NULL,&n,8,0)==false && n==0 ? 0:2;}
    struct Guard {int pre;int a[8];int post;} data={0x12345678,{0},0x13572468};
    memcpy(data.a,input[k],sizeof(data.a));
    size_t n=counts[k];
    bool output=shift_left_remove(data.a,&n,capacities[k],indexes[k]);
    if(output!=expectedReturn[k]||n!=afterCount[k])return 2;
    if(data.pre!=0x12345678||data.post!=0x13572468)return 3;
    for(int i=0;i<8;i++)if(data.a[i]!=after[k][i])return 4;
    return 0;
#else
    const int a1[]={9};
    const int a2[]={9};
    const int a3[]={4,9,4,9};
    const int a4[]={-1,-2,-1,0};
    const int a5[]={INT_MIN,0,INT_MAX};
    const int a6[]={3,3,3,3,3};
    const int a7[]={0,1,2,3,4,5,6,7};
    const int *a[]={NULL,NULL,a1,a2,a3,a4,a5,a6,a7};
    // Use index mapping so each of eight cases stays independent.
    const int *data[]={NULL,NULL,a1,a2,a3,a4,a5,a7};
    const size_t counts[]={0,1,1,1,4,4,3,8};
    const int target[]={0,2,9,3,9,-1,INT_MAX,6};
    const int expected[]={0,-1,1,0,2,2,1,1};
    (void)a;
    int result=count_target_recursive(data[k],counts[k],target[k]);
    return result==expected[k]?0:2;
#endif
}
int main(int argc,char **argv) {
    if(argc!=2)return 6;
    char *end=NULL;long k=strtol(argv[1],&end,10);
    if(end==argv[1]||*end!='\0'||k<0||k>=8)return 7;
    return check((int)k);
}
