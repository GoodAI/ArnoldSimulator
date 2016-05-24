#pragma once

#include <cstddef>
#include <cstdint>
#include <tuple>
#include <string>
#include <vector>
#include <unordered_set>

#include <tbb/tbbmalloc_proxy.h>

#include <pup.h>
#include <pup_stl.h>

//#define REPRODUCIBLE_EXECUTION

typedef uint32_t RegionIndex;
typedef uint32_t NeuronIndex;
typedef uint32_t NeuronId;
typedef uint32_t TerminalId;
typedef uint64_t RequestId;

#define REGION_INDEX_MASK 0xFFC00000
#define REGION_INDEX_OFFSET 22

#define BRAIN_REGION_INDEX 0
#define TEMP_REGION_INDEX (REGION_INDEX_MASK >> REGION_INDEX_OFFSET)
#define DELETED_NEURON_ID UINT32_MAX

#define REGION_INDEX_MIN (BRAIN_REGION_INDEX + 1)
#define REGION_INDEX_MAX (TEMP_REGION_INDEX - 1)
#define NEURON_INDEX_MIN 0
#define NEURON_INDEX_MAX ((DELETED_NEURON_ID & ~REGION_INDEX_MASK) - 1)

inline NeuronId GetNeuronId(RegionIndex regionIndex, NeuronIndex neuronIndex)
{
    return (regionIndex << REGION_INDEX_OFFSET) & neuronIndex;
}

inline NeuronIndex GetNeuronIndex(NeuronId neuronId)
{
    return neuronId & ~REGION_INDEX_MASK;
}

inline RegionIndex GetRegionIndex(NeuronId neuronId)
{
    return neuronId >> REGION_INDEX_OFFSET;
}

enum class Direction : uint8_t
{
    Forward,
    Backward
};

inline void operator|(PUP::er &p, Direction &direction)
{
    pup_bytes(&p, static_cast<void *>(&direction), sizeof(Direction));
}

#define OPPOSITE_DIRECTION(direction) (direction == Direction::Forward ? Direction::Backward : Direction::Forward)

typedef std::string BrainName;
typedef std::string BrainType;
typedef std::string BrainParams;
typedef std::string RegionName;
typedef std::string RegionType;
typedef std::string RegionParams;
typedef std::string NeuronType;
typedef std::string NeuronParams;

typedef std::string ConnectorName;
typedef std::pair<RegionIndex, ConnectorName> RemoteConnector;

typedef std::tuple<float, float, float> Point3D;
typedef std::tuple<float, float, float> Size3D;
typedef std::pair<Point3D, Size3D> Box3D;
typedef std::vector<Box3D> Boxes;

#define BOX_DEFAULT_MARGIN 10.0f
#define BOX_DEFAULT_SIZE_X 20.0f
#define BOX_DEFAULT_SIZE_Y 20.0f
#define BOX_DEFAULT_SIZE_Z 50.0f

inline bool IsInsideOfAny(const Point3D &point, const Boxes &boxes)
{
    for (auto it = boxes.begin(); it != boxes.end(); ++it) {
        float xLower = std::get<0>(it->first);
        float xUpper = std::get<0>(it->first) + std::get<0>(it->second);
        bool xWithin = (std::get<0>(point) >= xLower) && (std::get<0>(point) <= xUpper);

        float yLower = std::get<1>(it->first);
        float yUpper = std::get<1>(it->first) + std::get<1>(it->second);
        bool yWithin = (std::get<1>(point) >= yLower) && (std::get<1>(point) <= yUpper);

        float zLower = std::get<2>(it->first);
        float zUpper = std::get<2>(it->first) + std::get<2>(it->second);
        bool zWithin = (std::get<2>(point) >= zLower) && (std::get<2>(point) <= zUpper);

        if (xWithin && yWithin && zWithin) return true;
    }

    return false;
}

typedef std::tuple<NeuronId, NeuronType, NeuronParams> NeuronAdditionRequest;
typedef std::tuple<NeuronId, NeuronType, Point3D> NeuronAdditionReport;
typedef std::pair<NeuronId, NeuronId> ChildLink;

typedef std::vector<NeuronAdditionRequest> NeuronAdditionRequests;
typedef std::vector<NeuronAdditionReport> NeuronAdditionReports;
typedef std::vector<NeuronId> NeuronRemovals;
typedef std::vector<ChildLink> ChildLinks;

typedef std::unordered_set<NeuronId> NeuronsTriggered;
typedef std::unordered_set<NeuronIndex> NeuronIndices;
typedef std::unordered_set<RegionIndex> RegionIndices;

typedef std::tuple<RegionIndex, RegionName, RegionType, RegionParams> RegionAdditionRequest;
typedef std::tuple<RegionIndex, RegionName, RegionType, Box3D> RegionAdditionReport;
typedef std::tuple<RegionIndex, Box3D> RegionRepositionRequest;
typedef std::tuple<RegionIndex, Direction, ConnectorName, NeuronType, NeuronParams, size_t> ConnectorAdditionRequest;
typedef std::tuple<RegionIndex, Direction, ConnectorName, size_t> ConnectorAdditionReport;
typedef std::tuple<RegionIndex, Direction, ConnectorName> ConnectorRemoval;
typedef std::tuple<Direction, RegionIndex, ConnectorName, RegionIndex, ConnectorName> Connection;

typedef std::vector<RegionAdditionRequest> RegionAdditionRequests;
typedef std::vector<RegionAdditionReport> RegionAdditionReports;
typedef std::vector<RegionRepositionRequest> RegionRepositionRequests;
typedef std::vector<RegionIndex> RegionRemovals;
typedef std::vector<ConnectorAdditionRequest> ConnectorAdditionRequests;
typedef std::vector<ConnectorAdditionReport> ConnectorAdditionReports;
typedef std::vector<ConnectorRemoval> ConnectorRemovals;
typedef std::vector<Connection> Connections;

namespace PUP {

template <class A, class B>
inline void operator|(er &p, std::tuple<A, B> &t);
template <class A, class B, class C>
inline void operator|(er &p, std::tuple<A, B, C> &t);
template <class A, class B, class C, class D>
inline void operator|(er &p, std::tuple<A, B, C, D> &t);
template <class A, class B, class C, class D, class E>
inline void operator|(er &p, std::tuple<A, B, C, D, E> &t);
template <class A, class B, class C, class D, class E, class F>
inline void operator|(er &p, std::tuple<A, B, C, D, F> &t);

template <class A, class B>
inline void operator|(er &p, std::tuple<A, B> &t)
{
    p | std::get<0>(t);
    p | std::get<1>(t);
}

template <class A, class B, class C>
inline void operator|(er &p, std::tuple<A, B, C> &t)
{
    p | std::get<0>(t);
    p | std::get<1>(t);
    p | std::get<2>(t);
}

template <class A, class B, class C, class D>
inline void operator|(er &p, std::tuple<A, B, C, D> &t)
{
    p | std::get<0>(t);
    p | std::get<1>(t);
    p | std::get<2>(t);
    p | std::get<3>(t);
}

template <class A, class B, class C, class D, class E>
inline void operator|(er &p, std::tuple<A, B, C, D, E> &t)
{
    p | std::get<0>(t);
    p | std::get<1>(t);
    p | std::get<2>(t);
    p | std::get<3>(t);
    p | std::get<4>(t);
}

template <class A, class B, class C, class D, class E, class F>
inline void operator|(er &p, std::tuple<A, B, C, D, F> &t)
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
void hash_combine(std::size_t &seed, T const &key)
{
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
