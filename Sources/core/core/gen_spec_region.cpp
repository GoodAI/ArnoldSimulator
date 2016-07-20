#include "gen_spec_region.h"
#include "random.h"

const char *GenSpecRegion::Type = "GenSpecRegion";

GenSpecRegion::GenSpecRegion(RegionBase &base, json &params) : Region(base, params)
{
    json generalists = params["generalists"];

    size_t neuronCountX, neuronCountY, inputSizeX, inputSizeY, inputSize, inputStrideX, inputStrideY;
    neuronCountX = generalists["neuronCountX"].get<size_t>();
    neuronCountY = generalists["neuronCountY"].get<size_t>();
    inputSizeX = generalists["inputSizeX"].get<size_t>();
    inputSizeY = generalists["inputSizeY"].get<size_t>();
    inputSize = inputSizeX * inputSizeY;
    inputStrideX = generalists["inputStrideX"].get<size_t>();
    inputStrideY = generalists["inputStrideY"].get<size_t>();



    std::string neuronParams = params["neuronParams"].dump();

    // Place the parent/controller of the first layer.
    NeuronId parent = base.RequestNeuronAddition("GenSpecNeuron", neuronParams);

    NeuronId accumulator = base.RequestNeuronAddition("GenSpecAccNeuron", nullptr);

    NeuronId inputNeuron = mBase.GetInput("Input").neurons[0];

    Random::Engines::reference engine = Random::GetThreadEngine();
    std::uniform_real_distribution<float> randWeight(0.0f, 1.0f);

    for (int y = 0; y < neuronCountY; y++) {
        for (int x = 0; x < neuronCountX; x++) {
            // Create the first layer neuron and register it as a child for the uber-parent.
            NeuronId child = base.RequestNeuronAddition("GenSpecNeuron", neuronParams);
            base.RequestChildAddition(parent, child);

            // Connect the neuron to the input.
            // This is the only difference between the first layer and the other ones.
            // Specialists get the input from their parent neuron.
            Synapse::Data inputSynapseData;
            Synapse::Initialize(Synapse::Type::MultiWeighted, inputSynapseData);
            MultiWeightedSynapse* inputSynapse = reinterpret_cast<MultiWeightedSynapse*>(Synapse::Edit(inputSynapseData));
            inputSynapse->SetWeightCount(inputSynapseData, inputSize);

            std::unique_ptr<float> inputWeights(new float[inputSize]);
            for (int i = 0; i < inputSize; i++) {
                inputWeights.get()[i] = randWeight(engine);
            }
            inputSynapse->SetWeights(inputSynapseData, inputWeights.get(), inputSize);

            base.RequestSynapseAddition(Direction::Forward, inputNeuron, child, inputSynapseData);

            // Connect the neuron to the accumulator.
            Synapse::Data resultSynapse;
            Synapse::Initialize(Synapse::Type::Empty, resultSynapse);
            base.RequestSynapseAddition(Direction::Forward, child, accumulator, resultSynapse);
        }
    }
}

GenSpecRegion::~GenSpecRegion()
{
}

void GenSpecRegion::pup(PUP::er &p)
{
    //p | mConnectMore;
}

const char *GenSpecRegion::GetType() const
{
    return Type;
}

void GenSpecRegion::Control(size_t brainStep)
{
}

void GenSpecRegion::AcceptContributionFromNeuron(NeuronId neuronId, const uint8_t *contribution, size_t size)
{
}

size_t GenSpecRegion::ContributeToBrain(uint8_t *&contribution)
{
    return 0;
}
