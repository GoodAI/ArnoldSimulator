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

struct ViewportUpdate
{
    ViewportUpdate();

    size_t sinceBrainStep;
    size_t brainStepCount;
    RegionAdditionReports addedRegions;
    RegionAdditionReports repositionedRegions;
    RegionRemovals removedRegions;
    ConnectorAdditionReports addedConnectors;
    ConnectorRemovals removedConnectors;
    Connections addedConnections;
    Connections removedConnections;
    NeuronAdditionReports addedNeurons;
    NeuronAdditionReports repositionedNeurons;
    NeuronRemovals removedNeurons;
    Synapse::Links addedSynapses;
    Synapse::Links spikedSynapses;
    Synapse::Links removedSynapses;
    ChildLinks addedChildren;
    ChildLinks removedChildren;

    void pup(PUP::er &p);
};

class SimulateMsg : public CkMcastBaseMsg, public CMessage_SimulateMsg
{
public:
    bool doProgress;
    size_t brainStep;
    Boxes roiBoxes;
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

    virtual const char *GetType() const = 0;

    virtual void Control(size_t brainStep) = 0;

protected:
    BrainBase &mBase;
};

class BrainBase : public CBase_BrainBase
{
public:
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

    typedef std::pair<RequestId, bool> ViewportUpdateRequest;
    typedef std::list<ViewportUpdateRequest> ViewportUpdateRequests;

    static Brain *CreateBrain(const BrainType &type, BrainBase &base, json &params);

    BrainBase(const BrainType &type, const BrainParams &params);
    BrainBase(CkMigrateMessage *msg);
    ~BrainBase();

    BrainBase(const BrainBase &other) = delete;
    BrainBase &operator=(const BrainBase &other) = delete;

    void pup(PUP::er &p);

    const char *GetType() const;
    const char *GetName() const;

    const Terminals &GetTerminals() const;
    void CreateTerminal(const ConnectorName &name, Spike::Type spikeType, size_t neuronCount);
    void DeleteTerminal(const ConnectorName &name);
    void ConnectTerminal(const ConnectorName &name, const RemoteConnector &destination);
    void DisconnectTerminal(const ConnectorName &name, const RemoteConnector &destination);

    RegionIndex RequestRegionAddition(const RegionType &type, const RegionParams &params);
    void RequestRegionRemoval(RegionIndex regIdx);
    void RequestConnectorAddition(RegionIndex regIdx, Direction direction, const ConnectorName &name, 
        const NeuronType &neuronType, const NeuronParams &neuronParams, size_t neuronCount);
    void RequestConnectorRemoval(RegionIndex regIdx, Direction direction, const ConnectorName &name);
    void RequestConnectionAddition(Direction direction,
        RegionIndex srcRegIdx, const ConnectorName &srcConnectorName,
        RegionIndex destRegIdx, const ConnectorName &destConnectorName);
    void RequestConnectionRemoval(Direction direction,
        RegionIndex srcRegIdx, const ConnectorName &srcConnectorName,
        RegionIndex destRegIdx, const ConnectorName &destConnectorName);

    void PushSensoMotoricData(std::string &terminalName, std::vector<uint8_t> &data);
    void PullSensoMotoricData(std::string &terminalName, std::vector<uint8_t> &data);

    void RunSimulation(size_t brainSteps, bool untilStopped);
    void StopSimulation();
    void SetBrainStepsPerBodyStep(size_t brainSteps);
    void UpdateRegionOfInterest(Boxes &roiBoxes);
    void RequestViewportUpdate(RequestId requestId, bool full);

    void Simulate();
    void ReceiveTerminalData(Spike::BrainSink &data);

    void ChangeTopologyDone(long triggeredNeurons);
    void RegionSimulateDone(CkReductionMsg *msg);

private:
    bool mShouldStop;
    bool mShouldRunUntilStopped;
    size_t mBrainStepsToRun;
    size_t mBrainStepsPerBodyStep;

    Boxes mRoiBoxes;
    ViewportUpdateRequests mViewportUpdateRequests;
    ViewportUpdate mViewportUpdateAccumulator;

    size_t mBrainStep;
    NeuronId mNeuronIdCounter;
    RegionIndex mRegionIdxCounter;
    TerminalId mTerminalIdCounter;
    Terminals mTerminals;
    TerminalNameToId mTerminalNameToId;
    NeuronToTerminalId mNeuronToTerminalId;

    RegionAdditionRequests mRegionAdditions;
    RegionRemovals mRegionsRemovals;
    ConnectorAdditionRequests mConnectorAdditions;
    ConnectorRemovals mConnectorRemovals;
    Connections mConnectionAdditions;
    Connections mConnectionRemovals;

    BrainName mName;
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

    virtual const char *GetType() const override;

    virtual void Control(size_t brainStep) override;

    /*
    void SomeInternalFunction(SomeType1 someArg1, SomeType2 someArg2);

    entry void SomeFunctionForRegions(RegionIndex caller, SomeType1 someArg1, SomeType2 someArg2);
    */
};
