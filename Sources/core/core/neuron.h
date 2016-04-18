#pragma once

#include <tbb/tbbmalloc_proxy.h>
#include <tbb/spin_mutex.h>
#include <tbb/concurrent_vector.h>

#include <sparsehash/sparse_hash_set>
#include <sparsehash/sparse_hash_map>

#include <pup.h>

#include "common.h"
#include "spike.h"
#include "synapse.h"

class RegionBase;

class Neuron
{
public:
    typedef google::sparse_hash_map<NeuronId, Synapse::Data> InputSynapses;
    typedef google::sparse_hash_set<NeuronId> OutputSynapses;
    typedef google::sparse_hash_set<NeuronId> Children;
    typedef tbb::concurrent_vector<Spike::Data> Spikes;
    typedef tbb::spin_mutex::scoped_lock Lock;

    static Neuron *CreateNeuron(const std::string &type, RegionBase &region, NeuronId id, bool isIo = false);

    Neuron(RegionBase &region, NeuronId id, bool isIo = false);
    virtual ~Neuron();

    Neuron(const Neuron &other) = delete;
    Neuron &operator=(const Neuron &other) = delete;

    virtual void pup(PUP::er &p);

    virtual const char *GetType() = 0;
    NeuronId GetId() const;

    virtual void AdoptAsChild(Neuron *neuron, bool cloneSynapses = true);

    NeuronId GetParent() const;
    void SetParent(NeuronId parent);
    void UnsetParent();

    const Children &GetChildren() const;
    void AddChild(NeuronId child);
    void RemoveChild(NeuronId child);

    const InputSynapses &GetInputSynapses() const;
    void AddInputSynapse(NeuronId from, const Synapse::Data &data);
    void RemoveInputSynapse(NeuronId from);
    bool AccessInputSynapse(NeuronId from, Synapse::Accessor &accessor, bool doLock = true);

    const OutputSynapses &GetOutputSynapses() const;
    void AddOutputSynapse(NeuronId to);
    void RemoveOutputSynapse(NeuronId to); 
    bool AccessOutputSynapse(NeuronId to, Synapse::Accessor &accessor, bool doLock = true);
    
    void EnqueueSpike(Direction direction, const Spike::Data &data);
    void FlipSpikeQueues();
    
    virtual bool HandleSpikeGeneric(Direction direction, Spike::Editor &spike, Spike::Data &data);
    virtual bool HandleSpike(Direction direction, BinarySpike &spike, Spike::Data &data);
    virtual bool HandleSpike(Direction direction, DiscreteSpike &spike, Spike::Data &data);
    virtual bool HandleSpike(Direction direction, ContinuousSpike &spike, Spike::Data &data);
    virtual bool HandleSpike(Direction direction, VisualSpike &spike, Spike::Data &data);
    virtual bool HandleSpike(Direction direction, FunctionalSpike &spike, Spike::Data &data);

    void Simulate(size_t brainStep);
    virtual void Control(size_t brainStep) = 0;

protected:
    bool mIsIo;
    NeuronId mId;
    RegionBase &mRegion;
    tbb::spin_mutex mConnectionGuard;

    NeuronId mParent;
    Children mChildren;

    OutputSynapses mOutputSynapses;
    Spikes *mBackwardSpikesCurrent;
    Spikes *mBackwardSpikesNext;

    InputSynapses mInputSynapses;
    Spikes *mForwardSpikesCurrent;
    Spikes *mForwardSpikesNext;
};

class BoundaryNeuron : public Neuron
{
public:
    static const char *Type;

    BoundaryNeuron(RegionBase &region, NeuronId id, RegionId destRegId, GateLaneIdx laneIdx);
    virtual ~BoundaryNeuron();

    virtual void pup(PUP::er &p) override;

    virtual const char *GetType() override;

    virtual void AdoptAsChild(Neuron *neuron, bool cloneSynapses = true) override;

    RegionId GetDestinationRegionId();
    GateLaneIdx GetLaneIndex();

    virtual bool HandleSpikeGeneric(Direction direction, Spike::Editor &spike, Spike::Data &data) override;

    virtual void Control(size_t brainStep) override;

protected:
    RegionId mDestRegId;
    GateLaneIdx mLaneIdx;
};

class ThresholdNeuron : public Neuron
{
public:
    static const char *Type;

    ThresholdNeuron(RegionBase &region, NeuronId id, double threshold, bool isIo = false);
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
    Spike::Initialize(Spike::Type::Functional, mId, data);
    FunctionalSpike *spike = static_cast<FunctionalSpike *>(Spike::Edit(data));
    spike->SetFunction(data, static_cast<uint8_t>(function));
    spike->SetArguments(data, &args, sizeof(Arguments));

    mRegion.GetNeuron(receiver)->EnqueueSpike(direction, data);
}
