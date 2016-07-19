#include "gen_spec_region.h"

const char *GenSpecRegion::Type = "GenSpecRegion";

GenSpecRegion::GenSpecRegion(RegionBase &base, json &params) : Region(base, params)
{
    json generalists = params["generalists"];

    size_t neuronCountX, neuronCountY, inputSizeX, inputSizeY, inputStrideX, inputStrideY;
    neuronCountX = generalists["neuronCountX"].get<size_t>();
    neuronCountY = generalists["neuronCountY"].get<size_t>();
    inputSizeX = generalists["inputSizeX"].get<size_t>();
    inputSizeY = generalists["inputSizeY"].get<size_t>();
    inputStrideX = generalists["inputStrideX"].get<size_t>();
    inputStrideY = generalists["inputStrideY"].get<size_t>();

    std::string neuronParams = params["neuronParams"].dump();

    /*for (int y = 0; y < neuronCountY; y++) {
        for (int x = 0; x < neuronCountX; x++) {
            base.RequestNeuronAddition("GenSpecNeuron", neuronParams);
        }
    }*/
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
