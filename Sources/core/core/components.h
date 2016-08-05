#pragma once

#include "instance_cache.h"
#include "model_component_factory.h"

class Neuron;
class NeuronBase;

// Convenience template that can be used for most of neuron factories.
template<typename NeuronType>
Neuron* NeuronBuilder(NeuronBase &base, nlohmann::json &params)
{
    return new NeuronType(base, params);
}

typedef ModelComponentFactory<Neuron, NeuronBase> NeuronFactory;
