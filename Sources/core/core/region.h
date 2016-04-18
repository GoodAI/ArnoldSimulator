#pragma once

#include <vector>
#include <string>
#include <atomic>
#include <unordered_map>

#include <tbb/tbbmalloc_proxy.h>
#include <tbb/concurrent_vector.h>
#include <tbb/concurrent_unordered_set.h>
#include <tbb/concurrent_hash_map.h>

#include <json.hpp>

#include <pup.h>
#include <pup_stl.h>

#include "common.h"
#include "spike.h"
#include "synapse.h"
#include "region.decl.h"

using namespace nlohmann;

class Neuron;

class RegionBase;

class Region
{
public:
    Region(RegionBase &base, json &params);
    virtual ~Region() = default;

    Region(const Region &other) = delete;
    Region &operator=(const Region &other) = delete;

    virtual const char *GetType() = 0;

    virtual Spike::Editor *GetInputSpikeEditor(const ConnectorName &name) = 0;
    virtual Spike::Editor *GetOutputSpikeEditor(const ConnectorName &name) = 0;

    virtual Neuron *CreateNewInputNeuron(const ConnectorName &name) = 0;
    virtual Neuron *CreateNewOutputNeuron(const ConnectorName &name) = 0;

    virtual void Control(size_t brainStep, size_t &interactionsTriggeredCnt) = 0;

protected:
    RegionBase &mBase;
};

class RegionBase : public CBase_RegionBase
{
public:
    struct Gate
    {
        RegionId destinationRegionId;
        tbb::concurrent_vector<NeuronId> boundaryNeurons;
        tbb::concurrent_vector<Spike::Package> outboundSpikes;
        tbb::concurrent_vector<Spike::Package> inboundSpikes;
        std::vector<ConnectorName> servisedConnectors;
    };

    struct Connector
    {
        std::string name;
        tbb::concurrent_vector<NeuronId> ioNeurons;
        std::unordered_map<RemoteConnector, GateLaneIdx> connections;
    };

    static const NeuronId DeletedNeuronId;
    static const RegionId BrainRegionId;

    static Region *CreateRegion(const std::string &type, RegionBase &base, json &params);

    RegionBase(CProxy_BrainBase &brain, const std::string &type, const std::string &params);
    RegionBase(CkMigrateMessage *msg);
    ~RegionBase();

    RegionBase(const RegionBase &other) = delete;
    RegionBase &operator=(const RegionBase &other) = delete;

    void pup(PUP::er &p);

    RegionId GetId() const;

    NeuronId GetNewNeuronId();

    std::vector<std::pair<ConnectorName, size_t>> GetInputs() const;
    std::vector<RemoteConnector> GetInputConnections(const ConnectorName &name) const;
    
    void CreateInput(const ConnectorName &name, size_t size);
    void DeleteInput(const ConnectorName &name);
    void ConnectInput(const ConnectorName &name, const RemoteConnector &destination);
    void DisconnectInput(const ConnectorName &name, const RemoteConnector &destination);
    
    std::vector<std::pair<ConnectorName, size_t>> GetOutputs() const;
    std::vector<RemoteConnector> GetOutputConnections(const ConnectorName &name) const;

    void CreateOutput(const ConnectorName &name, size_t size);
    void DeleteOutput(const ConnectorName &name);
    void ConnectOutput(const ConnectorName &name, const RemoteConnector &destination);
    void DisconnectOutput(const ConnectorName &name, const RemoteConnector &destination);

    void ReceiveSensoMotoricData(Direction direction, const ConnectorName &from, std::vector<unsigned char> &data);
    void ReceiveSpikes(Direction direction, RegionId from, std::vector<Spike::Package> &spikes);

    const tbb::concurrent_vector<NeuronId> &GetAddedNeurons() const;
    const tbb::concurrent_vector<NeuronId> &GetRemovedNeurons() const;
    const tbb::concurrent_vector<Synapse::Addition> &GetAddedSynapses() const;
    const tbb::concurrent_vector<Synapse::Removal> &GetRemovedSynapses() const;

    Neuron *GetNeuron(NeuronId neuronId);
    void EnqueueSpike(Direction direction, RegionId destRegId, GateLaneIdx laneIdx, const Spike::Data &data);

    void AddNeuron(Neuron *neuron);
    void RemoveNeuron(NeuronId neuron);
    void AddSynapse(Direction direction, NeuronId from, NeuronId to, Synapse::Data &data);
    void RemoveSynapse(Direction direction, NeuronId from, NeuronId to);
    void AddChildToParent(NeuronId parent, NeuronId child);
    void RemoveChildFromParent(NeuronId parent, NeuronId child);

    void TriggerNeuron(NeuronId sender, NeuronId receiver);

    void Simulate(size_t brainStep);

private:
    CProxy_BrainBase mBrain;
    Region *mRegion;

    std::unordered_map<RegionId, Gate> mInputGates;
    std::unordered_map<RegionId, Gate> mOutputGates;

    std::unordered_map<ConnectorName, Connector> mInputConnectors;
    std::unordered_map<ConnectorName, Connector> mOutputConnectors;

    std::unordered_map<RemoteConnector, std::vector<ConnectorName>> mInputConnections;
    std::unordered_map<RemoteConnector, std::vector<ConnectorName>> mOutputConnections;

    std::atomic<NeuronId> mNeuronIdCounter;
    tbb::concurrent_unordered_set<std::string> mNeuronTypes;
    tbb::concurrent_hash_map<NeuronId, Neuron *> mNeuronsAll;

    tbb::concurrent_vector<NeuronId> mNeuronsToAdd;
    tbb::concurrent_vector<NeuronId> mNeuronsToRemove;
    tbb::concurrent_vector<Synapse::Addition> mSynapsesToAdd;
    tbb::concurrent_vector<Synapse::Removal> mSynapsesToRemove;

    tbb::concurrent_unordered_set<NeuronId> *mNeuronsTriggeredCurrent;
    tbb::concurrent_unordered_set<NeuronId> *mNeuronsTriggeredNext;
    tbb::concurrent_vector<Synapse::Transfer> mSynapticTransfersNext;
};

class ThresholdRegion : public Region
{
public:
    static const char *Type;

    ThresholdRegion(RegionBase &base, json &params);
    virtual ~ThresholdRegion();

    virtual const char *GetType() override;

    virtual Spike::Editor *GetInputSpikeEditor(const ConnectorName &name) override;
    virtual Spike::Editor *GetOutputSpikeEditor(const ConnectorName &name) override;

    virtual Neuron *CreateNewInputNeuron(const ConnectorName &name) override;
    virtual Neuron *CreateNewOutputNeuron(const ConnectorName &name) override;

    virtual void Control(size_t brainStep, size_t &interactionsTriggeredCnt) override;

    /*
    void SomeInternalFunction(SomeType1 someArg1, SomeType2 someArg2);

    void SomeFunctionForNeurons(Neuron *caller, SomeType1 someArg1, SomeType2 someArg2);

    entry void SomeFunctionForRegions(Direction direction, RegionId caller, SomeType1 someArg1, SomeType2 someArg2);

    sync entry void SomeFunctionForBrain(SomeType1 someArg1, SomeType2 someArg2);
    entry void SomeOtherFunctionForBrain(CkFuture, future, SomeType1 someArg1, SomeType2 someArg2);
    */
};
