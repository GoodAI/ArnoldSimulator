#include "catch.hpp"

#include "common.h"
#include "core.h"
#include "region.h"

#include "core_tests.h"
#include "mnist_reader.h"

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

void fillImageStream(std::ostream &images)
{
    // Fill in the temporary file.
    uint32_t magicNumber = 0x00000803;
    uint32_t digitCount = 2;
    uint32_t rows = 2;
    uint32_t cols = 2;

    std::vector<uint32_t> data;
    data.reserve(6);

    data.push_back(swap_endian(magicNumber));
    data.push_back(swap_endian(digitCount));
    data.push_back(swap_endian(rows));
    data.push_back(swap_endian(cols));

    data.push_back(swap_endian(0x0000FFFF));
    data.push_back(swap_endian(0x00FFFF00));

    size_t dataSize = data.size() * sizeof(uint32_t);

    char* dataBytes = reinterpret_cast<char*>(data.data());
    images.write(dataBytes, dataSize);

    images.seekp(0);
}

TEST_CASE("MNISTReader reads digits", "[bodies]")
{
    GIVEN("A temporary MNIST image stream and an empty label stream")
    {
        std::stringstream images(std::stringstream::in | std::stringstream::out | std::stringstream::binary);

        fillImageStream(images);

        REQUIRE(images.str().size() == 24);

        std::stringstream labels;

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
                std::vector<uint8_t> digit1, digit2, digit3;
                size_t digitSize = reader.GetDigitSize();
                digit1.reserve(digitSize);
                digit2.reserve(digitSize);
                digit3.reserve(digitSize);

                bool success;
                success = reader.TryReadDigit(digit1.data());
                REQUIRE(success);
                REQUIRE(digit1[0] == 0);
                REQUIRE(digit1[1] == 0);
                REQUIRE(digit1[2] == (uint8_t)255);
                REQUIRE(digit1[3] == (uint8_t)255);
                success = reader.TryReadDigit(digit2.data());
                REQUIRE(success);
                REQUIRE(digit2[0] == 0);
                REQUIRE(digit2[1] == (uint8_t)255);
                REQUIRE(digit2[2] == (uint8_t)255);
                REQUIRE(digit2[3] == 0);
                success = reader.TryReadDigit(digit3.data());
                REQUIRE(success);

                for (int i = 0; i < reader.GetDigitSize(); i++) {
                    REQUIRE(digit1[i] == digit3[i]);
                }
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
