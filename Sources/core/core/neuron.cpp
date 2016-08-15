#include "neuron.h"
#include "brain.h"
#include "random.h"
#include "components.h"

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
    //int neuronIndex = index.data()[1];

    int nodeNum = regionIndex % CkNumNodes();
    int rankNum = regionIndex % CkNodeSize(nodeNum);
    //int rankNum = neuronIndex % CkNodeSize(nodeNum);
    int peNum = rankNum + CkNodeFirst(nodeNum);

    return peNum;
}

Neuron::Neuron(NeuronBase &base, json &params) : mBase(base)
{
}

Neuron *NeuronBase::CreateNeuron(const NeuronType &type, NeuronBase &base, json &params)
{
    NeuronFactory *neuronFactory = NeuronFactory::GetInstance();
    return neuronFactory->Create(type, base, params);
}

void Neuron::HandleSpikeGeneric(Direction direction, Spike::Editor &spike, Spike::Data &data)
{
    return;
}

void Neuron::HandleSpike(Direction direction, BinarySpike &spike, Spike::Data &data)
{
    HandleSpikeGeneric(direction, spike, data);
}

void Neuron::HandleSpike(Direction direction, DiscreteSpike &spike, Spike::Data &data)
{
    HandleSpikeGeneric(direction, spike, data);
}

void Neuron::HandleSpike(Direction direction, ContinuousSpike &spike, Spike::Data &data)
{
    HandleSpikeGeneric(direction, spike, data);
}

void Neuron::HandleSpike(Direction direction, VisualSpike &spike, Spike::Data &data)
{
    HandleSpikeGeneric(direction, spike, data);
}

void Neuron::HandleSpike(Direction direction, FunctionalSpike &spike, Spike::Data &data)
{
    HandleSpikeGeneric(direction, spike, data);
}

void Neuron::HandleSpike(Direction direction, MultiByteSpike &spike, Spike::Data &data)
{
    HandleSpikeGeneric(direction, spike, data);
}

NeuronBase::NeuronBase(const NeuronType &type, const NeuronParams &params)
{
    json p;
    try {
        p = json::parse(params);
    } catch (std::invalid_argument &) { }

    Random::Engine &engine = Random::GetThreadEngine();
    std::uniform_real_distribution<float> randFloat(0.0, 1.0);

    mNeverSimulated = true;
    mTempIdCounter = GetNeuronId(TEMP_REGION_INDEX, NEURON_INDEX_MIN);

    if (p.find("position") != p.end()) {
        mPosition = Point3D(
            p["position"]["x"].get<float>(),
            p["position"]["y"].get<float>(),
            p["position"]["z"].get<float>());
    } else {
        mPosition = Point3D(randFloat(engine), randFloat(engine), randFloat(engine));
    }

    mNeuron = CreateNeuron(type, *this, p);
    mParent = DELETED_NEURON_ID;
    mChildren.set_deleted_key(DELETED_NEURON_ID);
    mInputSynapses.set_deleted_key(DELETED_NEURON_ID);
    mOutputSynapses.set_deleted_key(DELETED_NEURON_ID);
    mBackwardSpikesCurrent = new Spikes();
    mBackwardSpikesNext = new Spikes();
    mForwardSpikesCurrent = new Spikes();
    mForwardSpikesNext = new Spikes();
    mSectionInfo = CkSectionInfo();
}

NeuronBase::NeuronBase(CkMigrateMessage *msg) :
    mNeverSimulated(true), mTempIdCounter(0), mPosition(0.0f, 0.0f, 0.0f), mParent(0),
    mBackwardSpikesCurrent(nullptr), mBackwardSpikesNext(nullptr),
    mForwardSpikesCurrent(nullptr), mForwardSpikesNext(nullptr),
    mNeuron(nullptr)
{
    mChildren.set_deleted_key(DELETED_NEURON_ID);
    mInputSynapses.set_deleted_key(DELETED_NEURON_ID);
    mOutputSynapses.set_deleted_key(DELETED_NEURON_ID);
    mSectionInfo = CkSectionInfo();
}

NeuronBase::~NeuronBase()
{   
    delete mBackwardSpikesCurrent;
    delete mBackwardSpikesNext;
    delete mForwardSpikesCurrent;
    delete mForwardSpikesNext;

    delete mNeuron;
}

