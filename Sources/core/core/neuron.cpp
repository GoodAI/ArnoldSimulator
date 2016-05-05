#include "neuron.h"

#include "brain.h"

#include "core.decl.h"
#include "brain.decl.h"
#include "region.decl.h"

extern CkGroupID gMulticastGroupId;
extern CProxy_CompletionDetector gCompletionDetector;

extern CProxy_Core gCore;
extern CProxy_BrainBase gBrain;
extern CProxy_RegionBase gRegions;
extern CProxy_NeuronBase gNeurons;

NeuronMap::NeuronMap()
{
}

NeuronMap::NeuronMap(CkMigrateMessage *msg)
{
}

int NeuronMap::procNum(int arrayHdl, const CkArrayIndex &index)
{
    int regionIndex = index.data()[0];
    int neuronIndex = index.data()[1];

    int nodeNum = regionIndex % CkNumNodes();
    int rankNum = neuronIndex % CkNodeSize(nodeNum);
    int peNum = rankNum + CkNodeFirst(nodeNum);

    return peNum;
}

Neuron::Neuron(NeuronBase &base, json &params) : mBase(base)
{
}

Neuron *NeuronBase::CreateNeuron(const NeuronType &type, NeuronBase &base, json &params)
{
    if (type == ThresholdNeuron::Type) {
        return new ThresholdNeuron(base, params);
    } else {
        return nullptr;
    }
}

bool Neuron::HandleSpikeGeneric(Direction direction, Spike::Editor &spike, Spike::Data &data)
{
    return true;
}

bool Neuron::HandleSpike(Direction direction, BinarySpike &spike, Spike::Data &data)
{
    return HandleSpikeGeneric(direction, spike, data);
}

bool Neuron::HandleSpike(Direction direction, DiscreteSpike &spike, Spike::Data &data)
{
    return HandleSpikeGeneric(direction, spike, data);
}

bool Neuron::HandleSpike(Direction direction, ContinuousSpike &spike, Spike::Data &data)
{
    return HandleSpikeGeneric(direction, spike, data);
}

bool Neuron::HandleSpike(Direction direction, VisualSpike &spike, Spike::Data &data)
{
    return HandleSpikeGeneric(direction, spike, data);
}

bool Neuron::HandleSpike(Direction direction, FunctionalSpike &spike, Spike::Data &data)
{
    return HandleSpikeGeneric(direction, spike, data);
}

NeuronBase::NeuronBase(const NeuronType &type, const NeuronParams &params)
{
    json p = json::parse(params);

    mTempIdCounter = TEMP_REGION_INDEX;
    mNeuron = CreateNeuron(type, *this, p);
    mParent = DELETED_NEURON_ID;
    mChildren.set_deleted_key(DELETED_NEURON_ID);
    mInputSynapses.set_deleted_key(DELETED_NEURON_ID);
    mOutputSynapses.set_deleted_key(DELETED_NEURON_ID);
    mBackwardSpikesCurrent = new Spikes();
    mBackwardSpikesNext = new Spikes();
    mForwardSpikesCurrent = new Spikes();
    mForwardSpikesNext = new Spikes();
}

NeuronBase::NeuronBase(CkMigrateMessage *msg)
{
}

NeuronBase::~NeuronBase()
{
    for (auto it = mInputSynapses.begin(); it != mInputSynapses.end(); ++it) {
        Synapse::Release(it->second);
    }
    for (auto it = mOutputSynapses.begin(); it != mOutputSynapses.end(); ++it) {
        Synapse::Release(it->second);
    }

    for (auto it = mSynapseAdditions.begin(); it != mSynapseAdditions.end(); ++it) {
        Synapse::Release(std::get<3>(*it));
    }

    for (auto it = mBackwardSpikesCurrent->begin(); it != mBackwardSpikesCurrent->end(); ++it) {
        Spike::Release(*it);
    }
    for (auto it = mBackwardSpikesNext->begin(); it != mBackwardSpikesNext->end(); ++it) {
        Spike::Release(*it);
    }
    for (auto it = mForwardSpikesCurrent->begin(); it != mForwardSpikesCurrent->end(); ++it) {
        Spike::Release(*it);
    }
    for (auto it = mForwardSpikesNext->begin(); it != mForwardSpikesNext->end(); ++it) {
        Spike::Release(*it);
    }
    
    delete mBackwardSpikesCurrent;
    delete mBackwardSpikesNext;
    delete mForwardSpikesCurrent;
    delete mForwardSpikesNext;

    delete mNeuron;
}

