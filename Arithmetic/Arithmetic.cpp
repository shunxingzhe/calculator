// Arithmetic.cpp : 定义 DLL 应用程序的导出函数。
//

#include "stdafx.h"
#include <stdio.h>
#include <direct.h> //_getcwd(), _chdir()
#include <stdlib.h> //_MAX_PATH, system()
#include <io.h>     //_finddata_t, _findfirst(), _findnext(), _findclose()
#include <string.h>
#include <stdint.h>
#include <sys/stat.h>
#include <iostream>
#include <windows.h>

#define SWD_LOG_LEN 1024

struct LOG_CB   //只有一个RTT_BUFFER_UP
{
    unsigned int WrOff;
    unsigned int RdOff;
    char Buf[SWD_LOG_LEN];   //16个byte
};

static struct LOG_CB log_cb;

void my_sprintf(const char* format, ...)
{
    int len = 0;
    char Buf[SWD_LOG_LEN];   //16个byte
    va_list args;

    va_start(args, format);
    len = vsprintf(Buf, format, args);
    va_end(args);
    if ((len + log_cb.RdOff) > SWD_LOG_LEN)
    {
        memcpy(&log_cb.Buf[log_cb.RdOff], Buf, SWD_LOG_LEN - log_cb.RdOff);
        len -= (SWD_LOG_LEN - log_cb.RdOff);
        memcpy(log_cb.Buf, &Buf[SWD_LOG_LEN - log_cb.RdOff], len);
        log_cb.RdOff = len;
    }
    else
    {
        memcpy(&log_cb.Buf[log_cb.RdOff], Buf, len);
        log_cb.RdOff += len;
    }
}
void my_sprintf(const unsigned char* Buf, unsigned int len)
{
    if ((len + log_cb.RdOff) > SWD_LOG_LEN)
    {
        memcpy(&log_cb.Buf[log_cb.RdOff], Buf, SWD_LOG_LEN - log_cb.RdOff);
        len -= (SWD_LOG_LEN - log_cb.RdOff);
        memcpy(log_cb.Buf, &Buf[SWD_LOG_LEN - log_cb.RdOff], len);
        log_cb.RdOff = len;
    }
    else
    {
        memcpy(&log_cb.Buf[log_cb.RdOff], Buf, len);
        log_cb.RdOff += len;
    }
}

int Dll_log_read(uint8_t* p_rsp)
{
    int len = 0;
    if (log_cb.RdOff != log_cb.WrOff)
    {
        if (log_cb.RdOff > log_cb.WrOff)
        {
            len = log_cb.RdOff - log_cb.WrOff;
            memcpy(p_rsp, log_cb.Buf + log_cb.WrOff, len);
        }
        else
        {
            memcpy(p_rsp, log_cb.Buf + log_cb.WrOff, SWD_LOG_LEN - log_cb.WrOff);
            len = SWD_LOG_LEN - log_cb.WrOff;
            memcpy(p_rsp + SWD_LOG_LEN - log_cb.WrOff, log_cb.Buf, log_cb.RdOff);
            len += log_cb.RdOff;
        }
        log_cb.WrOff = log_cb.RdOff;
    }

    return len;
}

