#pragma once

#include <vector>

void PutFloatToByteVector(std::vector<uint8_t> &targetByteVector, float item);

void ConvertByteToFloatVector(std::vector<uint8_t> &byteVector, std::vector<float> &targetFloatVector);

// Export only for tests.
void AssingBufferToFloatVector(uint8_t *dataBuffer, size_t dataSize, std::vector<float> &targetFloatVector);