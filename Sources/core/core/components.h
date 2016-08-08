#pragma once

#include "instance_cache.h"
#include "model_component_factory.h"

class Neuron;
class NeuronBase;

class Region;
class RegionBase;

class Brain;
class BrainBase;

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

typedef ModelComponentFactory<Neuron, NeuronBase> NeuronFactory;
typedef ModelComponentFactory<Region, RegionBase> RegionFactory;
typedef ModelComponentFactory<Brain, BrainBase> BrainFactory;

class SynapseEditor;

typedef InstanceCache<SynapseEditor> SynapseEditorCache;
//typedef InstanceCache<Spike::Editor> SpikeEditorCache;