static const unsigned int crc32tab[] = {
 0x00000000L, 0x77073096L, 0xee0e612cL, 0x990951baL,
 0x076dc419L, 0x706af48fL, 0xe963a535L, 0x9e6495a3L,
 0x0edb8832L, 0x79dcb8a4L, 0xe0d5e91eL, 0x97d2d988L,
 0x09b64c2bL, 0x7eb17cbdL, 0xe7b82d07L, 0x90bf1d91L,
 0x1db71064L, 0x6ab020f2L, 0xf3b97148L, 0x84be41deL,
 0x1adad47dL, 0x6ddde4ebL, 0xf4d4b551L, 0x83d385c7L,
 0x136c9856L, 0x646ba8c0L, 0xfd62f97aL, 0x8a65c9ecL,
 0x14015c4fL, 0x63066cd9L, 0xfa0f3d63L, 0x8d080df5L,
 0x3b6e20c8L, 0x4c69105eL, 0xd56041e4L, 0xa2677172L,
 0x3c03e4d1L, 0x4b04d447L, 0xd20d85fdL, 0xa50ab56bL,
 0x35b5a8faL, 0x42b2986cL, 0xdbbbc9d6L, 0xacbcf940L,
 0x32d86ce3L, 0x45df5c75L, 0xdcd60dcfL, 0xabd13d59L,
 0x26d930acL, 0x51de003aL, 0xc8d75180L, 0xbfd06116L,
 0x21b4f4b5L, 0x56b3c423L, 0xcfba9599L, 0xb8bda50fL,
 0x2802b89eL, 0x5f058808L, 0xc60cd9b2L, 0xb10be924L,
 0x2f6f7c87L, 0x58684c11L, 0xc1611dabL, 0xb6662d3dL,
 0x76dc4190L, 0x01db7106L, 0x98d220bcL, 0xefd5102aL,
 0x71b18589L, 0x06b6b51fL, 0x9fbfe4a5L, 0xe8b8d433L,
 0x7807c9a2L, 0x0f00f934L, 0x9609a88eL, 0xe10e9818L,
 0x7f6a0dbbL, 0x086d3d2dL, 0x91646c97L, 0xe6635c01L,
 0x6b6b51f4L, 0x1c6c6162L, 0x856530d8L, 0xf262004eL,
 0x6c0695edL, 0x1b01a57bL, 0x8208f4c1L, 0xf50fc457L,
 0x65b0d9c6L, 0x12b7e950L, 0x8bbeb8eaL, 0xfcb9887cL,
 0x62dd1ddfL, 0x15da2d49L, 0x8cd37cf3L, 0xfbd44c65L,
 0x4db26158L, 0x3ab551ceL, 0xa3bc0074L, 0xd4bb30e2L,
 0x4adfa541L, 0x3dd895d7L, 0xa4d1c46dL, 0xd3d6f4fbL,
 0x4369e96aL, 0x346ed9fcL, 0xad678846L, 0xda60b8d0L,
 0x44042d73L, 0x33031de5L, 0xaa0a4c5fL, 0xdd0d7cc9L,
 0x5005713cL, 0x270241aaL, 0xbe0b1010L, 0xc90c2086L,
 0x5768b525L, 0x206f85b3L, 0xb966d409L, 0xce61e49fL,
 0x5edef90eL, 0x29d9c998L, 0xb0d09822L, 0xc7d7a8b4L,
 0x59b33d17L, 0x2eb40d81L, 0xb7bd5c3bL, 0xc0ba6cadL,
 0xedb88320L, 0x9abfb3b6L, 0x03b6e20cL, 0x74b1d29aL,
 0xead54739L, 0x9dd277afL, 0x04db2615L, 0x73dc1683L,
 0xe3630b12L, 0x94643b84L, 0x0d6d6a3eL, 0x7a6a5aa8L,
 0xe40ecf0bL, 0x9309ff9dL, 0x0a00ae27L, 0x7d079eb1L,
 0xf00f9344L, 0x8708a3d2L, 0x1e01f268L, 0x6906c2feL,
 0xf762575dL, 0x806567cbL, 0x196c3671L, 0x6e6b06e7L,
 0xfed41b76L, 0x89d32be0L, 0x10da7a5aL, 0x67dd4accL,
 0xf9b9df6fL, 0x8ebeeff9L, 0x17b7be43L, 0x60b08ed5L,
 0xd6d6a3e8L, 0xa1d1937eL, 0x38d8c2c4L, 0x4fdff252L,
 0xd1bb67f1L, 0xa6bc5767L, 0x3fb506ddL, 0x48b2364bL,
 0xd80d2bdaL, 0xaf0a1b4cL, 0x36034af6L, 0x41047a60L,
 0xdf60efc3L, 0xa867df55L, 0x316e8eefL, 0x4669be79L,
 0xcb61b38cL, 0xbc66831aL, 0x256fd2a0L, 0x5268e236L,
 0xcc0c7795L, 0xbb0b4703L, 0x220216b9L, 0x5505262fL,
 0xc5ba3bbeL, 0xb2bd0b28L, 0x2bb45a92L, 0x5cb36a04L,
 0xc2d7ffa7L, 0xb5d0cf31L, 0x2cd99e8bL, 0x5bdeae1dL,
 0x9b64c2b0L, 0xec63f226L, 0x756aa39cL, 0x026d930aL,
 0x9c0906a9L, 0xeb0e363fL, 0x72076785L, 0x05005713L,
 0x95bf4a82L, 0xe2b87a14L, 0x7bb12baeL, 0x0cb61b38L,
 0x92d28e9bL, 0xe5d5be0dL, 0x7cdcefb7L, 0x0bdbdf21L,
 0x86d3d2d4L, 0xf1d4e242L, 0x68ddb3f8L, 0x1fda836eL,
 0x81be16cdL, 0xf6b9265bL, 0x6fb077e1L, 0x18b74777L,
 0x88085ae6L, 0xff0f6a70L, 0x66063bcaL, 0x11010b5cL,
 0x8f659effL, 0xf862ae69L, 0x616bffd3L, 0x166ccf45L,
 0xa00ae278L, 0xd70dd2eeL, 0x4e048354L, 0x3903b3c2L,
 0xa7672661L, 0xd06016f7L, 0x4969474dL, 0x3e6e77dbL,
 0xaed16a4aL, 0xd9d65adcL, 0x40df0b66L, 0x37d83bf0L,
 0xa9bcae53L, 0xdebb9ec5L, 0x47b2cf7fL, 0x30b5ffe9L,
 0xbdbdf21cL, 0xcabac28aL, 0x53b39330L, 0x24b4a3a6L,
 0xbad03605L, 0xcdd70693L, 0x54de5729L, 0x23d967bfL,
 0xb3667a2eL, 0xc4614ab8L, 0x5d681b02L, 0x2a6f2b94L,
 0xb40bbe37L, 0xc30c8ea1L, 0x5a05df1bL, 0x2d02ef8dL
};

