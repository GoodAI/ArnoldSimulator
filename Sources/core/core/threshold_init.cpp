#include "threshold_init.h"
#include "threshold_neuron.h"
#include "threshold_region.h"
#include "threshold_brain.h"
#include "region.h"
#include "brain.h"

namespace ThresholdModel
{

void Init(NeuronFactory *neuronFactory, RegionFactory *regionFactory, BrainFactory *brainFactory)
{
    neuronFactory->Register("ThresholdNeuron",
        NeuronBuilder<ThresholdNeuron>);

    regionFactory->Register("ThresholdRegion",
        RegionBuilder<ThresholdRegion>);

    brainFactory->Register("ThresholdBrain",
        BrainBuilder<ThresholdBrain>);
}

}
