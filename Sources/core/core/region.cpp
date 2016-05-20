#include "region.h"

#include "brain.h"

extern CkGroupID gMulticastGroupId;
extern CProxy_CompletionDetector gCompletionDetector;

extern CProxy_Core gCore;
extern CProxy_BrainBase gBrain;
extern CProxy_RegionBase gRegions;
extern CProxy_NeuronBase gNeurons;

RegionMap::RegionMap()
{
}

RegionMap::RegionMap(CkMigrateMessage *msg)
{
}

int RegionMap::procNum(int arrayHdl, const CkArrayIndex &index)
{
    int regionIndex = index.data()[0];

    int nodeNum = regionIndex % CkNumNodes();
    int rankNum = regionIndex % CkNodeSize(nodeNum);
    int peNum = rankNum + CkNodeFirst(nodeNum);

    return peNum;
}

Region::Region(RegionBase &base, json &params) : mBase(base)
{
}

void RegionBase::Connector::pup(PUP::er &p)
{
    p | name;
    p | neurons;

    if (p.isUnpacking()) {
        size_t connectionCount; p | connectionCount;
        for (size_t i = 0; i < connectionCount; ++i) {
            RemoteConnector connector; p | connector;
            connections.insert(connector);
        }
    } else {
        size_t connectionCount = connections.size(); p | connectionCount;
        for (auto it = connections.begin(); it != connections.end(); ++it) {
            RemoteConnector connector(it->first, it->second);
            p | connector;
        }
    }
}

Region *RegionBase::CreateRegion(const RegionType &type, RegionBase &base, json &params)
{
    if (type == ThresholdRegion::Type) {
        return new ThresholdRegion(base, params);
    } else {
        return nullptr;
    }
}

RegionBase::RegionBase(const RegionType &type, const RegionParams &params) : 
    mUnlinking(false), mFullUpdate(false), mDoProgress(false), mBrainStep(0), 
    mNeuronIdxCounter(NEURON_INDEX_MIN), mNeuronSectionFilled(false), mRegion(nullptr)
{
    json p = json::parse(params);

    mRegion = RegionBase::CreateRegion(type, *this, p);
}

RegionBase::RegionBase(CkMigrateMessage *msg) :
    mUnlinking(false), mFullUpdate(false), mDoProgress(false), mBrainStep(0),
    mNeuronIdxCounter(0), mNeuronSectionFilled(false), mRegion(nullptr)
{
}

RegionBase::~RegionBase()
{
    if (mRegion) delete mRegion;
}

void RegionBase::pup(PUP::er &p)
{
    // TODO
}

const char *RegionBase::GetType() const
{
    if (mRegion) {
        return mRegion->GetType();
    } else {
        return "";
    }
}

const char *RegionBase::GetName() const
{
    return mName.c_str();
}

RegionIndex RegionBase::GetIndex() const
{
    return thisIndex;
}

NeuronIndex RegionBase::GetNewNeuronIndex()
{
    if (mNeuronIdxCounter == NEURON_INDEX_MAX) CkAbort("Neuron indices depleted.");
    return mNeuronIdxCounter++;
}

const RegionBase::Connectors &RegionBase::GetInputs() const
{
    return mInputConnectors;
}

const RegionBase::Connector &RegionBase::GetInput(const ConnectorName &name) const
{
    return mInputConnectors.at(name);
}

const RegionBase::Connectors &RegionBase::GetOutputs() const
{
    return mOutputConnectors;
}

const RegionBase::Connector &RegionBase::GetOutput(const ConnectorName &name) const
{
    return mOutputConnectors.at(name);
}

const NeuronAdditionRequests &RegionBase::GetNeuronAdditions() const
{
    return mNeuronAdditions;
}

const NeuronRemovals &RegionBase::GetNeuronRemovals() const
{
    return mNeuronRemovals;
}

const Synapse::Additions &RegionBase::GetSynapseAdditions() const
{
    return mSynapseAdditions;
}

const Synapse::Removals &RegionBase::GetSynapseRemovals() const
{
    return mSynapseRemovals;
}

const ChildLinks &RegionBase::GetChildAdditions() const
{
    return mChildAdditions;
}

const ChildLinks &RegionBase::GetChildRemovals() const
{
    return mChildRemovals;
}

