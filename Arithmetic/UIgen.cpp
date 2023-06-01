// Arithmetic.cpp : 定义 DLL 应用程序的导出函数。
//

#include "stdafx.h"
#include <stdio.h>
#include <direct.h> //_getcwd(), _chdir()
#include <stdlib.h> //_MAX_PATH, system()
#include <io.h>     //_finddata_t, _findfirst(), _findnext(), _findclose()
#include <string.h>
#include <stdint.h>

#define  IMG_TTPE_01     0x03 //Bit:01
#define  IMG_TTPE_BITMAP 0x00 //黑白
#define  IMG_TTPE_8BIT   0x01 //8位图
#define  IMG_TTPE_16BIT  0x02 //16位图

#define  IMG_TTPE_23         0x0C     //Bit:23
#define  IMG_TTPE_UNCOMPRESS 0x00     //未压缩
#define  IMG_TTPE_RLE        0x04     //RLE压缩
#define  IMG_TTPE_RLE_Tools  0x08     //RLE tools压缩
#define  IMG_TTPE_REV        0x0C     //RLE tools压缩

//#define _COMPRESS_TOOLS_  // 合并tools输出压缩的bin文件

//选择上面的一个即可
//#define  IMG_TTPE  IMG_TTPE_8BIT
#define  IMG_TTPE  IMG_TTPE_16BIT

#define _UNCOMPRESS_    // 合并未压缩的bin文件,并输出单个文件的压缩文件
#define _COMPRESS_      // 合并算法输出压缩的bin文件并输出单个压缩文件的解压文件
#define _DECOMPRESS_    // 合并算法输出解压的bin文件


#ifdef _COMPRESS_TOOLS_
#define BinFileFolder    "./batch_bin"
#define ALL_BIN_DATA_FILE_ABS_PATH    "./MergerOutput/all_pic_rle_tools.bin"
#define ALL_BIN_INFO_FILE_ABS_PATH    "./MergerOutput/image_rle_tools.txt"
#define ALL_BIN_INFO_H_FILE_ABS_PATH  "./MergerOutput/image_rle_tools.h"

#define ALL_BIN_DATA_FILE_REL_PATH    "../MergerOutput/all_pic_rle_tools.bin"
#define ALL_BIN_INFO_FILE_REL_PATH    "../MergerOutput/image_rle_tools.txt"
#define ALL_BIN_INFO_H_FILE_REL_PATH  "../MergerOutput/image_rle_tools.h"
#elif defined(_UNCOMPRESS_) ||  defined(_COMPRESS_) ||  defined(_DECOMPRESS_)
#define UNCOMPRESS_BinFileFolder                 "./batch_bin"
#define UNCOMPRESS_ALL_BIN_DATA_FILE_ABS_PATH    "./MergerOutput/all_pic.bin"
#define UNCOMPRESS_ALL_BIN_INFO_FILE_ABS_PATH    "./MergerOutput/all_pic.c"
#define UNCOMPRESS_ALL_BIN_INFO_H_FILE_ABS_PATH  "./MergerOutput/all_pic.h"

#define UNCOMPRESS_ALL_BIN_DATA_FILE_REL_PATH    "../MergerOutput/all_pic.bin"
#define UNCOMPRESS_ALL_BIN_INFO_FILE_REL_PATH    "../MergerOutput/all_pic.c"
#define UNCOMPRESS_ALL_BIN_INFO_H_FILE_REL_PATH  "../MergerOutput/all_pic.h"

//输出的压缩文件路径
#define CompressFile_REL_Path  "../batch_rle_bin_output/"
#define DecompressFile_REL_Path  "../batch_bin_output/"


#define COMPRESS_BinFileFolder                 "./batch_rle_bin_output"
#define COMPRESS_ALL_BIN_DATA_FILE_ABS_PATH    "./MergerOutput/all_pic_compress.bin"
#define COMPRESS_ALL_BIN_INFO_FILE_ABS_PATH    "./MergerOutput/all_pic_compress.c"
#define COMPRESS_ALL_BIN_INFO_H_FILE_ABS_PATH  "./MergerOutput/all_pic_compress.h"

#define COMPRESS_ALL_BIN_DATA_FILE_REL_PATH    "../MergerOutput/all_pic_compress.bin"
#define COMPRESS_ALL_BIN_INFO_FILE_REL_PATH    "../MergerOutput/all_pic_compress.c"
#define COMPRESS_ALL_BIN_INFO_H_FILE_REL_PATH  "../MergerOutput/all_pic_compress.h"

//输出的解压文件路径
#define DecompressFile_REL_Path  "../batch_bin_output/"

#define DECOMPRESS_BinFileFolder                 "./batch_bin_output"
#define DECOMPRESS_ALL_BIN_DATA_FILE_ABS_PATH    "./MergerOutput/all_pic_decompress.bin"
#define DECOMPRESS_ALL_BIN_INFO_FILE_ABS_PATH    "./MergerOutput/all_pic_decompress.c"
#define DECOMPRESS_ALL_BIN_INFO_H_FILE_ABS_PATH  "./MergerOutput/all_pic_decompress.h"

#define DECOMPRESS_ALL_BIN_DATA_FILE_REL_PATH    "../MergerOutput/all_pic_decompress.bin"
#define DECOMPRESS_ALL_BIN_INFO_FILE_REL_PATH    "../MergerOutput/all_pic_decompress.c"
#define DECOMPRESS_ALL_BIN_INFO_H_FILE_REL_PATH  "../MergerOutput/all_pic_decompress.h"
#endif




#define ARRAY_TYPE 0x00

#define UINT8_T_TYPE  0x00
#define UINT16_T_TYPE 0x01
#define UINT32_T_TYPE 0x02

#define FILE_ARRAY_ELEMENT_SZ 13
#define FILE_ARRAY_NAME_MAX_SZ 46+6
#define BUFFER_SIZE 1024

//以1.3寸屏的图片为主
#define FILE_MAX_SIZE 240*240*2

//FILE *all_bin_file; //输出bin文件
//FILE *bin_info_file;//输出*h文件

//uint32_t i,j,k;

#pragma pack(1)

struct file_array_element {
    uint32_t addr; //偏移地址
    uint32_t size; //偏移地址
    uint16_t width;//图片宽度
    uint16_t high; //图片高度
    uint8_t flag;  //bit-01(00:黑白图, 01:8位图, 10:16位全彩, 11:保留)
                   //bit-23(00:未压缩, 01:RLE压缩,  10:tools压缩,  11:保留)
};

struct file_info_array_unit {
    uint8_t array_type; //数组的类型 0:uint8_t  1:uint16_t  2:uint32_t
    int8_t  array_name[50];
    union {
        uint8_t element[FILE_ARRAY_ELEMENT_SZ];
        struct file_array_element array_element;
    };
};
#pragma pack()

#define ARRAY_STR_LOG 0

