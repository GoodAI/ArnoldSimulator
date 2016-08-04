#include "gen_spec_brain.h"

namespace GenSpecModel
{

const char *GenSpecBrain::Type = "GenSpecBrain";

GenSpecBrain::GenSpecBrain(BrainBase &base, json &params) : Brain(base, params)
{
}

GenSpecBrain::~GenSpecBrain()
{
}

void GenSpecBrain::pup(PUP::er &p)
{
}

const char *GenSpecBrain::GetType() const
{
    return Type;
}

void GenSpecBrain::Control(size_t brainStep)
{
}

void GenSpecBrain::AcceptContributionFromRegion(
    RegionIndex regIdx, const uint8_t *contribution, size_t size)
{
}

} // namespace GenSpecModel;
