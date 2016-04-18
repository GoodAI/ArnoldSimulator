#pragma once

#include <cstdint>
#include <string>

#include <pup.h>

//#define REPRODUCIBLE_EXECUTION

typedef uint32_t GateLaneIdx;
typedef uint32_t NeuronId;
typedef uint32_t RegionId;
typedef uint64_t RequestId;

#define DELETED_NEURON_ID 0
#define BRAIN_REGION_ID UINT32_MAX;

enum class Direction : uint8_t
{
    Forward,
    Backward
};

inline void operator|(PUP::er &p, Direction &direction) {
    pup_bytes(&p, (void *)&direction, sizeof(Direction));
}

#define OPPOSITE_DIRECTION(direction) (direction == Direction::Forward ? Direction::Backward : Direction::Forward)

typedef std::string ConnectorName;
typedef std::pair<RegionId, ConnectorName> RemoteConnector;
typedef std::tuple<RegionId, Direction, std::string, size_t> ConnectorAddition;
typedef std::tuple<RegionId, Direction, std::string> ConnectorRemoval;
typedef std::tuple<Direction, RegionId, std::string, RegionId, std::string> Connection;

template<typename T>
void hash_combine(std::size_t &seed, T const &key) {
    std::hash<T> hasher;
    seed ^= hasher(key) + 0x9e3779b97f4a7c15 + (seed << 6) + (seed >> 2);
}

namespace std 
{
    template<typename T1, typename T2>
    struct hash<std::pair<T1, T2>> 
    {
        std::size_t operator()(std::pair<T1, T2> const &p) const {
            std::size_t seed(0);
            ::hash_combine(seed, p.first);
            ::hash_combine(seed, p.second);
            return seed;
        }
    };
}
