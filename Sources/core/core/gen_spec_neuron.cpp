#include "gen_spec_neuron.h"

GenSpecNeuron::GenSpecNeuron(NeuronBase &base, json &params) : Neuron(base, params),
    mResult(0), mPreviousResult(0), mSynapseThreshold(0.5), mIsWinner(false)
{
    if (!params.empty()) {
        for (auto itParams = params.begin(); itParams != params.end(); ++itParams) {
            if (itParams.key() == "synapseThreshold" && itParams->is_number()) {
                mSynapseThreshold = itParams.value().get<double>();
            }
        }
    } 
}

GenSpecNeuron::~GenSpecNeuron()
{
}

void GenSpecNeuron::pup(PUP::er &p)
{
    p | mResult;
    p | mSynapseThreshold;
}

const char *GenSpecNeuron::Type = "GenSpecNeuron";

const char *GenSpecNeuron::GetType() const
{
    return Type;
}

void GenSpecNeuron::HandleSpike(Direction direction, MultiByteSpike &spike, Spike::Data &spikeData)
{
    // Forward pass - propagation of output->input.

    NeuronId sender = Spike::GetSender(spikeData);

    Synapse::Data *synapseData;

    synapseData = mBase.AccessInputSynapse(sender);
    MultiWeightedSynapse *synapse = static_cast<MultiWeightedSynapse *>(Synapse::Edit(*synapseData));

    uint16_t weightCount = synapse->GetWeightCount(*synapseData);
    std::unique_ptr<float[]> weights(new float[weightCount]);
    synapse->GetWeights(*synapseData, weights.get(), weightCount);

    std::unique_ptr<uint8_t[]> values(new uint8_t[weightCount]);
    spike.GetValues(spikeData, values.get(), weightCount);

    for (int i = 0; i < weightCount; i++) {
        mResult += weights[i] > mSynapseThreshold ? values[i] : 0;
    }
}

void GenSpecNeuron::HandleSpike(Direction direction, FunctionalSpike &spike, Spike::Data &data)
{
    switch (static_cast<Function>(spike.GetFunction(data))) {
    case Function::Result:
    {
        // A child has sent it's result for winner determination.
        ResultArgs args;
        spike.GetArguments(data, &args, sizeof(ResultArgs));

        if (args.result > mBestChildResult) {
            mBestChildResult = args.result;
            mBestChild = Spike::GetSender(data);
        }

        mChildrenAnswered++;

        auto childrenCount = mBase.GetChildren().size();
        if (childrenCount > 0 && mChildrenAnswered == childrenCount) {
            // Received results from all children, select winner and notify him.

            WinnerSelectedArgs winnerArgs;
            SendFunctionalSpike(Direction::Forward, mBestChild, Function::WinnerSelected, winnerArgs);

            mBestChildResult = 0;
            mChildrenAnswered = 0;
        }

        break;
    }
    case Function::WinnerSelected:
    {
        // This neuron has been selected as the winner of the last input signal.

        mIsWinner = true;
        break;
    }
    default:
        break;
    }
}

void GenSpecNeuron::CalculateObserver(ObserverType type, std::vector<uint8_t> &observerData)
{
    // TODO(HonzaS): Use composition here, provide observer classes.
    if (type == ObserverType::Greyscale) {
        //observerData.push_back(mAccumulatedActivation * 255 / (100 * mGeneralistActivation));
    }
}

void GenSpecNeuron::Control(size_t brainStep)
{
    if (mResult > 0) {
        // There has been accumulation of input spikes, process forward pass.

        ResultArgs args;
        args.result = mResult;

        SendFunctionalSpike(Direction::Backward, mBase.GetParent(), Function::Result, args);

        mPreviousResult = mResult;
        mResult = 0;
    }
    else if (mIsWinner) {
        // The neuron has been selected as winner of the last forwad pass.
        // Update weights, forward input, send result to accumulator.

        mIsWinner = false;
    }
}

size_t GenSpecNeuron::ContributeToRegion(uint8_t *&contribution)
{
    return 0;
}

void GenSpecNeuron::SendMultiByteSpike(Direction direction, NeuronId receiver, uint8_t *values, size_t count)
{
    Spike::Data data;
    Spike::Initialize(Spike::Type::MultiByte, mBase.GetId(), data);
    MultiByteSpike *spike = static_cast<MultiByteSpike *>(Spike::Edit(data));
    spike->SetValues(data, values, count);

    mBase.SendSpike(receiver, direction, data);
}

