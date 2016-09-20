#include "threshold_region.h"
#include "components.h"
#include "random.h"

namespace ThresholdModel
{

const char *ThresholdRegion::Type = "ThresholdRegion";

ThresholdRegion::ThresholdRegion(RegionBase &base, json &params) : Region(base, params)
{
}

ThresholdRegion::~ThresholdRegion()
{
}

void ThresholdRegion::pup(PUP::er &p)
{
    p | mConnectMore;
    p | mPruneAway;
}

const char *ThresholdRegion::GetType() const
{
    return Type;
}

void ThresholdRegion::Control(size_t brainStep)
{
    Random::Engine &engine = Random::GetThreadEngine();
    std::uniform_int_distribution<int> diceRoll(0, 63);

    std::vector<NeuronIndex> neuronIndices(
        mBase.GetNeuronIndices().begin(), mBase.GetNeuronIndices().end());
    std::uniform_int_distribution<size_t> randNeuron(0, neuronIndices.size() - 1);

    if (!neuronIndices.empty() && !diceRoll(engine)) {
        std::uniform_int_distribution<int> coinFlip(0, 1);
        if (coinFlip(engine)) {
            mPruneAway.push_back(GetNeuronId(mBase.GetIndex(), neuronIndices.at(randNeuron(engine))));
        }
        if (coinFlip(engine)) {
            mConnectMore.push_back(GetNeuronId(mBase.GetIndex(), neuronIndices.at(randNeuron(engine))));
        }
    }

    if (!mConnectMore.empty()) {
        std::uniform_int_distribution<uint16_t> randDelay(0, 1);
        std::uniform_real_distribution<double> randWeight(0, 1.0);
        for (auto it = mConnectMore.begin(); it != mConnectMore.end(); ++it) {
            Synapse::Data synapseData;
            Synapse::Initialize(SynapseEditorCache::GetInstance()->GetToken("Lagging"), synapseData);
            LaggingSynapse *synapse = static_cast<LaggingSynapse *>(Synapse::Edit(synapseData));
            synapse->SetDelay(synapseData, randDelay(engine));
            synapse->SetWeight(synapseData, randWeight(engine));
            NeuronId chosenTarget = GetNeuronId(mBase.GetIndex(), neuronIndices.at(randNeuron(engine)));
            if (*it != chosenTarget) {
                mBase.RequestSynapseAddition(Direction::Forward, chosenTarget, *it, synapseData);
            }
        }
    }
    mConnectMore.clear();

    for (auto it = mPruneAway.begin(); it != mPruneAway.end(); ++it) {
        mBase.RequestNeuronRemoval(*it);
    }
    mPruneAway.clear();
}

void ThresholdRegion::AcceptContributionFromNeuron(
    NeuronId neuronId, const uint8_t *contribution, size_t size)
{
    Random::Engine &engine = Random::GetThreadEngine();

    if (size == (sizeof(bool) + sizeof(size_t) * 4)) {
        const uint8_t *cur = contribution;

        bool wasTriggered = false;
        memcpy(&wasTriggered, cur, sizeof(bool));
        cur += sizeof(bool);

        size_t receivedSpikeCount = 0;
        memcpy(&receivedSpikeCount, cur, sizeof(size_t));
        cur += sizeof(size_t);

        size_t sentSpikeCount = 0;
        memcpy(&sentSpikeCount, cur, sizeof(size_t));
        cur += sizeof(size_t);

        size_t inputSynapseCount = 0;
        memcpy(&inputSynapseCount, cur, sizeof(size_t));
        cur += sizeof(size_t);

        size_t outputSynapseCount = 0;
        memcpy(&outputSynapseCount, cur, sizeof(size_t));
        //cur += sizeof(size_t);

        if (wasTriggered) {
            std::uniform_int_distribution<int> diceRoll(0, 5);
            std::uniform_int_distribution<int> coinFlip(0, 1);
            if (receivedSpikeCount > inputSynapseCount && sentSpikeCount == 0) {
                if (coinFlip(engine) && (inputSynapseCount < 2 * outputSynapseCount)) {
                    mConnectMore.push_back(neuronId);
                } else {
                    mPruneAway.push_back(neuronId);
                }
            }
        }
    }
}

size_t ThresholdRegion::ContributeToBrain(uint8_t *&contribution)
{
    size_t size = (sizeof(size_t) * 7);
    contribution = new uint8_t[size];
    uint8_t *cur = contribution;

    size_t addedNeurons = mBase.GetNeuronAdditions().size();
    memcpy(cur, &addedNeurons, sizeof(size_t));
    cur += sizeof(size_t);

    size_t removedNeurons = mBase.GetNeuronRemovals().size();
    memcpy(cur, &removedNeurons, sizeof(size_t));
    cur += sizeof(size_t);

    size_t addedSynapses = mBase.GetSynapseAdditions().size();
    memcpy(cur, &addedSynapses, sizeof(size_t));
    cur += sizeof(size_t);

    size_t removedSynapses = mBase.GetSynapseRemovals().size();
    memcpy(cur, &removedSynapses, sizeof(size_t));
    cur += sizeof(size_t);

    size_t addedChildLinks = mBase.GetChildAdditions().size();
    memcpy(cur, &addedChildLinks, sizeof(size_t));
    cur += sizeof(size_t);

    size_t removedChildLinks = mBase.GetChildRemovals().size();
    memcpy(cur, &removedChildLinks, sizeof(size_t));
    cur += sizeof(size_t);

    size_t triggeredNeurons = mBase.GetTriggeredNeurons().size();
    memcpy(cur, &triggeredNeurons, sizeof(size_t));
    //cur += sizeof(size_t);

    return size;
}

}
