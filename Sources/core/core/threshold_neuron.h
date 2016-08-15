#pragma once
#include "neuron.h"

namespace ThresholdModel
{

class ThresholdNeuron : public Neuron
{
public:
    static const char *Type;

    ThresholdNeuron(NeuronBase &base, json &params);
    virtual ~ThresholdNeuron();

    virtual void pup(PUP::er &p) override;

    virtual const char *GetType() const override;

    virtual void HandleSpike(Direction direction, ContinuousSpike &spike, Spike::Data &data) override;
    virtual void HandleSpike(Direction direction, FunctionalSpike &spike, Spike::Data &data) override;
    
    virtual void Control(size_t brainStep) override;

    virtual size_t ContributeToRegion(uint8_t *&contribution) override;

    void CalculateObserver(ObserverType type, std::vector<int32_t> &metadata, std::vector<uint8_t> &observerData) override;

    enum class Function : uint8_t
    {
        RequestThreshold,
        ReceiveThreshold,
        ChangeThreshold
    };

    struct RequestThresholdArgs
    {
    };

    struct ReceiveThresholdArgs
    {
        double threshold;
    };

    struct ChangeThresholdArgs
    {
        double average;
    };

    void RequestThreshold(Direction direction, NeuronId sender);
    void ReceiveThreshold(Direction direction, NeuronId sender, const ReceiveThresholdArgs &args);
    void ChangeThreshold(Direction direction, NeuronId sender, const ChangeThresholdArgs &args);

    template<typename Arguments>
    void SendFunctionalSpike(Direction direction, NeuronId receiver, Function function, Arguments &args);

    void SendContinuousSpike(Direction direction, NeuronId receiver, uint16_t delay, double intensity);

protected:
    double mThresholdActivation;
    double mAccumulatedActivation;
    size_t mReceivedSpikeCount;
    size_t mSentSpikeCount;
    bool mWasTriggered;
};

template<typename Arguments>
inline void ThresholdNeuron::SendFunctionalSpike(Direction direction, NeuronId receiver, Function function, Arguments &args)
{
    Spike::Data data;
    Spike::Initialize(SpikeEditorCache::GetInstance()->GetToken("Functional"), mBase.GetId(), data);
    FunctionalSpike *spike = static_cast<FunctionalSpike *>(Spike::Edit(data));
    spike->SetFunction(data, static_cast<uint8_t>(function));
    spike->SetArguments(data, &args, sizeof(Arguments));

    mBase.SendSpike(receiver, direction, data);
}

}
