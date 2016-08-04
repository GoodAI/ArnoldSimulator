#pragma once
#include "brain.h"

namespace GenSpecModel
{

class GenSpecBrain : public Brain
{
public:
    static const char *Type;

    GenSpecBrain(BrainBase &base, json &params);
    virtual ~GenSpecBrain();

    virtual void pup(PUP::er &p) override;

    virtual const char *GetType() const override;

    virtual void Control(size_t brainStep) override;

    virtual void AcceptContributionFromRegion(
        RegionIndex regIdx, const uint8_t *contribution, size_t size) override;
private:
};

} // namespace GenSpecModel;