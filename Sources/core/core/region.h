#pragma once

#include <string>
#include <vector>
#include <unordered_set>
#include <unordered_map>

#include <json.hpp>

#include <pup.h>
#include <pup_stl.h>
#include <completion.h>
#include <ckmulticast.h>

#include "common.h"
#include "spike.h"
#include "synapse.h"

#include "core.decl.h"

using namespace nlohmann;

class SimulateMsg;

class RegionBase;

class RegionMap : public CkArrayMap
{
public:
    RegionMap();
    explicit RegionMap(CkMigrateMessage *msg);
    virtual int procNum(int arrayHdl, const CkArrayIndex &index) override;
};

class Region
{
public:
    Region(RegionBase &base, json &params);
    virtual ~Region() = default;

    Region(const Region &other) = delete;
    Region &operator=(const Region &other) = delete;

    virtual void pup(PUP::er &p) = 0;

    virtual const char *GetType() const = 0;

    virtual void Control(size_t brainStep) = 0;

    virtual void AcceptContributionFromNeuron(
        NeuronId neuronId, const uint8_t *contribution, size_t size) = 0;
    virtual size_t ContributeToBrain(uint8_t *&contribution) = 0;

protected:
    RegionBase &mBase;
};

class RegionBase : public CBase_RegionBase
{
public:  
    struct Connector
    {
        ConnectorName name;
        std::vector<NeuronId> neurons;
        std::unordered_set<RemoteConnector> connections;

        void pup(PUP::er &p);
    };

    typedef std::unordered_map<ConnectorName, Connector> Connectors;

    static Region *CreateRegion(const RegionType &type, RegionBase &base, json &params);

    RegionBase(const RegionName &name, const RegionType &type, const Box3D &box, const RegionParams &params);
    explicit RegionBase(CkMigrateMessage *msg);
    ~RegionBase();

    RegionBase(const RegionBase &other) = delete;
    RegionBase &operator=(const RegionBase &other) = delete;

    virtual void pup(PUP::er &p) override;

    const char *GetType() const;
    const char *GetName() const;
    RegionIndex GetIndex() const;

    NeuronIndex GetNewNeuronIndex();

    const Connectors &GetInputs() const;
    const Connector &GetInput(const ConnectorName &name) const;
    
    const Connectors &GetOutputs() const;
    const Connector &GetOutput(const ConnectorName &name) const;

    const NeuronAdditionRequests &GetNeuronAdditions() const;
    const NeuronRemovals &GetNeuronRemovals() const;
    const Synapse::Additions &GetSynapseAdditions() const;
    const Synapse::Removals &GetSynapseRemovals() const;
    const ChildLinks &GetChildAdditions() const;
    const ChildLinks &GetChildRemovals() const;

    NeuronId RequestNeuronAddition(const NeuronType &type, const NeuronParams &params);
    void RequestNeuronRemoval(NeuronId neuronId);
    void RequestSynapseAddition(Direction direction, NeuronId from, NeuronId to, const Synapse::Data &data);
    void RequestSynapseRemoval(Direction direction, NeuronId from, NeuronId to);
    void RequestChildAddition(NeuronId parent, NeuronId child);
    void RequestChildRemoval(NeuronId parent, NeuronId child);

    void CreateInput(const ConnectorName &name, const NeuronType &neuronType, const NeuronParams &neuronParams, size_t neuronCount);
    void DeleteInput(const ConnectorName &name);
    void ConnectInput(const ConnectorName &name, const RemoteConnector &destination, bool syncSynapses);
    void DisconnectInput(const ConnectorName &name, const RemoteConnector &destination, bool syncSynapses);
    void ConnectInputNeurons(const ConnectorName &name, NeuronId destFirstNeuron);
    void DisconnectInputNeurons(const ConnectorName& name, NeuronId destFirstNeuron);

    void CreateOutput(const ConnectorName &name, const NeuronType &neuronType, const NeuronParams &neuronParams, size_t neuronCount);
    void DeleteOutput(const ConnectorName &name);
    void ConnectOutput(const ConnectorName &name, const RemoteConnector &destination, bool syncSynapses);
    void DisconnectOutput(const ConnectorName &name, const RemoteConnector &destination, bool syncSynapses);
    void ConnectOutputNeurons(const ConnectorName &name, NeuronId destFirstNeuron);
    void DisconnectOutputNeurons(const ConnectorName& name, NeuronId destFirstNeuron);

    void ReceiveSensoMotoricData(Direction direction, const ConnectorName &connectorName, Spike::BrainSource &data);
    void EnqueueSensoMotoricSpike(NeuronId receiver, const Spike::Data &data);

    void SetBox(Box3D &box);
    void Unlink();
    void PrepareTopologyChange(size_t brainStep, bool doProgress);
    void CommitTopologyChange();
    void Simulate(SimulateMsg *msg);

    void NeuronFlipSpikeQueuesDone(CkReductionMsg *msg);
    void NeuronSimulateDone(CkReductionMsg *msg);

private:
    RegionName mName;
    bool mBoxChanged;
    Point3D mPosition;
    Size3D mSize;

    bool mUnlinking;

    bool mDoUpdate;
    bool mDoFullUpdate;
    bool mDoProgress;
    size_t mBrainStep;
    Boxes mRoiBoxes;

    NeuronIndex mNeuronIdxCounter;
    NeuronIndices mNeuronIndices;

    Connectors mInputConnectors;
    Connectors mOutputConnectors;

    NeuronAdditionRequests mNeuronAdditions;
    NeuronRemovals mNeuronRemovals;
    Synapse::Additions mSynapseAdditions;
    Synapse::Removals mSynapseRemovals;
    ChildLinks mChildAdditions;
    ChildLinks mChildRemovals;

    Spike::BrainSink mBrainSink;
    NeuronsTriggered mNeuronsTriggered;

    bool mNeuronSectionFilled;
    CProxySection_NeuronBase mNeuronSection;
  
    Region *mRegion;
};

class ThresholdRegion : public Region
{
public:
    static const char *Type;

    ThresholdRegion(RegionBase &base, json &params);
    virtual ~ThresholdRegion();

    virtual void pup(PUP::er &p) override;

    virtual const char *GetType() const override;

    virtual void Control(size_t brainStep) override;

    virtual void AcceptContributionFromNeuron(
        NeuronId neuronId, const uint8_t *contribution, size_t size) override;
    virtual size_t ContributeToBrain(uint8_t *&contribution) override;
};