NeuronId RegionBase::RequestNeuronAddition(const NeuronType &type, const NeuronParams &params)
{
    NeuronId neuronId = GetNeuronId(thisIndex, GetNewNeuronIndex());
    mNeuronAdditions.push_back(std::make_tuple(neuronId, type, params));
    return neuronId;
}

void RegionBase::RequestNeuronRemoval(NeuronId neuronId)
{
    mNeuronRemovals.push_back(neuronId);
}

void RegionBase::RequestSynapseAddition(Direction direction, NeuronId from, NeuronId to, const Synapse::Data &data)
{
    mSynapseAdditions.push_back(std::make_tuple(direction, from, to, data));
}

void RegionBase::RequestSynapseRemoval(Direction direction, NeuronId from, NeuronId to)
{
    mSynapseRemovals.push_back(std::make_tuple(direction, from, to));
}

void RegionBase::RequestChildAddition(NeuronId parent, NeuronId child)
{
    mChildAdditions.push_back(std::make_pair(parent, child));
}

void RegionBase::RequestChildRemoval(NeuronId parent, NeuronId child)
{
    mChildRemovals.push_back(std::make_pair(parent, child));
}

void RegionBase::CreateInput(const ConnectorName &name,
    const NeuronType &neuronType, const NeuronParams &neuronParams, size_t neuronCount)
{
    Connector connector;
    connector.name = name;
    for (size_t i = 0; i < neuronCount; ++i) {
        connector.neurons.push_back(
            RequestNeuronAddition(neuronType, neuronParams));
    }
    mInputConnectors.insert(std::make_pair(name, connector));

    gCompletionDetector.ckLocalBranch()->consume();
}

void RegionBase::DeleteInput(const ConnectorName &name)
{
    auto itConn = mInputConnectors.find(name);
    if (itConn != mInputConnectors.end()) {
        Connector &connector = itConn->second;

        for (auto it = connector.connections.begin(); it != connector.connections.end(); ++it) {
            if (it->first != BRAIN_REGION_INDEX) {
                gCompletionDetector.ckLocalBranch()->produce();
                gRegions[it->first].DisconnectOutput(it->second, RemoteConnector(thisIndex, name), false);
                if (!connector.neurons.empty()) {
                    gCompletionDetector.ckLocalBranch()->produce();
                    gRegions[it->first].DisconnectOutputNeurons(
                        it->second, connector.neurons.at(0));
                }
            }
        }

        for (auto it = connector.neurons.begin(); it != connector.neurons.end(); ++it) {
            RequestNeuronRemoval(*it);
        }

        mInputConnectors.erase(name);
    }

    if (!mUnlinking) {
        gCompletionDetector.ckLocalBranch()->done();
        gCompletionDetector.ckLocalBranch()->consume();
    }
}

void RegionBase::ConnectInput(const ConnectorName &name, const RemoteConnector &destination, bool syncSynapses)
{
    auto itConn = mInputConnectors.find(name);
    if (itConn != mInputConnectors.end()) {
        Connector &connector = itConn->second;
        connector.connections.insert(destination);
        if (syncSynapses && !connector.neurons.empty()) {
            if (destination.first != BRAIN_REGION_INDEX) {
                gCompletionDetector.ckLocalBranch()->produce();
                gRegions[destination.first].ConnectOutputNeurons(
                    destination.second, connector.neurons.at(0));
            }
            gCompletionDetector.ckLocalBranch()->done();
        }
    }

    gCompletionDetector.ckLocalBranch()->consume();
}

void RegionBase::DisconnectInput(const ConnectorName &name, const RemoteConnector &destination, bool syncSynapses)
{
    auto itConn = mInputConnectors.find(name);
    if (itConn != mInputConnectors.end()) {
        Connector &connector = itConn->second;
        connector.connections.erase(destination);
        if (syncSynapses && !connector.neurons.empty()) {
            if (destination.first != BRAIN_REGION_INDEX) {
                gCompletionDetector.ckLocalBranch()->produce();
                gRegions[destination.first].DisconnectOutputNeurons(
                    destination.second, connector.neurons.at(0));
            }
            gCompletionDetector.ckLocalBranch()->done();
        }
    }

    gCompletionDetector.ckLocalBranch()->consume();
}

