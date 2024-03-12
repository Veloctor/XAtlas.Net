#include <cstdarg>
#include <cstdio>
#include <vector>
#define WIN32_LEAN_AND_MEAN
#define NOMINMAX
#include <Windows.h>
#include "LogWrapper.h"

StringFunc logFunc = nullptr;

const auto dllName = L"xatlas";
const auto xatlasSetPrintName = "xatlasSetPrint";

// xatlasPrintFunc的定义
typedef int (*xatlasPrintFunc)(const char*, ...);
// xatlasSetPrint函数指针类型的定义
typedef void (*xatlasSetPrintAction)(xatlasPrintFunc print, bool verbose);

static xatlasSetPrintAction resolveXAtlasPrintAction()
{
    HMODULE hModule = LoadLibrary(dllName);
    if (hModule == NULL)// 处理加载DLL失败的情况
        return NULL;
    auto pfnxatlasSetPrint = (xatlasSetPrintAction)GetProcAddress(hModule, xatlasSetPrintName);
    if (pfnxatlasSetPrint == NULL) {// 处理获取函数地址失败的情况
        FreeLibrary(hModule);
        return NULL;
    }
    return pfnxatlasSetPrint;
}

static int FormatAndLog(const char* format, ...) {
    va_list args;
    va_start(args, format);
    // 获取格式化字符串所需的长度
    int len = std::vsnprintf(nullptr, 0, format, args);
    va_end(args);

    // 创建足够大的字符串缓冲区
    std::vector<char> vec(len + 1);
    va_start(args, format);
    std::vsnprintf(vec.data(), vec.size(), format, args);
    va_end(args);

    //call the export log function with formatted string
    logFunc(vec.data(), len);
    return len; // 返回写入的字符数（不包括终止的'\0'）
}

int SetPrintFormatted(StringFunc func, bool verbose) {
    if (func == nullptr)
        return -1;
	logFunc = func;
    FormatAndLog("setting message log function 0x%p\n", func);
    auto xatSetPrint = resolveXAtlasPrintAction();
    if (xatSetPrint == nullptr) {
        FormatAndLog("failed to resolve xatlasSetPrint from xatlas.dll, LastError: %d\n", GetLastError());
        return -3;
    }
    xatSetPrint(&FormatAndLog, verbose);
    return 0;
}
