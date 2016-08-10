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

thread_local bool Random::mTlsAlreadyInitialized = false;
thread_local Random::Engine Random::mTlsEngine;

Random::Engine &Random::GetThreadEngine()
{
    if (!mTlsAlreadyInitialized) {
        mTlsAlreadyInitialized = true;
        while (instance.mSeedGuard.test_and_set(std::memory_order_acquire)) { ; }
        mTlsEngine.seed(instance.mSeedDistribution(instance.mSeedEngine));
        instance.mSeedGuard.clear(std::memory_order_release);
    }

    return mTlsEngine;
}