//生成数组声明字符串
void FileInfoToArrayStr_h(struct file_info_array_unit file_info_t, uint8_t* outstr_p, uint8_t* len_p)
{
    uint8_t ar_i = 0;
    uint8_t hex_h, hex_l;

    //1、数组类型拼接
    switch (file_info_t.array_type)
    {
    case UINT8_T_TYPE:  strcpy((char*)outstr_p, "extern const unsigned char "); break;
    case UINT16_T_TYPE: strcpy((char*)outstr_p, "extern const unsigned short "); break;
    case UINT32_T_TYPE: strcpy((char*)outstr_p, "extern const unsigned int "); break;
    default:
        printf("---------------%s---------------\r\n", __FUNCTION__);
        printf("array type is error, line = %s", __LINE__);
        while (1);
        break;
    }
    //打印
#if ARRAY_STR_LOG
    printf("%s\n", outstr_p);
    printf("1 strlen(outstr_p) = %d\n", strlen(outstr_p));
#endif // ARRAY_STR_LOG


    //2、数组名拼接
    strcat((char*)outstr_p, (const char*)file_info_t.array_name);
    //打印
#if ARRAY_STR_LOG
    printf("%s\n", outstr_p);
    printf("2 strlen(outstr_p) = %d\n", strlen(outstr_p));
#endif // ARRAY_STR_LOG


    //3、左方括号拼接
    strcat((char*)outstr_p, "[");


    //4、数组大小拼接
    *len_p = strlen((const char*)outstr_p); //偏移
    outstr_p[*len_p] = FILE_ARRAY_ELEMENT_SZ / 10 + 0x30;
    outstr_p[*len_p + 1] = FILE_ARRAY_ELEMENT_SZ % 10 + 0x30;


    //5、右方括号到左花括号拼接
    strcat((char*)outstr_p, "];");
    *len_p = strlen((const char*)outstr_p); //偏移
    //补足空格, 使等号对齐
    {
        uint8_t n = 0;
        for (n = 0; n < (FILE_ARRAY_NAME_MAX_SZ + 7 - *len_p); n++)
            strcat((char*)outstr_p, " ");
    }

    //8、注释拼接
    {
        //左注释符号
        strcat((char*)outstr_p, "  /* ");

        //地址偏移
        *len_p = strlen((const char*)outstr_p);//获取下标
        outstr_p[*len_p] = '0';
        outstr_p[*len_p + 1] = 'x';
        outstr_p[*len_p + 2] = '\0';

        *len_p = strlen((const char*)outstr_p);//获取下标
        for (ar_i = 0; ar_i < 4; ar_i++)
        {
            hex_h = ((uint8_t)(file_info_t.array_element.addr >> (24 - ar_i * 8))) / 16 + 0x30;
            hex_l = ((uint8_t)(file_info_t.array_element.addr >> (24 - ar_i * 8))) % 16 + 0x30;
            outstr_p[*len_p + 0 + 2 * ar_i] = (hex_h > 57) ? (hex_h + 7) : (hex_h);
            outstr_p[*len_p + 1 + 2 * ar_i] = (hex_l > 57) ? (hex_l + 7) : (hex_l);
            outstr_p[*len_p + 2 + 2 * ar_i] = '\0';//字符串结束符
        }

        //大小
        strcat((char*)outstr_p, ", 0x");
        *len_p = strlen((const char*)outstr_p);//获取下标
        for (ar_i = 0; ar_i < 4; ar_i++)
        {
            hex_h = ((uint8_t)(file_info_t.array_element.size >> (24 - ar_i * 8))) / 16 + 0x30;
            hex_l = ((uint8_t)(file_info_t.array_element.size >> (24 - ar_i * 8))) % 16 + 0x30;
            outstr_p[*len_p + 0 + 2 * ar_i] = (hex_h > 57) ? (hex_h + 7) : (hex_h);
            outstr_p[*len_p + 1 + 2 * ar_i] = (hex_l > 57) ? (hex_l + 7) : (hex_l);
            outstr_p[*len_p + 2 + 2 * ar_i] = '\0';//字符串结束符
        }

        //宽度
        strcat((char*)outstr_p, ", ");
        *len_p = strlen((const char*)outstr_p);//获取下标
        outstr_p[*len_p + 0] = ((uint8_t)(file_info_t.array_element.width)) / 100 + 0x30;//百位
        outstr_p[*len_p + 1] = ((uint8_t)(file_info_t.array_element.width)) % 100 / 10 + 0x30;//十位
        outstr_p[*len_p + 2] = ((uint8_t)(file_info_t.array_element.width)) % 10 + 0x30;//个位
        outstr_p[*len_p + 3] = '\0';//字符串结束符

        //高度
        strcat((char*)outstr_p, ", ");
        *len_p = strlen((const char*)outstr_p);//获取下标
        outstr_p[*len_p + 0] = ((uint8_t)(file_info_t.array_element.high)) / 100 + 0x30;//百位
        outstr_p[*len_p + 1] = ((uint8_t)(file_info_t.array_element.high)) % 100 / 10 + 0x30;//十位
        outstr_p[*len_p + 2] = ((uint8_t)(file_info_t.array_element.high)) % 10 + 0x30;//个位
        outstr_p[*len_p + 3] = '\0';//字符串结束符

//        //彩图，黑白图
//        strcat((char*)outstr_p, ", ");
//        if((file_info_t.array_element.flag & IMG_TTPE_01) == IMG_TTPE_BITMAP){
//            strcat((char*)outstr_p, "mono, ");
//        }
//        else if((file_info_t.array_element.flag & IMG_TTPE_01) == IMG_TTPE_8BIT){
//            strcat((char*)outstr_p, "8bit color, ");
//        }
//        else if((file_info_t.array_element.flag & IMG_TTPE_01) == IMG_TTPE_16BIT){
//            strcat((char*)outstr_p, "16bit color, ");
//        }
//        else {
//            printf("------------------\r\n");
//            printf("-------%d------\r\n", __LINE__);
//            printf("------------------\r\n");
//            while(1);
//        }
//
//        //压缩，未压缩
//        strcat((char*)outstr_p, ", ");
//        if((file_info_t.array_element.flag & IMG_TTPE_23) == IMG_TTPE_UNCOMPRESS){
//            strcat((char*)outstr_p, "normal");
//        }
//        else if((file_info_t.array_element.flag & IMG_TTPE_23) == IMG_TTPE_RLE){
//            strcat((char*)outstr_p, "RLE");
//        }
//        else if((file_info_t.array_element.flag & IMG_TTPE_23) == IMG_TTPE_RLE_Tools){
//            strcat((char*)outstr_p, "RLE Tools");
//        }
//        else {
//            printf("------------------\r\n");
//            printf("-------%d------\r\n", __LINE__);
//            printf("------------------\r\n");
//            while(1);
//        }

         //右注释符号
        strcat((char*)outstr_p, "  */");
    }
    //打印
#if ARRAY_STR_LOG
    printf("%s\n", outstr_p);
    printf("6 strlen(outstr_p) = %d\n", strlen(outstr_p));
#endif // ARRAY_STR_LOG

    //9、尾部换行符拼接并计算长度
    strcat((char*)outstr_p, "\r\n");
    *len_p = strlen((const char*)outstr_p);

    //打印
#if ARRAY_STR_LOG
    printf("%s\n", outstr_p);
    printf("7 strlen(outstr_p) = %d\n", strlen(outstr_p));
#endif // ARRAY_STR_LOG
}

