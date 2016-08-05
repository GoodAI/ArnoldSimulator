#include "threshold_init.h"
#include "threshold_neuron.h"
#include "region.h"
#include "brain.h"

namespace ThresholdModel
{

void init(NeuronFactory *neuronFactory, RegionFactory *regionFactory, BrainFactory *brainFactory)
{
    neuronFactory->Register("ThresholdNeuron",
        NeuronBuilder<ThresholdNeuron>);

    regionFactory->Register("ThresholdRegion",
        RegionBuilder<ThresholdRegion>);

    brainFactory->Register("ThresholdBrain",
        BrainBuilder<ThresholdBrain>);
}

}
