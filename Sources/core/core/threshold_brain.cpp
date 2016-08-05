#include "threshold_brain.h"

namespace ThresholdModel
{

const char *ThresholdBrain::Type = "ThresholdBrain";

ThresholdBrain::ThresholdBrain(BrainBase &base, json &params) : Brain(base, params),
    mAddedNeurons(0), mRemovedNeurons(0), mAddedSynapses(0), mRemovedSynapses(0),
    mAddedChildLinks(0), mRemovedChildLinks(0), mTriggeredNeurons(0)
{
}

ThresholdBrain::~ThresholdBrain()
{
}

void ThresholdBrain::pup(PUP::er &p)
{
    p | mAddedNeurons;
    p | mRemovedNeurons;
    p | mAddedSynapses;
    p | mRemovedSynapses;
    p | mAddedChildLinks;
    p | mRemovedChildLinks;
    p | mTriggeredNeurons;
}

const char *ThresholdBrain::GetType() const
{
    return Type;
}

void ThresholdBrain::Control(size_t brainStep)
{
    CkPrintf("%u: +N %u, -N %u, +S %u, -S %u, +C %u, -C %u, TRG %u\n", 
        brainStep, mAddedNeurons, mRemovedNeurons, mAddedSynapses, mRemovedSynapses,
        mAddedChildLinks, mRemovedChildLinks, mTriggeredNeurons);

    mAddedNeurons = 0; mRemovedNeurons = 0; mAddedSynapses = 0; mRemovedSynapses = 0;
    mAddedChildLinks = 0; mRemovedChildLinks = 0; mTriggeredNeurons = 0;
}

void ThresholdBrain::AcceptContributionFromRegion(
    RegionIndex regIdx, const uint8_t *contribution, size_t size)
{
    if (size == (sizeof(size_t) * 7)) {
        const uint8_t *cur = contribution;

        size_t addedNeurons = 0;
        std::memcpy(&addedNeurons, cur, sizeof(size_t));
        cur += sizeof(size_t);
        mAddedNeurons += addedNeurons;

        size_t removedNeurons = 0;
        std::memcpy(&removedNeurons, cur, sizeof(size_t));
        cur += sizeof(size_t);
        mRemovedNeurons += removedNeurons;

        size_t addedSynapses = 0;
        std::memcpy(&addedSynapses, cur, sizeof(size_t));
        cur += sizeof(size_t);
        mAddedSynapses += addedSynapses;

        size_t removedSynapses = 0;
        std::memcpy(&removedSynapses, cur, sizeof(size_t));
        cur += sizeof(size_t);
        mRemovedSynapses += removedSynapses;

        size_t addedChildLinks = 0;
        std::memcpy(&addedChildLinks, cur, sizeof(size_t));
        cur += sizeof(size_t);
        mAddedChildLinks += addedChildLinks;

        size_t removedChildLinks = 0;
        std::memcpy(&removedChildLinks, cur, sizeof(size_t));
        cur += sizeof(size_t);
        mRemovedChildLinks += removedChildLinks;

        size_t triggeredNeurons = 0;
        std::memcpy(&triggeredNeurons, cur, sizeof(size_t));
        //cur += sizeof(size_t);
        mTriggeredNeurons += triggeredNeurons;
    }
}

}