//生成数组定义字符串
void FileInfoToArrayStr(struct file_info_array_unit file_info_t, uint8_t* outstr_p, uint8_t* len_p)
{
    uint8_t ar_i = 0;
    uint8_t hex_h, hex_l;

    //1、数组类型拼接
    switch (file_info_t.array_type)
    {
    case UINT8_T_TYPE:  strcpy((char*)outstr_p, "const unsigned char "); break;
    case UINT16_T_TYPE: strcpy((char*)outstr_p, "const unsigned short "); break;
    case UINT32_T_TYPE: strcpy((char*)outstr_p, "const unsigned int "); break;
    default:
        printf("---------------%s---------------\r\n", __FUNCTION__);
        printf("array type is error, line = %s", __LINE__);
        while (1);
        break;
    }
    //打印
#if ARRAY_STR_LOG
    printf("%s\n", outstr_p);
    printf("1 strlen(outstr_p) = %d\n", strlen(outstr_p));
#endif // ARRAY_STR_LOG


    //2、数组名拼接
    strcat((char*)outstr_p, (const char*)file_info_t.array_name);
    //打印
#if ARRAY_STR_LOG
    printf("%s\n", outstr_p);
    printf("2 strlen(outstr_p) = %d\n", strlen(outstr_p));
#endif // ARRAY_STR_LOG


    //3、左方括号拼接
    strcat((char*)outstr_p, "[");


    //4、数组大小拼接
    *len_p = strlen((const char*)outstr_p); //偏移
    outstr_p[*len_p] = FILE_ARRAY_ELEMENT_SZ / 10 + 0x30;
    outstr_p[*len_p + 1] = FILE_ARRAY_ELEMENT_SZ % 10 + 0x30;


    //5、右方括号到左花括号拼接
    strcat((char*)outstr_p, "]");
    *len_p = strlen((const char*)outstr_p); //偏移
    //补足空格, 使等号对齐
    {
        uint8_t n = 0;
        for (n = 0; n < (FILE_ARRAY_NAME_MAX_SZ - *len_p); n++)
            strcat((char*)outstr_p, " ");
    }
    strcat((char*)outstr_p, "= {");
    //打印
#if ARRAY_STR_LOG
    printf("%s\n", outstr_p);
    printf("3 strlen(outstr_p) = %d\n", strlen(outstr_p));
#endif // ARRAY_STR_LOG


    //6、数组元素拼接
    * len_p = strlen((const char*)outstr_p); //计算长度
    for (ar_i = 0; ar_i < FILE_ARRAY_ELEMENT_SZ; ar_i++)
    {
        hex_h = file_info_t.element[ar_i] / 16 + 0x30;
        hex_l = file_info_t.element[ar_i] % 16 + 0x30;

        outstr_p[*len_p + 0 + ar_i * 5] = '0';
        outstr_p[*len_p + 1 + ar_i * 5] = 'x';
        outstr_p[*len_p + 2 + ar_i * 5] = (hex_h > 57) ? (hex_h + 7) : (hex_h);
        outstr_p[*len_p + 3 + ar_i * 5] = (hex_l > 57) ? (hex_l + 7) : (hex_l);
        if (ar_i < FILE_ARRAY_ELEMENT_SZ - 1)
        {
            outstr_p[*len_p + 4 + ar_i * 5] = ',';
            outstr_p[*len_p + 5 + ar_i * 5] = '\0';
        }
        else
        {
            outstr_p[*len_p + 4 + ar_i * 5] = '\0';
        }
    }
    //打印
#if ARRAY_STR_LOG
    printf("%s\n", outstr_p);
    printf("4 strlen(outstr_p) = %d\n", strlen(outstr_p));
#endif // ARRAY_STR_LOG


    //7、数组尾部
    strcat((char*)outstr_p, "};");
    *len_p = strlen((const char*)outstr_p);
    //打印
#if ARRAY_STR_LOG
    printf("%s\n", outstr_p);
    printf("5 strlen(outstr_p) = %d\n", strlen(outstr_p));
#endif // ARRAY_STR_LOG

    //8、注释拼接
    {
        //左注释符号
        strcat((char*)outstr_p, "  /* ");

        //地址偏移
        *len_p = strlen((const char*)outstr_p);//获取下标
        outstr_p[*len_p] = '0';
        outstr_p[*len_p + 1] = 'x';
        outstr_p[*len_p + 2] = '\0';

        *len_p = strlen((const char*)outstr_p);//获取下标
        for (ar_i = 0; ar_i < 4; ar_i++)
        {
            hex_h = ((uint8_t)(file_info_t.array_element.addr >> (24 - ar_i * 8))) / 16 + 0x30;
            hex_l = ((uint8_t)(file_info_t.array_element.addr >> (24 - ar_i * 8))) % 16 + 0x30;
            outstr_p[*len_p + 0 + 2 * ar_i] = (hex_h > 57) ? (hex_h + 7) : (hex_h);
            outstr_p[*len_p + 1 + 2 * ar_i] = (hex_l > 57) ? (hex_l + 7) : (hex_l);
            outstr_p[*len_p + 2 + 2 * ar_i] = '\0';//字符串结束符
        }

        //大小
        strcat((char*)outstr_p, ", 0x");
        *len_p = strlen((const char*)outstr_p);//获取下标
        for (ar_i = 0; ar_i < 4; ar_i++)
        {
            hex_h = ((uint8_t)(file_info_t.array_element.size >> (24 - ar_i * 8))) / 16 + 0x30;
            hex_l = ((uint8_t)(file_info_t.array_element.size >> (24 - ar_i * 8))) % 16 + 0x30;
            outstr_p[*len_p + 0 + 2 * ar_i] = (hex_h > 57) ? (hex_h + 7) : (hex_h);
            outstr_p[*len_p + 1 + 2 * ar_i] = (hex_l > 57) ? (hex_l + 7) : (hex_l);
            outstr_p[*len_p + 2 + 2 * ar_i] = '\0';//字符串结束符
        }

        //宽度
        strcat((char*)outstr_p, ", ");
        *len_p = strlen((const char*)outstr_p);//获取下标
        outstr_p[*len_p + 0] = ((uint8_t)(file_info_t.array_element.width)) / 100 + 0x30;//百位
        outstr_p[*len_p + 1] = ((uint8_t)(file_info_t.array_element.width)) % 100 / 10 + 0x30;//十位
        outstr_p[*len_p + 2] = ((uint8_t)(file_info_t.array_element.width)) % 10 + 0x30;//个位
        outstr_p[*len_p + 3] = '\0';//字符串结束符

        //高度
        strcat((char*)outstr_p, ", ");
        *len_p = strlen((const char*)outstr_p);//获取下标
        outstr_p[*len_p + 0] = ((uint8_t)(file_info_t.array_element.high)) / 100 + 0x30;//百位
        outstr_p[*len_p + 1] = ((uint8_t)(file_info_t.array_element.high)) % 100 / 10 + 0x30;//十位
        outstr_p[*len_p + 2] = ((uint8_t)(file_info_t.array_element.high)) % 10 + 0x30;//个位
        outstr_p[*len_p + 3] = '\0';//字符串结束符

//        //彩图，黑白图
//        strcat((char*)outstr_p, ", ");
//        if((file_info_t.array_element.flag & IMG_TTPE_01) == IMG_TTPE_BITMAP){
//            strcat((char*)outstr_p, "mono, ");
//        }
//        else if((file_info_t.array_element.flag & IMG_TTPE_01) == IMG_TTPE_8BIT){
//            strcat((char*)outstr_p, "8bit color, ");
//        }
//        else if((file_info_t.array_element.flag & IMG_TTPE_01) == IMG_TTPE_16BIT){
//            strcat((char*)outstr_p, "16bit color, ");
//        }
//        else {
//            printf("------------------\r\n");
//            printf("-------%d------\r\n", __LINE__);
//            printf("------------------\r\n");
//            while(1);
//        }
//
//        //压缩，未压缩
//        strcat((char*)outstr_p, ", ");
//        if((file_info_t.array_element.flag & IMG_TTPE_23) == IMG_TTPE_UNCOMPRESS){
//            strcat((char*)outstr_p, "normal");
//        }
//        else if((file_info_t.array_element.flag & IMG_TTPE_23) == IMG_TTPE_RLE){
//            strcat((char*)outstr_p, "RLE");
//        }
//        else if((file_info_t.array_element.flag & IMG_TTPE_23) == IMG_TTPE_RLE_Tools){
//            strcat((char*)outstr_p, "RLE Tools");
//        }
//        else {
//            printf("------------------\r\n");
//            printf("-------%d------\r\n", __LINE__);
//            printf("------------------\r\n");
//            while(1);
//        }


         //右注释符号
        strcat((char*)outstr_p, "  */");
    }
    //打印
#if ARRAY_STR_LOG
    printf("%s\n", outstr_p);
    printf("6 strlen(outstr_p) = %d\n", strlen(outstr_p));
#endif // ARRAY_STR_LOG

    //9、尾部换行符拼接并计算长度
    strcat((char*)outstr_p, "\r\n");
    *len_p = strlen((const char*)outstr_p);

    //打印
#if ARRAY_STR_LOG
    printf("%s\n", outstr_p);
    printf("7 strlen(outstr_p) = %d\n", strlen(outstr_p));
#endif // ARRAY_STR_LOG
}

