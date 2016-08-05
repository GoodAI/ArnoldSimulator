#include "init.h"
#include "components.h"
#include "gen_spec_init.h"
#include "threshold_init.h"

void initializeComponents()
{
    GenSpecModel::init(
        NeuronFactory::GetInstance(),
        RegionFactory::GetInstance(),
        BrainFactory::GetInstance());
    ThresholdModel::init(
        NeuronFactory::GetInstance(),
        RegionFactory::GetInstance(),
        BrainFactory::GetInstance());
}
