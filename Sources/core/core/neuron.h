#pragma once

#include <string>
#include <vector>

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

#include "neuron.decl.h"

using namespace nlohmann;

class SimulateMsg;

class NeuronBase;

class NeuronMap : public CkArrayMap
{
public:
    NeuronMap();
    NeuronMap(CkMigrateMessage *msg);
    int procNum(int arrayHdl, const CkArrayIndex &index);
};

class Neuron
{
public:
    Neuron(NeuronBase &base, json &params);
    virtual ~Neuron() = default;

    Neuron(const Neuron &other) = delete;
    Neuron &operator=(const Neuron &other) = delete;

    virtual void pup(PUP::er &p) = 0;

    virtual const char *GetType() = 0;

    virtual void Control(size_t brainStep) = 0;

    virtual bool HandleSpikeGeneric(Direction direction, Spike::Editor &spike, Spike::Data &data);
    virtual bool HandleSpike(Direction direction, BinarySpike &spike, Spike::Data &data);
    virtual bool HandleSpike(Direction direction, DiscreteSpike &spike, Spike::Data &data);
    virtual bool HandleSpike(Direction direction, ContinuousSpike &spike, Spike::Data &data);
    virtual bool HandleSpike(Direction direction, VisualSpike &spike, Spike::Data &data);
    virtual bool HandleSpike(Direction direction, FunctionalSpike &spike, Spike::Data &data);

protected:
    NeuronBase &mBase;
};

class NeuronBase : public CBase_NeuronBase
{
public:
    typedef google::sparse_hash_map<NeuronId, Synapse::Data> Synapses;
    typedef google::sparse_hash_set<NeuronId> Children;
    typedef std::vector<Spike::Data> Spikes;

    static Neuron *CreateNeuron(const NeuronType &type, NeuronBase &base, json &params);

    NeuronBase(const NeuronType &type, const NeuronParams &params);
    NeuronBase(CkMigrateMessage *msg);
    ~NeuronBase();

    NeuronBase(const NeuronBase &other) = delete;
    NeuronBase &operator=(const NeuronBase &other) = delete;

    void pup(PUP::er &p);

    const char *GetType();
    NeuronIndex GetIndex() const;
    NeuronId GetId() const;

    NeuronId GetParent() const;
    const Children &GetChildren() const;
    void AdoptAsChild(NeuronId neuronId, bool cloneSynapses = true);

    const Synapses &GetInputSynapses() const;
    Synapse::Data *AccessInputSynapse(NeuronId from);
    void CommitInputSynapse(NeuronId from);

    const Synapses &GetOutputSynapses() const;
    Synapse::Data *AccessOutputSynapse(NeuronId to);
    void CommitOutputSynapse(NeuronId from);

    NeuronId RequestNeuronAddition(const NeuronType &type, const NeuronParams &params);
    void RequestNeuronRemoval(NeuronId neuronId);
    void RequestSynapseAddition(Direction direction, NeuronId from, NeuronId to, const Synapse::Data &data);
    void RequestSynapseRemoval(Direction direction, NeuronId from, NeuronId to);
    void RequestChildAddition(NeuronId parent, NeuronId child);
    void RequestChildRemoval(NeuronId parent, NeuronId child);

    void SetParent(NeuronId parent);
    void UnsetParent();    
    
    void AddChild(NeuronId child);
    void RemoveChild(NeuronId child);

    void AddInputSynapse(NeuronId from, const Synapse::Data &data);
    void SynchronizeInputSynapse(NeuronId from, const Synapse::Data &data);
    void RemoveInputSynapse(NeuronId from);    

    void AddOutputSynapse(NeuronId to, const Synapse::Data &data);
    void SynchronizeOutputSynapse(NeuronId from, const Synapse::Data &data);
    void RemoveOutputSynapse(NeuronId to);
    
    void SendSpike(NeuronId receiver, Direction direction, const Spike::Data &data);
    void EnqueueSpike(Direction direction, const Spike::Data &data);

    void FlipSpikeQueues();
    void Simulate(SimulateMsg *msg);

protected:
    NeuronId mTempIdCounter;

    NeuronId mParent;
    Children mChildren;

    Synapses mInputSynapses;
    Synapses mOutputSynapses;

    NeuronAdditions mNeuronAdditions;
    NeuronRemovals mNeuronRemovals;
    Synapse::Additions mSynapseAdditions;
    Synapse::Removals mSynapseRemovals;
    ChildAdditions mChildAdditions;
    ChildRemovals mChildRemovals;

    NeuronsTriggered mNeuronsTriggered;

    Spikes *mBackwardSpikesCurrent;
    Spikes *mBackwardSpikesNext;    
    Spikes *mForwardSpikesCurrent;
    Spikes *mForwardSpikesNext;

    Neuron *mNeuron;
};

class ThresholdNeuron : public Neuron
{
public:
    static const char *Type;

    ThresholdNeuron(NeuronBase &base, json &params);
    virtual ~ThresholdNeuron();

    virtual void pup(PUP::er &p) override;

    virtual const char *GetType() override;

    virtual bool HandleSpike(Direction direction, ContinuousSpike &spike, Spike::Data &data) override;
    virtual bool HandleSpike(Direction direction, FunctionalSpike &spike, Spike::Data &data) override;
    
    virtual void Control(size_t brainStep) override;

    enum class Function : uint8_t
    {
        RequestThreshold,
        ReceiveThreshold,
        ChangeThreshold
    };

    struct ReceiveThresholdArgs
    {
        double threshold;
    };

    struct ChangeThresholdArgs
    {
        double delta;
    };

    void RequestThreshold(Direction direction, NeuronId sender);
    void ReceiveThreshold(Direction direction, NeuronId sender, const ReceiveThresholdArgs &args);
    void ChangeThreshold(Direction direction, NeuronId sender, const ChangeThresholdArgs &args);

    template<typename Arguments>
    void SendFunctionalSpike(Direction direction, NeuronId receiver, Function function, Arguments &args);

protected:
    double mThreshold;
};

template<typename Arguments>
inline void ThresholdNeuron::SendFunctionalSpike(Direction direction, NeuronId receiver, Function function, Arguments &args)
{
    Spike::Data data;
    Spike::Initialize(Spike::Type::Functional, mBase.GetId(), data);
    FunctionalSpike *spike = static_cast<FunctionalSpike *>(Spike::Edit(data));
    spike->SetFunction(data, static_cast<uint8_t>(function));
    spike->SetArguments(data, &args, sizeof(Arguments));

    mBase.SendSpike(receiver, direction, data);
}
