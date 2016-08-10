#pragma once

#include <cstdint>
#include <random>
#include <atomic>

#include "common.h"

class Random
{
public:
    typedef std::mt19937_64 Engine;

    static Engine &GetThreadEngine();

private:
    Random();
    Random(const Random &other) = delete;
    Random &operator=(const Random &other) = delete;

    static Random instance;

    Engine mSeedEngine;
    std::atomic_flag mSeedGuard = ATOMIC_FLAG_INIT;
    std::uniform_int_distribution<uint64_t> mSeedDistribution;

    static thread_local bool mTlsAlreadyInitialized;
    static thread_local Engine mTlsEngine;
};
