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

/**
 * @brief ll_crc24_generate()
 *
 * @param[in] seed
 * @param[in] pdata
 * @param[in] len
 *
 * @return
 *
 * @example:
 * ADV: {0x00,0x0d,0x08,0xac,0xd9,0xc9,0x84,0xf0,0x02,0x01,0x02,0x03,0x09,0x68,0x79}
 * seed: 0x555555
 * crc24: 0x53f51b
 **/
uint32_t ll_crc24_generate(uint32_t seed,uint8_t* ppdata, unsigned len)
{
    unsigned i, j;
    for (i = 0; i < len; ++i)
    {
        for (j = 0; j < 8; ++j)
        {
            uint8_t factor = ((ppdata[i] >> j) ^ (seed >> 23)) & 0x01;

            seed <<= 1;
            seed = (seed & ~(1 << 10)) | ((seed ^ (factor << 10)) & (1 << 10));
            seed = (seed & ~(1 << 9)) | ((seed ^ (factor << 9)) & (1 << 9));
            seed = (seed & ~(1 << 6)) | ((seed ^ (factor << 6)) & (1 << 6));
            seed = (seed & ~(1 << 4)) | ((seed ^ (factor << 4)) & (1 << 4));
            seed = (seed & ~(1 << 3)) | ((seed ^ (factor << 3)) & (1 << 3));
            seed = (seed & ~(1 << 1)) | ((seed ^ (factor << 1)) & (1 << 1));
            seed = (seed & ~(1 << 0)) | factor;
        }
    }
    return seed & 0x00FFFFFF;
}


