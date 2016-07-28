#pragma once

#include "neuron.h"

class GenSpecAccNeuron : public Neuron
{
public:
	typedef std::pair<NeuronId, uint8_t> NeuronResult;

	static const char *Type;
	GenSpecAccNeuron(NeuronBase &base, json &params);
	virtual ~GenSpecAccNeuron();

	void pup(PUP::er &p) override;

	const char *GetType() const override;

	void HandleSpike(Direction direction, FunctionalSpike &spike, Spike::Data &data) override;

	void Control(size_t brainStep) override;

	size_t ContributeToRegion(uint8_t *&contribution) override;

	void SendMultiByteSpike(Direction direction, NeuronId receiver, uint8_t *values, size_t count);
protected:
	std::map<NeuronId, uint8_t> mAccumulatedResults;
	NeuronId mOutputNeuron;
	NeuronId mNextDigitNeuron;
};
