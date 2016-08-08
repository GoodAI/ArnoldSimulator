#include "threshold_neuron.h"
#include "data_utils.h"

namespace ThresholdModel
{

ThresholdNeuron::ThresholdNeuron(NeuronBase &base, json &params) : Neuron(base, params), 
    mThresholdActivation(0.0), mAccumulatedActivation(0.0),
    mReceivedSpikeCount(0), mSentSpikeCount(0)
{
    if (!params.empty()) {
        for (auto itParams = params.begin(); itParams != params.end(); ++itParams) {
            if (itParams.key() == "threshold" && itParams->is_number()) {
                mThresholdActivation = itParams.value().get<double>();
            }
        }
    } 
}

ThresholdNeuron::~ThresholdNeuron()
{
}

void ThresholdNeuron::pup(PUP::er &p)
{
    p | mThresholdActivation;
    p | mAccumulatedActivation;
    p | mReceivedSpikeCount;
    p | mSentSpikeCount;
}

const char *ThresholdNeuron::Type = "ThresholdNeuron";

const char *ThresholdNeuron::GetType() const
{
    return Type;
}

void ThresholdNeuron::HandleSpike(Direction direction, ContinuousSpike &spike, Spike::Data &data)
{
    uint16_t delay = spike.GetDelay(data);
    if (delay == 0) {

        NeuronId sender = Spike::GetSender(data);

        Synapse::Data *synapseData;
        if (direction == Direction::Forward) {
            synapseData = mBase.AccessInputSynapse(sender);
        } else {
            synapseData = mBase.AccessOutputSynapse(sender);
        }

        double synapseWeight = 1.0;
        if (Synapse::GetType(*synapseData) == SynapseEditorCache::GetInstance()->GetToken("Weighted")) {
            WeightedSynapse *synapse = static_cast<WeightedSynapse *>(Synapse::Edit(*synapseData));
            synapseWeight = synapse->GetWeight(*synapseData);
        }

        double spikeIntensity = spike.GetIntensity(data);

        mAccumulatedActivation += spikeIntensity * synapseWeight;
        ++mReceivedSpikeCount;

    } else {
        spike.SetDelay(data, --delay);
        mBase.EnqueueSpike(direction, data);
    }
}

void ThresholdNeuron::CalculateObserver(ObserverType type, std::vector<int32_t> &metadata, std::vector<uint8_t> &observerData)
{
    // TODO(HonzaS): Use composition here, provide observer classes.
    if (type == ObserverType::FloatTensor) {

        metadata.push_back(3);  // First dimension (width).
        metadata.push_back(2);  // Second dimension (height).
        
        float activationRatio = 0.0f;
        if (mThresholdActivation != 0.0f) {
            activationRatio = mAccumulatedActivation / mThresholdActivation;
        }

        PutFloatToByteVector(observerData, activationRatio);
        PutFloatToByteVector(observerData, activationRatio/2.0f);
        PutFloatToByteVector(observerData, activationRatio/3.0f);

        PutFloatToByteVector(observerData, -activationRatio);
        PutFloatToByteVector(observerData, -activationRatio/2.0f);
        PutFloatToByteVector(observerData, -activationRatio/3.0f);
    }
}

void ThresholdNeuron::HandleSpike(Direction direction, FunctionalSpike &spike, Spike::Data &data)
{
    switch (static_cast<Function>(spike.GetFunction(data))) {
        case Function::RequestThreshold:
        {
            RequestThreshold(direction, Spike::GetSender(data));
            break;
        }
        case Function::ReceiveThreshold:
        {
            ReceiveThresholdArgs args;
            spike.GetArguments(data, &args, sizeof(ReceiveThresholdArgs));
            ReceiveThreshold(direction, Spike::GetSender(data), args);
            break;
        }
        case Function::ChangeThreshold:
        {
            ChangeThresholdArgs args;
            spike.GetArguments(data, &args, sizeof(ChangeThresholdArgs));
            ChangeThreshold(direction, Spike::GetSender(data), args);
            break;
        }
        default:
            break;
    }
}

void ThresholdNeuron::Control(size_t brainStep)
{
    if (mAccumulatedActivation >= 100 * mThresholdActivation) {
        mAccumulatedActivation = 0.0;
        mSentSpikeCount = 0;

        //if (brainStep % 2) {
            for (auto it = mBase.GetOutputSynapses().begin(); it != mBase.GetOutputSynapses().end(); ++it) {
                
                uint16_t delay = 0;
                Synapse::Data *synapseData;
                synapseData = mBase.AccessOutputSynapse(it->first);
                if (Synapse::GetType(*synapseData) == SynapseEditorCache::GetInstance()->GetToken("Lagging")) {
                    LaggingSynapse *synapse = static_cast<LaggingSynapse *>(Synapse::Edit(*synapseData));
                    delay = synapse->GetDelay(*synapseData);
                }

                SendContinuousSpike(Direction::Forward, it->first, delay, mThresholdActivation);
                ++mSentSpikeCount;
            }
        /*} else {
            for (auto it = mBase.GetInputSynapses().begin(); it != mBase.GetInputSynapses().end(); ++it) {
                NeuronId sender = mBase.GetId();
                NeuronId receiver = it->first;
                RequestThresholdArgs request;
                if (GetRegionIndex(sender) == GetRegionIndex(receiver)) {
                    SendFunctionalSpike<RequestThresholdArgs>(
                        Direction::Backward, receiver, Function::RequestThreshold, request);
                }
            }
        }*/
    }

    /*
    auto engine = Random::GetThreadEngine();
    size_t inputSynapseCount = mBase.GetInputSynapses().size();

    if (mReceivedSpikeCount * 10 < inputSynapseCount && mSentSpikeCount > 0) {
        std::uniform_int_distribution<size_t> randSynapse(0, inputSynapseCount - 1);
        NeuronId from = DELETED_NEURON_ID;
        size_t whenToStop = randSynapse(engine);
        for (auto it = mBase.GetInputSynapses().begin(); it != mBase.GetInputSynapses().end(); ++it) {
            if (whenToStop == 0) {
                from = it->first;
                break;
            }
            --whenToStop;
        }
        mBase.RequestSynapseRemoval(Direction::Forward, from, mBase.GetId());
    } else if (mReceivedSpikeCount > 10 * mSentSpikeCount && mReceivedSpikeCount < 100 * mSentSpikeCount) {
        json params = {
            { "threshold", mThresholdActivation }
        };
        NeuronId child = mBase.RequestNeuronAddition(GetType(), params.dump());
        mBase.AdoptAsChild(child, true);
    }
    */
}

size_t ThresholdNeuron::ContributeToRegion(uint8_t *&contribution)
{
    size_t inputSynapseCount = mBase.GetInputSynapses().size();
    size_t outputSynapseCount = mBase.GetOutputSynapses().size();

    size_t size = (sizeof(size_t) * 4);
    contribution = new uint8_t[size];
    uint8_t *cur = contribution;

    std::memcpy(cur, &mReceivedSpikeCount, sizeof(size_t));
    cur += sizeof(size_t);  

    std::memcpy(cur, &mSentSpikeCount, sizeof(size_t));
    cur += sizeof(size_t);

    std::memcpy(cur, &inputSynapseCount, sizeof(size_t));
    cur += sizeof(size_t);

    std::memcpy(cur, &outputSynapseCount, sizeof(size_t));
    //cur += sizeof(size_t);

    if (mSentSpikeCount > 0) {
        mReceivedSpikeCount = 0;
    }

    return size;
}

void ThresholdNeuron::RequestThreshold(Direction direction, NeuronId sender)
{
    ReceiveThresholdArgs response;
    response.threshold = mThresholdActivation;

    SendFunctionalSpike<ReceiveThresholdArgs>(
        OPPOSITE_DIRECTION(direction), sender, Function::ReceiveThreshold, response);
}

void ThresholdNeuron::ReceiveThreshold(Direction direction, NeuronId sender, const ReceiveThresholdArgs &args)
{
    ChangeThresholdArgs response;
    response.delta = mThresholdActivation - ((mThresholdActivation + args.threshold) / 2);

    SendFunctionalSpike<ChangeThresholdArgs>(
        OPPOSITE_DIRECTION(direction), sender, Function::ChangeThreshold, response);
}

void ThresholdNeuron::ChangeThreshold(Direction direction, NeuronId sender, const ChangeThresholdArgs &args)
{
    mThresholdActivation += args.delta;
}

void ThresholdNeuron::SendContinuousSpike(Direction direction, NeuronId receiver, uint16_t delay, double intensity)
{
    Spike::Data data;
    Spike::Initialize(SpikeEditorCache::GetInstance()->GetToken("Continuous"), mBase.GetId(), data);
    ContinuousSpike *spike = static_cast<ContinuousSpike *>(Spike::Edit(data));
    spike->SetDelay(data, delay);
    spike->SetIntensity(data, mThresholdActivation);

    mBase.SendSpike(receiver, direction, data);
}

}