void RegionBase::ConnectInputNeurons(const ConnectorName &name, NeuronId destFirstNeuron)
{
    auto itConn = mInputConnectors.find(name);
    if (itConn != mInputConnectors.end()) {
        Connector &connector = itConn->second;
        Synapse::Data synapse;
        Synapse::Initialize(Synapse::Type::Weighted, synapse);
        NeuronId destNeuronId = destFirstNeuron;
        for (auto it = connector.neurons.begin(); it != connector.neurons.end(); ++it) {
            RequestSynapseAddition(Direction::Forward, destNeuronId++, *it, synapse);
        }
    }

    gCompletionDetector.ckLocalBranch()->consume();
}

void RegionBase::DisconnectInputNeurons(const ConnectorName &name, NeuronId destFirstNeuron)
{
    auto itConn = mInputConnectors.find(name);
    if (itConn != mInputConnectors.end()) {
        Connector &connector = itConn->second;
        NeuronId destNeuronId = destFirstNeuron;
        for (auto it = connector.neurons.begin(); it != connector.neurons.end(); ++it) {
            RequestSynapseRemoval(Direction::Forward, destNeuronId++, *it);
        }
    }

    gCompletionDetector.ckLocalBranch()->consume();
}

void RegionBase::CreateOutput(const ConnectorName &name,
    const NeuronType &neuronType, const NeuronParams &neuronParams, size_t neuronCount)
{
    Connector connector;
    connector.name = name;
    for (size_t i = 0; i < neuronCount; ++i) {
        connector.neurons.push_back(
            RequestNeuronAddition(neuronType, neuronParams));
    }
    mOutputConnectors.insert(std::make_pair(name, connector));

    gCompletionDetector.ckLocalBranch()->consume();
}

void RegionBase::DeleteOutput(const ConnectorName &name)
{
    auto itConn = mInputConnectors.find(name);
    if (itConn != mInputConnectors.end()) {
        Connector &connector = itConn->second;

        for (auto it = connector.connections.begin(); it != connector.connections.end(); ++it) {
            if (it->first != BRAIN_REGION_INDEX) {
                gCompletionDetector.ckLocalBranch()->produce();
                gRegions[it->first].DisconnectInput(it->second, RemoteConnector(thisIndex, name), false);
                if (!connector.neurons.empty()) {
                    gCompletionDetector.ckLocalBranch()->produce();
                    gRegions[it->first].DisconnectInputNeurons(
                        it->second, connector.neurons.at(0));
                }
            }
        }

        for (auto it = connector.neurons.begin(); it != connector.neurons.end(); ++it) {
            RequestNeuronRemoval(*it);
        }

        mInputConnectors.erase(name);
    }

    if (!mUnlinking) {
        gCompletionDetector.ckLocalBranch()->done();
        gCompletionDetector.ckLocalBranch()->consume();
    }
}

void RegionBase::ConnectOutput(const ConnectorName &name, const RemoteConnector &destination, bool syncSynapses)
{
    auto itConn = mOutputConnectors.find(name);
    if (itConn != mOutputConnectors.end()) {
        Connector &connector = itConn->second;
        connector.connections.insert(destination);
        if (syncSynapses && !connector.neurons.empty()) {
            if (destination.first != BRAIN_REGION_INDEX) {
                gCompletionDetector.ckLocalBranch()->produce();
                gRegions[destination.first].ConnectInputNeurons(
                    destination.second, connector.neurons.at(0));
            }
            gCompletionDetector.ckLocalBranch()->done();
        }
    }

    gCompletionDetector.ckLocalBranch()->consume();
}

void RegionBase::DisconnectOutput(const ConnectorName &name, const RemoteConnector &destination, bool syncSynapses)
{
    auto itConn = mOutputConnectors.find(name);
    if (itConn != mOutputConnectors.end()) {
        Connector &connector = itConn->second;
        connector.connections.erase(destination);
        if (syncSynapses && !connector.neurons.empty()) {
            if (destination.first != BRAIN_REGION_INDEX) {
                gCompletionDetector.ckLocalBranch()->produce();
                gRegions[destination.first].DisconnectInputNeurons(
                    destination.second, connector.neurons.at(0));
            }
            gCompletionDetector.ckLocalBranch()->done();
        }
    }

    gCompletionDetector.ckLocalBranch()->consume();
}

