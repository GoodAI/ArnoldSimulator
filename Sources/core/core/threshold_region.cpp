#include "threshold_region.h"

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
    /*
    auto engine = Random::GetThreadEngine();
    
    if (!mConnectMore.empty()) {
        std::vector<NeuronIndex> neuronIndices(
            mBase.GetNeuronIndices().begin(), mBase.GetNeuronIndices().end());
        std::uniform_int_distribution<size_t> randNeuron(0, neuronIndices.size() - 1);
        std::uniform_int_distribution<uint16_t> randDelay(0, 1);
        std::uniform_real_distribution<double> randWeight(0, 1.0);
        for (auto it = mConnectMore.begin(); it != mConnectMore.end(); ++it) {
            Synapse::Data synapseData;
            Synapse::Initialize(Synapse::Type::Lagging, synapseData);
            LaggingSynapse *synapse = static_cast<LaggingSynapse *>(Synapse::Edit(synapseData));
            synapse->SetDelay(synapseData, randDelay(engine));
            synapse->SetWeight(synapseData, randWeight(engine));
            mBase.RequestSynapseAddition(Direction::Forward, 
                neuronIndices.at(randNeuron(engine)), *it, synapseData);
        }
    }

    for (auto it = mPruneAway.begin(); it != mPruneAway.end(); ++it) {
        mBase.RequestNeuronRemoval(*it);
    }
    */
}

void ThresholdRegion::AcceptContributionFromNeuron(
    NeuronId neuronId, const uint8_t *contribution, size_t size)
{
    if (size == (sizeof(size_t) * 4)) {
        const uint8_t *cur = contribution;

        size_t receivedSpikeCount = 0;
        std::memcpy(&receivedSpikeCount, cur, sizeof(size_t));
        cur += sizeof(size_t);

        size_t sentSpikeCount = 0;
        std::memcpy(&sentSpikeCount, cur, sizeof(size_t));
        cur += sizeof(size_t);

        size_t inputSynapseCount = 0;
        std::memcpy(&inputSynapseCount, cur, sizeof(size_t));
        cur += sizeof(size_t);

        size_t outputSynapseCount = 0;
        std::memcpy(&outputSynapseCount, cur, sizeof(size_t));
        //cur += sizeof(size_t);

        /*
        if (receivedSpikeCount > inputSynapseCount && sentSpikeCount == 0) {
            if (inputSynapseCount < 10 * outputSynapseCount) {
                mConnectMore.push_back(neuronId);
            } else {
                mPruneAway.push_back(neuronId);
            }
        }
        */
    }
}

size_t ThresholdRegion::ContributeToBrain(uint8_t *&contribution)
{
    size_t size = (sizeof(size_t) * 7);
    contribution = new uint8_t[size];
    uint8_t *cur = contribution;

    size_t addedNeurons = mBase.GetNeuronAdditions().size();
    std::memcpy(cur, &addedNeurons, sizeof(size_t));
    cur += sizeof(size_t);

    size_t removedNeurons = mBase.GetNeuronRemovals().size();
    std::memcpy(cur, &removedNeurons, sizeof(size_t));
    cur += sizeof(size_t);

    size_t addedSynapses = mBase.GetSynapseAdditions().size();
    std::memcpy(cur, &addedSynapses, sizeof(size_t));
    cur += sizeof(size_t);

    size_t removedSynapses = mBase.GetSynapseRemovals().size();
    std::memcpy(cur, &removedSynapses, sizeof(size_t));
    cur += sizeof(size_t);

    size_t addedChildLinks = mBase.GetChildAdditions().size();
    std::memcpy(cur, &addedChildLinks, sizeof(size_t));
    cur += sizeof(size_t);

    size_t removedChildLinks = mBase.GetChildRemovals().size();
    std::memcpy(cur, &removedChildLinks, sizeof(size_t));
    cur += sizeof(size_t);

    size_t triggeredNeurons = mBase.GetTriggeredNeurons().size();
    std::memcpy(cur, &triggeredNeurons, sizeof(size_t));
    //cur += sizeof(size_t);

    return size;
}

}
