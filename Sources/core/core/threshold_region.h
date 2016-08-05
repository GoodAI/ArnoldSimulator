#pragma once
#include "region.h"

namespace ThresholdModel
{

class ThresholdRegion : public Region
{
public:
    static const char *Type;

    ThresholdRegion(RegionBase &base, json &params);
    virtual ~ThresholdRegion();

    virtual void pup(PUP::er &p) override;

    virtual const char *GetType() const override;

    virtual void Control(size_t brainStep) override;

    virtual void AcceptContributionFromNeuron(
        NeuronId neuronId, const uint8_t *contribution, size_t size) override;
    virtual size_t ContributeToBrain(uint8_t *&contribution) override;

private:
    std::vector<NeuronId> mConnectMore;
    std::vector<NeuronId> mPruneAway;
};

}
