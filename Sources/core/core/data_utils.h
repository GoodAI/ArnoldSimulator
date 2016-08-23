#pragma once

/// @file

#include <vector>

#include "log.h"

void PutFloatToByteVector(std::vector<uint8_t> &targetByteVector, float item);

void ConvertByteToFloatVector(std::vector<uint8_t> &byteVector, std::vector<float> &targetFloatVector);

// Export only for tests.
void AssingBufferToFloatVector(uint8_t *dataBuffer, size_t dataSize, std::vector<float> &targetFloatVector);

template <typename T>
T SwapEndian(T u)
{
    static_assert (CHAR_BIT == 8, "CHAR_BIT != 8");

    union
    {
        T u;
        unsigned char u8[sizeof(T)];
    } source, dest;

    source.u = u;

    for (size_t k = 0; k < sizeof(T); k++)
        dest.u8[k] = source.u8[sizeof(T) - k - 1];

    return dest.u;
}

/// @brief A memcpy wrapper that checks buffer size and logs errors
/// 
/// @param dataSize is the number of bytes to be copied from destination to source.
/// @param bufferSize is a buffer size that you want to check for being greater or equal to the data size.
///     If the buffer is smaller, the copying is skipped and a warning is logged.
///     (bufferSize may refer to source or destination depending on what you do.)
/// @param funcName is just for logging purposes (it is recommended to pass the __func__ macro in there)
inline void CheckedMemCopy(void *destination, const void *source, size_t dataSize, size_t bufferSize, const char *funcName)
{
    if (bufferSize < dataSize) {
        Log(LogLevel::Warn, "%s: Buffer too small (%d < %d).", funcName, bufferSize, dataSize);
        return;
    }

    std::memcpy(destination, source, dataSize);
}
