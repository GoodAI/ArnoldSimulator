#pragma once

#include <sstream>

#include <charm.h>

enum class LogLevel
{
    Error,
    Warn,
    Info,
    Debug,
    Verbose
};

bool ShouldPrintLogItem(LogLevel level);

void WriteLogItemPrefix(std::ostringstream &stream, LogLevel level);

//void Log(LogLevel level, const char * format, ...);

#define Log(level, format, ...) \
    do { \
        if (ShouldPrintLogItem(level)) { \
            std::ostringstream stream; \
            WriteLogItemPrefix(stream, (level)); \
            stream << (format); \
            CkPrintf(stream.str().c_str(), ##__VA_ARGS__); \
        } \
    } while(0)