//文件初始化函数
void file_init(char* file_path)
{
    FILE* file_p = fopen(file_path, "w+");
    if (file_p == NULL)
    {
        printf("---------------%s---------------\r\n", __FUNCTION__);
        perror("file error:");
        while (1);
    }
    else
    {
        fclose(file_p);
    }
}

//存储数组定义信息
void file_info_storage(const char* file_path, char* save_str, uint8_t str_len)
{
    //bin_info_file = fopen(ALL_BIN_INFO_FILE_PATH, "wb+");
    //FILE * file_p = fopen(ALL_BIN_INFO_FILE_REL_PATH, "ab+");
    FILE* file_p = fopen(file_path, "ab+");


    if (file_p == NULL)
    {
        printf("---------------%s---------------\r\n", __FUNCTION__);
        perror("fopen error");
        printf("ALL_BIN_DATA_FILE_PATH is \"%s\"", file_path);
        while (1);
    }
    else
    {
        fseek(file_p, 0, SEEK_END);
        fwrite(save_str, 1, str_len, file_p);
    }
    fclose(file_p);
}

//存储数组声明信息
void file_info_storage_h(const char* file_path, char* save_str, uint8_t str_len)
{
    //bin_info_file = fopen(ALL_BIN_INFO_FILE_PATH, "wb+");
    //FILE * file_p = fopen(ALL_BIN_INFO_H_FILE_REL_PATH, "ab+");
    FILE* file_p = fopen(file_path, "ab+");
    if (file_p == NULL)
    {
        printf("---------------%s---------------\r\n", __FUNCTION__);
        perror("fopen error");
        printf("ALL_BIN_DATA_FILE_PATH is \"%s\"", file_path);
        while (1);
    }
    else
    {
        fseek(file_p, 0, SEEK_END);
        fwrite(save_str, 1, str_len, file_p);
    }
    fclose(file_p);
}

//存储合并文件数据
void file_data_storage(const char* file_path, char* file_name)
{
    uint8_t data_buffer[BUFFER_SIZE] = { 0 };
    int rc1;

    //FILE * bin_data_file = fopen(ALL_BIN_INFO_FILE_PATH, "wb+");
    //FILE * all_fp = fopen(ALL_BIN_DATA_FILE_REL_PATH, "ab+");
    FILE* all_fp = fopen(file_path, "ab+");

    FILE* file_fp = fopen(file_name, "rb");

    if ((all_fp == NULL) || (file_fp == NULL))
    {
        printf("---------------%s---------------\r\n", __FUNCTION__);
        perror("fopen error");
        printf("ALL_BIN_DATA_FILE_PATH is \"%s\"", file_path);
        printf("file_name is \"%s\"", file_name);
        while (1);
    }
    else
    {
        //跳转到All bin文件末尾
        fseek(all_fp, 0, SEEK_END);

        //读取数据，存储到all bin文件中
        while ((rc1 = fread(data_buffer, sizeof(unsigned char), BUFFER_SIZE, file_fp)) != 0)
        {
            fwrite(data_buffer, sizeof(unsigned char), rc1, all_fp);
        }
    }

    //关闭文件句柄
    fclose(all_fp);
    fclose(file_fp);
}

//从文件名中提取图片的长和高
void calculate_pic_width_and_high(char* filename_p, uint8_t name_len, uint16_t* width_t, uint16_t* high_t)
{
    uint8_t w_len = 0;//宽字节数
    uint8_t h_len = 0;//高字节数
    uint8_t wh_len = 0;//宽x高字节数

    char* str1 = 0;
    uint8_t wh_t[10] = { 0 };
    uint8_t wh_t1[10] = { 0 };
    uint8_t wh_t2[10] = { 0 };

    //截取尾部固定长度含长和宽的字符串
    strncpy((char*)wh_t, filename_p + (name_len - 8), 8);
    //查找是否有'_'
    str1 = strchr((char*)wh_t, '_');

    if (strlen(str1) == strlen((const char*)wh_t))
    {
        strncpy((char*)wh_t2, str1 + 1, strlen(str1) - 1);
        str1 = strchr((char*)wh_t2, '_');

        if (str1 == NULL)
        {
            strncpy((char*)wh_t1, (const char*)wh_t2, strlen((const char*)wh_t2));
        }
        else
        {
            memset(wh_t1, 0x00, sizeof(wh_t1));
            strncpy((char*)wh_t1, str1 + 1, strlen(str1) - 1);
        }
    }
    else
    {
        strncpy((char*)wh_t1, str1 + 1, strlen(str1) - 1);
    }

    //    printf( "%s\n", wh_t);
    //    printf( "%d\n", strlen(wh_t));
    //
    //    printf( "%s\n", str1);
    //    printf( "%d\n", strlen(str1));
    //
    //    printf( "%s\n", wh_t2);
    //    printf( "%d\n", strlen(wh_t2));
    //
    //    printf( "%s\n", wh_t1);
    //    printf( "%d\n", strlen(wh_t1));
    //    while(1);

    str1 = strchr((char*)wh_t1, 'x');
    wh_len = strlen((const char*)wh_t1);
    h_len = strlen(str1) - 1;
    w_len = wh_len - strlen(str1);

    //获取宽度
    switch (w_len)
    {
    case 1:
    {
        *width_t = (wh_t1[0] - 0x30);
    }
    break;
    case 2:
    {
        *width_t = (wh_t1[0] - 0x30) * 10;
        *width_t += (wh_t1[1] - 0x30);
    }
    break;
    case 3:
    {
        *width_t = (wh_t1[0] - 0x30) * 100;
        *width_t += (wh_t1[1] - 0x30) * 10;
        *width_t += (wh_t1[2] - 0x30);
    }
    break;
    default:
        printf("---------------pic width is error---------------\r\n");
        printf("%s\r\n", filename_p);
        printf("pic width is %d\r\n", w_len);
        printf("%s\n", wh_t);
        printf("%s\n", wh_t1);
        while (1);
        break;
    }


    //获取高度
    switch (h_len)
    {
    case 1:
    {
        *high_t = (wh_t1[w_len + 1] - 0x30);
    }
    break;
    case 2:
    {
        *high_t = (wh_t1[w_len + 1] - 0x30) * 10;
        *high_t += (wh_t1[w_len + 2] - 0x30);
    }
    break;
    case 3:
    {
        *high_t = (wh_t1[w_len + 1] - 0x30) * 100;
        *high_t += (wh_t1[w_len + 2] - 0x30) * 10;
        *high_t += (wh_t1[w_len + 3] - 0x30);
    }
    break;
    default:
        printf("---------------pic width is error---------------\r\n");
        printf("pic high is %d\r\n", h_len);
        while (1);
        break;
    }


}