void NeuronBase::pup(PUP::er &p)
{
    p | mNeverSimulated;
    p | mTempIdCounter;

    p | mPosition;

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

        size_t triggeredCount; p | triggeredCount;
        mNeuronsTriggered.reserve(triggeredCount);
        for (size_t i = 0; i < triggeredCount; ++i) {
            NeuronId triggered; p | triggered;
            mNeuronsTriggered.insert(triggered);
        }

        json neuronParams;
        RegionType neuronType;
        p | neuronType;
        mNeuron = CreateNeuron(neuronType, *this, neuronParams);
        if (mNeuron) mNeuron->pup(p);
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

        size_t triggeredCount = mNeuronsTriggered.size(); p | triggeredCount;
        for (auto it = mNeuronsTriggered.begin(); it != mNeuronsTriggered.end(); ++it) {
            NeuronId triggered = *it; p | triggered;
        }

        RegionType neuronType;
        if (mNeuron) neuronType = mNeuron->GetType();
        p | neuronType;
        if (mNeuron) mNeuron->pup(p);
    }
}

const char *NeuronBase::GetType() const
{
    if (mNeuron) {
        return mNeuron->GetType();
    } else {
        return "";
    }
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
                Synapse::Clone(it->second, data);
            } else {
                Synapse::Initialize(Synapse::GetType(it->second), data);
            }
        }
        RequestSynapseAddition(Direction::Forward, it->first, GetId(), data);
    }

    for (auto it = mOutputSynapses.begin(); it != mOutputSynapses.end(); ++it) {
        Synapse::Data data;
        {
            if (cloneSynapses) {
                Synapse::Clone(it->second, data);
            } else {
                Synapse::Initialize(Synapse::GetType(it->second), data);
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
        gCompletionDetector.ckLocalBranch()->produce();
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
        gCompletionDetector.ckLocalBranch()->produce();
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
    RegionIndex fromRegIdx = GetRegionIndex(from);
    RegionIndex toRegIdx = GetRegionIndex(to);
    bool fromValid = (fromRegIdx == TEMP_REGION_INDEX) || (fromRegIdx == thisIndex.x);
    bool toValid = (toRegIdx == TEMP_REGION_INDEX) || (toRegIdx == thisIndex.x);
    if (fromValid && toValid) {
        mSynapseAdditions.push_back(std::make_tuple(direction, from, to, data));
    }
}

void NeuronBase::RequestSynapseRemoval(Direction direction, NeuronId from, NeuronId to)
{
    RegionIndex fromRegIdx = GetRegionIndex(from);
    RegionIndex toRegIdx = GetRegionIndex(to);
    bool fromValid = (fromRegIdx == TEMP_REGION_INDEX) || (fromRegIdx == thisIndex.x);
    bool toValid = (toRegIdx == TEMP_REGION_INDEX) || (toRegIdx == thisIndex.x);
    if (fromValid && toValid) {
        mSynapseRemovals.push_back(std::make_tuple(direction, from, to));
    }
}

void NeuronBase::RequestChildAddition(NeuronId parent, NeuronId child)
{
    RegionIndex parentRegIdx = GetRegionIndex(parent);
    RegionIndex childRegIdx = GetRegionIndex(child);
    bool parentValid = (parentRegIdx == TEMP_REGION_INDEX) || (parentRegIdx == thisIndex.x);
    bool childValid = (childRegIdx == TEMP_REGION_INDEX) || (childRegIdx == thisIndex.x);
    if (parentValid && childValid) {
        mChildAdditions.push_back(std::make_pair(parent, child));
    }
}

void NeuronBase::RequestChildRemoval(NeuronId parent, NeuronId child)
{
    mChildRemovals.push_back(std::make_pair(parent, child));
}

void NeuronBase::SetParent(NeuronId parent)
{
    mParent = parent;
    gCompletionDetector.ckLocalBranch()->consume();
}

void NeuronBase::UnsetParent()
{
    mParent = DELETED_NEURON_ID;
    gCompletionDetector.ckLocalBranch()->consume();
}

void NeuronBase::AddChild(NeuronId child)
{
    mChildren.insert(child);
    gCompletionDetector.ckLocalBranch()->consume();
}

void NeuronBase::RemoveChild(NeuronId child)
{
    mChildren.erase(child);
    gCompletionDetector.ckLocalBranch()->consume();
}

void NeuronBase::AddInputSynapse(NeuronId from, const Synapse::Data &data)
{
    mInputSynapses.insert(std::make_pair(from, data));
    gCompletionDetector.ckLocalBranch()->consume();
}

void NeuronBase::SynchronizeInputSynapse(NeuronId from, const Synapse::Data &data)
{
    auto it = mInputSynapses.find(from);
    if (it != mInputSynapses.end()) {
        mInputSynapses[from] = data;
    }
    gCompletionDetector.ckLocalBranch()->consume();
}

void NeuronBase::RemoveInputSynapse(NeuronId from)
{
    mInputSynapses.erase(from);
    gCompletionDetector.ckLocalBranch()->consume();
}

void NeuronBase::AddOutputSynapse(NeuronId to, const Synapse::Data &data)
{
    mOutputSynapses.insert(std::make_pair(to, data));
    gCompletionDetector.ckLocalBranch()->consume();
}

void NeuronBase::SynchronizeOutputSynapse(NeuronId to, const Synapse::Data &data)
{
    auto it = mOutputSynapses.find(to);
    if (it != mOutputSynapses.end()) {
        mOutputSynapses[to] = data;
    }
    gCompletionDetector.ckLocalBranch()->consume();
}

void NeuronBase::RemoveOutputSynapse(NeuronId to)
{
    mOutputSynapses.erase(to);
    gCompletionDetector.ckLocalBranch()->consume();
}

bool NeuronBase::AdaptPosition()
{
    return false;
}

void NeuronBase::SendSpike(NeuronId receiver, Direction direction, const Spike::Data &data)
{
    if (AccessInputSynapse(receiver) || AccessOutputSynapse(receiver)) {
        RegionIndex destRegIdx = GetRegionIndex(receiver);
        if (destRegIdx == BRAIN_REGION_INDEX) {
            gCompletionDetector.ckLocalBranch()->produce();
            gRegions[thisIndex.x].EnqueueSensoMotoricSpike(receiver, data);
        } else {
            if (destRegIdx != thisIndex.x) {
                gCompletionDetector.ckLocalBranch()->produce();
                gRegions[destRegIdx].TriggerRemotelyTriggeredNeuron(receiver);
            } else {
                mNeuronsTriggered.insert(receiver);
            }
            gCompletionDetector.ckLocalBranch()->produce();
            gNeurons(destRegIdx, GetNeuronIndex(receiver)).EnqueueSpike(direction, data);
        }
    }
}

void NeuronBase::EnqueueSpike(Direction direction, const Spike::Data &data)
{
    if (direction == Direction::Forward) {
        mForwardSpikesNext->push_back(data);
    } else {
        mBackwardSpikesNext->push_back(data);
    }
    gCompletionDetector.ckLocalBranch()->consume();
}

void NeuronBase::Unlink()
{
    if (mParent != DELETED_NEURON_ID) {
        gCompletionDetector.ckLocalBranch()->produce();
        gNeurons(GetRegionIndex(mParent), GetNeuronIndex(mParent)).RemoveChild(
            GetNeuronId(thisIndex.x, thisIndex.y));
        mParent = DELETED_NEURON_ID;
    }

    if (!mChildren.empty()) {
        gCompletionDetector.ckLocalBranch()->produce(mChildren.size());
        for (auto it = mChildren.begin(); it != mChildren.end(); ++it) {
            gNeurons(GetRegionIndex(*it), GetNeuronIndex(*it)).UnsetParent();
        }
        mChildren.clear();
    }

    if (!mInputSynapses.empty()) {
        gCompletionDetector.ckLocalBranch()->produce(mInputSynapses.size());
        for (auto it = mInputSynapses.begin(); it != mInputSynapses.end(); ++it) {
            gNeurons(GetRegionIndex(it->first), GetNeuronIndex(it->first)).RemoveOutputSynapse(
                GetNeuronId(thisIndex.x, thisIndex.y));
        }
        mInputSynapses.clear();
    }

    if (!mOutputSynapses.empty()) {
        gCompletionDetector.ckLocalBranch()->produce(mOutputSynapses.size());
        for (auto it = mOutputSynapses.begin(); it != mOutputSynapses.end(); ++it) {
            gNeurons(GetRegionIndex(it->first), GetNeuronIndex(it->first)).RemoveInputSynapse(
                GetNeuronId(thisIndex.x, thisIndex.y));
        }
        mOutputSynapses.clear();
    }

    gCompletionDetector.ckLocalBranch()->done();
    gCompletionDetector.ckLocalBranch()->consume();
}

void NeuronBase::FlipSpikeQueues(EmptyMsg *msg)
{
    std::swap(mForwardSpikesCurrent, mForwardSpikesNext);
    std::swap(mBackwardSpikesCurrent, mBackwardSpikesNext);

    mSectionInfo = CkSectionInfo();
    CkGetSectionInfo(mSectionInfo, msg);
    CkCallback cb(CkReductionTarget(RegionBase, NeuronFlipSpikeQueuesDone), gRegions[thisIndex.x]);
    CProxy_CkMulticastMgr(gMulticastGroupId).ckLocalBranch()->contribute(
        0, nullptr, CkReduction::nop, mSectionInfo, cb);
}

void NeuronBase::Simulate(SimulateMsg *msg)
{
    bool doUpdate = msg->doUpdate;
    bool doFullUpdate = msg->doFullUpdate;
    bool doProgress = msg->doProgress;
    size_t brainStep = msg->brainStep;
    Boxes roiBoxes = msg->roiBoxes;
    Boxes roiBoxesLast = msg->roiBoxesLast;
    Observers observers = msg->observers;

    NeuronId neuronId = GetNeuronId(thisIndex.x, thisIndex.y);

    bool changedPosition = false;
    bool wasInsideOfAny = (doFullUpdate || mNeverSimulated) ? false : IsInsideOfAny(mPosition, roiBoxesLast);
    if (wasInsideOfAny) {
        changedPosition = AdaptPosition();
    }
    bool isInsideOfAny = IsInsideOfAny(mPosition, roiBoxes);
    bool skipTopologyReport = !doUpdate || (!wasInsideOfAny && !isInsideOfAny) ||
        (wasInsideOfAny && isInsideOfAny && !changedPosition);

    NeuronAdditionReports addedNeurons;
    NeuronAdditionReports repositionedNeurons;
    NeuronRemovals removedNeurons;
    Synapse::Links addedSynapses;
    Synapse::Links spikedSynapses;
    Synapse::Links removedSynapses;
    ChildLinks addedChildren;
    ChildLinks removedChildren;

    if (!skipTopologyReport) {
        if (!wasInsideOfAny && isInsideOfAny) {

            addedNeurons.push_back(NeuronAdditionReport(neuronId, GetType(), mPosition));
            addedSynapses.reserve(mInputSynapses.size() + mOutputSynapses.size());
            for (auto it = mInputSynapses.begin(); it != mInputSynapses.end(); ++it) {
                addedSynapses.push_back(Synapse::Link(it->first, neuronId));
            }
            for (auto it = mOutputSynapses.begin(); it != mOutputSynapses.end(); ++it) {
                addedSynapses.push_back(Synapse::Link(neuronId, it->first));
            }
            addedChildren.reserve(1 + mChildren.size());
            addedChildren.push_back(ChildLink(mParent, neuronId));
            for (auto it = mChildren.begin(); it != mChildren.end(); ++it) {
                addedChildren.push_back(ChildLink(neuronId, *it));
            }
        } else if (wasInsideOfAny && !isInsideOfAny) {

            removedNeurons.push_back((neuronId));
            removedSynapses.reserve(mInputSynapses.size() + mOutputSynapses.size());
            for (auto it = mInputSynapses.begin(); it != mInputSynapses.end(); ++it) {
                removedSynapses.push_back(Synapse::Link(it->first, neuronId));
            }
            for (auto it = mOutputSynapses.begin(); it != mOutputSynapses.end(); ++it) {
                removedSynapses.push_back(Synapse::Link(neuronId, it->first));
            }
            removedChildren.reserve(1 + mChildren.size());
            removedChildren.push_back(ChildLink(mParent, neuronId));
            for (auto it = mChildren.begin(); it != mChildren.end(); ++it) {
                removedChildren.push_back(ChildLink(neuronId, *it));
            }

        } else if (changedPosition) {
            repositionedNeurons.push_back(NeuronAdditionReport(neuronId, GetType(), mPosition));
        }
    }

    uint8_t *customContributionPtr = nullptr;
    size_t customContributionSize = 0;

    ObserverResults observerResults;

    if (doProgress) {
        bool wasSpiked = !mForwardSpikesCurrent->empty() || !mBackwardSpikesCurrent->empty();

        for (auto it = mForwardSpikesCurrent->begin(); it != mForwardSpikesCurrent->end(); ++it) {
            if (mNeuron) Spike::Edit(*it)->Accept(Direction::Forward, *mNeuron, *it);
        }

        for (auto it = mBackwardSpikesCurrent->begin(); it != mBackwardSpikesCurrent->end(); ++it) {
            if (mNeuron) Spike::Edit(*it)->Accept(Direction::Backward, *mNeuron, *it);
        }

        mForwardSpikesCurrent->clear();
        mBackwardSpikesCurrent->clear();

        if (mNeuron && wasSpiked) {
            mNeuron->Control(brainStep);
            customContributionSize = mNeuron->ContributeToRegion(customContributionPtr);

            for (auto observer : observers) {
                if (std::get<0>(observer) == neuronId) {
                    ObserverResult currentResult(observer, std::vector<int32_t>(), std::vector<uint8_t>());
                    mNeuron->CalculateObserver(std::get<1>(observer), std::get<1>(currentResult), std::get<2>(currentResult));
                    observerResults.push_back(currentResult);
                }
            }
        }
    }
    
    // Spikes happened during Control(), we can gather them for UI now.
    if (isInsideOfAny) {
        spikedSynapses.reserve(mNeuronsTriggered.size());
        for (auto it = mNeuronsTriggered.begin(); it != mNeuronsTriggered.end(); ++it) {
            spikedSynapses.push_back(Synapse::Link(neuronId, *it));
        }
    }

    gCompletionDetector.ckLocalBranch()->done();

    bool skipDynamicityReport =
        mNeuronAdditions.empty() && mNeuronRemovals.empty() && mSynapseAdditions.empty() &&
        mSynapseRemovals.empty() && mChildAdditions.empty() && mChildRemovals.empty();
    
    uint8_t *resultPtr = nullptr;
    size_t resultSize = 0;
    
    for (size_t i = 0; i < 2; ++i) {

        PUP::sizer sizer;
        PUP::toMem toMem(resultPtr);
        PUP::er *p = (i == 0) ? static_cast<PUP::er *>(&sizer) : static_cast<PUP::er *>(&toMem);

        *p | neuronId;

        size_t triggeredCount = mNeuronsTriggered.size(); *p | triggeredCount;
        for (auto it = mNeuronsTriggered.begin(); it != mNeuronsTriggered.end(); ++it) {
            NeuronId triggered = *it; *p | triggered;
        }

        *p | customContributionSize;
        if (customContributionSize > 0) {
            (*p)(customContributionPtr, customContributionSize);
        }

        *p | observerResults;

        *p | skipDynamicityReport;
        if (!skipDynamicityReport) {
            *p | mNeuronAdditions;
            *p | mNeuronRemovals;
            *p | mSynapseAdditions;
            *p | mSynapseRemovals;
            *p | mChildAdditions;
            *p | mChildRemovals;
        }
        
        *p | skipTopologyReport;
        if (!skipTopologyReport) {
            *p | addedNeurons;
            *p | repositionedNeurons;
            *p | removedNeurons;
            *p | addedSynapses;
            *p | removedSynapses;
            *p | addedChildren;
            *p | removedChildren;
        }

        *p | isInsideOfAny;
        if (isInsideOfAny) {
            *p | spikedSynapses;
        }

        if (i == 0) {
            resultSize = sizer.size();
            resultPtr = new uint8_t[resultSize];
        }
    }

    // If there is no progress, FlipSpikeQueues wasn't called and we need to reset the section info.
    if (!doProgress)
        mSectionInfo = CkSectionInfo();

    CkGetSectionInfo(mSectionInfo, msg);
    CkCallback cb(CkReductionTarget(RegionBase, NeuronSimulateDone), gRegions[thisIndex.x]);
    CProxy_CkMulticastMgr(gMulticastGroupId).ckLocalBranch()->contribute(
        resultSize, resultPtr, CkReduction::set, mSectionInfo, cb);

    mNeuronsTriggered.clear();

    if (!skipDynamicityReport) {
        mNeuronAdditions.clear();
        mNeuronRemovals.clear();
        mSynapseAdditions.clear();
        mSynapseRemovals.clear();
        mChildAdditions.clear();
        mChildRemovals.clear();
    }

    mNeverSimulated = false;

    delete[] resultPtr;
    if (customContributionSize > 0) delete[] customContributionPtr;
}

#include "neuron.def.h"
