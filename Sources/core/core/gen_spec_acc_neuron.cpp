#include "gen_spec_acc_neuron.h"
#include "log.h"

GenSpecAccNeuron::GenSpecAccNeuron(NeuronBase &base, json &params) : Neuron(base, params)
{
	mOutputNeuron = params["output"].get<NeuronId>();
	mNextDigitNeuron = params["nextDigit"].get<NeuronId>();
}

GenSpecAccNeuron::~GenSpecAccNeuron()
{
}

void GenSpecAccNeuron::pup(PUP::er &p)
{
}

const char *GenSpecAccNeuron::Type = "GenSpecAccNeuron";

const char *GenSpecAccNeuron::GetType() const
{
    return Type;
}

void GenSpecAccNeuron::HandleSpike(Direction direction, DiscreteSpike &spike, Spike::Data &spikeData)
{
	uint8_t value = spike.GetIntensity(spikeData);
	mAccumulatedResults.push_back(NeuronResult(spikeData.sender, value));

	if (mAccumulatedResults.size() > 0 && mAccumulatedResults.size() == mBase.GetInputSynapses().size()) {
		// We got all the results, send spikes to output connectors.
		Log(LogLevel::Info, "Accumulator received info from all neurons, sending info to outputs");

		// First, sort the results by NeuronId, so that neurons keep their position in the output.

		std::sort(mAccumulatedResults.begin(), mAccumulatedResults.end(), [](const NeuronResult &a, const NeuronResult &b) -> bool {
			return a.first < b.first;
		});

		std::unique_ptr<uint8_t[]> outputPtr(new uint8_t[mAccumulatedResults.size()]);
		uint8_t *output = outputPtr.get();

		size_t i = 0;
		for (const auto &resultPair : mAccumulatedResults) {
			output[i] = resultPair.second;
			i++;
		}

		// Send the data to output.
		SendMultiByteSpike(Direction::Forward, mOutputNeuron, output, mAccumulatedResults.size());

		// Send the next digit signal.
		Spike::Data data;
		Spike::Initialize(Spike::Type::Binary, mBase.GetId(), data);
		mBase.SendSpike(mNextDigitNeuron, Direction::Forward, data);

		mAccumulatedResults.clear();
	}
}

void GenSpecAccNeuron::Control(size_t brainStep)
{
}

size_t GenSpecAccNeuron::ContributeToRegion(uint8_t *&contribution)
{
    return 0;
}

void GenSpecAccNeuron::SendMultiByteSpike(Direction direction, NeuronId receiver, uint8_t *values, size_t count)
{
    Spike::Data data;
    Spike::Initialize(Spike::Type::MultiByte, mBase.GetId(), data);
    MultiByteSpike *spike = static_cast<MultiByteSpike *>(Spike::Edit(data));
    spike->SetValues(data, values, count);

    mBase.SendSpike(receiver, direction, data);
}
