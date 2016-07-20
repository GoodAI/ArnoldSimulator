#include "catch.hpp"

#include "data_utils.h"


SCENARIO("Convert byte vector to float vector", "[data-utils]")
{
    GIVEN("An empty byte vector")
    {
        std::vector<uint8_t> byteVec;
        std::vector<float> floatVec;

        WHEN("It is converted to float vector")
        {
            ConvertByteToFloatVector(byteVec, floatVec);
        }
        THEN("I get an empty float vector")
        {
            REQUIRE(floatVec.size() == 0);
        }

        WHEN("I push one float into the byte vector")
        {
            float nearPi = 3.14159f;
            PutFloatToByteVector(byteVec, nearPi);

            AND_WHEN("Converted to float vector")
            {
                ConvertByteToFloatVector(byteVec, floatVec);

                THEN("The resulting vector contains the original value")
                {
                    REQUIRE(floatVec.size() == 1);
                    REQUIRE(floatVec[0] == nearPi);
                }
            }
            AND_WHEN("I push an extra byte and then convert to float vector")
            {
                byteVec.push_back(1);

                ConvertByteToFloatVector(byteVec, floatVec);

                THEN("It converts the one value")
                {
                    REQUIRE(floatVec.size() == 1);
                    REQUIRE(floatVec[0] == nearPi);
                }
            }
        }

        WHEN("I push multiple floats into the byte vector and convert it")
        {
            PutFloatToByteVector(byteVec, 0.0f);
            PutFloatToByteVector(byteVec, 1.1f);
            PutFloatToByteVector(byteVec, 2.2f);

            ConvertByteToFloatVector(byteVec, floatVec);

            THEN("I get those original values")
            {
                REQUIRE(floatVec.size() == 3);
                REQUIRE(floatVec[0] == 0.0f);
                REQUIRE(floatVec[2] == 2.2f);
            }
        }
    }

    GIVEN("A misaligned byte buffer with some floats")
    {
        std::vector<uint8_t> byteVec;
        float nearPi = 3.14159f;

        PutFloatToByteVector(byteVec, nearPi);
        PutFloatToByteVector(byteVec, nearPi);

        std::unique_ptr<uint8_t[]> buffer(new uint8_t[byteVec.size() + 3]);

        uint8_t *misalignedBuffer = buffer.get() + 3;
        memcpy(misalignedBuffer, &byteVec[0], byteVec.size());
        
        WHEN("I convert it to float vector")
        {
            std::vector<float> floatVec;
            AssingBufferToFloatVector(misalignedBuffer, byteVec.size(), floatVec);
            
            THEN("I still get the correct values")
            {
                REQUIRE(floatVec.size() == 2);
                REQUIRE(floatVec[0] == nearPi);
                REQUIRE(floatVec[1] == nearPi);
            }
        }
    }
}
