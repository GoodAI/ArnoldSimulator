#include "gen_spec_region.h"
#include "random.h"
#include "log.h"
#include "components.h"

namespace GenSpecModel
{

const char *GenSpecRegion::Type = "GenSpecRegion";

GenSpecRegion::GenSpecRegion(RegionBase &base, json &params) : Region(base, params),
    mBrainStepCounter(0), mBrainStepsPerEvolution(10), mSpecialistCount(1),
    mSpecializingGeneralistCount(0)
{
    if (params.find("brainStepsPerEvolution") != params.end()) {
        mBrainStepsPerEvolution = params["brainStepsPerEvolution"].get<size_t>();
    }

    if (params.find("specializingGeneralistCount") != params.end()) {
        mSpecializingGeneralistCount = params["specializingGeneralistCount"].get<size_t>();
    }

    if (params.find("specialistCount") != params.end()) {
        mSpecialistCount = params["specialistCount"].get<size_t>();
    }

    if (params.find("layerCountLimit") != params.end()) {
        mLayerCountLimit = params["layerCountLimit"].get<size_t>();
    }

    json generalists = params["generalists"];

    size_t inputSizeX, inputSizeY;
    mNeuronCountX = generalists["neuronCountX"].get<size_t>();
    mNeuronCountY = generalists["neuronCountY"].get<size_t>();
    inputSizeX = generalists["inputSizeX"].get<size_t>();
    inputSizeY = generalists["inputSizeY"].get<size_t>();
    mInputSize = inputSizeX * inputSizeY;

    mNeuronParams = generalists["neuronParams"];

    NeuronId inputNeuron = mBase.GetInput("Input").neurons[0];
    NeuronId outputNeuron = mBase.GetOutput("Output").neurons[0];
    NeuronId nextDigitNeuron = mBase.GetOutput("NextDigit").neurons[0];

    // Layers + input, output, accumulator.
    size_t maxLayers = 3 + mLayerCountLimit;
    mLayerSpacing = 1.0 / float(maxLayers-1);

    // Place the parent/controller of the first layer.
    Point3D position(0.0f, 1.0f, 0.5f);
    NeuronId parent = RequestNeuronWithPosition("GenSpecNeuron", 0, position);

    json accParams;
    accParams["output"] = outputNeuron;
    accParams["nextDigit"] = nextDigitNeuron;
    accParams["position"]["x"] = 1.0 - mLayerSpacing;
    accParams["position"]["y"] = 0.5;
    accParams["position"]["z"] = 0.5;

    mAccumulatorNeuron = mBase.RequestNeuronAddition("GenSpecAccNeuron", accParams.dump());
    Synapse::Data outputSynapseData;
    mBase.RequestSynapseAddition(Direction::Forward, mAccumulatorNeuron, outputNeuron, outputSynapseData);

    Synapse::Data nextDigitSynapseData;
    mBase.RequestSynapseAddition(Direction::Forward, mAccumulatorNeuron, nextDigitNeuron, outputSynapseData);

    mNeuronParams["accumulatorId"] = mAccumulatorNeuron;

    float marginX = (1.0 / float(mNeuronCountX)) / 2.0;
    float marginY = (1.0 / float(mNeuronCountY)) / 2.0;

    for (int y = 0; y < mNeuronCountY; y++) {
        for (int x = 0; x < mNeuronCountX; x++) {
            // Swap X and Z in 3D, position the first layer of generalist neurons in a grid.
            Point3D genPosition(mLayerSpacing, marginY + (float(y) / float(mNeuronCountY)), marginX + (float(x) / float(mNeuronCountX)));
            CreateSpecialist(parent, inputNeuron, 1, genPosition);
        }
    }
}

NeuronId GenSpecRegion::RequestNeuronWithPosition(const char* neuronType, size_t layer, const Point3D &position)
{
    SetParamsPosition(mNeuronParams, position);
    NeuronId neuron = mBase.RequestNeuronAddition(neuronType, mNeuronParams.dump());
    mTopology[neuron] = std::pair<size_t, Point3D>(layer, position);

    return neuron;
}

GenSpecRegion::~GenSpecRegion()
{
}

void GenSpecRegion::pup(PUP::er &p)
{
    // TODO
}

const char *GenSpecRegion::GetType() const
{
    return Type;
}

void GenSpecRegion::Control(size_t brainStep)
{
    CkPrintf("\nstep\n");
    if (mBrainStepCounter % mBrainStepsPerEvolution == 0) {
        // Generate specialists.

        typedef std::pair<NeuronId, float> GenPair;

        std::vector<GenPair> sortingBin(mGenValues.begin(), mGenValues.end());
        std::sort(sortingBin.begin(), sortingBin.end(), [](const GenPair &a, const GenPair &b) -> bool {
            return a.second > b.second;
        });

        CkPrintf("\nGeneralization values: \n\n");
        for (const GenPair &genValue : sortingBin) {
            CkPrintf("Neuron %d: %f\n", GetNeuronIndex(genValue.first), genValue.second);
        }

        int index = 0;
        for (int i = 0; i < mSpecializingGeneralistCount; i++) {
            if (index >= sortingBin.size())
                break;

            if (sortingBin[index].second <= 0) {
                Log(LogLevel::Info, "No generalists need to specialize");
                break;
            }

            NeuronId parent = sortingBin[index].first;
            index++;

            const std::pair<size_t, Point3D> &parentTopology = mTopology[parent];

            if (parentTopology.first >= mLayerCountLimit) {
                i--;
                continue;
            }

            size_t layer = parentTopology.first + 1;
            const Point3D &parentPosition = parentTopology.second;

            const float neuronSpace = (1.0 / mNeuronCountX) / std::pow(mSpecialistCount, layer - 1);
            const float neuronSpaceMargin = neuronSpace / 2.0;
            const float previousLayerSpaceMargin = (1.0 / mNeuronCountX) / std::pow(mSpecialistCount, layer - 2) / 2.0;

            Log(LogLevel::Info, "Neuron %d is spawning %d children", GetNeuronIndex(parent), mSpecialistCount);
            for (int j = 0; j < mSpecialistCount; j++) {

                // X: move to the right of the parent.
                float x = std::get<0>(parentPosition) + mLayerSpacing;

                // Y: the same as the parent.
                float y = std::get<1>(parentPosition);

                // Z: divide the space of the parent by mSpecialistCount and use it for the specialists.
                float z = std::get<2>(parentPosition) - previousLayerSpaceMargin + neuronSpaceMargin + (j * neuronSpace);

                if (x > 1 || y > 1 || z > 1) {
                    CkPrintf("jou\n");
                }

                Point3D position(x, y, z);

                CreateSpecialist(parent, parent, layer, position);
            }
        }
    }

    mBrainStepCounter++;
}

void GenSpecRegion::AcceptContributionFromNeuron(NeuronId neuronId, const uint8_t *contribution, size_t size)
{
    if (size == sizeof(float) + 1) {
        bool isLeaf = contribution[size - 1];

        if (isLeaf) {
            float value;
            std::memcpy(&value, contribution, size-1);
            mGenValues[neuronId] = value;
        } else {
            // If the neuron stopped being a leaf node, discard his previous values.
            if (mGenValues.find(neuronId) != mGenValues.end()) {
                mGenValues.erase(neuronId);
            }
        }
    }
}

size_t GenSpecRegion::ContributeToBrain(uint8_t *&contribution)
{
    return 0;
}

void GenSpecRegion::CreateSpecialist(NeuronId parent, NeuronId inputProvider, size_t layer, const Point3D &position)
{
    CkPrintf("CREATING layer %d, position %f:%f:%f\n", layer, std::get<0>(position), std::get<1>(position), std::get<2>(position));
    Random::Engines::reference engine = Random::GetThreadEngine();
    std::uniform_real_distribution<float> randWeight(0.0f, 1.0f);

    SetParamsPosition(mNeuronParams, position);

    NeuronId child = mBase.RequestNeuronAddition("GenSpecNeuron", mNeuronParams.dump());
    mBase.RequestChildAddition(parent, child);

    mTopology[child] = std::pair<size_t, Point3D>(layer, position);

    // Connect the neuron to the input.
    // This is the only difference between the first layer and the other ones.
    // Specialists get the input from their parent neuron.
    Synapse::Data inputSynapseData;
    Synapse::Initialize(SynapseEditorCache::GetInstance()->GetToken("MultiWeighted"), inputSynapseData, mInputSize);
    MultiWeightedSynapse* inputSynapse = reinterpret_cast<MultiWeightedSynapse*>(Synapse::Edit(inputSynapseData));

    std::unique_ptr<float> inputWeights(new float[mInputSize]);
    for (int i = 0; i < mInputSize; i++) {
        inputWeights.get()[i] = randWeight(engine);
    }
    inputSynapse->SetWeights(inputSynapseData, inputWeights.get(), mInputSize);

    mBase.RequestSynapseAddition(Direction::Forward, inputProvider, child, inputSynapseData);

    // Connect the neuron to the mAccumulatorNeuron.
    Synapse::Data resultSynapse;
    Synapse::Initialize(SynapseEditorCache::GetInstance()->GetToken("Empty"), resultSynapse);
    mBase.RequestSynapseAddition(Direction::Forward, child, mAccumulatorNeuron, resultSynapse);
}

void GenSpecRegion::SetParamsPosition(json &params, const Point3D &position)
{
    params["position"]["x"] = std::get<0>(position);
    params["position"]["y"] = std::get<1>(position);
    params["position"]["z"] = std::get<2>(position);
}

} // namespace GenSpecModel;
