#include "threshold_init.h"
#include "threshold_neuron.h"

namespace ThresholdModel
{

void init(NeuronFactory *neuronFactory)
{
    neuronFactory->Register("ThresholdNeuron",
        NeuronBuilder<ThresholdNeuron>);
}

}