//压缩
int Rle_Encode_P(unsigned char* inbuf, int inSize, unsigned char* outbuf, int onuBufSize)
{
    unsigned char* src = inbuf;
    int idx;
    int encSize = 0;

    while (src < (inbuf + inSize))
    {
        unsigned char value = *src++;
        idx = 1;

        // printf("%x\r\n", value);

        while ((*src == value) && (idx < 63) && (src < (inbuf + inSize)))
        {
            src++;
            idx++;
        }

        if ((encSize + idx + 1) > onuBufSize) /*输出缓冲区空间不够了*/
        {
            return -1;
        }

        if (idx > 1)
        {
            outbuf[encSize++] = idx | 0xC0;
            outbuf[encSize++] = value;

            //            printf("%x\r\n", (i | 0xC0));
            //            printf("%x\r\n", value);
        }
        else
        {
            //            if((value & 0xC0) == 0xC0)
            //            {
            //                outbuf[encSize++] = 0xC1;
            //            }
            //            outbuf[encSize++] = value;

            if ((value & 0xC0) == 0xC0)
            {
                if (value != 0xC0)
                {
                    outbuf[encSize++] = 0xC1;
                }
            }
            outbuf[encSize++] = value;
            //            printf("%x\r\n", 0xC1);
            //            printf("%x\r\n", value);
        }
    }

    return encSize;
}


//解压
int Rle_Decode_P(unsigned char* inbuf, int inSize, unsigned char* outbuf, int onuBufSize)
{
    unsigned char* src = inbuf;
    int i;
    int decSize = 0;
    int count = 0;

    while (src < (inbuf + inSize))
    {
        unsigned char value = *src++;
        int count = 1;

        if ((value & 0xC0) == 0xC0) /*是否有块属性标志*/
        {
            if (value != 0xC0)
            {
                count = value & 0x3F; /*低位是count*/
                value = *src++;
            }
        }
        else
        {
            count = 1;
        }

        if ((decSize + count) > onuBufSize) /*输出缓冲区空间不够了*/
        {
            return -1;
        }
        for (i = 0; i < count; i++)
        {
            outbuf[decSize++] = value;
        }
    }

    return decSize;
}


//判断连续三个字节数据是否相同
static unsigned char IsRepetitionStart(unsigned char* src, int src_len, int srcPos)
{
    unsigned char byte1 = src[srcPos];
    unsigned char byte2 = src[srcPos + 1];
    unsigned char byte3 = src[srcPos + 2];

    if (srcPos + 3 >= src_len) return 0;

    return ((byte1 == byte2) && (byte2 == byte3));
}

//获取连续相同数据的个数
static unsigned char GetRepetitionCount(unsigned char* src, int src_len, int srcPos)
{
    int endLeft = srcPos + 127;
    unsigned char count = 1;
    int cycle_i = 0;

    if (endLeft >= src_len) endLeft = src_len - 1;

    //    printf("cycle_i-->%d, endLeft = %d\r\n", srcPos, endLeft);

    for (cycle_i = srcPos; cycle_i < endLeft; cycle_i++)
    {
        if (src[cycle_i] != src[cycle_i + 1]) break;
        count++;
        if (count == 127) break;

        //        printf("cycle_i-->%d\r\n", cycle_i);
        //        printf("count-->%d\r\n", count);
    }
    return count;
}

//获取连续不相同数据的个数
static unsigned char GetNonRepetitionCount(unsigned char* src, int src_len, int srcPos)
{
    int endLeft = srcPos + 127;
    unsigned char count = 1;
    int cycle_i = 0;

    //不足三个字节
    if ((src_len - srcPos) == 3)
    {
        printf("2 src_len-->%d\r\n", src_len);
        printf("2 srcPos-->%d\r\n", srcPos);
        return 3;
    }
    if ((src_len - srcPos) == 2)
    {
        printf("1 src_len-->%d\r\n", src_len);
        printf("1 srcPos-->%d\r\n", srcPos);
        return 2;
    }
    if ((src_len - srcPos) == 1)
    {
        printf("0 src_len-->%d\r\n", src_len);
        printf("0 srcPos-->%d\r\n", srcPos);
        return 1;
    }
    //    if((src_len - srcPos) == 1)
    //    {
    //        printf("0 src_len-->%d\r\n", src_len);
    //        printf("0 srcPos-->%d\r\n", srcPos);
    //        return 0;
    //    }



    if (endLeft >= src_len) endLeft = src_len - 1;

    for (cycle_i = srcPos; cycle_i < endLeft; cycle_i++)
    {
        if (IsRepetitionStart(src, src_len, cycle_i)) break;
        count++;
        if (count == 127) break;
    }

    return count;
}

//压缩
int Rle_Encode_O(unsigned char* inbuf, int inSize, unsigned char* outbuf, int outBufSize)
{
    unsigned char* src = (unsigned char*)inbuf;

    int cycle_i = 0;

    int encSize = 0;//输出数组下标
    int inbufPos = 0;//输入数组下标

    while (inbufPos < inSize)
    {
        int count = 0;  //重复数和不重复数量
        if (IsRepetitionStart(src, inSize, inbufPos)) /*是否连续三个字节数据相同？*/
        {
            if ((encSize + 2) > outBufSize)  /*输出缓冲区空间不够了*/
            {
                return -1;
            }
            count = GetRepetitionCount(src, inSize, inbufPos); /*得到连续重复的数据个数*/

            outbuf[encSize++] = count | 0x80;
            outbuf[encSize++] = src[inbufPos];
            inbufPos += count;
        }
        else
        {
            count = GetNonRepetitionCount(src, inSize, inbufPos);/*得到连续不重复的数据个数*/

            if (count == 0)//最后一个了
            {
                //printf("inSize-->%d\r\n", inSize);
                //printf("inbufPos-->%d\r\n", inbufPos);
                //printf("__LINE__-->%d\r\n", __LINE__);
                //while(1);
                return -2;
            }

            if ((encSize + count + 1) > outBufSize)  /*输出缓冲区空间不够了*/
            {
                return -1;
            }

            outbuf[encSize++] = count;

            //printf("2-->%d\r\n", count);


            for (cycle_i = 0; cycle_i < count; cycle_i++)  /*逐个复制这些数据*/
            {
                outbuf[encSize++] = src[inbufPos++];
            }

        }
    }
    return encSize;
}


//解压
int Rle_Decode_O(unsigned char* inbuf, int inSize, unsigned char* outbuf, int onuBufSize)
{
    int i;
    unsigned char* src = inbuf;
    int decSize = 0;
    int count = 0;

    while (src < (inbuf + inSize))
    {
        unsigned char sign = *src++;
        int count = sign & 0x7F;

        if ((decSize + count) > onuBufSize)
        {
            return -1;
        }

        if ((sign & 0x80) == 0x80)
        {
            for (i = 0; i < count; i++)
            {
                outbuf[decSize++] = *src;
            }
            src++;
        }
        else
        {
            for (i = 0; i < count; i++)
            {
                outbuf[decSize++] = *src++;
            }
        }
    }
    return decSize;
}


//文件压缩存储
#if defined(_UNCOMPRESS_)
void Bin_rle_compress_handle(struct _finddata_t fileinfo)
{
    uint8_t read_buff[FILE_MAX_SIZE] = { 0 };
    uint8_t write_buff[FILE_MAX_SIZE] = { 0 };
    uint8_t decode_buff[FILE_MAX_SIZE] = { 0 };

    int32_t file_size = 0;
    int32_t maxsize = FILE_MAX_SIZE;
    int32_t encode_size = 0;
    int32_t decode_size = 0;



    int8_t original_file_path[150] = { 0 };
    int8_t new_file_path[150] = { 0 };
    int8_t decompress_file_path[150] = { 0 };


    strcpy((char*)original_file_path, fileinfo.name);

    strcpy((char*)new_file_path, CompressFile_REL_Path);
    strcat((char*)new_file_path, fileinfo.name);

    strcpy((char*)decompress_file_path, DecompressFile_REL_Path);
    strcat((char*)decompress_file_path, fileinfo.name);

    //    strncat(new_file_path, fileinfo.name, strlen(fileinfo.name)-4);
    //    strcat(new_file_path, ".bin");


    //    printf("%s\r\n",original_file_path);
    //    printf("%s\r\n",new_file_path);

    FILE* fp1 = fopen((const char*)original_file_path, "rb+");
    FILE* fp2 = fopen((const char*)new_file_path, "wb+");
    //FILE * fp3 = fopen(decompress_file_path, "wb+");

    if ((fp1 == NULL) || (fp2 == NULL))
    {
        printf("%s\r\n", __FUNCTION__);
        perror("fopen error");
        while (1);
    }
    else
    {
        file_size = fread(read_buff, sizeof(uint8_t), FILE_MAX_SIZE, fp1);

        encode_size = Rle_Encode_O(read_buff, file_size, write_buff, maxsize);
        fwrite(write_buff, sizeof(uint8_t), encode_size, fp2);

        //        decode_size = Rle_Decode_O(write_buff, encode_size, decode_buff, maxsize);
        //        fwrite(decode_buff, sizeof(uint8_t), decode_size, fp3);

        fclose(fp1);
        fclose(fp2);
        //        fclose(fp3);
                //while(1);
    }
}
#endif

