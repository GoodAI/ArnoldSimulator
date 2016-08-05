#pragma once
#include "brain.h"

namespace ThresholdModel
{

class ThresholdBrain : public Brain
{
public:
    static const char *Type;

    ThresholdBrain(BrainBase &base, json &params);
    virtual ~ThresholdBrain();

    virtual void pup(PUP::er &p) override;

    virtual const char *GetType() const override;

    virtual void Control(size_t brainStep) override;

    virtual void AcceptContributionFromRegion(
        RegionIndex regIdx, const uint8_t *contribution, size_t size) override;

private:
    size_t mAddedNeurons;
    size_t mRemovedNeurons;
    size_t mAddedSynapses;
    size_t mRemovedSynapses;
    size_t mAddedChildLinks;
    size_t mRemovedChildLinks;
    size_t mTriggeredNeurons;
};

}
