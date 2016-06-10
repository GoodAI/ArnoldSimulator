#include "catch.hpp"

#include "log.h"

TEST_CASE("Simple logging test", "[logging]")
{
    Log(LogLevel::Error, "Test error message #%d\n", 1);
    Log(LogLevel::Warn, "Test warning message #%d, %d, %s\n", 1, 2, "three");
    Log(LogLevel::Info, "Test info message\n");
    Log(LogLevel::Debug, "Test debug message\n");
    Log(LogLevel::Verbose, "Test verbose message\n");
}
