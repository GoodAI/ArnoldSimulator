#include "random.h"
#include "region.h"

#include "neuron.h"

Neuron *Neuron::CreateNeuron(const std::string &type, RegionBase &region, NeuronId id, bool isIo)
{
    if (type == ThresholdNeuron::Type) {
        return new ThresholdNeuron(region, id, isIo);
    } else {
        return nullptr;
    }
}

Neuron::Neuron(RegionBase &region, NeuronId id, bool isIo) : mRegion(region), mId(id), mIsIo(isIo)
{
    mParent = RegionBase::DeletedNeuronId;
    mChildren.set_deleted_key(RegionBase::DeletedNeuronId);
    mOutputSynapses.set_deleted_key(RegionBase::DeletedNeuronId);
    mInputSynapses.set_deleted_key(RegionBase::DeletedNeuronId);
    mBackwardSpikesCurrent = new Spikes();
    mBackwardSpikesNext = new Spikes();
    mForwardSpikesCurrent = new Spikes();
    mForwardSpikesNext = new Spikes();
}

Neuron::~Neuron()
{
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
}

void Neuron::pup(PUP::er &p)
{
    p | mId;
    p | mIsIo;
    p | mParent;

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
            mOutputSynapses.insert(to);
        }    

        size_t backwardSpikesCurrentCount; p | backwardSpikesCurrentCount;
        for (size_t i = 0; i < backwardSpikesCurrentCount; ++i) {
            Spike::Data data; data.pup(p);
            mBackwardSpikesCurrent->push_back(data);
        }

        size_t backwardSpikesNextCount; p | backwardSpikesNextCount;
        for (size_t i = 0; i < backwardSpikesNextCount; ++i) {
            Spike::Data data; data.pup(p);
            mBackwardSpikesNext->push_back(data);
        }

        size_t forwardSpikesCurrentCount; p | forwardSpikesCurrentCount;
        for (size_t i = 0; i < forwardSpikesCurrentCount; ++i) {
            Spike::Data data; data.pup(p);
            mForwardSpikesCurrent->push_back(data);
        }

        size_t forwardSpikesNextCount; p | forwardSpikesNextCount;
        for (size_t i = 0; i < forwardSpikesNextCount; ++i) {
            Spike::Data data; data.pup(p);
            mForwardSpikesNext->push_back(data);
        }
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
            NeuronId to = *it; p | to;
        }

        size_t backwardSpikesCurrentCount = mBackwardSpikesCurrent->size(); p | backwardSpikesCurrentCount;
        for (auto it = mBackwardSpikesCurrent->begin(); it != mBackwardSpikesCurrent->end(); ++it) {
            Spike::Data data = *it; data.pup(p);
        }

        size_t backwardSpikesNextCount = mBackwardSpikesNext->size(); p | backwardSpikesNextCount;
        for (auto it = mBackwardSpikesNext->begin(); it != mBackwardSpikesNext->end(); ++it) {
            Spike::Data data = *it; data.pup(p);
        }

        size_t forwardSpikesCurrentCount = mForwardSpikesCurrent->size(); p | forwardSpikesCurrentCount;
        for (auto it = mForwardSpikesCurrent->begin(); it != mForwardSpikesCurrent->end(); ++it) {
            Spike::Data data = *it; data.pup(p);
        }

        size_t forwardSpikesNextCount = mForwardSpikesNext->size(); p | forwardSpikesNextCount;
        for (auto it = mForwardSpikesNext->begin(); it != mForwardSpikesNext->end(); ++it) {
            Spike::Data data = *it; data.pup(p);
        }
    }
}

NeuronId Neuron::GetId() const
{
    return mId;
}

void Neuron::AdoptAsChild(Neuron *neuron, bool cloneSynapses)
{
    if (mIsIo) {
        delete neuron;
        return;
    }

    mRegion.AddNeuron(neuron);

    for (auto it = mInputSynapses.begin(); it != mInputSynapses.end(); ++it) {
        Synapse::Data data;
        {
            Synapse::Accessor ac;
            Synapse::Edit(it->second, ac);
            if (cloneSynapses) {
                Synapse::Initialize(Synapse::GetType(ac.GetData()), data);
            } else {
                Synapse::Clone(ac.GetData(), data);
            }
        }
        mRegion.AddSynapse(Direction::Forward, it->first, mId, data);
    }

    for (auto it = mOutputSynapses.begin(); it != mOutputSynapses.end(); ++it) {
        Synapse::Data data;
        {
            Synapse::Accessor ac;
            Synapse::Edit(mRegion.GetNeuron(*it)->mInputSynapses[mId], ac);
            if (cloneSynapses) {
                Synapse::Initialize(Synapse::GetType(ac.GetData()), data);
            } else {
                Synapse::Clone(ac.GetData(), data);
            }
        }
        mRegion.AddSynapse(Direction::Forward, mId, *it, data);
    }

    mRegion.AddChildToParent(mId, neuron->GetId());
}

