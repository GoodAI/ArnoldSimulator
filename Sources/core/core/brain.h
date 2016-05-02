#pragma once

#include <tuple>
#include <vector>
#include <list>
#include <set>
#include <string>
#include <unordered_set>
#include <unordered_map>

#include <sparsehash/sparse_hash_set>
#include <sparsehash/sparse_hash_map>

#include <json.hpp>

#include <pup.h>
#include <pup_stl.h>
#include <completion.h>
#include <ckmulticast.h>

#include "common.h"
#include "spike.h"
#include "synapse.h"
#include "body.h"

#include "brain.decl.h"

using namespace nlohmann;

class SimulateMsg : public CkMcastBaseMsg, public CMessage_SimulateMsg
{
public:
    size_t brainStep;
};

class BrainBase;

class BrainMap : public CkArrayMap
{
public:
    BrainMap();
    BrainMap(CkMigrateMessage *msg);
    int procNum(int arrayHdl, const CkArrayIndex &index);
};

class Brain
{
public:
    Brain(BrainBase &base, json &params);
    virtual ~Brain() = default;

    Brain(const Brain &other) = delete;
    Brain &operator=(const Brain &other) = delete;

    virtual void pup(PUP::er &p) = 0;

    virtual const char *GetType() = 0;

    virtual void Control(size_t brainStep) = 0;

protected:
    BrainBase &mBase;
};

class BrainBase : public CBase_BrainBase
{
public:
    typedef std::tuple<RegionIndex, RegionType, RegionParams> RegionAddition;
    typedef std::tuple<RegionIndex, Direction, ConnectorName, NeuronType, NeuronParams, size_t> ConnectorAddition;
    typedef std::tuple<RegionIndex, Direction, ConnectorName> ConnectorRemoval;
    typedef std::tuple<Direction, RegionIndex, ConnectorName, RegionIndex, ConnectorName> Connection;

    typedef std::vector<RegionAddition> RegionAdditions;
    typedef std::vector<RegionIndex> RegionRemovals;
    typedef std::vector<ConnectorAddition> ConnectorAdditions;
    typedef std::vector<ConnectorRemoval> ConnectorRemovals;
    typedef std::vector<Connection> ConnectionAdditions;
    typedef std::vector<Connection> ConnectionRemovals;

    typedef uint32_t TerminalId;

    struct Terminal
    {
        TerminalId id;
        ConnectorName name;
        Spike::Type spikeType;
        NeuronId firstNeuron;
        size_t neuronCount;
        std::vector<uint8_t> data;
        std::unordered_set<RemoteConnector> connections;
    };

    typedef std::unordered_map<TerminalId, Terminal> Terminals;
    typedef std::unordered_map<ConnectorName, TerminalId> TerminalNameToId;
    typedef google::sparse_hash_map<NeuronId, TerminalId> NeuronToTerminalId;

    static Brain *CreateBrain(const BrainType &type, BrainBase &base, json &params);

    BrainBase(const BrainType &type, const BrainParams &params);
    BrainBase(CkMigrateMessage *msg);
    ~BrainBase();

    BrainBase(const BrainBase &other) = delete;
    BrainBase &operator=(const BrainBase &other) = delete;

    void pup(PUP::er &p);

    const char *GetType();

    const Terminals &GetTerminals() const;
    void CreateTerminal(const ConnectorName &name, Spike::Type spikeType, NeuronId firstNeuron, size_t neuronCount);
    void DeleteTerminal(const ConnectorName &name);
    void ConnectTerminal(const ConnectorName &name, const RemoteConnector &destination);
    void DisconnectTerminal(const ConnectorName &name, const RemoteConnector &destination);

    void AddRegion(RegionIndex regIdx, const RegionType &type, const RegionParams &params);
    void RemoveRegion(RegionIndex regIdx);
    void AddConnector(RegionIndex regIdx, Direction direction, const ConnectorName &name, size_t size);
    void RemoveConnector(RegionIndex regIdx, Direction direction, const ConnectorName &name);
    void AddConnection(Direction direction,
        RegionIndex srcRegIdx, const ConnectorName &srcConnectorName,
        RegionIndex destRegIdx, const ConnectorName &destConnectorName);
    void RemoveConnection(Direction direction,
        RegionIndex srcRegIdx, const ConnectorName &srcConnectorName,
        RegionIndex destRegIdx, const ConnectorName &destConnectorName);

    void PushSensoMotoricData(std::string &terminalName, std::vector<uint8_t> &data);
    void PullSensoMotoricData(std::string &terminalName, std::vector<uint8_t> &data);

    void StartSimulation();
    void StopSimulation();
    void SetBrainStepsPerBodyStep(size_t steps);
    void RequestSynapticTransfers(RequestId requestId);

    void Simulate();
    void ReceiveTerminalData(Spike::BrainSink &data);

    void ChangeTopologyDone(long triggeredNeurons);
    void RegionSimulateDone(CkReductionMsg *msg);

private:
    bool mShouldStop;
    size_t mBrainStepsPerBodyStep;
    std::list<RequestId> mSynapticTransferRequests;

    size_t mBrainStep;

    TerminalId mTerminalIdCounter;
    Terminals mTerminals;
    TerminalNameToId mTerminalNameToId;
    NeuronToTerminalId mNeuronToTerminalId;

    RegionAdditions mRegionAdditions;
    RegionRemovals mRegionsToRemove;
    std::vector<ConnectorAddition> mConnectorsToAdd;
    std::vector<ConnectorRemoval> mConnectorsToRemove;
    std::vector<Connection> mConnectionsToAdd;
    std::vector<Connection> mConnectionsToRemove;

    Body *mBody;
    Brain *mBrain;
};

class ThresholdBrain : public Brain
{
public:
    static const char *Type;

    ThresholdBrain(BrainBase &base, json &params);
    virtual ~ThresholdBrain();

    virtual void pup(PUP::er &p) override;

    virtual const char *GetType() override;

    virtual void Control(size_t brainStep) override;

    /*
    void SomeInternalFunction(SomeType1 someArg1, SomeType2 someArg2);

    entry void SomeFunctionForRegions(RegionIndex caller, SomeType1 someArg1, SomeType2 someArg2);
    */
};
