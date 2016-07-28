#pragma once

#include "neuron.h"
#include <queue>

namespace Functions
{
    enum class Function : uint8_t
    {
        Result,
        WinnerSelected
    };

    struct ResultArgs
    {
        uint64_t result;
        bool isLeaf;
    };

    struct WinnerSelectedArgs
    {
		NeuronId winner;
    };
};

class GenSpecNeuron : public Neuron
{
public:
    static const char *Type;

    GenSpecNeuron(NeuronBase &base, json &params);
    virtual ~GenSpecNeuron();

    void pup(PUP::er &p) override;

    const char *GetType() const override;

    // Discrete spike for signal propagation (input/result/output).
    void HandleSpike(Direction direction, MultiByteSpike &spike, Spike::Data &data) override;
    // Functional spike for layer winner determination.
    void HandleSpike(Direction direction, FunctionalSpike &spike, Spike::Data &data) override;
    
    void Control(size_t brainStep) override;

    size_t ContributeToRegion(uint8_t *&contribution) override;

    void CalculateObserver(ObserverType type, std::vector<int32_t> &metadata, std::vector<uint8_t> &observerData) override;

    template<typename Arguments>
    void SendFunctionalSpike(Direction direction, NeuronId receiver, Functions::Function function, Arguments &args);

    void SendMultiByteSpike(Direction direction, NeuronId receiver, uint8_t *values, size_t count);

    void SendDiscreteSpike(Direction direction, NeuronId receiver, uint64_t value);

protected:
    uint64_t mResult;
    uint64_t mPreviousResult;
    double mSynapseThreshold;
    bool mIsWinner;
    size_t mChildrenAnswered;
    NeuronId mBestChild;
    uint64_t mBestChildResult;

	size_t mInputSize;

	// If this neuron wins, it needs to forward the input to the children.
	std::unique_ptr<uint8_t[]> mLastInputPtr;
	NeuronId mAccumulatorId;

	size_t mGenValueLimit;
	float mActiveInputsVsRegisteredInputs;
	std::deque<float> mGeneralizationValues;

	// How much is this neuron generalizing?
	float mGenFactor;

	void UpdateWeights();

	float GetInputDifference(uint8_t *oldInput, uint8_t *newInput);
};

template<typename Arguments>
inline void GenSpecNeuron::SendFunctionalSpike(Direction direction, NeuronId receiver, Functions::Function function, Arguments &args)
{
    Spike::Data data;
    Spike::Initialize(Spike::Type::Functional, mBase.GetId(), data);
    FunctionalSpike *spike = static_cast<FunctionalSpike *>(Spike::Edit(data));
    spike->SetFunction(data, static_cast<uint8_t>(function));
    spike->SetArguments(data, &args, sizeof(Arguments));

    mBase.SendSpike(receiver, direction, data);
}