unsigned int crc32_fun(const unsigned char* buf, unsigned int size)
{
    unsigned int i, crc;
    crc = 0xFFFFFFFF;

    for (i = 0; i < size; i++)
        crc = crc32tab[(crc ^ buf[i]) & 0xff] ^ (crc >> 8);

    return crc ^ 0xFFFFFFFF;
}
static const unsigned char crc8tab[] = { 
0x00, 0x07, 0x0e, 0x09, 0x1c, 0x1b,
0x12, 0x15, 0x38, 0x3f, 0x36, 0x31, 0x24, 0x23, 0x2a,
0x2d, 0x70, 0x77, 0x7e, 0x79, 0x6c, 0x6b, 0x62, 0x65,
0x48, 0x4f, 0x46, 0x41, 0x54, 0x53, 0x5a, 0x5d, 0xe0,
0xe7, 0xee, 0xe9, 0xfc, 0xfb, 0xf2, 0xf5, 0xd8, 0xdf,
0xd6, 0xd1, 0xc4, 0xc3, 0xca, 0xcd, 0x90, 0x97, 0x9e,
0x99, 0x8c, 0x8b, 0x82, 0x85, 0xa8, 0xaf, 0xa6, 0xa1,
0xb4, 0xb3, 0xba, 0xbd, 0xc7, 0xc0, 0xc9, 0xce, 0xdb,
0xdc, 0xd5, 0xd2, 0xff, 0xf8, 0xf1, 0xf6, 0xe3, 0xe4,
0xed, 0xea, 0xb7, 0xb0, 0xb9, 0xbe, 0xab, 0xac, 0xa5,
0xa2, 0x8f, 0x88, 0x81, 0x86, 0x93, 0x94, 0x9d, 0x9a,
0x27, 0x20, 0x29, 0x2e, 0x3b, 0x3c, 0x35, 0x32, 0x1f,
0x18, 0x11, 0x16, 0x03, 0x04, 0x0d, 0x0a, 0x57, 0x50,
0x59, 0x5e, 0x4b, 0x4c, 0x45, 0x42, 0x6f, 0x68, 0x61,
0x66, 0x73, 0x74, 0x7d, 0x7a, 0x89, 0x8e, 0x87, 0x80,
0x95, 0x92, 0x9b, 0x9c, 0xb1, 0xb6, 0xbf, 0xb8, 0xad,
0xaa, 0xa3, 0xa4, 0xf9, 0xfe, 0xf7, 0xf0, 0xe5, 0xe2,
0xeb, 0xec, 0xc1, 0xc6, 0xcf, 0xc8, 0xdd, 0xda, 0xd3,
0xd4, 0x69, 0x6e, 0x67, 0x60, 0x75, 0x72, 0x7b, 0x7c,
0x51, 0x56, 0x5f, 0x58, 0x4d, 0x4a, 0x43, 0x44, 0x19,
0x1e, 0x17, 0x10, 0x05, 0x02, 0x0b, 0x0c, 0x21, 0x26,
0x2f, 0x28, 0x3d, 0x3a, 0x33, 0x34, 0x4e, 0x49, 0x40,
0x47, 0x52, 0x55, 0x5c, 0x5b, 0x76, 0x71, 0x78, 0x7f,
0x6a, 0x6d, 0x64, 0x63, 0x3e, 0x39, 0x30, 0x37, 0x22,
0x25, 0x2c, 0x2b, 0x06, 0x01, 0x08, 0x0f, 0x1a, 0x1d,
0x14, 0x13, 0xae, 0xa9, 0xa0, 0xa7, 0xb2, 0xb5, 0xbc,
0xbb, 0x96, 0x91, 0x98, 0x9f, 0x8a, 0x8d, 0x84, 0x83,
0xde, 0xd9, 0xd0, 0xd7, 0xc2, 0xc5, 0xcc, 0xcb, 0xe6,
0xe1, 0xe8, 0xef, 0xfa, 0xfd, 0xf4, 0xf3 
};
unsigned char crc8_fun(const unsigned char* buf, unsigned int size)
{
    unsigned char result = 0x00;
    for (int i = 0; i < size; i++) {
        result = crc8tab[(result ^ (buf[i] & 0xFF)) & 0xFF];
    }
    return (unsigned char)(result & 0xFFL);
}

