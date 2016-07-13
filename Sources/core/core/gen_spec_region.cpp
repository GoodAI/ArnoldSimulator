#include "gen_spec_region.h"

const char *GenSpecRegion::Type = "GenSpecRegion";

GenSpecRegion::GenSpecRegion(RegionBase &base, json &params) : Region(base, params)
{
}

GenSpecRegion::~GenSpecRegion()
{
}

void GenSpecRegion::pup(PUP::er &p)
{
    //p | mConnectMore;
}

const char *GenSpecRegion::GetType() const
{
    return Type;
}

void GenSpecRegion::Control(size_t brainStep)
{
}

void GenSpecRegion::AcceptContributionFromNeuron(NeuronId neuronId, const uint8_t *contribution, size_t size)
{
}

size_t GenSpecRegion::ContributeToBrain(uint8_t *&contribution)
{
    return 0;
}
