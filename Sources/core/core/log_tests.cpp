#include "catch.hpp"

#include "log.h"

TEST_CASE("Simple logging test", "[logging]")
{
    LogLevel originalLogLevel = GetLogLevel();

    SECTION("Try logging all levels")
    {
        SetLogLevel(LogLevel::Verbose);
        REQUIRE(GetLogLevel() == LogLevel::Verbose);

        Log(LogLevel::Error, "Test error message #%d.", 1);
        Log(LogLevel::Warn, "Test warning message #%d, %d, %s.", 1, 2, "three");
        Log(LogLevel::Info, "Test info message.");
        Log(LogLevel::Debug, "Test debug message.");
        Log(LogLevel::Verbose, "Test verbose message. With extra empty line.\n");
    }

    SECTION("Set lower log level")
    {
        SetLogLevel(LogLevel::Warn);
        REQUIRE(GetLogLevel() == LogLevel::Warn);

        Log(LogLevel::Info, "This should not be printed.");
        Log(LogLevel::Error, "Should be printed.");
        Log(LogLevel::Warn, "Should be printed.");
    }

    SetLogLevel(originalLogLevel);
}
