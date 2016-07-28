#include "gen_spec_region.h"
#include "random.h"
#include "log.h"

const char *GenSpecRegion::Type = "GenSpecRegion";

GenSpecRegion::GenSpecRegion(RegionBase &base, json &params) : Region(base, params),
	mBrainStepCounter(0), mBrainStepsPerEvolution(10), mSpecialistCount(1), mSpecializingGeneralistCount(0)
{
    // This is necessary if we want to remove items from the map.
    // It's specific to the google sparse implementation, remove this if we go for a normal map.
    mGenValues.set_deleted_key(0);

	if (params.find("brainStepsPerEvolution") != params.end()) {
		mBrainStepsPerEvolution = params["brainStepsPerEvolution"].get<size_t>();
	}

	if (params.find("specializingGeneralistCount") != params.end()) {
		mSpecializingGeneralistCount = params["specializingGeneralistCount"].get<size_t>();
	}

	if (params.find("specialistCount") != params.end()) {
		mSpecialistCount = params["specialistCount"].get<size_t>();
	}

    json generalists = params["generalists"];

    size_t neuronCountX, neuronCountY, inputSizeX, inputSizeY, inputStrideX, inputStrideY;
    neuronCountX = generalists["neuronCountX"].get<size_t>();
    neuronCountY = generalists["neuronCountY"].get<size_t>();
    inputSizeX = generalists["inputSizeX"].get<size_t>();
    inputSizeY = generalists["inputSizeY"].get<size_t>();
    mInputSize = inputSizeX * inputSizeY;

	mNeuronParams = generalists["neuronParams"];

    // Place the parent/controller of the first layer.
    NeuronId parent = mBase.RequestNeuronAddition("GenSpecNeuron", mNeuronParams.dump());

	NeuronId outputNeuron = mBase.GetOutput("Output").neurons[0];
	NeuronId nextDigitNeuron = mBase.GetOutput("NextDigit").neurons[0];

	json accParams;
	accParams["output"] = outputNeuron;
	accParams["nextDigit"] = nextDigitNeuron;

    mAccumulatorNeuron = mBase.RequestNeuronAddition("GenSpecAccNeuron", accParams.dump());
	Synapse::Data outputSynapseData;
	mBase.RequestSynapseAddition(Direction::Forward, mAccumulatorNeuron, outputNeuron, outputSynapseData);

	Synapse::Data nextDigitSynapseData;
	mBase.RequestSynapseAddition(Direction::Forward, mAccumulatorNeuron, nextDigitNeuron, outputSynapseData);

    NeuronId inputNeuron = mBase.GetInput("Input").neurons[0];

    mNeuronParams["accumulatorId"] = mAccumulatorNeuron;

    for (int y = 0; y < neuronCountY; y++) {
        for (int x = 0; x < neuronCountX; x++) {
            CreateSpecialists(parent, inputNeuron);
        }
    }
}

GenSpecRegion::~GenSpecRegion()
{
}

void GenSpecRegion::pup(PUP::er &p)
{
	// TODO
}

const char *GenSpecRegion::GetType() const
{
    return Type;
}

void GenSpecRegion::Control(size_t brainStep)
{
    CkPrintf("\nstep\n");
	if (mBrainStepCounter % mBrainStepsPerEvolution == 0) {
		// Generate specialists.

		typedef std::pair<NeuronId, float> GenPair;

		std::vector<GenPair> sortingBin(mGenValues.begin(), mGenValues.end());
		std::sort(sortingBin.begin(), sortingBin.end(), [](const GenPair &a, const GenPair &b) -> bool {
			return a.second > b.second;
		});

        CkPrintf("\nGeneralization values: \n\n");
        for (const GenPair &genValue : mGenValues) {
            CkPrintf("Neuron %d: %f\n", GetNeuronIndex(genValue.first), genValue.second);
        }

		for (int i = 0; i < mSpecializingGeneralistCount && i < sortingBin.size(); i++) {
            NeuronId parent = sortingBin[i].first;
            Log(LogLevel::Info, "Neuron %d is spawning %d children", GetNeuronIndex(parent), mSpecialistCount);
            for (int j = 0; j < mSpecialistCount; j++) {
                CreateSpecialists(parent, parent);
            }
		}
	}

	mBrainStepCounter++;
}

void GenSpecRegion::AcceptContributionFromNeuron(NeuronId neuronId, const uint8_t *contribution, size_t size)
{
	if (size == sizeof(float) + 1) {
		bool isLeaf = contribution[size - 1];

		if (isLeaf) {
			float value;
			std::memcpy(&value, contribution, size-1);
			mGenValues[neuronId] = value;
		} else {
			// If the neuron stopped being a leaf node, discard his previous values.
            if (mGenValues.find(neuronId) != mGenValues.end()) {
                mGenValues.erase(neuronId);
            }
		}
	}
}

size_t GenSpecRegion::ContributeToBrain(uint8_t *&contribution)
{
    return 0;
}

void GenSpecRegion::CreateSpecialists(NeuronId parent, NeuronId inputProvider)
{
    Random::Engines::reference engine = Random::GetThreadEngine();
    std::uniform_real_distribution<float> randWeight(0.0f, 1.0f);

    NeuronId child = mBase.RequestNeuronAddition("GenSpecNeuron", mNeuronParams.dump());
    mBase.RequestChildAddition(parent, child);

    // Connect the neuron to the input.
    // This is the only difference between the first layer and the other ones.
    // Specialists get the input from their parent neuron.
    Synapse::Data inputSynapseData;
    Synapse::Initialize(Synapse::Type::MultiWeighted, inputSynapseData, mInputSize);
    MultiWeightedSynapse* inputSynapse = reinterpret_cast<MultiWeightedSynapse*>(Synapse::Edit(inputSynapseData));

    std::unique_ptr<float> inputWeights(new float[mInputSize]);
    for (int i = 0; i < mInputSize; i++) {
        inputWeights.get()[i] = randWeight(engine);
    }
    inputSynapse->SetWeights(inputSynapseData, inputWeights.get(), mInputSize);

    mBase.RequestSynapseAddition(Direction::Forward, inputProvider, child, inputSynapseData);

    // Connect the neuron to the mAccumulatorNeuron.
    Synapse::Data resultSynapse;
    Synapse::Initialize(Synapse::Type::Empty, resultSynapse);
    mBase.RequestSynapseAddition(Direction::Forward, child, mAccumulatorNeuron, resultSynapse);
}
