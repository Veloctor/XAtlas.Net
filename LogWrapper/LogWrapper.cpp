#include "LogWrapper.h"
#include "xatlas.h"
#include <cstdarg>
#include <cstdio>
#include <cstdlib>
//this dll doesn't work yet
void SetLogFunc(LogFunc func, bool verbose) {
	logFunc = func;
    xatlas::SetPrint(FormatToConsole, verbose);
}

static int FormatToConsole(const char* format, ...) {
    // 假设我们不知道最终字符串的具体长度，先尝试一个较小的缓冲区
    int size = 100; // 初始缓冲区大小
    char* buffer = (char*)malloc(size);
    if (buffer == NULL) return -1; // 内存分配失败

    va_list args;
    va_start(args, format);
    // 尝试将格式化的字符串写入缓冲区
    int nwritten = vsnprintf(buffer, size, format, args);
    if (nwritten < 0) {
        // 格式化错误
        va_end(args);
        free(buffer);
        return -1;
    }
    if (nwritten >= size) {
        // 缓冲区太小，需要更大的缓冲区
        size = nwritten + 1; // 需要的大小
        char* newBuffer = (char*)realloc(buffer, size);
        if (newBuffer == NULL) {
            free(buffer);
            va_end(args);
            return -1; // 内存重新分配失败
        }
        buffer = newBuffer;
        // 使用新的缓冲区大小重新尝试格式化
        va_start(args, format); // 重新初始化args
        nwritten = vsnprintf(buffer, size, format, args);
        if (nwritten < 0 || nwritten >= size) {
            // 如果还是失败，就放弃
            va_end(args);
            free(buffer);
            return -1;
        }
    }
    va_end(args);

    // 此时，buffer包含了格式化后的字符串
    // 根据需要处理buffer（例如，打印或返回给调用者）
    // 注意：调用者需要负责释放buffer
    logFunc(buffer); // 示例：打印到控制台

    free(buffer); // 如果不再需要，释放内存
    return nwritten; // 返回写入的字符数（不包括终止的'\0'）
}