void NeuronBase::pup(PUP::er &p)
{
    p | mTempIdCounter;

    p | mParent;

    p | *mBackwardSpikesCurrent;
    p | *mBackwardSpikesNext;
    p | *mForwardSpikesCurrent;
    p | *mForwardSpikesNext;

    p | mNeuronAdditions;
    p | mNeuronRemovals;
    p | mSynapseAdditions;
    p | mSynapseRemovals;
    p | mChildAdditions;
    p | mChildRemovals;

    if (p.isUnpacking()) {
        size_t childrenCount; p | childrenCount;
        for (size_t i = 0; i < childrenCount; ++i) {
            NeuronId child; p | child;
            mChildren.insert(child);
        }

        size_t inputSynapsesCount; p | inputSynapsesCount;
        for (size_t i = 0; i < inputSynapsesCount; ++i) {
            NeuronId from; p | from;
            Synapse::Data data; data.pup(p);
            mInputSynapses.insert(std::make_pair(from, data));
        }

        size_t outputSynapsesCount; p | outputSynapsesCount;
        for (size_t i = 0; i < outputSynapsesCount; ++i) {
            NeuronId to; p | to;
            Synapse::Data data; data.pup(p);
            mOutputSynapses.insert(std::make_pair(to, data));
        }

        json params;
        std::string neuronType;
        p | neuronType;
        mNeuron = CreateNeuron(neuronType, *this, params);
        mNeuron->pup(p);
    } else {
        size_t childrenCount = mChildren.size(); p | childrenCount;
        for (auto it = mChildren.begin(); it != mChildren.end(); ++it) {
            NeuronId child = *it; p | child;
        }

        size_t inputSynapsesCount = mInputSynapses.size(); p | inputSynapsesCount;
        for (auto it = mInputSynapses.begin(); it != mInputSynapses.end(); ++it) {
            NeuronId from = it->first; p | from;
            Synapse::Data data = it->second; data.pup(p);
        }

        size_t outputSynapsesCount = mOutputSynapses.size(); p | outputSynapsesCount;
        for (auto it = mOutputSynapses.begin(); it != mOutputSynapses.end(); ++it) {
            NeuronId to = it->first; p | to;
            Synapse::Data data = it->second; data.pup(p);
        }

        std::string neuronType(mNeuron->GetType());
        p | neuronType;
        mNeuron->pup(p);
    }
}

const char *NeuronBase::GetType()
{
    return mNeuron->GetType();
}

NeuronIndex NeuronBase::GetIndex() const
{
    return thisIndex.y;
}

NeuronId NeuronBase::GetId() const
{
    return GetNeuronId(thisIndex.x, thisIndex.y);
}

NeuronId NeuronBase::GetParent() const
{
    return mParent;
}

const NeuronBase::Children &NeuronBase::GetChildren() const
{
    return mChildren;
}

void NeuronBase::AdoptAsChild(NeuronId neuronId, bool cloneSynapses)
{
    for (auto it = mInputSynapses.begin(); it != mInputSynapses.end(); ++it) {
        Synapse::Data data;
        {
            if (cloneSynapses) {
                Synapse::Initialize(Synapse::GetType(it->second), data);
            } else {
                Synapse::Clone(it->second, data);
            }
        }
        RequestSynapseAddition(Direction::Forward, it->first, GetId(), data);
    }

    for (auto it = mOutputSynapses.begin(); it != mOutputSynapses.end(); ++it) {
        Synapse::Data data;
        {
            if (cloneSynapses) {
                Synapse::Initialize(Synapse::GetType(it->second), data);
            } else {
                Synapse::Clone(it->second, data);
            }
        }
        RequestSynapseAddition(Direction::Forward, GetId(), it->first, data);
    }

    RequestChildAddition(GetId(), neuronId);
}

const NeuronBase::Synapses &NeuronBase::GetInputSynapses() const
{
    return mInputSynapses;
}

Synapse::Data *NeuronBase::AccessInputSynapse(NeuronId from)
{
    auto it = mInputSynapses.find(from);
    if (it != mInputSynapses.end()) {
        return &it->second;
    } else {
        return nullptr;
    }
}