void RegionBase::ConnectOutputNeurons(const ConnectorName &name, NeuronId destFirstNeuron)
{
    auto itConn = mOutputConnectors.find(name);
    if (itConn != mOutputConnectors.end()) {
        Connector &connector = itConn->second;
        Synapse::Data synapse;
        Synapse::Initialize(Synapse::Type::Weighted, synapse);
        NeuronId destNeuronId = destFirstNeuron;
        for (auto it = connector.neurons.begin(); it != connector.neurons.end(); ++it) {
            RequestSynapseAddition(Direction::Forward, *it, destNeuronId++, synapse);
        }
    }

    gCompletionDetector.ckLocalBranch()->consume();
}

void RegionBase::DisconnectOutputNeurons(const ConnectorName &name, NeuronId destFirstNeuron)
{
    auto itConn = mOutputConnectors.find(name);
    if (itConn != mOutputConnectors.end()) {
        Connector &connector = itConn->second;
        NeuronId destNeuronId = destFirstNeuron;
        for (auto it = connector.neurons.begin(); it != connector.neurons.end(); ++it) {
            RequestSynapseRemoval(Direction::Forward, *it, destNeuronId++);
        }
    }

    gCompletionDetector.ckLocalBranch()->consume();
}

void RegionBase::ReceiveSensoMotoricData(Direction direction, const ConnectorName &connectorName, Spike::BrainSource &data)
{
    Connector *connector = nullptr;
    
    if (direction == Direction::Forward) {
        auto itConn = mInputConnectors.find(connectorName);
        if (itConn != mInputConnectors.end()) {
            connector = &itConn->second;
        }
    } else {
        auto itConn = mOutputConnectors.find(connectorName);
        if (itConn != mOutputConnectors.end()) {
            connector = &itConn->second;
        }
    }

    if (connector) {
        if (data.size() == connector->neurons.size()) {
            gCompletionDetector.ckLocalBranch()->produce(data.size());
            for (size_t i = 0; i < data.size(); ++i) {
                NeuronId neuronId = connector->neurons[i];
                gNeurons(GetRegionIndex(neuronId), GetNeuronIndex(neuronId)).EnqueueSpike(direction, data[i]);
            }
        }
    }

    gCompletionDetector.ckLocalBranch()->done();
    gCompletionDetector.ckLocalBranch()->consume();
}

void RegionBase::EnqueueSensoMotoricSpike(NeuronId receiver, const Spike::Data &data)
{
    mBrainSink.push_back(std::make_pair(receiver, data));
}

void RegionBase::Unlink()
{
    mUnlinking = true;
    while (!mInputConnectors.empty()) {
        DeleteInput(mInputConnectors.begin()->first);
    }
    while (!mOutputConnectors.empty()) {
        DeleteOutput(mOutputConnectors.begin()->first);
    }
    mUnlinking = false;

    gCompletionDetector.ckLocalBranch()->done();
    gCompletionDetector.ckLocalBranch()->consume();

    for (auto it = mNeuronIndices.begin(); it != mNeuronIndices.end(); ++it) {
        gNeurons(thisIndex, *it).ckDestroy();
    }
    mNeuronIndices.clear();
}

void RegionBase::PrepareTopologyChange(size_t brainStep, bool doProgress)
{
    if (mRegion && doProgress) mRegion->Control(brainStep);

    CkCallback cb(CkReductionTarget(BrainBase, SimulateRegionPrepareTopologyChangeDone), gBrain[0]);
    std::unordered_set<NeuronId> deletedNeurons;
    deletedNeurons.insert(mNeuronRemovals.begin(), mNeuronRemovals.end());
    size_t deletedNeuronCount = deletedNeurons.size();
    contribute(sizeof(size_t), &deletedNeuronCount, CkReduction::sum_ulong_long, cb);
}

