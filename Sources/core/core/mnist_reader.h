#pragma once
#include <fstream>
#include <vector>
#include <memory>

class MnistReader
{
protected:
    uint32_t mDigitCount;
    uint32_t mRowCount;
    uint32_t mColumnCount;
    size_t mDigitSize;

    size_t mNextDigitIndex;

    std::vector<uint8_t*> mDigitValues;
    std::vector<uint8_t> mDigitLabels;

public:
    MnistReader();
    ~MnistReader();
    void Load(std::istream &images, std::istream &labels);
    bool TryReadDigit(uint8_t* dataPtr, uint8_t &label);
    inline size_t GetDigitCount()
    {
        return mDigitCount;
    }

    inline size_t GetDigitSize()
    {
        return mDigitSize;
    }

    inline size_t GetRowCount()
    {
        return mRowCount;
    }

    inline size_t GetColumnCount()
    {
        return mColumnCount;
    }
};