void NeuronBase::CommitInputSynapse(NeuronId from)
{
    auto it = mInputSynapses.find(from);
    if (it != mInputSynapses.end()) {
        gNeurons(GetRegionIndex(from), GetNeuronIndex(from)).SynchronizeOutputSynapse(GetId(), it->second);
    }
}

const NeuronBase::Synapses &NeuronBase::GetOutputSynapses() const
{
    return mOutputSynapses;
}

Synapse::Data *NeuronBase::AccessOutputSynapse(NeuronId to)
{
    auto it = mOutputSynapses.find(to);
    if (it != mOutputSynapses.end()) {
        return &it->second;
    } else {
        return nullptr;
    }
}

void NeuronBase::CommitOutputSynapse(NeuronId to)
{
    auto it = mOutputSynapses.find(to);
    if (it != mOutputSynapses.end()) {
        gNeurons(GetRegionIndex(to), GetNeuronIndex(to)).SynchronizeInputSynapse(GetId(), it->second);
    }
}

NeuronId NeuronBase::RequestNeuronAddition(const NeuronType &type, const NeuronParams &params)
{
    NeuronId tempId = mTempIdCounter++;
    mNeuronAdditions.push_back(std::make_tuple(tempId, type, params));
    return tempId;
}

void NeuronBase::RequestNeuronRemoval(NeuronId neuronId)
{
    mNeuronRemovals.push_back(neuronId);
}

void NeuronBase::RequestSynapseAddition(Direction direction, NeuronId from, NeuronId to, const Synapse::Data &data)
{
    mSynapseAdditions.push_back(std::make_tuple(direction, from, to, data));
}

void NeuronBase::RequestSynapseRemoval(Direction direction, NeuronId from, NeuronId to)
{
    mSynapseRemovals.push_back(std::make_tuple(direction, from, to));
}

void NeuronBase::RequestChildAddition(NeuronId parent, NeuronId child)
{
    mChildAdditions.push_back(std::make_pair(parent, child));
}

void NeuronBase::RequestChildRemoval(NeuronId parent, NeuronId child)
{
    mChildRemovals.push_back(std::make_pair(parent, child));
}

void NeuronBase::SetParent(NeuronId parent)
{
    mParent = parent;
}

void NeuronBase::UnsetParent()
{
    mParent = DELETED_NEURON_ID;
}

void NeuronBase::AddChild(NeuronId child)
{
    mChildren.insert(child);
}

void NeuronBase::RemoveChild(NeuronId child)
{
    mChildren.erase(child);
}

void NeuronBase::AddInputSynapse(NeuronId from, const Synapse::Data &data)
{
    mInputSynapses.insert(std::make_pair(from, data));
}

void NeuronBase::SynchronizeInputSynapse(NeuronId from, const Synapse::Data &data)
{
    auto it = mInputSynapses.find(from);
    if (it != mInputSynapses.end()) {
        mInputSynapses[from] = data;
    }
}

void NeuronBase::RemoveInputSynapse(NeuronId from)
{
    mInputSynapses.erase(from);
}

void NeuronBase::AddOutputSynapse(NeuronId to, const Synapse::Data &data)
{
    mOutputSynapses.insert(std::make_pair(to, data));
}

void NeuronBase::SynchronizeOutputSynapse(NeuronId to, const Synapse::Data &data)
{
    auto it = mOutputSynapses.find(to);
    if (it != mOutputSynapses.end()) {
        mOutputSynapses[to] = data;
    }
}

void NeuronBase::RemoveOutputSynapse(NeuronId to)
{
    mOutputSynapses.erase(to);
}

void NeuronBase::SendSpike(NeuronId receiver, Direction direction, const Spike::Data &data)
{
    gNeurons(GetRegionIndex(receiver), GetNeuronIndex(receiver)).EnqueueSpike(direction, data);
}

void NeuronBase::EnqueueSpike(Direction direction, const Spike::Data &data)
{
    if (direction == Direction::Forward) {
        mForwardSpikesNext->push_back(data);
    } else {
        mBackwardSpikesNext->push_back(data);
    }
}

void NeuronBase::FlipSpikeQueues()
{
    std::swap(mForwardSpikesCurrent, mForwardSpikesNext);
    std::swap(mBackwardSpikesCurrent, mBackwardSpikesNext);
}

