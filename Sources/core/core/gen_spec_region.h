#pragma once
#include "region.h"
#include "google/sparse_hash_map"

namespace GenSpecModel
{

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
    size_t mInputSize, mNeuronCountX, mNeuronCountY;
    NeuronId mAccumulatorNeuron;
    json mNeuronParams;

    size_t mBrainStepCounter;
    size_t mBrainStepsPerEvolution;
    size_t mSpecialistCount;
    size_t mSpecializingGeneralistCount;

    std::map<NeuronId, float> mGenValues;
    // Map neurons to their layer number and position.
    std::map<NeuronId, std::pair<size_t, Point3D>> mTopology;

    size_t mLayerCountLimit;

    float mLayerSpacing;

    // parent: the neuron that decides who wins among his children.
    // inputProvider: where the input data comes from.
    // The distinction is there because for the first layer of neurons, the input comes
    // from the connector, but their parent is a "dummy" generalist.
    void CreateSpecialist(NeuronId parent, NeuronId inputProvider, size_t layer, const Point3D &position);

    NeuronId RequestNeuronWithPosition(const char* neuronType, size_t layer, const Point3D &position);
    void SetParamsPosition(json &params, const Point3D &position);
};

} // namespace GenSpecModel;
