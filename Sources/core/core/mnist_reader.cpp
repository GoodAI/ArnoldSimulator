#include "mnist_reader.h"
#include "log.h"
#include "data_utils.h"

MnistReader::MnistReader()
    : mDigitCount(0), mRowCount(0), mColumnCount(0), mDigitSize(0), mNextDigitIndex(0)
{
}

MnistReader::~MnistReader()
{
    for (int i = 0; i < mDigitValues.size(); i++) {
        delete[] mDigitValues[i];
    }
}

void MnistReader::Load(std::istream &images, std::istream &labels)
{
    uint32_t buffer;
    char* bufferPtr = reinterpret_cast<char*>(&buffer);

    uint32_t imagesMagicNumber, labelsMagicNumber, labelsCount;

    // Read metadata of the images file.
    images.read(bufferPtr, 4);
    imagesMagicNumber = SwapEndian(buffer);

    images.read(bufferPtr, 4);
    mDigitCount = SwapEndian(buffer);

    images.read(bufferPtr, 4);
    mRowCount = SwapEndian(buffer);

    images.read(bufferPtr, 4);
    mColumnCount = SwapEndian(buffer);

    // Read metadata of the labels file.
    labels.read(bufferPtr, 4);
    labelsMagicNumber = SwapEndian(buffer);

    labels.read(bufferPtr, 4);
    labelsCount = SwapEndian(buffer);

    const uint32_t expectedImagesMagicNumber = 2051;
    if (imagesMagicNumber != expectedImagesMagicNumber) {
        Log(LogLevel::Error, "Images magic number %d doesn't match expected value %d",
            imagesMagicNumber, expectedImagesMagicNumber);
        return;
    }

    const uint32_t expectedLabelsMagicNumber = 2049;
    if (labelsMagicNumber != expectedLabelsMagicNumber) {
        Log(LogLevel::Error, "Labels magic number %d doesn't match expected value %d",
            labelsMagicNumber, expectedLabelsMagicNumber);
        return;
    }

    if (mDigitCount == 0) {
        Log(LogLevel::Warn, "No images are present, TryReadDigit will not provide a digit");
    }

    if (mDigitCount != labelsCount) {
        Log(LogLevel::Error, "Different number of images and labels. %d vs %d",
            mDigitCount, labelsCount);
        return;
    }

    mDigitSize = mRowCount * mColumnCount;

    mDigitValues.reserve(mDigitCount);
    mDigitLabels.reserve(mDigitCount);

    for(int i = 0; i < mDigitCount; i++) {
        uint8_t label;
        labels.read(reinterpret_cast<char*>(&label), 1);
        mDigitLabels.push_back(label);

        char *data = new char[mDigitSize];
        images.read(data, mDigitSize);
        mDigitValues.push_back(reinterpret_cast<uint8_t*>(data));
    }

    Log(LogLevel::Info, "MNIST file loaded. Image count: %d. Size: %dx%d",
        mDigitCount, mRowCount, mColumnCount);
}

bool MnistReader::TryReadDigit(uint8_t* dataPtr, uint8_t &label)
{
    if (mDigitCount == 0)
        return false;

    if (mNextDigitIndex >= mDigitValues.size())
        mNextDigitIndex = 0;

    memcpy(dataPtr, mDigitValues[mNextDigitIndex], mDigitSize);
    label = mDigitLabels[mNextDigitIndex];

    mNextDigitIndex++;

    return true;
}
