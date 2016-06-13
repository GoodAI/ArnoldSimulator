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

/// Set log level for the current process.
void SetLogLevel(LogLevel level);

LogLevel GetLogLevel();

/// Don't use. Intended for the macro definition.
bool internal_ShouldPrintLogItem(LogLevel level);

/// Don't use. Intended for the macro definition.
void internal_WriteLogItemPrefix(std::ostringstream &stream, LogLevel level);

#define Log(level, format, ...) \
    do { \
        if (internal_ShouldPrintLogItem(level)) { \
            std::ostringstream stream; \
            internal_WriteLogItemPrefix(stream, (level)); \
            stream << (format); \
            CkPrintf(stream.str().c_str(), ##__VA_ARGS__); \
        } \
    } while(0)
