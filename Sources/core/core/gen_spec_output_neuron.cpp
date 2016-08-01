#include "gen_spec_output_neuron.h"
#include "log.h"

GenSpecOutputNeuron::GenSpecOutputNeuron(NeuronBase &base, json &params) : Neuron(base, params)
{
}

GenSpecOutputNeuron::~GenSpecOutputNeuron()
{
}

void GenSpecOutputNeuron::pup(PUP::er &p)
{
}

const char *GenSpecOutputNeuron::Type = "GenSpecOutputNeuron";

const char *GenSpecOutputNeuron::GetType() const
{
    return Type;
}

void GenSpecOutputNeuron::HandleSpike(Direction direction, MultiByteSpike &spike, Spike::Data &spikeData)
{
    CkPrintf("Output neuron got spike, forwarding\n");

    for (const auto &outputPair : mBase.GetOutputSynapses()) {
        SendMultiByteSpike(Direction::Forward, outputPair.first, spike.GetValues(spikeData), spike.GetValueCount(spikeData));
    }
}

void GenSpecOutputNeuron::Control(size_t brainStep)
{
}

size_t GenSpecOutputNeuron::ContributeToRegion(uint8_t *&contribution)
{
    return 0;
}

void GenSpecOutputNeuron::SendMultiByteSpike(Direction direction, NeuronId receiver, const uint8_t *values, size_t count)
{
    Spike::Data data;
    Spike::Initialize(Spike::Type::MultiByte, mBase.GetId(), data);
    MultiByteSpike *spike = static_cast<MultiByteSpike *>(Spike::Edit(data));
    spike->SetValues(data, values, count);

    mBase.SendSpike(receiver, direction, data);
}
