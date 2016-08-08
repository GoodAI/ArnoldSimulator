#include "gen_spec_acc_neuron.h"
#include "gen_spec_neuron.h"
#include "components.h"
#include "log.h"

namespace GenSpecModel
{

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

void GenSpecAccNeuron::HandleSpike(Direction direction, FunctionalSpike &spike, Spike::Data &spikeData)
{
    GenSpecFunctions::Function function = static_cast<GenSpecFunctions::Function>(spike.GetFunction(spikeData));

    switch (function) {
        case GenSpecFunctions::Function::Result:
        {
            GenSpecFunctions::ResultArgs args;
            spike.GetArguments(spikeData, &args, sizeof(GenSpecFunctions::ResultArgs));

            uint8_t value = args.result;
            mAccumulatedResults[spikeData.sender] = value;

            if (args.isLeaf == true) {
                // The leaf node is definitely the last one to arrive, because the ones before him
                // would have arrived in the previous time steps.
                Log(LogLevel::Info, "Accumulator received info from all active neurons, sending info to outputs");


                // Fill in the neurons that didn't send a result (inactive).
                for (const std::pair<NeuronId, Synapse::Data> &inputSynapse : mBase.GetInputSynapses()) {
                    NeuronId input = inputSynapse.first;
                    if (mAccumulatedResults.find(input) == mAccumulatedResults.end()) {
                        mAccumulatedResults[input] = 0;
                    }
                }

                // First, sort the results by NeuronId, so that neurons keep their position in the output.

                std::vector<NeuronResult> results(mAccumulatedResults.begin(), mAccumulatedResults.end());
                mAccumulatedResults.clear();

                std::sort(results.begin(), results.end(), [](const NeuronResult &a, const NeuronResult &b) -> bool {
                    return a.first < b.first;
                });

                std::unique_ptr<uint8_t[]> outputPtr(new uint8_t[results.size()]);
                uint8_t *output = outputPtr.get();

                size_t i = 0;
                for (const auto &resultPair : results) {
                    output[i] = resultPair.second;
                    i++;
                }

                // Send the data to output.
                SendMultiByteSpike(Direction::Forward, mOutputNeuron, output, results.size());

                // Send the next digit signal.
                Spike::Data data;
                Spike::Initialize(SpikeEditorCache::GetInstance()->GetToken("Binary"), mBase.GetId(), data);
                mBase.SendSpike(mNextDigitNeuron, Direction::Forward, data);
            }
            break;
        }
        default: break;
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
    Spike::Initialize(SpikeEditorCache::GetInstance()->GetToken("MultiByte"), mBase.GetId(), data);
    MultiByteSpike *spike = static_cast<MultiByteSpike *>(Spike::Edit(data));
    spike->SetValues(data, values, count);

    mBase.SendSpike(receiver, direction, data);
}

} // namespace GenSpecModel;
