#include "init.h"

void initializeComponents()
{
    GenSpecModel::init(NeuronFactory::GetInstance());
}