NeuronId Neuron::GetParent() const
{
    return mParent;
}

void Neuron::SetParent(NeuronId parent)
{
    mParent = parent;
}

void Neuron::UnsetParent()
{
    mParent = RegionBase::DeletedNeuronId;
}

const Neuron::Children &Neuron::GetChildren() const
{
    return mChildren;
}

void Neuron::AddChild(NeuronId child)
{
    Lock lock(mConnectionGuard);
    mChildren.insert(child);
}

void Neuron::RemoveChild(NeuronId child)
{
    Lock lock(mConnectionGuard);
    mChildren.erase(child);
}

const Neuron::InputSynapses &Neuron::GetInputSynapses() const
{
    return mInputSynapses;
}

void Neuron::AddInputSynapse(NeuronId from, const Synapse::Data &data)
{
    Lock lock(mConnectionGuard);
    mInputSynapses.insert(std::make_pair(from, data));
}

void Neuron::RemoveInputSynapse(NeuronId from)
{
    Lock lock(mConnectionGuard);
    mInputSynapses.erase(from);
}

bool Neuron::AccessInputSynapse(NeuronId from, Synapse::Accessor &accessor, bool doLock)
{
    auto it = mInputSynapses.find(from);
    if (it != mInputSynapses.end()) {
        Synapse::Edit(it->second, accessor, doLock);
        return true;
    } else {
        return false;
    }
}

const Neuron::OutputSynapses &Neuron::GetOutputSynapses() const
{
    return mOutputSynapses;
}

void Neuron::AddOutputSynapse(NeuronId to)
{
    Lock lock(mConnectionGuard);
    mOutputSynapses.insert(to);
}

void Neuron::RemoveOutputSynapse(NeuronId to)
{
    Lock lock(mConnectionGuard);
    mOutputSynapses.erase(to);
}

bool Neuron::AccessOutputSynapse(NeuronId to, Synapse::Accessor &accessor, bool doLock)
{
    auto it = mOutputSynapses.find(to);
    if (it != mOutputSynapses.end()) {
        Synapse::Edit(mRegion.GetNeuron(*it)->mInputSynapses[mId], accessor, doLock);
        return true;
    } else {
        return false;
    }
}

void Neuron::EnqueueSpike(Direction direction, const Spike::Data &data)
{
    if (direction == Direction::Forward) {
        mForwardSpikesNext->push_back(data);
    }
}

void Neuron::FlipSpikeQueues()
{
    std::swap(mForwardSpikesCurrent, mForwardSpikesNext);
    std::swap(mBackwardSpikesCurrent, mBackwardSpikesNext);
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

void Neuron::Simulate(size_t brainStep)
{
    for (auto it = mForwardSpikesCurrent->begin(); it != mForwardSpikesCurrent->end(); ++it) {
        bool release = Spike::Edit(*it)->Accept(Direction::Forward, *this, *it);
        if (release) Spike::Release(*it);
    }

    for (auto it = mBackwardSpikesCurrent->begin(); it != mBackwardSpikesCurrent->end(); ++it) {
        bool release = Spike::Edit(*it)->Accept(Direction::Backward, *this, *it);
        if (release) Spike::Release(*it);
    }

    mForwardSpikesCurrent->clear();
    mBackwardSpikesCurrent->clear();

    Control(brainStep);
}

BoundaryNeuron::BoundaryNeuron(RegionBase &region, NeuronId id, RegionId destRegId, GateLaneIdx laneIdx) :
    Neuron(region, id), mDestRegId(destRegId), mLaneIdx(laneIdx)
{
}

BoundaryNeuron::~BoundaryNeuron()
{
}

void BoundaryNeuron::pup(PUP::er &p)
{
    Neuron::pup(p);
    p | mDestRegId;
    p | mLaneIdx;
}

const char *BoundaryNeuron::Type = "BoundaryNeuron";

const char *BoundaryNeuron::GetType()
{
    return Type;
}

void BoundaryNeuron::AdoptAsChild(Neuron *neuron, bool cloneSynapses)
{
    delete neuron;
}

RegionId BoundaryNeuron::GetDestinationRegionId()
{
    return mDestRegId;
}

GateLaneIdx BoundaryNeuron::GetLaneIndex()
{
    return mLaneIdx;
}

bool BoundaryNeuron::HandleSpikeGeneric(Direction direction, Spike::Editor &spike, Spike::Data &data)
{
    // TODO
    return true;
}

void BoundaryNeuron::Control(size_t brainStep)
{
}

ThresholdNeuron::ThresholdNeuron(RegionBase &region, NeuronId id, double threshold, bool isIo) :
    Neuron(region, id, isIo), mThreshold(threshold)
{
}

ThresholdNeuron::~ThresholdNeuron()
{
}

void ThresholdNeuron::pup(PUP::er &p)
{
    Neuron::pup(p);
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
            mForwardSpikesNext->push_back(data);
        } else {
            mBackwardSpikesNext->push_back(data);
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