uint8_t reverseBits(uint8_t data)
{
    int i;
    int r = 0;
    for (i = 0; i < 8; i++)
    {
        r |= (data & 1);
        data >>= 1;
        if (i != (8 - 1))
        {
            r <<= 1;
        }
    }
    return r;
}
/*白化，LFSR的方式进行白化*/
void bleWhiten(uint8_t chan,uint8_t* data, uint8_t start_index, uint8_t len) {
    // Implementing whitening with LFSR
    uint8_t  m;
    //最高位一定要是1
    uint8_t whitenCoeff= reverseBits(chan) | 2;
    //uint8_t whitenCoeff = 0XA6;
    data += start_index;
    //while (len--) {
     for(int i= start_index;i< len+ start_index;i++)
    {
        for (m = 1; m; m <<= 1) {
            if (whitenCoeff & 0x80) {
                whitenCoeff ^= 0x11;
                (*data) ^= m;
            }
            whitenCoeff <<= 1;
        }
        data++;
    }
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
unsigned int saveRbw(char* Outfilename, unsigned char rotation, unsigned char ext_opt)
{
    unsigned char j = 0, data = 0;
    unsigned int i = 0, k = 0,z=0;
    if (_access(Outfilename, 0) == 0) {//文件存在删除
        if (remove((char*)Outfilename) == 0) {
            my_sprintf("delete origin file succeed\r\n");
        }
        else {
            my_sprintf("delete origin file faile\r\n");
            return 0;
        }
    }
    FILE* fpWrite = fopen((char*)Outfilename, "w");
    if (fpWrite == NULL)
    {
        my_sprintf("create file error\r\n");
        return 0;
    }
    fprintf(fpWrite, "const unsigned char image_rbw[] = {\n");
    unsigned char r = 0, g = 0, b = 0;
    if (ext_opt == 1)
    {
        #define COLOR_RED_White1    2
        #define COLOR_White1 		0
        #define COLOR_RED1			1
        #define COLOR_BLACK1 		3
        for (z = 0; z < bmpwidth; z += 2)
        {
            for (i = 0; i < bmpheight; i++)
            {
                //位图全部的像素，是按照自下向上，自左向右的顺序排列的。   RGB数据也是倒着念的，原始数据是按B、G、R的顺序排列的。
                if (rotation == 1)
                {
                    r = pBmpBuf[(bmpheight - i - 1) * linebyte + z * 3 + 2];
                    g = pBmpBuf[(bmpheight - i - 1) * linebyte + z * 3 + 1];
                    b = pBmpBuf[(bmpheight - i - 1) * linebyte + z * 3];
                }
                else
                {
                    r = pBmpBuf[i * linebyte + z * 3 + 2];
                    g = pBmpBuf[i * linebyte + z * 3 + 1];
                    b = pBmpBuf[i * linebyte + z * 3];
                }
                if ((b > 0x7f) && (g > 0x7f) && (r > 0x7f))data |= COLOR_White1 << ((3-j) * 2);
                else if ((b <= 0x7f) && (g <= 0x7f) && (r >= 0x7f))data |= COLOR_RED1 << ((3 - j) * 2);
                else if ((b <= 0x7f) && (g <= 0x7f) && (r <= 0x7f))data |= COLOR_BLACK1 << ((3 - j) * 2);
                else data |= COLOR_RED_White1 << ((3 - j) * 2);
                j++;
                //位图全部的像素，是按照自下向上，自左向右的顺序排列的。   RGB数据也是倒着念的，原始数据是按B、G、R的顺序排列的。
                if (rotation == 1)
                {
                    r = pBmpBuf[(bmpheight - i - 1) * linebyte + (z + 1) * 3 + 2];
                    g = pBmpBuf[(bmpheight - i - 1) * linebyte + (z + 1) * 3 + 1];
                    b = pBmpBuf[(bmpheight - i - 1) * linebyte + (z + 1) * 3];
                }
                else
                {
                    r = pBmpBuf[i * linebyte + (z + 1) * 3 + 2];
                    g = pBmpBuf[i * linebyte + (z + 1) * 3 + 1];
                    b = pBmpBuf[i * linebyte + (z + 1) * 3];
                }
                if ((b > 0x7f) && (g > 0x7f) && (r > 0x7f))data |= COLOR_White1 << ((3 - j) * 2);
                else if ((b <= 0x7f) && (g <= 0x7f) && (r >= 0x7f))data |= COLOR_RED1 << ((3 - j) * 2);
                else if ((b <= 0x7f) && (g <= 0x7f) && (r <= 0x7f))data |= COLOR_BLACK1 << ((3 - j) * 2);
                else data |= COLOR_RED_White1 << ((3 - j) * 2);
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
        #define COLOR_RED_AND_White 0
        #define COLOR_White 		1
        #define COLOR_RED			2
        #define COLOR_BLACK 		3
        for (z = 0; z < bmpheight; z++)
        {
            for (i = 0; i < linebyte; i += 3)
            {
                //位图全部的像素，是按照自下向上，自左向右的顺序排列的。   RGB数据也是倒着念的，原始数据是按B、G、R的顺序排列的。
                if (rotation == 1)
                {
                    r = pBmpBuf[(bmpheight - z - 1) * linebyte + i + 2];
                    g = pBmpBuf[(bmpheight - z - 1) * linebyte + i + 1];
                    b = pBmpBuf[(bmpheight - z - 1) * linebyte + i];
                }
                else
                {
                    r = pBmpBuf[z * linebyte + i + 2];
                    g = pBmpBuf[z * linebyte + i + 1];
                    b = pBmpBuf[z * linebyte + i];
                }
                if ((b > 0x7f) && (g > 0x7f) && (r > 0x7f))data |= COLOR_White << (j * 2);
                else if ((b <= 0x7f) && (g <= 0x7f) && (r >= 0x7f))data |= COLOR_RED << (j * 2);
                else if ((b <= 0x7f) && (g <= 0x7f) && (r <= 0x7f))data |= COLOR_BLACK << (j * 2);
                else data |= COLOR_RED_AND_White << (j * 2);
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
unsigned int bmp_to_rbw(const unsigned char* filename, unsigned int size,const unsigned char* Outfilename, unsigned int Outfilesize, unsigned char rotation, unsigned char ext_opt)
{
    unsigned int ret = 0;
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
    int len = MultiByteToWideChar(CP_ACP, 0, (char *)filename, -1, NULL, 0);
    MultiByteToWideChar(CP_UTF8, 0, (char*)filename, -1, uni_buf, len);
    //if (stat((const char *)filename, &buffer) != 0)
    if (_wstat64(uni_buf, &buffer) != 0)
    {
        my_sprintf("file not exists\r\n");
        return 0;
    }
    my_sprintf("file size:%d\r\n", buffer.st_size);
    if (FALSE == readBmp((char *)filename))
        my_sprintf("readfile error!\r\n");
    my_sprintf("bmp info width:%d height:%d linebyte:%d\r\n", bmpwidth, bmpheight, linebyte);

    ret = saveRbw((char*)Outfilename, rotation, ext_opt);
    if (ret == 0)
        my_sprintf("savefile error!\r\n");
    my_sprintf("save data total byte:%d\r\n", ret);
    return ret;
}
unsigned int big_bin_handle(const unsigned char* filename, unsigned int size, const unsigned char* Outfilename, unsigned int Outfilesize, unsigned char funtion)
{
    unsigned int ret = 0;
    static HBITMAP hBitmap, hSysBitmap[5];
    my_sprintf("Intputfilename:");
    my_sprintf(filename, size);
    my_sprintf("\r\n");
    my_sprintf("Outfilename:");
    my_sprintf(Outfilename, Outfilesize);
    my_sprintf("\r\n");
    my_sprintf("funtion:%d\r\n", funtion);
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

    char* file_contents = (char*)malloc(sb.st_size);
    fread(file_contents, sb.st_size, 1, in_file);

    for (ret = 0; ret < sb.st_size; ret++)
    {
        if (funtion == 0)
        {
            file_contents[ret] = ~file_contents[ret];
        }
    }

    FILE* output_file = fopen((char*)Outfilename, "wb+");
    if (!output_file) {
        my_sprintf("fopen out file error!\r\n");
        return 0;
    }

    fwrite(file_contents, 1, sb.st_size, output_file);
    my_sprintf("Done Writing!\n");
    fclose(output_file);

    fclose(in_file);
    free(file_contents);

    return ret;
}