//文件解压存储
#if defined(_COMPRESS_)
void Bin_rle_decompress_handle(const char* p_file_path, struct _finddata_t fileinfo)
{
    uint8_t read_buff[FILE_MAX_SIZE] = { 0 };
    uint8_t write_buff[FILE_MAX_SIZE] = { 0 };
    uint8_t decode_buff[FILE_MAX_SIZE] = { 0 };

    int32_t file_size = 0;
    int32_t maxsize = FILE_MAX_SIZE;
    int32_t encode_size = 0;
    int32_t decode_size = 0;

    int8_t original_file_path[150] = { 0 };
    int8_t new_file_path[150] = { 0 };

    strcpy((char*)original_file_path, fileinfo.name);
    //strcpy(new_file_path, DecompressFile_REL_Path);
    strcpy((char*)new_file_path, p_file_path);
    strcat((char*)new_file_path, fileinfo.name);
    //    strncat(new_file_path, fileinfo.name, strlen(fileinfo.name)-4);
    //    strcat(compress_file_path, ".bin");


    //    printf("%s\r\n", original_file_path);
    //    printf("%s\r\n", new_file_path);

    FILE* fp1 = fopen((const char*)original_file_path, "rb+");
    FILE* fp2 = fopen((const char*)new_file_path, "wb+");

    if ((fp1 == NULL) || (fp2 == NULL))
    {
        printf("%s\r\n", __FUNCTION__);
        perror("fopen error");
        while (1);
    }
    else
    {
        file_size = fread(read_buff, sizeof(uint8_t), FILE_MAX_SIZE, fp1);

        encode_size = Rle_Decode_O(read_buff, file_size, write_buff, maxsize);
        fwrite(write_buff, sizeof(uint8_t), encode_size, fp2);
        //decode_size = Rle_Decode_P(write_buff,encode_size,decode_buff,maxsize);
        //fwrite(decode_buff,sizeof(uint8_t),decode_size,fp2);
        fclose(fp1);
        fclose(fp2);
    }

    //while(1);
}
#endif // defined

#ifdef _UNCOMPRESS_
void uncompress_handle(void)
{
    struct file_info_array_unit  file_info_array = { 0 };
    char work_path[_MAX_PATH];
    long hFile;
    struct _finddata_t fileinfo;
    uint8_t w_buffer[500] = { 0 };
    uint8_t w_buffer_len = 0;
    uint32_t file_num = 0, file_size = 0;


    //初始化变量
    {
        w_buffer_len = 0;
        memset((char*)w_buffer, 0x00, sizeof(w_buffer));

        file_init(UNCOMPRESS_ALL_BIN_DATA_FILE_ABS_PATH);
        file_init(UNCOMPRESS_ALL_BIN_INFO_FILE_ABS_PATH);
        file_init(UNCOMPRESS_ALL_BIN_INFO_H_FILE_ABS_PATH);

        if (FILE_ARRAY_ELEMENT_SZ != sizeof(file_info_array.array_element))
        {
            printf("FILE_ARRAY_ELEMENT_SZ is error \r\n");
            printf("FILE_ARRAY_ELEMENT_SZ =  %d\n", FILE_ARRAY_ELEMENT_SZ);
            printf("sizeof(file_info_array.element) = %d\n", sizeof(file_info_array.array_element));
            while (1);
        }
    }



    //获取当前工作目录
    {

        if (_getcwd(work_path, _MAX_PATH) == NULL)
        {
            perror("_getcwd error");
        }
        else
        {
            printf("%s\n", work_path);
        }

        system("rd /S /Q batch_bin_output");
        system("rd /S /Q batch_rle_bin_output");
        system("rd /S /Q MergerOutput");
        mkdir("batch_bin_output");
        mkdir("batch_rle_bin_output");
        mkdir("MergerOutput");

        //更改当前工作目录 - 相对路径方式
        if (_chdir(UNCOMPRESS_BinFileFolder))
        {
            printf("Unable to locate the directory you specified \n");
        }
        else
        {
            _getcwd(work_path, _MAX_PATH);          //重新获取当前工作目录
            printf("The CWD is: %s\n", work_path);  //输出当前工作目录
            //system( "home_battery_0_34x20.ebm"); //system用于执行命令行指令
        }
    }

    //查找当前目录中符合要求的文件, 并输出文件的相关信息
    if ((hFile = _findfirst("*.*", &fileinfo)) != -1L)
    {
        do
        {
            //if (!(fileinfo.attrib & _A_SUBDIR)  && (strcmp(fileinfo.name, "home_week_2_en_58x24.ebm") == 0)) //检查是不是目录, 如果不是,则进行处理
            //if (!(fileinfo.attrib & _A_SUBDIR)  && (strcmp(fileinfo.name, "h1xDaySlash_10x15.ebm") == 0)) //检查是不是目录, 如果不是,则进行处理
            if (!(fileinfo.attrib & _A_SUBDIR)) //检查是不是目录, 如果不是,则进行处理
            {
                file_num++;
#if defined(_UNCOMPRESS_)
                Bin_rle_compress_handle(fileinfo);
#endif // defined

#if 1
                {
                    //清除中间变量
                    w_buffer_len = 0;
                    memset((char*)w_buffer, 0x00, sizeof(w_buffer));
                    memset((char*)&file_info_array, 0x00, sizeof(file_info_array));

                    //设置输出数组信息
                    file_info_array.array_type = UINT8_T_TYPE;
                    strncpy((char*)file_info_array.array_name, fileinfo.name, strlen(fileinfo.name) - 4);
                    file_info_array.array_element.addr = file_size;//offset
                    file_info_array.array_element.size = fileinfo.size;//offset

                    //计算长和宽
                    calculate_pic_width_and_high(fileinfo.name, (strlen(fileinfo.name) - 4), &file_info_array.array_element.width, &file_info_array.array_element.high);


#if defined(_UNCOMPRESS_) || defined(_DECOMPRESS_)
                    if (IMG_TTPE == IMG_TTPE_8BIT)
                    {
                        if (fileinfo.size != (file_info_array.array_element.width * file_info_array.array_element.high))
                        {
                            printf("width or high is error! picture name --------> %s \r\n", fileinfo.name);
                            printf("width x high = %d\r\n", file_info_array.array_element.width * file_info_array.array_element.high * 2);
                            printf("fileinfo.size = %d\r\n", fileinfo.size);
                            while (1);
                        }
                    }
                    else if (IMG_TTPE == IMG_TTPE_16BIT)
                    {
                        if (fileinfo.size != (file_info_array.array_element.width * file_info_array.array_element.high * 2))
                        {
                            printf("width or high is error! picture name --------> %s \r\n", fileinfo.name);
                            printf("width x high = %d\r\n", file_info_array.array_element.width * file_info_array.array_element.high * 2);
                            printf("fileinfo.size = %d\r\n", fileinfo.size);
                            while (1);
                        }
                    }
                    else
                    {
                        printf("IMG_TTPE is error! IMG_TTPE --------> %x \r\n", IMG_TTPE);
                        printf("Error line = %d\r\n", __LINE__);
                        printf("fileinfo.size = %d\r\n", fileinfo.size);
                        while (1);
                    }
#endif // defined

                    file_info_array.array_element.flag |= IMG_TTPE;//bit-01(00:黑白图, 01:8位图, 10:16位全彩, 11:保留)
                    file_info_array.array_element.flag |= IMG_TTPE_UNCOMPRESS;//bit-23(00:未压缩, 01:RLE压缩,  10:保留,  11:保留)


                    //将数组信息转成字符串
                    FileInfoToArrayStr(file_info_array, w_buffer, &w_buffer_len);
                    //                    printf("%s\n", w_buffer);
                                        //数组字符串存储到文件中
                    file_info_storage(UNCOMPRESS_ALL_BIN_INFO_FILE_REL_PATH, (char*)w_buffer, w_buffer_len);

                    //清除中间变量
                    w_buffer_len = 0;
                    memset((char*)w_buffer, 0x00, sizeof(w_buffer));

                    //将数组声明信息转成字符串
                    FileInfoToArrayStr_h(file_info_array, w_buffer, &w_buffer_len);
                    //                    printf("%s\n", w_buffer);
                                        //数组字符串存储到头文件中
                    file_info_storage_h(UNCOMPRESS_ALL_BIN_INFO_H_FILE_REL_PATH, (char*)w_buffer, w_buffer_len);

                    file_data_storage(UNCOMPRESS_ALL_BIN_DATA_FILE_REL_PATH, fileinfo.name);

                    //printf("%d  %s, %d bytes\n", i, fileinfo.name, fileinfo.size);

                }
#endif // 1

                printf("%d  %s, %d bytes\n", file_num, fileinfo.name, fileinfo.size);
                //printf("%03d  %s\n", file_num, file_info_array.array_name);

                file_size += fileinfo.size;
            }
        } while (_findnext(hFile, &fileinfo) == 0);


        printf("all_pic = %d\n", file_size);
        _findclose(hFile);
    }


}
#endif



