#include "gen_spec_next_digit_neuron.h"
#include "log.h"

GenSpecNextDigitNeuron::GenSpecNextDigitNeuron(NeuronBase &base, json &params) : Neuron(base, params)
{
}

GenSpecNextDigitNeuron::~GenSpecNextDigitNeuron()
{
}

void GenSpecNextDigitNeuron::pup(PUP::er &p)
{
}

const char *GenSpecNextDigitNeuron::Type = "GenSpecNextDigitNeuron";

const char *GenSpecNextDigitNeuron::GetType() const
{
    return Type;
}

void GenSpecNextDigitNeuron::HandleSpike(Direction direction, BinarySpike &spike, Spike::Data &spikeData)
{
    CkPrintf("NextDigit neuron got spike, forwarding\n");

    for (const auto &outputPair : mBase.GetOutputSynapses()) {
        Spike::Data data;
        Spike::Initialize(Spike::Type::Binary, mBase.GetId(), data);
        mBase.SendSpike(outputPair.first, Direction::Forward, data);
    }
}

void GenSpecNextDigitNeuron::Control(size_t brainStep)
{
}

size_t GenSpecNextDigitNeuron::ContributeToRegion(uint8_t *&contribution)
{
    return 0;
}
