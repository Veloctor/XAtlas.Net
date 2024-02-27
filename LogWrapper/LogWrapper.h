#pragma once

// Unity native plugin API
// Compatible with C99

#if defined(__CYGWIN32__)
#define EXPORT __declspec(dllexport)
#elif defined(WIN32) || defined(_WIN32) || defined(__WIN32__) || defined(_WIN64) || defined(WINAPI_FAMILY)
#define EXPORT __declspec(dllexport)
#elif defined(__MACH__) || defined(__ANDROID__) || defined(__linux__) || defined(LUMIN)
#define EXPORT __attribute__ ((visibility ("default")))
#else
#define EXPORT
#endif

extern "C" {
	typedef void (*LogFunc)(const char* content);

	LogFunc logFunc;

	EXPORT void SetLogFunc(LogFunc func, bool verbose);
}