void RegionBase::CommitTopologyChange()
{
    if (!mNeuronAdditions.empty()) {
        gNeurons.beginInserting();
        for (auto it = mNeuronAdditions.begin(); it != mNeuronAdditions.end(); ++it) {
            mNeuronIndices.insert(std::get<0>(*it));
            gNeurons(thisIndex, std::get<0>(*it)).insert(std::get<1>(*it), std::get<2>(*it));
        }
        gNeurons.doneInserting();
    }

    if (!mSynapseAdditions.empty()) {
        for (auto it = mSynapseAdditions.begin(); it != mSynapseAdditions.end(); ++it) {
            gCompletionDetector.ckLocalBranch()->produce(2);
            NeuronId from = std::get<0>(*it) == Direction::Forward ? std::get<1>(*it) : std::get<2>(*it);
            NeuronId to = std::get<0>(*it) == Direction::Forward ? std::get<2>(*it) : std::get<1>(*it);
            gNeurons(GetRegionIndex(from), GetNeuronIndex(from)).AddOutputSynapse(to, std::get<3>(*it));
            gNeurons(GetRegionIndex(to), GetNeuronIndex(to)).AddInputSynapse(from, std::get<3>(*it));
        }
    }

    if (!mChildAdditions.empty()) {
        for (auto it = mChildAdditions.begin(); it != mChildAdditions.end(); ++it) {
            gCompletionDetector.ckLocalBranch()->produce(2);
            NeuronId parent = std::get<0>(*it);
            NeuronId child = std::get<1>(*it);
            gNeurons(GetRegionIndex(parent), GetNeuronIndex(parent)).AddChild(child);
            gNeurons(GetRegionIndex(child), GetNeuronIndex(child)).SetParent(parent);
        }
    }

    if (!mChildRemovals.empty()) {
        for (auto it = mChildRemovals.begin(); it != mChildRemovals.end(); ++it) {
            gCompletionDetector.ckLocalBranch()->produce(2);
            NeuronId parent = std::get<0>(*it);
            NeuronId child = std::get<1>(*it);
            gNeurons(GetRegionIndex(parent), GetNeuronIndex(parent)).RemoveChild(child);
            gNeurons(GetRegionIndex(child), GetNeuronIndex(child)).UnsetParent();
        }
    }

    if (!mSynapseRemovals.empty()) {
        for (auto it = mSynapseRemovals.begin(); it != mSynapseRemovals.end(); ++it) {
            gCompletionDetector.ckLocalBranch()->produce(2);
            NeuronId from = std::get<0>(*it) == Direction::Forward ? std::get<1>(*it) : std::get<2>(*it);
            NeuronId to = std::get<0>(*it) == Direction::Forward ? std::get<2>(*it) : std::get<1>(*it);
            gNeurons(GetRegionIndex(from), GetNeuronIndex(from)).RemoveOutputSynapse(to);
            gNeurons(GetRegionIndex(to), GetNeuronIndex(to)).RemoveInputSynapse(from);
        }
    }

    if (!mNeuronRemovals.empty()) {
        gCompletionDetector.ckLocalBranch()->produce(mNeuronRemovals.size());
        CkVec<CkArrayIndex2D> deletedNeuronIndices;
        {
            std::unordered_set<NeuronId> deletedNeurons;
            deletedNeurons.insert(mNeuronRemovals.begin(), mNeuronRemovals.end());
            for (auto it = deletedNeurons.begin(); it != deletedNeurons.end(); ++it) {
                CkArrayIndex2D index(GetRegionIndex(*it), GetNeuronIndex(*it));
                deletedNeuronIndices.push_back(index);
            }
        }
        CProxySection_NeuronBase neuronSection = CProxySection_NeuronBase::ckNew(
            gNeurons.ckGetArrayID(), deletedNeuronIndices.getVec(), deletedNeuronIndices.size());
        neuronSection.ckSectionDelegate(CProxy_CkMulticastMgr(gMulticastGroupId).ckLocalBranch());
        neuronSection.Unlink();
    }

    gCompletionDetector.ckLocalBranch()->done();

    CkCallback cb(CkReductionTarget(BrainBase, SimulateRegionCommitTopologyChangeDone), gBrain[0]);
    size_t triggeredNeuronCount = mNeuronsTriggered.size();
    contribute(sizeof(size_t), &triggeredNeuronCount, CkReduction::sum_ulong_long, cb);
}

