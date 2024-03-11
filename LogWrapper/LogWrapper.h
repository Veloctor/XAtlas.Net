#pragma once

// Unity native plugin API
#if defined(__CYGWIN32__)
#define EXPORT extern "C" __declspec(dllexport)
#elif defined(WIN32) || defined(_WIN32) || defined(__WIN32__) || defined(_WIN64) || defined(WINAPI_FAMILY)
#define EXPORT extern "C" __declspec(dllexport)
#elif defined(__MACH__) || defined(__ANDROID__) || defined(__linux__) || defined(LUMIN)
#define EXPORT __attribute__ ((visibility ("default")))
#else
#define EXPORT
#endif

typedef void (*StringFunc)(const char* formatted, int strLen);

EXPORT int SetPrintFormatted(StringFunc func, bool verbose);
