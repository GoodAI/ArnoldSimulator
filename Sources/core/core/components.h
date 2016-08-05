#pragma once

#include "component_registry.h"
#include "component_factory.h"

class Neuron;
class NeuronBase;

typedef ComponentFactory<Neuron, NeuronBase> NeuronFactory;
