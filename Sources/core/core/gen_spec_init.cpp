#include "gen_spec_init.h"
#include "gen_spec_acc_neuron.h"
#include "gen_spec_input_neuron.h"
#include "gen_spec_neuron.h"
#include "gen_spec_next_digit_neuron.h"
#include "gen_spec_output_neuron.h"
#include "gen_spec_region.h"
#include "gen_spec_brain.h"

namespace GenSpecModel
{

Neuron *CreateGenSpecAccNeuron(NeuronBase &base, json &params)
{
    return new GenSpecAccNeuron(base, params);
}

void init(NeuronFactory *neuronFactory, RegionFactory *regionFactory, BrainFactory *brainFactory)
{
    // Shown are three ways of neuron registration.

    // Example of a function pointer use.
    neuronFactory->Register("GenSpecAccNeuron", CreateGenSpecAccNeuron);

    // Example of lambda function use.
    neuronFactory->Register("GenSpecInputNeuron",
        [](NeuronBase &base, json &params) -> Neuron* {
            return new GenSpecInputNeuron(base, params);
        });

    // DRY approach to function pointers.
    neuronFactory->Register("GenSpecNeuron",
        NeuronBuilder<GenSpecNeuron>);
    neuronFactory->Register("GenSpecNextDigitNeuron",
        NeuronBuilder<GenSpecNextDigitNeuron>);
    neuronFactory->Register("GenSpecOutputNeuron",
        NeuronBuilder<GenSpecOutputNeuron>);

    regionFactory->Register("GenSpecRegion",
        RegionBuilder<GenSpecRegion>);

    brainFactory->Register("GenSpecBrain",
        BrainBuilder<GenSpecBrain>);
}

}
