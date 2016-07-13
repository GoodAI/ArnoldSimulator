#pragma once
#include "region.h"

class GenSpecRegion : public Region
{
public:
    static const char *Type;

    GenSpecRegion(RegionBase &base, json &params);
    virtual ~GenSpecRegion();

    virtual void pup(PUP::er &p) override;

    virtual const char *GetType() const override;

    virtual void Control(size_t brainStep) override;

    virtual void AcceptContributionFromNeuron(
        NeuronId neuronId, const uint8_t *contribution, size_t size) override;
    virtual size_t ContributeToBrain(uint8_t *&contribution) override;
//private:
};
