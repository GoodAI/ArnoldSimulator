#pragma once

#include "neuron.h"

namespace GenSpecModel
{

class GenSpecOutputNeuron : public Neuron
{
public:
    typedef std::pair<NeuronId, uint8_t> NeuronResult;

    static const char *Type;
    GenSpecOutputNeuron(NeuronBase &base, json &params);
    virtual ~GenSpecOutputNeuron();

    void pup(PUP::er &p) override;

    const char *GetType() const override;

    void HandleSpike(Direction direction, MultiByteSpike &spike, Spike::Data &data) override;

    void Control(size_t brainStep) override;

    size_t ContributeToRegion(uint8_t *&contribution) override;

    void SendMultiByteSpike(Direction direction, NeuronId receiver, const uint8_t *values, size_t count);
};

} // namespace GenSpecModel;
