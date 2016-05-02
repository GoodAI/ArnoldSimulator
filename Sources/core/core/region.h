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

#include "region.decl.h"

using namespace nlohmann;

class SimulateMsg;

class RegionBase;

class Region
{
public:
    Region(RegionBase &base, json &params);
    virtual ~Region() = default;

    Region(const Region &other) = delete;
    Region &operator=(const Region &other) = delete;

    virtual void pup(PUP::er &p) = 0;

    virtual const char *GetType() = 0;

    virtual void Control(size_t brainStep) = 0;

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
    };

    typedef std::unordered_map<ConnectorName, Connector> Connectors;

    static Region *CreateRegion(const RegionType &type, RegionBase &base, json &params);

    RegionBase(const RegionType &type, const RegionParams &params);
    RegionBase(CkMigrateMessage *msg);
    ~RegionBase();

    RegionBase(const RegionBase &other) = delete;
    RegionBase &operator=(const RegionBase &other) = delete;

    void pup(PUP::er &p);

    const char *GetType();
    RegionIndex GetIndex() const;

    NeuronId GetNewNeuronId();

    const Connectors &GetInputs() const;
    const Connector &GetInput(const ConnectorName &name) const;
    
    const Connectors &GetOutputs() const;
    const Connector &GetOutput(const ConnectorName &name) const;

    const NeuronAdditions &GetNeuronAdditions() const;
    const NeuronRemovals &GetNeuronRemovals() const;
    const Synapse::Additions &GetSynapseAdditions() const;
    const Synapse::Removals &GetSynapseRemovals() const;
    const ChildAdditions &GetChildAdditions() const;
    const ChildRemovals &GetChildRemovals() const;

    NeuronId RequestNeuronAddition(const NeuronType &type, const NeuronParams &params);
    void RequestNeuronRemoval(NeuronId neuronId);
    void RequestSynapseAddition(Direction direction, NeuronId from, NeuronId to, const Synapse::Data &data);
    void RequestSynapseRemoval(Direction direction, NeuronId from, NeuronId to);
    void RequestChildAddition(NeuronId parent, NeuronId child);
    void RequestChildRemoval(NeuronId parent, NeuronId child);

    void CreateInput(const ConnectorName &name, Spike::Type spikeType, const NeuronType &neuronType, const NeuronParams &neuronParams, size_t neuronCount);
    void DeleteInput(const ConnectorName &name);
    void ConnectInput(const ConnectorName &name, const RemoteConnector &destination);
    void DisconnectInput(const ConnectorName &name, const RemoteConnector &destination);

    void CreateOutput(const ConnectorName &name, Spike::Type spikeType, const NeuronType &neuronType, const NeuronParams &neuronParams, size_t neuronCount);
    void DeleteOutput(const ConnectorName &name);
    void ConnectOutput(const ConnectorName &name, const RemoteConnector &destination);
    void DisconnectOutput(const ConnectorName &name, const RemoteConnector &destination);

    void ReceiveSensoMotoricData(Direction direction, const ConnectorName &connectorName, Spike::BrainSource &data);
    void EnqueueSensoMotoricSpike(NeuronId receiver, const Spike::Data &data);

    void ChangeTopology();
    void Simulate(SimulateMsg *msg);

    void NeuronSimulateDone(CkReductionMsg *msg);

private:
    NeuronId mNeuronIdCounter;

    Connectors mInputConnectors;
    Connectors mOutputConnectors;

    NeuronAdditions mNeuronAdditions;
    NeuronRemovals mNeuronRemovals;
    Synapse::Additions mSynapseAdditions;
    Synapse::Removals mSynapseRemovals;
    ChildAdditions mChildAdditions;
    ChildRemovals mChildRemovals;  

    std::vector<CkArrayIndex2D> mNeuronsTriggered;
    Synapse::Transfers mSynapticTransfers;
    Spike::BrainSink mBrainSink;

    Region *mRegion;
};

class ThresholdRegion : public Region
{
public:
    static const char *Type;

    ThresholdRegion(RegionBase &base, json &params);
    virtual ~ThresholdRegion();

    virtual void pup(PUP::er &p) override;

    virtual const char *GetType() override;

    virtual void Control(size_t brainStep) override;

    /*
    void SomeInternalFunction(SomeType1 someArg1, SomeType2 someArg2);

    void SomeFunctionForNeurons(NeuronId caller, SomeType1 someArg1, SomeType2 someArg2);

    entry void SomeFunctionForRegions(Direction direction, RegionIndex caller, SomeType1 someArg1, SomeType2 someArg2);

    sync entry void SomeFunctionForBrain(SomeType1 someArg1, SomeType2 someArg2);
    entry void SomeOtherFunctionForBrain(CkFuture, future, SomeType1 someArg1, SomeType2 someArg2);
    */
};
