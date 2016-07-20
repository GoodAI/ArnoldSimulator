#include "log.h"

#include <memory>

#include "data_utils.h"

void PutFloatToByteVector(std::vector<uint8_t> &targetByteVector, float item)
{
        uint8_t *bytes = reinterpret_cast<uint8_t *>(&item);

        for (int i = 0; i < sizeof(float); ++i)
            targetByteVector.push_back(bytes[i]);
}

void AssingBufferToFloatVector(uint8_t *dataBuffer, size_t dataSize, std::vector<float> &targetFloatVector)
{
    auto floatDataSize = dataSize / sizeof(float);

    if (dataSize != floatDataSize * sizeof(float)) {
        Log(LogLevel::Warn, "Observer data size not multiple of sizeof(float). Truncating.");
    }

    if (reinterpret_cast<uintptr_t>(dataBuffer) % sizeof(float) == 0) {  // Check alignment.
        float * floatDataBuffer = reinterpret_cast<float*>(dataBuffer);
        targetFloatVector.assign(floatDataBuffer, floatDataBuffer + floatDataSize);
    } else {
        Log(LogLevel::Warn, "Observer float data not alligned. Extra alloc & copy needed.");  // I doubt this ever happens.
        std::unique_ptr<float[]> tempFloatBuffer(new float[floatDataSize]);  // NOTE: Allowing clients to provide a buffer would enable its reuse.
        memcpy(tempFloatBuffer.get(), dataBuffer, floatDataSize * sizeof(float));
        targetFloatVector.assign(tempFloatBuffer.get(), tempFloatBuffer.get() + floatDataSize);
    }
}

void ConvertByteToFloatVector(std::vector<uint8_t> &byteVector, std::vector<float> &targetFloatVector) 
{
    AssingBufferToFloatVector(&byteVector[0], byteVector.size(), targetFloatVector);
}

