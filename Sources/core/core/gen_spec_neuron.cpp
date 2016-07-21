#include "gen_spec_neuron.h"
#include "log.h"

GenSpecNeuron::GenSpecNeuron(NeuronBase &base, json &params) : Neuron(base, params),
    mResult(0), mPreviousResult(0), mSynapseThreshold(0.5), mIsWinner(false),
    mChildrenAnswered(0), mBestChild(0), mBestChildResult(0)
{
	if (params.empty()) {
		Log(LogLevel::Error, "GenSpecNeuron: Parameters not received");
		return;
	}

	mSynapseThreshold = params["synapseThreshold"].get<double>();
	auto inputSizeX = params["inputSizeX"].get<size_t>();
	auto inputSizeY = params["inputSizeY"].get<size_t>();
	mInputSize = inputSizeX * inputSizeY;

	mLastInputPtr = std::unique_ptr<uint8_t[]>(new uint8_t[mInputSize]);

	const json &accumulatorId = params["accumulatorId"];
	// accumulatorId would be empty for the generalist that is parent of the first layer.
	if (!accumulatorId.empty())
		mAccumulatorId = params["accumulatorId"].get<NeuronId>();
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
	Log(LogLevel::Info, "Neuron #%d received input", mBase.GetIndex());

    NeuronId sender = Spike::GetSender(spikeData);

    Synapse::Data *synapseData;

    synapseData = mBase.AccessInputSynapse(sender);
    MultiWeightedSynapse *synapse = static_cast<MultiWeightedSynapse *>(Synapse::Edit(*synapseData));

    uint16_t weightCount = synapse->GetWeightCount(*synapseData);
    std::unique_ptr<float[]> weights(new float[weightCount]);
    synapse->GetWeights(*synapseData, weights.get(), weightCount);

	uint8_t *values = mLastInputPtr.get();
    spike.GetValues(spikeData, values, weightCount);

    for (int i = 0; i < weightCount; i++) {
        mResult += ((weights[i] > mSynapseThreshold) ? values[i] : 0);
    }

    ResultArgs args;
    args.result = mResult;
    SendFunctionalSpike(Direction::Backward, mBase.GetParent(), Function::Result, args);
}

void GenSpecNeuron::HandleSpike(Direction direction, FunctionalSpike &spike, Spike::Data &spikeData)
{
    switch (static_cast<Function>(spike.GetFunction(spikeData))) {
    case Function::Result:
    {
		Log(LogLevel::Info, "Neuron #%d received child results", mBase.GetIndex());
        // A child has sent it's result for winner determination.
        ResultArgs args;
        spike.GetArguments(spikeData, &args, sizeof(ResultArgs));

        if (args.result > mBestChildResult) {
            mBestChildResult = args.result;
            mBestChild = Spike::GetSender(spikeData);
        }

        mChildrenAnswered++;

        auto childrenCount = mBase.GetChildren().size();
        // mBestChild is 0 when no children answered with a higher-than-zero result.
        if (childrenCount > 0 && mChildrenAnswered == childrenCount && mBestChild != 0) {
            // Received results from all children, select winner and notify him.

            WinnerSelectedArgs winnerArgs;
			winnerArgs.winner = mBestChild;
			for (auto child : mBase.GetChildren()) {
				SendFunctionalSpike(Direction::Forward, child, Function::WinnerSelected, winnerArgs);
			}

            mBestChildResult = 0;
            mChildrenAnswered = 0;
            mBestChild = 0;
        }

        break;
    }
    case Function::WinnerSelected:
    {
		Log(LogLevel::Info, "Neuron #%d received winner info", mBase.GetIndex());

		WinnerSelectedArgs winnerArgs;
		spike.GetArguments(spikeData, &winnerArgs, sizeof(WinnerSelectedArgs));

		size_t result = 0;

		if (winnerArgs.winner == mBase.GetId()) {
			Log(LogLevel::Info, "Neuron #%d is winner", mBase.GetIndex());
			// This neuron is "activated".
			result = 1;

			mIsWinner = true;
			UpdateWeights();

			for (const NeuronId &child : mBase.GetChildren()) {
				SendMultiByteSpike(Direction::Forward, child, mLastInputPtr.get(), mInputSize);
			}

		}

		SendDiscreteSpike(Direction::Forward, mAccumulatorId, result);

        break;
    }
    default:
        break;
    }
}

void GenSpecNeuron::UpdateWeights()
{
}

void GenSpecNeuron::CalculateObserver(ObserverType type, std::vector<int32_t> &metadata, std::vector<uint8_t> &observerData)
{
    // TODO(HonzaS): Use composition here, provide observer classes.
    if (type == ObserverType::FloatTensor) {
        //observerData.push_back(mAccumulatedActivation * 255 / (100 * mGeneralistActivation));
    }
}

void GenSpecNeuron::Control(size_t brainStep)
{
    // The neuron is only reactive, everything is handled in HandleSpike methods.
    // The only accumulation happens when children send their results and the reaction is
    // done immediatelly when the last message arrives.
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

void GenSpecNeuron::SendDiscreteSpike(Direction direction, NeuronId receiver, uint64_t value)
{
    Spike::Data data;
    Spike::Initialize(Spike::Type::Discrete, mBase.GetId(), data);
    DiscreteSpike *spike = static_cast<DiscreteSpike *>(Spike::Edit(data));
	spike->SetIntensity(data, value);

    mBase.SendSpike(receiver, direction, data);
}

