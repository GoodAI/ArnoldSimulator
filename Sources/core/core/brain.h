#pragma once

#include <tuple>
#include <vector>
#include <list>
#include <set>
#include <string>
#include <functional>
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

#include "core.decl.h"

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
    bool fullUpdate;
    bool doProgress;
    size_t brainStep;
    Boxes roiBoxes;

    static void *pack(SimulateMsg *msg);
    static SimulateMsg *unpack(void *buf);
};

class BrainBase;

class BrainMap : public CkArrayMap
{
public:
    BrainMap();
    BrainMap(CkMigrateMessage *msg);
    virtual int procNum(int arrayHdl, const CkArrayIndex &index) override;
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

    virtual void AcceptContributionFromRegion(
        RegionIndex regIdx, const uint8_t *contribution, size_t size) = 0;

protected:
    BrainBase &mBase;
};

class BrainBase : public CBase_BrainBase
{
public:
    struct Terminal
    {
        bool isSensor;
        TerminalId id;
        ConnectorName name;
        Spike::Type spikeType;
        NeuronId firstNeuron;
        size_t neuronCount;
        std::vector<uint8_t> data;
        std::unordered_set<RemoteConnector> connections;

        void pup(PUP::er &p);
    };

    typedef std::unordered_map<TerminalId, Terminal> Terminals;
    typedef std::unordered_map<ConnectorName, TerminalId> TerminalNameToId;
    typedef google::sparse_hash_map<NeuronId, TerminalId> NeuronToTerminalId;

    typedef std::list<RequestId> ViewportUpdateRequests;

    static Brain *CreateBrain(const BrainType &type, BrainBase &base, json &params);

    BrainBase(const BrainType &type, const BrainParams &params);
    BrainBase(CkMigrateMessage *msg);
    ~BrainBase();

    BrainBase(const BrainBase &other) = delete;
    BrainBase &operator=(const BrainBase &other) = delete;

    virtual void pup(PUP::er &p) override;

    const char *GetType() const;
    const char *GetName() const;

    NeuronIndex GetNewNeuronIndex();
    RegionIndex GetNewRegionIndex();

    const Terminals &GetTerminals() const;
    void CreateTerminal(const ConnectorName &name, Spike::Type spikeType, size_t neuronCount);
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

    void ReceiveTerminalData(Spike::BrainSink &data);

    void RunSimulation(size_t brainSteps, bool untilStopped);
    void StopSimulation();
    void SetBrainStepsPerBodyStep(size_t brainSteps);
    void UpdateRegionOfInterest(Boxes &roiBoxes);
    void RequestViewportUpdate(RequestId requestId, bool full);

    void Simulate();
    void SimulateBrainControl();
    void SimulateBrainControlDone();
    void SimulateAddRegions();
    void SimulateAddRegionsDone();
    void SimulateAddConnectors();
    void SimulateAddConnectorsDone();
    void SimulateAddRemoveConnections();
    void SimulateAddRemoveConnectionsDone();
    void SimulateRemoveConnectors();
    void SimulateRemoveConnectorsDone();
    void SimulateRemoveRegions();
    void SimulateRemoveRegionsDone();
    void SimulateRegionPrepareTopologyChange();
    void SimulateRegionPrepareTopologyChangeDone(size_t deletedNeurons);
    void SimulateRegionCommitTopologyChange();
    void SimulateRegionCommitTopologyChangeDone(size_t triggeredNeurons);
    void SimulateAllTopologyChangesDelivered();
    void SimulateBodySimulate();
    void SimulateBodySimulateDone();
    void SimulateRegionSimulate();
    void SimulateRegionSimulateDone(CkReductionMsg *msg);
    void SimulateAllSpikesDelivered();
    void SimulateDone();

private:
    BrainName mName;

    bool mDoFullViewportUpdate;
    bool mViewportUpdateFlushed;
    bool mDoSimulationProgress;
    bool mIsSimulationRunning;

    bool mRegionCommitTopologyChangeDone;
    bool mRegionSimulateDone;
    bool mAllTopologyChangesDelivered;
    bool mAllSpikesDelivered;

    size_t mDeletedNeurons;
    size_t mTriggeredNeurons;

    size_t mBrainStep;
    size_t mBrainStepsToRun;
    size_t mBrainStepsPerBodyStep;
    
    NeuronIndex mNeuronIdxCounter;
    RegionIndex mRegionIdxCounter;
    TerminalId mTerminalIdCounter;
    RegionIndices mRegionIndices;

    Boxes mRoiBoxes;
    ViewportUpdateRequests mViewportUpdateRequests;
    ViewportUpdate mViewportUpdateAccumulator;

    Terminals mTerminals;
    TerminalNameToId mTerminalNameToId;
    NeuronToTerminalId mNeuronToTerminalId;

    RegionAdditionRequests mRegionAdditions;
    RegionRemovals mRegionRemovals;
    ConnectorAdditionRequests mConnectorAdditions;
    ConnectorRemovals mConnectorRemovals;
    Connections mConnectionAdditions;
    Connections mConnectionRemovals;

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

    virtual void AcceptContributionFromRegion(
        RegionIndex regIdx, const uint8_t *contribution, size_t size) override;
};
