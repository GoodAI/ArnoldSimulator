#pragma once

#include <cstddef>
#include <cstdint>
#include <tuple>
#include <string>
#include <vector>

#include <tbb/tbbmalloc_proxy.h>

#include <pup.h>
#include <pup_stl.h>

//#define REPRODUCIBLE_EXECUTION

typedef uint32_t RegionIndex;
typedef uint32_t NeuronIndex;
typedef uint32_t NeuronId;
typedef uint64_t RequestId;

#define REGION_INDEX_MASK 0xFFC00000
#define REGION_INDEX_OFFSET 22

#define BRAIN_REGION_INDEX 0
#define TEMP_REGION_INDEX REGION_INDEX_MASK
#define DELETED_NEURON_ID UINT32_MAX

inline NeuronId GetNeuronId(RegionIndex regionIndex, NeuronIndex neuronIndex) {
    return (regionIndex << REGION_INDEX_OFFSET) & neuronIndex;
}

inline NeuronIndex GetNeuronIndex(NeuronId neuronId) {
    return neuronId & ~REGION_INDEX_MASK;
}

inline RegionIndex GetRegionIndex(NeuronId neuronId) {
    return neuronId >> REGION_INDEX_OFFSET;
}

enum class Direction : uint8_t
{
    Forward,
    Backward
};

inline void operator|(PUP::er &p, Direction &direction) {
    pup_bytes(&p, (void *)&direction, sizeof(Direction));
}

#define OPPOSITE_DIRECTION(direction) (direction == Direction::Forward ? Direction::Backward : Direction::Forward)

typedef std::string BrainType;
typedef std::string BrainParams;
typedef std::string RegionType;
typedef std::string RegionParams;
typedef std::string NeuronType;
typedef std::string NeuronParams;

typedef std::string ConnectorName;
typedef std::pair<RegionIndex, ConnectorName> RemoteConnector;

typedef std::tuple<NeuronId, NeuronType, NeuronParams> NeuronAddition;
typedef std::pair<NeuronId, NeuronId> ChildAddition;
typedef std::pair<NeuronId, NeuronId> ChildRemoval;

typedef std::vector<NeuronAddition> NeuronAdditions;
typedef std::vector<NeuronId> NeuronRemovals;
typedef std::vector<ChildAddition> ChildAdditions;
typedef std::vector<ChildRemoval> ChildRemovals;

typedef std::vector<NeuronId> NeuronsTriggered;

namespace PUP {

template <class A, class B>
inline void operator|(er &p, typename std::tuple<A, B> &t);
template <class A, class B, class C>
inline void operator|(er &p, typename std::tuple<A, B, C> &t);
template <class A, class B, class C, class D>
inline void operator|(er &p, typename std::tuple<A, B, C, D> &t);
template <class A, class B, class C, class D, class E>
inline void operator|(er &p, typename std::tuple<A, B, C, D, E> &t);
template <class A, class B, class C, class D, class E, class F>
inline void operator|(er &p, typename std::tuple<A, B, C, D, F> &t);

template <class A, class B>
inline void operator|(er &p, typename std::tuple<A, B> &t)
{
    p | std::get<0>(t);
    p | std::get<1>(t);
}

template <class A, class B, class C>
inline void operator|(er &p, typename std::tuple<A, B, C> &t)
{
    p | std::get<0>(t);
    p | std::get<1>(t);
    p | std::get<2>(t);
}

template <class A, class B, class C, class D>
inline void operator|(er &p, typename std::tuple<A, B, C, D> &t)
{
    p | std::get<0>(t);
    p | std::get<1>(t);
    p | std::get<2>(t);
    p | std::get<3>(t);
}

template <class A, class B, class C, class D, class E>
inline void operator|(er &p, typename std::tuple<A, B, C, D, E> &t)
{
    p | std::get<0>(t);
    p | std::get<1>(t);
    p | std::get<2>(t);
    p | std::get<3>(t);
    p | std::get<4>(t);
}

template <class A, class B, class C, class D, class E, class F>
inline void operator|(er &p, typename std::tuple<A, B, C, D, F> &t)
{
    p | std::get<0>(t);
    p | std::get<1>(t);
    p | std::get<2>(t);
    p | std::get<3>(t);
    p | std::get<4>(t);
    p | std::get<5>(t);
}

}

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
