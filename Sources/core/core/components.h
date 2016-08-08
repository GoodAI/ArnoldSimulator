#pragma once

#include "instance_cache.h"
#include "model_component_factory.h"
#include "synapse.h"
#include "spike.h"
#include "neuron.h"
#include "region.h"
#include "brain.h"

// Convenience templates that can be used for most of the factories.
template<typename TNeuron>
Neuron* NeuronBuilder(NeuronBase &base, nlohmann::json &params)
{
    return new TNeuron(base, params);
}

template<typename TRegion>
Region* RegionBuilder(RegionBase &base, nlohmann::json &params)
{
    return new TRegion(base, params);
}

template<typename TBrain>
Brain* BrainBuilder(BrainBase &base, nlohmann::json &params)
{
    return new TBrain(base, params);
}

typedef ModelComponentFactory<Neuron, NeuronBase, Token64> NeuronFactory;
typedef ModelComponentFactory<Region, RegionBase, Token64> RegionFactory;
typedef ModelComponentFactory<Brain, BrainBase, Token64> BrainFactory;

typedef InstanceCache<Synapse::Editor, Synapse::Type> SynapseEditorCache;
typedef InstanceCache<Spike::Editor, Spike::Type> SpikeEditorCache;