#ifdef _COMPRESS_
void compress_handle(void)
{

    struct file_info_array_unit  file_info_array = { 0 };
    char work_path[_MAX_PATH];
    long hFile;
    struct _finddata_t fileinfo;
    uint8_t w_buffer[500] = { 0 };
    uint8_t w_buffer_len = 0;
    uint32_t file_num = 0, file_size = 0;



    printf("compress_handle\n");

    //获取当前工作目录
    {

        if (_getcwd(work_path, _MAX_PATH) == NULL)
        {
            perror("_getcwd error");
        }
        else
        {
            printf("%s\n", work_path);
        }

        //更改当前工作目录 - 相对路径方式
        if (_chdir("../"))
        {
            printf("Unable to locate the directory you specified \n");
        }
        else
        {
            _getcwd(work_path, _MAX_PATH);          //重新获取当前工作目录
            printf("The CWD is: %s\n", work_path);  //输出当前工作目录
        }
    }

    //初始化变量
    {
        w_buffer_len = 0;
        memset((char*)w_buffer, 0x00, sizeof(w_buffer));

        file_init(COMPRESS_ALL_BIN_DATA_FILE_ABS_PATH);
        file_init(COMPRESS_ALL_BIN_INFO_FILE_ABS_PATH);
        file_init(COMPRESS_ALL_BIN_INFO_H_FILE_ABS_PATH);

        if (FILE_ARRAY_ELEMENT_SZ != sizeof(file_info_array.array_element))
        {
            printf("FILE_ARRAY_ELEMENT_SZ is error \r\n");
            printf("FILE_ARRAY_ELEMENT_SZ =  %d\n", FILE_ARRAY_ELEMENT_SZ);
            printf("sizeof(file_info_array.element) = %d\n", sizeof(file_info_array.array_element));
            while (1);
        }
    }

    //获取当前工作目录
    {

        if (_getcwd(work_path, _MAX_PATH) == NULL)
        {
            perror("_getcwd error");
        }
        else
        {
            printf("%s\n", work_path);
        }

        //更改当前工作目录 - 相对路径方式
        if (_chdir(COMPRESS_BinFileFolder))
        {
            printf("Unable to locate the directory you specified \n");
        }
        else
        {
            _getcwd(work_path, _MAX_PATH);          //重新获取当前工作目录
            printf("The CWD is: %s\n", work_path);  //输出当前工作目录
            //system( "home_battery_0_34x20.ebm"); //system用于执行命令行指令
        }
    }



    if ((hFile = _findfirst("*.*", &fileinfo)) != -1L)
    {
        do
        {
            //if (!(fileinfo.attrib & _A_SUBDIR)  && (strcmp(fileinfo.name, "home_week_2_en_58x24.ebm") == 0)) //检查是不是目录, 如果不是,则进行处理
            //if (!(fileinfo.attrib & _A_SUBDIR)  && (strcmp(fileinfo.name, "h1xDaySlash_10x15.ebm") == 0)) //检查是不是目录, 如果不是,则进行处理
            if (!(fileinfo.attrib & _A_SUBDIR)) //检查是不是目录, 如果不是,则进行处理
            {
                file_num++;
                Bin_rle_decompress_handle(DecompressFile_REL_Path, fileinfo);

#if 1
                {
                    //清除中间变量
                    w_buffer_len = 0;
                    memset((char*)w_buffer, 0x00, sizeof(w_buffer));
                    memset((char*)&file_info_array, 0x00, sizeof(file_info_array));

                    //设置输出数组信息
                    file_info_array.array_type = UINT8_T_TYPE;
                    strncpy((char*)file_info_array.array_name, fileinfo.name, strlen(fileinfo.name) - 4);
                    file_info_array.array_element.addr = file_size;//offset
                    file_info_array.array_element.size = fileinfo.size;//offset

                    //计算长和宽
                    calculate_pic_width_and_high(fileinfo.name, (strlen(fileinfo.name) - 4), &file_info_array.array_element.width, &file_info_array.array_element.high);

                    file_info_array.array_element.flag |= IMG_TTPE;//bit-01(00:黑白图, 01:8位图, 10:16位全彩, 11:保留)

                    file_info_array.array_element.flag |= IMG_TTPE_RLE;

                    //将数组信息转成字符串
                    FileInfoToArrayStr(file_info_array, w_buffer, &w_buffer_len);
                    //                    printf("%s\n", w_buffer);
                                        //数组字符串存储到文件中
                    file_info_storage(COMPRESS_ALL_BIN_INFO_FILE_REL_PATH, (char*)w_buffer, w_buffer_len);

                    //清除中间变量
                    w_buffer_len = 0;
                    memset((char*)w_buffer, 0x00, sizeof(w_buffer));

                    //将数组声明信息转成字符串
                    FileInfoToArrayStr_h(file_info_array, w_buffer, &w_buffer_len);
                    //                    printf("%s\n", w_buffer);
                                        //数组字符串存储到头文件中
                    file_info_storage_h(COMPRESS_ALL_BIN_INFO_H_FILE_REL_PATH, (char*)w_buffer, w_buffer_len);

                    file_data_storage(COMPRESS_ALL_BIN_DATA_FILE_REL_PATH, fileinfo.name);

                    //printf("%d  %s, %d bytes\n", i, fileinfo.name, fileinfo.size);

                }
#endif // 1

                printf("%d  %s, %d bytes\n", file_num, fileinfo.name, fileinfo.size);
                //printf("%03d  %s\n", file_num, file_info_array.array_name);

                file_size += fileinfo.size;
            }
        } while (_findnext(hFile, &fileinfo) == 0);

        printf("all_pic_compress byte = %d\n", file_size);
        _findclose(hFile);
    }
}

