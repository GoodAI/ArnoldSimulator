#include <iomanip>
#include <chrono>

#include "log.h"

bool ShouldPrintLogItem(LogLevel level)
{
    // TODO(Premek): Implement the check;
    return true;
}

static void WriteLogLevel(std::ostringstream &stream, LogLevel level)
{
    switch (level) {
        case LogLevel::Error: { stream << "Err!"; break; }
        case LogLevel::Warn: { stream << "Warn"; break; }
        case LogLevel::Info: { stream << "Info"; break; }
        case LogLevel::Debug: { stream << "Debg"; break; }
        case LogLevel::Verbose: { stream << "Verb"; break; }
        default: { stream << "????"; break; }
    }

    stream << ": ";
}

static void WriteLogTimestamp(std::ostringstream &stream)
{
    std::chrono::system_clock::time_point nowTimePoint = std::chrono::system_clock::now();
    time_t nowTimeT = std::chrono::system_clock::to_time_t(nowTimePoint);
    tm *localTime = localtime(&nowTimeT);
    
    // NOTE: this prints one hour less even if localTime->tm_isdst == 1
    stream << std::put_time(localTime, "%F %H:%M:%S");

    std::chrono::system_clock::time_point timeRoundedToSeconds = std::chrono::system_clock::from_time_t(mktime(localTime));

    long long milliseconds = std::chrono::duration_cast<std::chrono::milliseconds>(
        nowTimePoint - timeRoundedToSeconds).count();

    stream << '.' << std::setfill('0') << std::setw(3) << milliseconds << ' ';
}

void WriteLogItemPrefix(std::ostringstream &stream, LogLevel level)
{
    WriteLogLevel(stream, level);

    WriteLogTimestamp(stream);
}
