#pragma once

#include "components.h"
#include "gen_spec_neuron.h"

namespace GenSpecModel
{

class Foo
{
    
};

// This function is called from the core init code.
void init(NeuronFactory *neuronFactory);

Neuron *CreateGenSpecNeuron(NeuronBase &base, json &params);

template<typename NeuronType>
Neuron* NeuronBuilder(NeuronBase &base, json &params)
{
    return new NeuronType(base, params);
}

}