void RegionBase::Simulate(SimulateMsg *msg)
{
    mFullUpdate = msg->fullUpdate;
    mDoProgress = msg->doProgress;
    mBrainStep = msg->brainStep;
    mRoiBoxes = msg->roiBoxes;
    
    if (!mNeuronRemovals.empty()) {
        std::unordered_set<NeuronId> deletedNeurons;
        deletedNeurons.insert(mNeuronRemovals.begin(), mNeuronRemovals.end());
        for (auto it = deletedNeurons.begin(); it != deletedNeurons.end(); ++it) {
            mNeuronIndices.erase(*it);
            gNeurons(GetRegionIndex(*it), GetNeuronIndex(*it)).ckDestroy();
        }
    }

    CkVec<CkArrayIndex2D> sectionNeuronIndices;
    if (mFullUpdate && !mNeuronIndices.empty()) {
        for (auto it = mNeuronIndices.begin(); it != mNeuronIndices.end(); ++it) {
            CkArrayIndex2D index(GetRegionIndex(*it), GetNeuronIndex(*it));
            sectionNeuronIndices.push_back(index);
        }

    } else if (mDoProgress && !mNeuronsTriggered.empty()) {
        for (auto it = mNeuronsTriggered.begin(); it != mNeuronsTriggered.end(); ++it) {
            CkArrayIndex2D index(GetRegionIndex(*it), GetNeuronIndex(*it));
            sectionNeuronIndices.push_back(index);
        }
    }

    if (sectionNeuronIndices.size() > 0) {
        mNeuronSectionFilled = true;
        CProxySection_NeuronBase mNeuronSection = CProxySection_NeuronBase::ckNew(
            gNeurons.ckGetArrayID(), sectionNeuronIndices.getVec(), sectionNeuronIndices.size());
        mNeuronSection.ckSectionDelegate(CProxy_CkMulticastMgr(gMulticastGroupId).ckLocalBranch());
    }

    if (mDoProgress && mNeuronSectionFilled) {
        EmptyMsg *emptyMsg = new EmptyMsg();
        mNeuronSection.FlipSpikeQueues(emptyMsg);
    } else {
        NeuronFlipSpikeQueuesDone(nullptr);
    }

    delete msg;
}

void RegionBase::NeuronFlipSpikeQueuesDone(CkReductionMsg *msg)
{
    if (mNeuronSectionFilled) {

        SimulateMsg *simulateMsg = new SimulateMsg();
        simulateMsg->fullUpdate = mFullUpdate;
        simulateMsg->doProgress = mDoProgress;
        simulateMsg->brainStep = mBrainStep;
        simulateMsg->roiBoxes = mRoiBoxes;

        mNeuronSection.Simulate(simulateMsg);

    } else {
        NeuronSimulateDone(nullptr);
    }

    if (msg) delete msg;
}

void RegionBase::NeuronSimulateDone(CkReductionMsg *msg)
{
    if (mNeuronSectionFilled) {
        mNeuronSectionFilled = false;
        mNeuronSection = CProxySection_NeuronBase();
    }

    uint8_t *resultPtr = nullptr;
    size_t resultSize = 0;

    // TODO generate upward result

    if (msg) {
        CkReduction::setElement *current = static_cast<CkReduction::setElement *>(msg->getData());
        while (current != nullptr) {
            int *result = reinterpret_cast<int *>(&current->data);

            // TODO integrate downward result

            current = current->next();
        }

        delete msg;
    }

    CkCallback cb(CkReductionTarget(BrainBase, SimulateRegionSimulateDone), gBrain[0]);
    contribute(resultSize, resultPtr, CkReduction::set, cb);
}

const char *ThresholdRegion::Type = "ThresholdRegion";

ThresholdRegion::ThresholdRegion(RegionBase &base, json &params) : Region(base, params)
{
    // TODO
}

ThresholdRegion::~ThresholdRegion()
{
}

void ThresholdRegion::pup(PUP::er &p)
{
}

const char *ThresholdRegion::GetType() const
{
    return Type;
}

void ThresholdRegion::Control(size_t brainStep)
{
    // TODO
}

void ThresholdRegion::AcceptContributionFromNeuron(
    NeuronId neuronId, const uint8_t *contribution, size_t size)
{
    // TODO
}

size_t ThresholdRegion::ContributeToBrain(uint8_t *&contribution)
{
    // TODO
    contribution = nullptr;
    return 0;
}

#include "region.def.h"
