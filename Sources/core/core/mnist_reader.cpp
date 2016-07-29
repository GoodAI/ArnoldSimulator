#include "mnist_reader.h"
#include "log.h"

MnistReader::MnistReader()
    : mDigitCount(0), mRowCount(0), mColumnCount(0), mDigitSize(0), mNextDigitIndex(0)
{
}

MnistReader::~MnistReader()
{
    for (int i = 0; i < mDigits.size(); i++) {
        delete[] mDigits[i];
    }
}

void MnistReader::Load(std::istream &images, std::istream &labels)
{
    uint32_t buffer;
    char* bufferPtr = reinterpret_cast<char*>(&buffer);

    uint32_t magicNumber;
    images.read(bufferPtr, 4);
    magicNumber = swap_endian(buffer);

    images.read(bufferPtr, 4);
    mDigitCount = swap_endian(buffer);

    images.read(bufferPtr, 4);
    mRowCount = swap_endian(buffer);

    images.read(bufferPtr, 4);
    mColumnCount = swap_endian(buffer);

    const uint32_t expectedMagicNumber = 2051;
    if (magicNumber != expectedMagicNumber) {
        Log(LogLevel::Error, "Magic number %d doesn't match expected value %d",
            magicNumber, expectedMagicNumber);
        return;
    }

    if (mDigitCount == 0) {
        Log(LogLevel::Warn, "No images are present, TryReadDigit will not provide a digit");
    }

    mDigitSize = mRowCount * mColumnCount;

    mDigits.reserve(mDigitCount);

    size_t count = mDigitCount;
    for(int i = 0; i < mDigitCount; i++) {
        char* data = new char[mDigitSize];
        images.read(data, mDigitSize);
        mDigits.push_back(reinterpret_cast<uint8_t*>(data));
    }

    Log(LogLevel::Info, "MNIST file loaded. Image count: %d. Size: %dx%d",
        mDigitCount, mRowCount, mColumnCount);
}

bool MnistReader::TryReadDigit(uint8_t* dataPtr)
{
    if (mDigitCount == 0)
        return false;

    if (mNextDigitIndex >= mDigits.size())
        mNextDigitIndex = 0;

    std::memcpy(dataPtr, mDigits[mNextDigitIndex], mDigitSize);

    mNextDigitIndex++;

    return true;
}
