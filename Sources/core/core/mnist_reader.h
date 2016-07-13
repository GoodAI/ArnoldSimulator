#pragma once
#include <fstream>
#include <vector>

template <typename T>
T swap_endian(T u)
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

class MnistReader
{
protected:
    uint32_t mDigitCount;
    uint32_t mRowCount;
    uint32_t mColumnCount;
    size_t mDigitSize;

    size_t mNextDigitIndex;

    std::vector<uint8_t*> mDigits;

public:
    MnistReader();
    ~MnistReader();
    void Load(std::istream &images, std::istream &labels);
    bool TryReadDigit(uint8_t* dataPtr);
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