#endif



#ifdef _DECOMPRESS_
void decompress_handle(void)
{
    struct file_info_array_unit  file_info_array = { 0 };
    char work_path[_MAX_PATH];
    long hFile;
    struct _finddata_t fileinfo;
    uint8_t w_buffer[500] = { 0 };
    uint8_t w_buffer_len = 0;
    uint32_t file_num = 0, file_size = 0;


    //获取当前工作目录
    {

        if (_getcwd(work_path, _MAX_PATH) == NULL)
        {
            perror("_getcwd error");
        }
        else
        {
            printf("%s\n", work_path);
        }

        //更改当前工作目录 - 相对路径方式
        if (_chdir("../"))
        {
            printf("Unable to locate the directory you specified \n");
        }
        else
        {
            _getcwd(work_path, _MAX_PATH);          //重新获取当前工作目录
            printf("The CWD is: %s\n", work_path);  //输出当前工作目录
            //system( "home_battery_0_34x20.ebm"); //system用于执行命令行指令
        }
    }






    //初始化变量
    {
        w_buffer_len = 0;
        memset((char*)w_buffer, 0x00, sizeof(w_buffer));

        file_init(DECOMPRESS_ALL_BIN_DATA_FILE_ABS_PATH);
        file_init(DECOMPRESS_ALL_BIN_INFO_FILE_ABS_PATH);
        file_init(DECOMPRESS_ALL_BIN_INFO_H_FILE_ABS_PATH);

        if (FILE_ARRAY_ELEMENT_SZ != sizeof(file_info_array.array_element))
        {
            printf("FILE_ARRAY_ELEMENT_SZ is error \r\n");
            printf("FILE_ARRAY_ELEMENT_SZ =  %d\n", FILE_ARRAY_ELEMENT_SZ);
            printf("sizeof(file_info_array.element) = %d\n", sizeof(file_info_array.array_element));
            while (1);
        }
    }


    //获取当前工作目录
    {

        if (_getcwd(work_path, _MAX_PATH) == NULL)
        {
            perror("_getcwd error");
        }
        else
        {
            printf("%s\n", work_path);
        }

        //更改当前工作目录 - 相对路径方式
        if (_chdir(DECOMPRESS_BinFileFolder))
        {
            printf("Unable to locate the directory you specified \n");
        }
        else
        {
            _getcwd(work_path, _MAX_PATH);          //重新获取当前工作目录
            printf("The CWD is: %s\n", work_path);  //输出当前工作目录
            //system( "home_battery_0_34x20.ebm"); //system用于执行命令行指令
        }
    }




    //查找当前目录中符合要求的文件, 并输出文件的相关信息
    if ((hFile = _findfirst("*.*", &fileinfo)) != -1L)
    {
        do
        {
            //if (!(fileinfo.attrib & _A_SUBDIR)  && (strcmp(fileinfo.name, "home_week_2_en_58x24.ebm") == 0)) //检查是不是目录, 如果不是,则进行处理
            //if (!(fileinfo.attrib & _A_SUBDIR)  && (strcmp(fileinfo.name, "h1xDaySlash_10x15.ebm") == 0)) //检查是不是目录, 如果不是,则进行处理
            if (!(fileinfo.attrib & _A_SUBDIR)) //检查是不是目录, 如果不是,则进行处理
            {
                file_num++;
#if 1
                {
                    //清除中间变量
                    w_buffer_len = 0;
                    memset((char*)w_buffer, 0x00, sizeof(w_buffer));
                    memset((char*)&file_info_array, 0x00, sizeof(file_info_array));

                    //设置输出数组信息
                    file_info_array.array_type = UINT8_T_TYPE;
                    strncpy((char*)file_info_array.array_name, fileinfo.name, strlen(fileinfo.name) - 4);
                    file_info_array.array_element.addr = file_size;//offset
                    file_info_array.array_element.size = fileinfo.size;//offset

                    //计算长和宽
                    calculate_pic_width_and_high(fileinfo.name, (strlen(fileinfo.name) - 4), &file_info_array.array_element.width, &file_info_array.array_element.high);


#if defined(_UNCOMPRESS_) || defined(_DECOMPRESS_)
                    if (IMG_TTPE == IMG_TTPE_8BIT)
                    {
                        if (fileinfo.size != (file_info_array.array_element.width * file_info_array.array_element.high))
                        {
                            printf("width or high is error! picture name --------> %s \r\n", fileinfo.name);
                            printf("width x high = %d\r\n", file_info_array.array_element.width * file_info_array.array_element.high * 2);
                            printf("fileinfo.size = %d\r\n", fileinfo.size);
                            while (1);
                        }
                    }
                    else if (IMG_TTPE == IMG_TTPE_16BIT)
                    {
                        if (fileinfo.size != (file_info_array.array_element.width * file_info_array.array_element.high * 2))
                        {
                            printf("width or high is error! picture name --------> %s \r\n", fileinfo.name);
                            printf("width x high = %d\r\n", file_info_array.array_element.width * file_info_array.array_element.high * 2);
                            printf("fileinfo.size = %d\r\n", fileinfo.size);
                            while (1);
                        }
                    }
                    else
                    {
                        printf("IMG_TTPE is error! IMG_TTPE --------> %x \r\n", IMG_TTPE);
                        printf("Error line = %d\r\n", __LINE__);
                        printf("fileinfo.size = %d\r\n", fileinfo.size);
                        while (1);
                    }
#endif // defined

                    //              file_info_array.array_element.width = 0x00;
                    //               file_info_array.array_element.high  = 0x01;


                    file_info_array.array_element.flag |= IMG_TTPE;//bit-01(00:黑白图, 01:8位图, 10:16位全彩, 11:保留)


                    file_info_array.array_element.flag |= IMG_TTPE_UNCOMPRESS;//bit-23(00:未压缩, 01:RLE压缩,  10:保留,  11:保留)

                    //将数组信息转成字符串
                    FileInfoToArrayStr(file_info_array, w_buffer, &w_buffer_len);
                    //                    printf("%s\n", w_buffer);
                                        //数组字符串存储到文件中
                    file_info_storage(DECOMPRESS_ALL_BIN_INFO_FILE_REL_PATH, (char*)w_buffer, w_buffer_len);

                    //清除中间变量
                    w_buffer_len = 0;
                    memset((char*)w_buffer, 0x00, sizeof(w_buffer));

                    //将数组声明信息转成字符串
                    FileInfoToArrayStr_h(file_info_array, w_buffer, &w_buffer_len);
                    //                    printf("%s\n", w_buffer);
                                        //数组字符串存储到头文件中
                    file_info_storage_h(DECOMPRESS_ALL_BIN_INFO_H_FILE_REL_PATH, (char*)w_buffer, w_buffer_len);

                    file_data_storage(DECOMPRESS_ALL_BIN_DATA_FILE_REL_PATH, fileinfo.name);

                    //printf("%d  %s, %d bytes\n", i, fileinfo.name, fileinfo.size);

                }
#endif // 1

                printf("%d  %s, %d bytes\n", file_num, fileinfo.name, fileinfo.size);
                //printf("%03d  %s\n", file_num, file_info_array.array_name);

                file_size += fileinfo.size;
            }
        } while (_findnext(hFile, &fileinfo) == 0);


        printf("all_pic_decompress byte = %d\n", file_size);
        _findclose(hFile);
    }
}
#endif