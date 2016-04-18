#pragma once

#include <cstdint>
#include <random>

#include <tbb/spin_mutex.h>
#include <tbb/enumerable_thread_specific.h>

#include "common.h"

class Random
{
public:
    typedef tbb::enumerable_thread_specific<std::mt19937_64> Engines;

    static Engines::reference GetThreadEngine();

private:
    Random();
    Random(const Random &other) = delete;
    Random &operator=(const Random &other) = delete;

    static Random instance;

    tbb::spin_mutex mSeedGuard;
    std::mt19937_64 mSeedEngine;
    std::uniform_int_distribution<uint64_t> mSeedDistribution;

    Engines mEngines;
};
