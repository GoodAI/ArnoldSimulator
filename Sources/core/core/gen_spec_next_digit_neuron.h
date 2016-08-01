#pragma once

#include "neuron.h"

class GenSpecNextDigitNeuron : public Neuron
{
public:
    typedef std::pair<NeuronId, uint8_t> NeuronResult;

    static const char *Type;
    GenSpecNextDigitNeuron(NeuronBase &base, json &params);
    virtual ~GenSpecNextDigitNeuron();

    void pup(PUP::er &p) override;

    const char *GetType() const override;

    void HandleSpike(Direction direction, BinarySpike &spike, Spike::Data &data) override;

    void Control(size_t brainStep) override;

    size_t ContributeToRegion(uint8_t *&contribution) override;
};
