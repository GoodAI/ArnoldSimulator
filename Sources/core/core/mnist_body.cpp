#include "mnist_body.h"
#include "log.h"

const char *MnistBody::Type = "MNISTBinaryBody";

MnistBody::MnistBody(json &params) : Body(params)
{
    if (params.count("imagesFile") == 0) {
        Log(LogLevel::Error, "MNIST file with images not specified");
        return;
    }

    std::string imagesFileName = params["imagesFile"].get<std::string>();
    std::string labelsFileName = params["labelsFile"].get<std::string>();

    std::ifstream images(imagesFileName, std::ios::binary);
    std::ifstream labels(labelsFileName, std::ios::binary);

    if (!images.good()) {
        Log(LogLevel::Error, "Images file not found at: %s", imagesFileName.c_str());
        return;
    }

    if (!labels.good()) {
        Log(LogLevel::Error, "Labels file not found at: %s", labelsFileName.c_str());
        return;
    }

    mDigitReader.Load(images, labels);

    size_t sensorNeuronCount = mSensorsInfo["Digit"].second;
    size_t digitSize = mDigitReader.GetDigitSize();
    if (digitSize != sensorNeuronCount) {
        Log(LogLevel::Error, "Sensor neuron count (%d) is different from the MNIST digit size (%d)", sensorNeuronCount, digitSize);
        return;
    }
}

MnistBody::~MnistBody()
{
}

void MnistBody::pup(PUP::er &p)
{
    Body::pup(p);
}

const char *MnistBody::GetType()
{
    return Type;
}

void MnistBody::Simulate(
    size_t bodyStep,
    std::function<void(const std::string &, std::vector<uint8_t> &)> pushSensoMotoricData,
    std::function<void(const std::string &, std::vector<uint8_t> &)> pullSensoMotoricData)
{
    const std::string actuatorName("NextDigit");

    ExpectedDataInfo nextDigitInfo = mActuatorsInfo[actuatorName];
    size_t nextDigitElemSize = nextDigitInfo.first;
    size_t nextDigitElemCount = nextDigitInfo.second;

    std::vector<uint8_t> actuatorData;
    pullSensoMotoricData(actuatorName, actuatorData);

    // Check if a digit should be shown.
    bool firstStep = bodyStep == 1;
    bool isActuatorDataPresent = actuatorData.size() == nextDigitElemSize * nextDigitElemCount;
    bool nextDigitRequested = isActuatorDataPresent && actuatorData[0] == 1;
    bool nextDigit = firstStep || nextDigitRequested;

    if (!nextDigit)
        return;

    const std::string sensorName("Digit");
    ExpectedDataInfo digitInfo = mSensorsInfo[sensorName];
    size_t digitElemCount = digitInfo.second;

    std::vector<uint8_t> sensorData;
    sensorData.resize(digitElemCount);

    uint8_t* dataPtr = sensorData.data();
    if (!mDigitReader.TryReadDigit(dataPtr)) {
        Log(LogLevel::Warn, "There are no digits available in the images file");
        return;
    }

    for (int y = 0; y < 28; y++) {
        for (int x = 0; x < 28; x++) {
            CkPrintf("%03d ", sensorData[y * 28 + x]);
        }
        CkPrintf("\n");
    }

    pushSensoMotoricData(sensorName, sensorData);
}
