#include "init.h"
#include "components.h"
#include "gen_spec_init.h"
#include "threshold_init.h"

void initializeComponents()
{
    NeuronFactory *neuronFactory = NeuronFactory::GetInstance();
    RegionFactory *regionFactory = RegionFactory::GetInstance();
    BrainFactory *brainFactory = BrainFactory::GetInstance();

    GenSpecModel::init(neuronFactory, regionFactory, brainFactory);
    ThresholdModel::init(neuronFactory, regionFactory, brainFactory);
}
