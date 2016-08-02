#pragma once

#include <vector>

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
