#pragma once
#include "region.h"
#include "google/sparse_hash_map"

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
protected:
    size_t mInputSize;
    NeuronId mAccumulatorNeuron;
    json mNeuronParams;

    size_t mBrainStepCounter;
    size_t mBrainStepsPerEvolution;
    size_t mSpecialistCount;
    size_t mSpecializingGeneralistCount;
    google::sparse_hash_map<NeuronId, float> mGenValues;

    // parent: the neuron that decides who wins among his children.
    // inputProvider: where the input data comes from.
    // The distinction is there because for the first layer of neurons, the input comes
    // from the connector, but their parent is a "dummy" generalist.
    void CreateSpecialists(NeuronId parent, NeuronId inputProvider);
};