void NeuronBase::Simulate(SimulateMsg *msg)
{
    size_t brainStep = msg->brainStep;
    delete msg;

    for (auto it = mForwardSpikesCurrent->begin(); it != mForwardSpikesCurrent->end(); ++it) {
        bool release = Spike::Edit(*it)->Accept(Direction::Forward, *mNeuron, *it);
        if (release) Spike::Release(*it);
    }

    for (auto it = mBackwardSpikesCurrent->begin(); it != mBackwardSpikesCurrent->end(); ++it) {
        bool release = Spike::Edit(*it)->Accept(Direction::Backward, *mNeuron, *it);
        if (release) Spike::Release(*it);
    }

    mForwardSpikesCurrent->clear();
    mBackwardSpikesCurrent->clear();

    mNeuron->Control(brainStep);
    
    /*
    int result[3];
    result[0] = 1; // Copy the desired values into the result.
    result[1] = 2;
    result[2] = 3;
    CkCallback cb(CkReductionTarget(RegionBase, NeuronSimulateDone), gRegions[thisIndex.x]);
    // Contribute the result to the reductiontarget cb.
    contribute(3*sizeof(int), result, CkReduction::set, cb);
    */
}

ThresholdNeuron::ThresholdNeuron(NeuronBase &base, json &params) : Neuron(base, params)
{
    // TODO extract mThreshold value from json params (can be empty)
    mThreshold = 0;
}

ThresholdNeuron::~ThresholdNeuron()
{
}

void ThresholdNeuron::pup(PUP::er &p)
{
    p | mThreshold;
}

const char *ThresholdNeuron::Type = "ThresholdNeuron";

const char *ThresholdNeuron::GetType()
{
    return Type;
}

bool ThresholdNeuron::HandleSpike(Direction direction, ContinuousSpike &spike, Spike::Data &data)
{
    uint16_t delay = spike.GetDelay(data);
    if (delay == 0) {

        // TODO

        return true;
    } else {
        spike.SetDelay(data, --delay);
        if (direction == Direction::Forward) {
            mBase.EnqueueSpike(Direction::Forward, data);
        } else {
            mBase.EnqueueSpike(Direction::Backward, data);
        }
        return false;
    }
}

bool ThresholdNeuron::HandleSpike(Direction direction, FunctionalSpike &spike, Spike::Data &data)
{
    switch (static_cast<Function>(spike.GetFunction(data))) {
        case Function::RequestThreshold:
        {
            RequestThreshold(direction, Spike::GetSender(data));
            break;
        }
        case Function::ReceiveThreshold:
        {
            ReceiveThresholdArgs args;
            spike.GetArguments(data, &args, sizeof(ReceiveThresholdArgs));
            ReceiveThreshold(direction, Spike::GetSender(data), args);
            break;
        }
        case Function::ChangeThreshold:
        {
            ChangeThresholdArgs args;
            spike.GetArguments(data, &args, sizeof(ChangeThresholdArgs));
            ChangeThreshold(direction, Spike::GetSender(data), args);
            break;
        }
        default:
            break;
    }
    return true;
}

void ThresholdNeuron::Control(size_t brainStep)
{
    // TODO

    /*
    if (spikeCount > 0) {
        spikeCount = 0;
        for (auto it = outputToExperts.begin(); it != outputToExperts.end(); ++it) {
            (*it)->EnqueueClosure(Direction::Forward, this, [=](Direction direction, TestExpert *caller, TestExpert *callee) {
                callee->ReceiveSpike(direction, caller);
            });
        }
    }
    */
}

void ThresholdNeuron::RequestThreshold(Direction direction, NeuronId sender)
{
    ReceiveThresholdArgs response;
    response.threshold = mThreshold;

    SendFunctionalSpike<ReceiveThresholdArgs>(
        OPPOSITE_DIRECTION(direction), sender, Function::ReceiveThreshold, response);
}

void ThresholdNeuron::ReceiveThreshold(Direction direction, NeuronId sender, const ReceiveThresholdArgs &args)
{
    ChangeThresholdArgs response;
    response.delta = mThreshold - ((mThreshold + args.threshold) / 2);

    SendFunctionalSpike<ChangeThresholdArgs>(
        OPPOSITE_DIRECTION(direction), sender, Function::ChangeThreshold, response);
}

void ThresholdNeuron::ChangeThreshold(Direction direction, NeuronId sender, const ChangeThresholdArgs &args)
{
    mThreshold += args.delta;
}

#include "neuron.def.h"
