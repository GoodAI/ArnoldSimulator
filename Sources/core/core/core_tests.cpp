#include "catch.hpp"

#include "common.h"
#include "core.h"
#include "region.h"

#include "core_tests.h"
#include "mnist_reader.h"
#include "data_utils.h"

TEST_CASE("NeuronId packing is correct", "[common]")
{
    const NeuronIndex neuronIndex = 500000;
    const RegionIndex regionIndex = 400;

    REQUIRE(neuronIndex <= NEURON_INDEX_MAX);
    REQUIRE(regionIndex <= REGION_INDEX_MAX);

    NeuronId neuronId = GetNeuronId(regionIndex, neuronIndex);

    REQUIRE(GetNeuronIndex(neuronId) == neuronIndex);
    REQUIRE(GetRegionIndex(neuronId) == regionIndex);
}

bool fileExists(const std::string& fileName)
{
    std::ifstream stream(fileName);
    return stream.good();
}

const uint32_t DIGIT_1 = 0x0000FFFF;
const uint32_t DIGIT_2 = 0x00FFFF00;

const char LABEL_1 = 1;
const char LABEL_2 = 2;

void fillImageStream(std::ostream &images)
{
    // Fill in the temporary file.
    uint32_t magicNumber = 0x00000803;
    uint32_t digitCount = 2;
    uint32_t rows = 2;
    uint32_t cols = 2;

    std::vector<uint32_t> data;
    data.reserve(6);

    data.push_back(SwapEndian(magicNumber));
    data.push_back(SwapEndian(digitCount));
    data.push_back(SwapEndian(rows));
    data.push_back(SwapEndian(cols));

    data.push_back(SwapEndian(DIGIT_1));
    data.push_back(SwapEndian(DIGIT_2));

    size_t dataSize = data.size() * sizeof(uint32_t);

    char* dataBytes = reinterpret_cast<char*>(data.data());
    images.write(dataBytes, dataSize);

    images.seekp(0);
}

void fillLabelStream(std::ostream &labels)
{
    // Fill in the temporary file.
    uint32_t magicNumber = 0x00000801;
    uint32_t labelCount = 2;

    magicNumber = SwapEndian(magicNumber);
    labelCount = SwapEndian(labelCount);

    std::unique_ptr<uint8_t[]> dataPtr(new uint8_t[10]);
    uint8_t *data = dataPtr.get();

    std::memcpy(data, &magicNumber, 4);
    std::memcpy(data + 4, &labelCount, 4);
    data[8] = LABEL_1;
    data[9] = LABEL_2;

    labels.write(reinterpret_cast<char*>(data), 10);

    labels.seekp(0);
}

TEST_CASE("MNISTReader reads digits", "[bodies]")
{
    GIVEN("A temporary MNIST image stream and an empty label stream")
    {
        std::stringstream images(std::stringstream::in | std::stringstream::out | std::stringstream::binary);

        fillImageStream(images);

        REQUIRE(images.str().size() == 24);

        std::stringstream labels(std::stringstream::in | std::stringstream::out | std::stringstream::binary);

        fillLabelStream(labels);

        REQUIRE(labels.str().size() == 10);

        MnistReader reader;

        WHEN("The image stream is loaded")
        {
            reader.Load(images, labels);

            THEN("There are 2 digits loaded")
            {
                REQUIRE(reader.GetDigitCount() == 2);
            }
            AND_THEN("The digits are 2x2 'pixels'")
            {
                REQUIRE(reader.GetColumnCount() == 2);
                REQUIRE(reader.GetRowCount() == 2);
            }
            AND_THEN("Two digits can be retrieved")
            {
                uint8_t label1, label2;
                std::vector<uint8_t> digit1, digit2, digit3;
                size_t digitSize = reader.GetDigitSize();
                digit1.reserve(digitSize);
                digit2.reserve(digitSize);
                digit3.reserve(digitSize);

                bool success;
                success = reader.TryReadDigit(digit1.data(), label1);
                REQUIRE(success);

                uint32_t digitData1;
                std::memcpy(&digitData1, &digit1[0], 4);
                REQUIRE(SwapEndian(digitData1) == DIGIT_1);
                REQUIRE(label1 == LABEL_1);

                success = reader.TryReadDigit(digit2.data(), label2);
                REQUIRE(success);

                uint32_t digitData2;
                std::memcpy(&digitData2, &digit2[0], 4);
                REQUIRE(SwapEndian(digitData2) == DIGIT_2);
                REQUIRE(label2 == LABEL_2);
            }
            AND_THEN("The reader restarts when it reaches end")
            {
                uint8_t label1, label2, label3;
                std::vector<uint8_t> digit1, digit2, digit3;
                size_t digitSize = reader.GetDigitSize();
                digit1.reserve(digitSize);
                digit2.reserve(digitSize);
                digit3.reserve(digitSize);

                bool success;
                success = reader.TryReadDigit(digit1.data(), label1);
                REQUIRE(success);
                success = reader.TryReadDigit(digit2.data(), label2);
                REQUIRE(success);
                success = reader.TryReadDigit(digit3.data(), label3);
                REQUIRE(success);

                uint32_t digitData1;
                std::memcpy(&digitData1, &digit1[0], 4);
                uint32_t digitData3;
                std::memcpy(&digitData3, &digit3[0], 4);

                REQUIRE(digitData1 == digitData3);
                REQUIRE(label1 == label3);
            }
        }
    }
}

extern CProxy_RegionBase gRegions;

void SetupCharmTests()
{
    Box3D box;
    box.first = Point3D(0.0f, 0.0f, 0.0f);
    box.second = Size3D(BOX_DEFAULT_SIZE_X, BOX_DEFAULT_SIZE_Y, BOX_DEFAULT_SIZE_Z);

    gRegions[0].insert("Roger", "type", box, "{}");
    gRegions.doneInserting();
}

TEST_CASE("Region can foo", "[charm]")
{
    CProxyElement_RegionBase regionProxy = gRegions[0];

    Box3D box;
    box.first = Point3D(1.0f, 2.0f, 3.0f);
    box.second = Size3D(BOX_DEFAULT_SIZE_X, BOX_DEFAULT_SIZE_Y, BOX_DEFAULT_SIZE_Z);
    regionProxy.SetBox(box);

    RegionBase *region = regionProxy.ckLocal();

    REQUIRE(region != nullptr);
    REQUIRE(strcmp(region->GetName(), "Roger") == 0);
}
