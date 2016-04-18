#include <chrono>

#include "random.h"

Random::Random()
{
#ifdef REPRODUCIBLE_EXECUTION
    mSeedEngine.seed(0);
#else
    mSeedEngine.seed(std::chrono::high_resolution_clock::now().time_since_epoch().count());
#endif
}

Random Random::instance;

Random::Engines::reference Random::GetThreadEngine()
{
    bool alreadyInitializedEngine = false;
    Engines::reference engine = instance.mEngines.local(alreadyInitializedEngine);

    if (!alreadyInitializedEngine) {
        tbb::spin_mutex::scoped_lock lock(instance.mSeedGuard);
        engine.seed(instance.mSeedDistribution(instance.mSeedEngine));
    }

    return engine;
}