int bmpwidth, bmpheight, linebyte;
unsigned char* pBmpBuf;  //存储图像数据

bool readBmp(char* bmpName)
{
    FILE* fp;
    if ((fp = fopen(bmpName, "rb")) == NULL)  //以二进制的方式打开文件
    {
        my_sprintf("The file was not opened\r\n");
        return FALSE;
    }
    if (fseek(fp, sizeof(BITMAPFILEHEADER), 0))  //跳过BITMAPFILEHEADE
    {
        my_sprintf("jump errpr\r\n");
        return FALSE;
    }
    BITMAPINFOHEADER infoHead;
    fread(&infoHead, sizeof(BITMAPINFOHEADER), 1, fp);   //从fp中读取BITMAPINFOHEADER信息到infoHead中,同时fp的指针移动
    bmpwidth = infoHead.biWidth;
    bmpheight = infoHead.biHeight;
    linebyte = (bmpwidth * 24 / 8 + 3) / 4 * 4; //计算每行的字节数，24：该图片是24位的bmp图，3：确保不丢失像素

    //cout<<bmpwidth<<" "<<bmpheight<<endl;
    pBmpBuf = new unsigned char[linebyte * bmpheight];
    fread(pBmpBuf, sizeof(char), linebyte * bmpheight, fp);
    fclose(fp);   //关闭文件
    return TRUE;
}

