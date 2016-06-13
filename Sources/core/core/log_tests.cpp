#include "catch.hpp"

#include "log.h"

TEST_CASE("Simple logging test", "[logging]")
{
    LogLevel originalLogLevel = GetLogLevel();

    SECTION("Try logging all levels")
    {
        SetLogLevel(LogLevel::Verbose);
        REQUIRE(GetLogLevel() == LogLevel::Verbose);

        Log(LogLevel::Error, "Test error message #%d.\n", 1);
        Log(LogLevel::Warn, "Test warning message #%d, %d, %s.\n", 1, 2, "three");
        Log(LogLevel::Info, "Test info message.\n");
        Log(LogLevel::Debug, "Test debug message.\n");
        Log(LogLevel::Verbose, "Test verbose message. With extra empty line.\n\n");
    }
    SECTION("Set lower log level")
    {
        SetLogLevel(LogLevel::Warn);
        REQUIRE(GetLogLevel() == LogLevel::Warn);

        Log(LogLevel::Info, "This should not be prited.\n");
        Log(LogLevel::Error, "Should be prited.\n");
        Log(LogLevel::Warn, "Should be prited.\n");
    }

    SetLogLevel(originalLogLevel);
}