unsigned char data_convert(unsigned char data)
{
    unsigned char data2 = 0, data3 = 0;
    data2 = data & 0x99;
    data3 = (data & 0x22) << 1;
    data2 |= data3;
    data3 = (data & 0x44) >> 1;
    data2 |= data3;
    data = data2;
    return data;
}
unsigned char Outfile_check(char* Outfilename)
{
    if (_access(Outfilename, 0) == 0) {//文件存在删除
        if (remove((char*)Outfilename) == 0) {
            my_sprintf("delete origin file succeed\r\n");
        }
        else {
            my_sprintf("delete origin file faile\r\n");
            return 0;
        }
    }
    return 1;
}
unsigned int saveRbw(char* Outfilename, unsigned char rotation, unsigned char ext_opt, unsigned char extract_mode)
{
    unsigned char j = 0, data = 0;
    unsigned int i = 0, k = 0,z=0;
    unsigned char color_red_white=0, color_white = 0, color_red=0, color_black=0;
    int  height = 0;

    my_sprintf("\r\n");
    my_sprintf("extract_mode:%d\r\n", extract_mode);

    if (Outfile_check(Outfilename) == 0)    return 0;

    FILE* fpWrite = fopen((char*)Outfilename, "w");
    if (fpWrite == NULL)
    {
        my_sprintf("create file error\r\n");
        return 0;
    }
    fprintf(fpWrite, "const unsigned char image_rbw[] = {\n");
    unsigned char r = 0, g = 0, b = 0;

    if (extract_mode == 1)
    {
        color_red_white = 2;
        color_white = 0;
        color_red = 1;
        color_black = 3;
    }
    else if(extract_mode == 2)
    {
        color_red_white = 0;
        color_white = 2;
        color_red = 1;
        color_black = 3;
    }
    else if (extract_mode == 3)
    {
        color_red_white = 1;
        color_white = 0;
        color_red = 2;
        color_black = 3;
    }
    else
    {
        color_red_white = 0;
        color_white = 1;
        color_red = 2;
        color_black = 3;
    }
    if (ext_opt == 2)
    {
        if ((bmpheight % 12) != 0)height = (bmpheight / 12 + 1) * 12;
        else height = bmpheight;
        my_sprintf("height_adj:%d\r\n", height);
    }
    else height = bmpheight;
    if ((ext_opt == 1) || (ext_opt == 2))
    {
        for (z = 0; z < bmpwidth; z += 2)
        {
            for (i = 0; i < height; i++)
            {
                //位图全部的像素，是按照自下向上，自左向右的顺序排列的。   RGB数据也是倒着念的，原始数据是按B、G、R的顺序排列的。
                if (rotation == 1)
                {
                    if (i >= bmpheight)
                    {
                        r = 0;
                        g = 0;
                        b = 0;
                    }
                    else
                    {
                        r = pBmpBuf[(bmpheight - i - 1) * linebyte + z * 3 + 2];
                        g = pBmpBuf[(bmpheight - i - 1) * linebyte + z * 3 + 1];
                        b = pBmpBuf[(bmpheight - i - 1) * linebyte + z * 3];
                    }
                }
                else
                {
                    if (i >= bmpheight)
                    {
                        r = 0;
                        g = 0;
                        b = 0;
                    }
                    else
                    {
                        r = pBmpBuf[i * linebyte + z * 3 + 2];
                        g = pBmpBuf[i * linebyte + z * 3 + 1];
                        b = pBmpBuf[i * linebyte + z * 3];
                    }
                }
                if ((b > 0x7f) && (g > 0x7f) && (r > 0x7f))data |= color_white << ((3 - j) * 2);
                else if ((b <= 0x7f) && (g <= 0x7f) && (r >= 0x7f))data |= color_red << ((3 - j) * 2);
                else if ((b <= 0x7f) && (g <= 0x7f) && (r <= 0x7f))data |= color_black << ((3 - j) * 2);
                else data |= color_red_white << ((3 - j) * 2);
                j++;
                //位图全部的像素，是按照自下向上，自左向右的顺序排列的。   RGB数据也是倒着念的，原始数据是按B、G、R的顺序排列的。
                if (rotation == 1)
                {
                    if (i >= bmpheight)
                    {
                        r = 0;
                        g = 0;
                        b = 0;
                    }
                    else
                    {
                        r = pBmpBuf[(bmpheight - i - 1) * linebyte + (z + 1) * 3 + 2];
                        g = pBmpBuf[(bmpheight - i - 1) * linebyte + (z + 1) * 3 + 1];
                        b = pBmpBuf[(bmpheight - i - 1) * linebyte + (z + 1) * 3];
                    }
                }
                else
                {
                    if (i >= bmpheight)
                    {
                        r = 0;
                        g = 0;
                        b = 0;
                    }
                    else
                    {
                        r = pBmpBuf[i * linebyte + (z + 1) * 3 + 2];
                        g = pBmpBuf[i * linebyte + (z + 1) * 3 + 1];
                        b = pBmpBuf[i * linebyte + (z + 1) * 3];
                    }
                }
                if ((b > 0x7f) && (g > 0x7f) && (r > 0x7f))data |= color_white << ((3 - j) * 2);
                else if ((b <= 0x7f) && (g <= 0x7f) && (r >= 0x7f))data |= color_red << ((3 - j) * 2);
                else if ((b <= 0x7f) && (g <= 0x7f) && (r <= 0x7f))data |= color_black << ((3 - j) * 2);
                else data |= color_red_white << ((3 - j) * 2);
                j++;

                if (j >= 4)
                {
                    data = data_convert(data);
                    fprintf(fpWrite, "0x%02X,", data);
                    k++;
                    if ((k % 16) == 0)fprintf(fpWrite, "\n");
                    j = 0;
                    data = 0;
                }
            }
        }
    }
    else
    {
        for (z = 0; z < height; z++)
        {
            for (i = 0; i < linebyte; i += 3)
            {
                //位图全部的像素，是按照自下向上，自左向右的顺序排列的。   RGB数据也是倒着念的，原始数据是按B、G、R的顺序排列的。
                if (rotation == 1)
                {
                    if (z >= bmpheight)
                    {
                        r = 0;
                        g = 0;
                        b = 0;
                    }
                    else
                    {
                        r = pBmpBuf[(bmpheight - z - 1) * linebyte + i + 2];
                        g = pBmpBuf[(bmpheight - z - 1) * linebyte + i + 1];
                        b = pBmpBuf[(bmpheight - z - 1) * linebyte + i];
                    }
                }
                else
                {
                    if (z >= bmpheight)
                    {
                        r = 0;
                        g = 0;
                        b = 0;
                    }
                    else
                    {
                        r = pBmpBuf[z * linebyte + i + 2];
                        g = pBmpBuf[z * linebyte + i + 1];
                        b = pBmpBuf[z * linebyte + i];
                    }
                }
                if ((b > 0x7f) && (g > 0x7f) && (r > 0x7f))data |= color_white << (j * 2);
                else if ((b <= 0x7f) && (g <= 0x7f) && (r >= 0x7f))data |= color_red << (j * 2);
                else if ((b <= 0x7f) && (g <= 0x7f) && (r <= 0x7f))data |= color_black << (j * 2);
                else data |= color_red_white << (j * 2);
                j++;
                if (j >= 4)
                {
                    fprintf(fpWrite, "0x%02X,", data);
                    j = 0;
                    data = 0;
                    k++;
                    if ((k % 16) == 0)fprintf(fpWrite, "\n");
                }
            }
        }
    }
    
    fprintf(fpWrite, "\n};");
    fclose(fpWrite);
    return k;
}
unsigned int saveBlackWhite(char* Outfilename, unsigned char rotation, unsigned char ext_opt)
{
    unsigned char j = 0, data = 0;
    unsigned int i = 0, k = 0, z = 0;
    int  height=0;

    if (Outfile_check(Outfilename) == 0)    return 0;

    FILE* fpWrite = fopen((char*)Outfilename, "w");
    if (fpWrite == NULL)
    {
        my_sprintf("create file error\r\n");
        return 0;
    }
    fprintf(fpWrite, "const unsigned char image_black_white[] = {\n");
    unsigned char r = 0, g = 0, b = 0,y;
    if (ext_opt == 1)
    {
        if ((bmpheight % 12) != 0)height = (bmpheight / 12 + 1) * 12;
        else height = bmpheight;
        my_sprintf("height_adj:%d\r\n", height);
    }
    else height = bmpheight;
    for (z = 0; z < bmpwidth; z += 2)
    {
        for (i = 0; i < height; i++)
        {
            //位图全部的像素，是按照自下向上，自左向右的顺序排列的。   RGB数据也是倒着念的，原始数据是按B、G、R的顺序排列的。
            if (rotation == 1)
            {
                if (i >= bmpheight)
                {
                    r = 0;
                    g = 0;
                    b = 0;
                }
                else
                {
                    r = pBmpBuf[(bmpheight - i - 1) * linebyte + z * 3 + 2];
                    g = pBmpBuf[(bmpheight - i - 1) * linebyte + z * 3 + 1];
                    b = pBmpBuf[(bmpheight - i - 1) * linebyte + z * 3];
                }
            }
            else
            {
                if (i>= bmpheight)
                {
                    r = 0;
                    g = 0;
                    b = 0;
                }
                else
                {
                    r = pBmpBuf[i * linebyte + z * 3 + 2];
                    g = pBmpBuf[i * linebyte + z * 3 + 1];
                    b = pBmpBuf[i * linebyte + z * 3];
                }
            }
            y = 0.299 * r + 0.587 * g + 0.114 * b;
            data = data << 1;
            if (y < 128)
                data |= 0x01;
            j++;
            //位图全部的像素，是按照自下向上，自左向右的顺序排列的。   RGB数据也是倒着念的，原始数据是按B、G、R的顺序排列的。
            if (rotation == 1)
            {
                if (i >= bmpheight)
                {
                    r = 0;
                    g = 0;
                    b = 0;
                }
                else
                {
                    r = pBmpBuf[(bmpheight - i - 1) * linebyte + (z + 1) * 3 + 2];
                    g = pBmpBuf[(bmpheight - i - 1) * linebyte + (z + 1) * 3 + 1];
                    b = pBmpBuf[(bmpheight - i - 1) * linebyte + (z + 1) * 3];
                }
            }
            else
            {
                if (i >= bmpheight)
                {
                    r = 0;
                    g = 0;
                    b = 0;
                }
                else
                {
                    r = pBmpBuf[i * linebyte + (z + 1) * 3 + 2];
                    g = pBmpBuf[i * linebyte + (z + 1) * 3 + 1];
                    b = pBmpBuf[i * linebyte + (z + 1) * 3];
                }
            }
            y = 0.299 * r + 0.587 * g + 0.114 * b;
            data = data << 1;
            if (y < 128)
                data |= 0x01;
            j++;

            if (j >= 8)
            {
                fprintf(fpWrite, "0x%02X,", data);
                k++;
                if ((k % 16) == 0)fprintf(fpWrite, "\n");
                j = 0;
                data = 0;
            }
        }
    }

    fprintf(fpWrite, "\n};");
    fclose(fpWrite);
    return k;
}
void data8Color(unsigned char color, FILE* fpWrite)
{
    static unsigned int k = 0;
    static char j = 7;
    static unsigned char  data = 0;
    if (color > 0x7f)  data |= (1 << j);
    j--;
    if (j < 0)
    {
        fprintf(fpWrite, "0x%02X,", data);
        k++;
        if ((k % 16) == 0)fprintf(fpWrite, "\n");
        j = 7;
        data = 0;
    }
}
unsigned int save8Color(char* Outfilename, unsigned char rotation, unsigned char ext_opt)
{
    unsigned int i = 0, z = 0;
    int  height = 0;

    if (Outfile_check(Outfilename) == 0)    return 0;

    FILE* fpWrite = fopen((char*)Outfilename, "w");
    if (fpWrite == NULL)
    {
        my_sprintf("create file error\r\n");
        return 0;
    }
    fprintf(fpWrite, "const unsigned char image_8Color[] = {\n");
    unsigned char r = 0, g = 0, b = 0, y;
    if (ext_opt == 1)
    {
        if ((bmpheight % 12) != 0)height = (bmpheight / 12 + 1) * 12;
        else height = bmpheight;
        my_sprintf("height_adj:%d\r\n", height);
    }
    else height = bmpheight;
    for (z = 0; z < bmpwidth; z += 2)
    {
        for (i = 0; i < height; i++)
        {
            //位图全部的像素，是按照自下向上，自左向右的顺序排列的。   RGB数据也是倒着念的，原始数据是按B、G、R的顺序排列的。
            if (rotation == 1)
            {
                if (i >= bmpheight)
                {
                    r = 0;
                    g = 0;
                    b = 0;
                }
                else
                {
                    r = pBmpBuf[(bmpheight - i - 1) * linebyte + z * 3 + 2];
                    g = pBmpBuf[(bmpheight - i - 1) * linebyte + z * 3 + 1];
                    b = pBmpBuf[(bmpheight - i - 1) * linebyte + z * 3];
                }
            }
            else
            {
                if (i >= bmpheight)
                {
                    r = 0;
                    g = 0;
                    b = 0;
                }
                else
                {
                    r = pBmpBuf[i * linebyte + z * 3 + 2];
                    g = pBmpBuf[i * linebyte + z * 3 + 1];
                    b = pBmpBuf[i * linebyte + z * 3];
                }
            }
            data8Color(b, fpWrite);
            data8Color(g, fpWrite);
            data8Color(r, fpWrite);
            //位图全部的像素，是按照自下向上，自左向右的顺序排列的。   RGB数据也是倒着念的，原始数据是按B、G、R的顺序排列的。
            if (rotation == 1)
            {
                if (i >= bmpheight)
                {
                    r = 0;
                    g = 0;
                    b = 0;
                }
                else
                {
                    r = pBmpBuf[(bmpheight - i - 1) * linebyte + (z + 1) * 3 + 2];
                    g = pBmpBuf[(bmpheight - i - 1) * linebyte + (z + 1) * 3 + 1];
                    b = pBmpBuf[(bmpheight - i - 1) * linebyte + (z + 1) * 3];
                }
            }
            else
            {
                if (i >= bmpheight)
                {
                    r = 0;
                    g = 0;
                    b = 0;
                }
                else
                {
                    r = pBmpBuf[i * linebyte + (z + 1) * 3 + 2];
                    g = pBmpBuf[i * linebyte + (z + 1) * 3 + 1];
                    b = pBmpBuf[i * linebyte + (z + 1) * 3];
                }
            }
            data8Color(b, fpWrite);
            data8Color(g, fpWrite);
            data8Color(r, fpWrite);
        }
    }

    fprintf(fpWrite, "\n};");
    fclose(fpWrite);
    return (bmpwidth* height*3/8);
}
unsigned int bmp_open_check(const unsigned char* filename, unsigned int size, const unsigned char* Outfilename, unsigned int Outfilesize, unsigned char rotation, unsigned char ext_opt)
{
    static HBITMAP hBitmap, hSysBitmap[5];
    my_sprintf("Intputfilename:");
    my_sprintf(filename, size);
    my_sprintf("\r\n");
    my_sprintf("Outfilename:");
    my_sprintf(Outfilename, Outfilesize);
    my_sprintf("\r\n");
    my_sprintf("rotation:%d ext_opt:%d\r\n", rotation, ext_opt);
    //struct stat buffer;
    struct _stat64 buffer;
    WCHAR uni_buf[MAX_PATH] = { 0 };
    int len = MultiByteToWideChar(CP_ACP, 0, (char*)filename, -1, NULL, 0);
    MultiByteToWideChar(CP_UTF8, 0, (char*)filename, -1, uni_buf, len);
    //if (stat((const char *)filename, &buffer) != 0)
    if (_wstat64(uni_buf, &buffer) != 0)
    {
        my_sprintf("file not exists\r\n");
        return 0;
    }
    my_sprintf("file size:%d\r\n", buffer.st_size);
    if (FALSE == readBmp((char*)filename))
        my_sprintf("readfile error!\r\n");
    my_sprintf("bmp info width:%d height:%d linebyte:%d\r\n", bmpwidth, bmpheight, linebyte);

    return 1;
}
unsigned int bmp_to_rbw(const unsigned char* filename, unsigned int size,const unsigned char* Outfilename, unsigned int Outfilesize, unsigned char rotation, unsigned char ext_opt, unsigned char extract_mode)
{
    unsigned int ret = 0;
    if (bmp_open_check(filename, size, Outfilename, Outfilesize, rotation, ext_opt)==0) return 0;

    ret = saveRbw((char*)Outfilename, rotation, ext_opt, extract_mode);
    if (ret == 0)
        my_sprintf("savefile error!\r\n");
    my_sprintf("save data total byte:%d\r\n", ret);
    return ret;
}
unsigned int bmp_to_BlackWhite(const unsigned char* filename, unsigned int size, const unsigned char* Outfilename, unsigned int Outfilesize, unsigned char rotation, unsigned char ext_opt)
{
    unsigned int ret = 0;
    if (bmp_open_check(filename, size, Outfilename, Outfilesize, rotation, ext_opt) == 0)   return 0;

    ret = saveBlackWhite((char*)Outfilename, rotation, ext_opt);
    if (ret == 0)
        my_sprintf("savefile error!\r\n");
    my_sprintf("save data total byte:%d\r\n", ret);
    return ret;
}
unsigned int bmp_to_8Color(const unsigned char* filename, unsigned int size, const unsigned char* Outfilename, unsigned int Outfilesize, unsigned char rotation, unsigned char ext_opt)
{
    unsigned int ret = 0;
    if (bmp_open_check(filename, size, Outfilename, Outfilesize, rotation, ext_opt) == 0)   return 0;

    ret = save8Color((char*)Outfilename, rotation, ext_opt);
    if (ret == 0)
        my_sprintf("savefile error!\r\n");
    my_sprintf("save data total byte:%d\r\n", ret);
    return ret;
}
//parm1,parm2只是在funtion==1的时候有用
unsigned int big_bin_handle(const unsigned char* filename, unsigned int size, const unsigned char* Outfilename, unsigned int Outfilesize, unsigned char funtion, unsigned char parm1, unsigned char parm2)
{
    unsigned int ret = 0,index=0;
    static HBITMAP hBitmap, hSysBitmap[5];
    my_sprintf("Intputfilename:");
    my_sprintf(filename, size);
    my_sprintf("\r\n");
    my_sprintf("Outfilename:");
    my_sprintf(Outfilename, Outfilesize);
    my_sprintf("\r\n");
    my_sprintf("funtion:%d parm1:%x parm2:%d\r\n", funtion,parm1,parm2);
    //struct stat buffer;
    struct _stat64 buffer;
    WCHAR uni_buf[MAX_PATH] = { 0 };
    int len = MultiByteToWideChar(CP_ACP, 0, (char*)filename, -1, NULL, 0);
    MultiByteToWideChar(CP_UTF8, 0, (char*)filename, -1, uni_buf, len);
    //if (stat((const char *)filename, &buffer) != 0)
    if (_wstat64(uni_buf, &buffer) != 0)
    {
        my_sprintf("file not exists\r\n");
        return 0;
    }

    FILE* in_file = fopen((char*)filename, "rb");
    if (!in_file) {
        my_sprintf("readfile error!\r\n");
        return 0;
    }

    struct stat sb;
    if (stat((char*)filename, &sb) == -1) {
        my_sprintf("file format error!\r\n");
        return 0;
    }
    unsigned char* file_contents = (unsigned char*)malloc(sb.st_size);
    fread(file_contents, sb.st_size, 1, in_file);
    for (ret = 0; ret < sb.st_size; ret++)
    {
        if (funtion == 0)
            file_contents[index++] = ~file_contents[ret];
        else if (funtion == 1)
        {
            if (file_contents[ret] == parm1)
            {
                file_contents[index++] = file_contents[ret + 1];
            }
        }
    }
    if (index > 0)
    {
        FILE* output_file = fopen((char*)Outfilename, "wb+");
        if (!output_file) {
            my_sprintf("fopen out file error!\r\n");
            free(file_contents);
            return 0;
        }

        fwrite(file_contents, 1, index, output_file);
        fclose(output_file);
    }
    
    my_sprintf("Done Writing sz:%d\n", index);
    fclose(in_file);
    free(file_contents);

    return index;
}
