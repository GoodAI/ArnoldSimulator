#include "brain.h"

extern CkGroupID gMulticastGroupId;
extern CProxy_CompletionDetector gCompletionDetector;

extern CProxy_Core gCore;
extern CProxy_BrainBase gBrain;
extern CProxy_RegionBase gRegions;
extern CProxy_NeuronBase gNeurons;

ViewportUpdate::ViewportUpdate() : sinceBrainStep(0), brainStepCount(0)
{
}

void ViewportUpdate::pup(PUP::er &p)
{
    p | sinceBrainStep;
    p | brainStepCount;
    p | addedRegions;
    p | repositionedRegions;
    p | removedRegions;
    p | addedConnectors;
    p | removedConnectors;
    p | addedConnections;
    p | removedConnections;
    p | addedNeurons;
    p | repositionedNeurons;
    p | removedNeurons;
    p | addedSynapses;
    p | spikedSynapses;
    p | removedSynapses;
    p | addedChildren;
    p | removedChildren;
}

void *SimulateMsg::pack(SimulateMsg *msg)
{
    size_t boxCnt = msg->roiBoxes.size();
    size_t size = (sizeof(bool) * 2) + (sizeof(size_t) * 2) + (sizeof(Box3D) * boxCnt);
    char *buf = (char *)CkAllocBuffer(msg, size);
    char *cur = buf;

    std::memcpy(cur, &msg->fullUpdate, sizeof(bool));
    cur += sizeof(bool);

    std::memcpy(cur, &msg->doProgress, sizeof(bool));
    cur += sizeof(bool);

    std::memcpy(cur, &msg->brainStep, sizeof(size_t));
    cur += sizeof(size_t);

    std::memcpy(cur, &boxCnt, sizeof(size_t));
    cur += sizeof(size_t);

    std::memcpy(cur, msg->roiBoxes.data(), sizeof(Box3D) * boxCnt);
    cur += sizeof(Box3D) * boxCnt;

    delete msg;
    return (void *)buf;
}

SimulateMsg *SimulateMsg::unpack(void *buf)
{
    char* cur = (char *)buf;
    SimulateMsg* msg = (SimulateMsg *)CkAllocBuffer(buf, sizeof(SimulateMsg));
    msg = new ((void *)msg) SimulateMsg();

    int num_nodes;
    memcpy(&num_nodes, cur, sizeof(int));
    cur = cur + sizeof(int);

    std::memcpy(&msg->fullUpdate, cur, sizeof(bool));
    cur += sizeof(bool);

    std::memcpy(&msg->doProgress, cur, sizeof(bool));
    cur += sizeof(bool);

    std::memcpy(&msg->brainStep, cur, sizeof(size_t));
    cur += sizeof(size_t);

    size_t boxCnt = 0;
    std::memcpy(&boxCnt, cur, sizeof(size_t));
    cur += sizeof(size_t);

    msg->roiBoxes.reserve(boxCnt);
    std::memcpy(msg->roiBoxes.data(), cur, sizeof(Box3D) * boxCnt);
    cur += sizeof(Box3D) * boxCnt;

    CkFreeMsg(buf);
    return msg;
}

BrainMap::BrainMap()
{
}

BrainMap::BrainMap(CkMigrateMessage *msg)
{
}

int BrainMap::procNum(int arrayHdl, const CkArrayIndex &index)
{
    return 0;
}

Brain::Brain(BrainBase &base, json &params) : mBase(base)
{
}

void BrainBase::Terminal::pup(PUP::er &p)
{
    p | isSensor;
    p | id;
    p | name;
    p | spikeType;
    p | firstNeuron;
    p | neuronCount;
    p | data;

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

Brain *BrainBase::CreateBrain(const BrainType &type, BrainBase &base, json &params)
{
    if (type == ThresholdBrain::Type) {
        return new ThresholdBrain(base, params);
    } else {
        return nullptr;
    }
}

BrainBase::BrainBase(const BrainType &type, const BrainParams &params) : 
    mDoFullViewportUpdate(false), mViewportUpdateFlushed(false),
    mDoSimulationProgress(false), mIsSimulationRunning(false),
    mRegionCommitTopologyChangeDone(false), mRegionSimulateDone(false),
    mAllTopologyChangesDelivered(false), mAllSpikesDelivered(false),
    mTriggeredNeurons(0), mDeletedNeurons(0),
    mBrainStep(0), mBrainStepsToRun(0), mBrainStepsPerBodyStep(10),
    mNeuronIdxCounter(NEURON_INDEX_MIN), mRegionIdxCounter(REGION_INDEX_MIN), mTerminalIdCounter(0),
    mBody(nullptr), mBrain(nullptr)
{
    mNeuronToTerminalId.set_deleted_key(DELETED_NEURON_ID);

    json p = json::parse(params);

    mBrain = BrainBase::CreateBrain(type, *this, p);
}

BrainBase::BrainBase(CkMigrateMessage *msg)
{
    setMigratable(false);
}

BrainBase::~BrainBase()
{
    if (mBrain) delete mBrain;
    if (mBody) delete mBody;
}

void BrainBase::pup(PUP::er &p)
{
    // TODO
}

const char *BrainBase::GetType() const
{
    if (mBrain) {
        return mBrain->GetType();
    } else {
        return "";
    }
}

const char *BrainBase::GetName() const
{
    return mName.c_str();
}

RegionIndex BrainBase::GetNewNeuronIndex()
{
    if (mNeuronIdxCounter == NEURON_INDEX_MAX) CkAbort("Neuron indices depleted.");
    return mNeuronIdxCounter++;
}

RegionIndex BrainBase::GetNewRegionIndex()
{
    if (mNeuronIdxCounter == REGION_INDEX_MAX) CkAbort("Region indices depleted.");
    return mRegionIdxCounter++;
}

const BrainBase::Terminals &BrainBase::GetTerminals() const
{
    return mTerminals;
}

void BrainBase::CreateTerminal(const ConnectorName &name, Spike::Type spikeType, size_t neuronCount)
{
    Terminal terminal;
    terminal.id = mTerminalIdCounter++;
    terminal.name = name;
    terminal.spikeType = spikeType;
    terminal.firstNeuron = GetNeuronId(BRAIN_REGION_INDEX, mNeuronIdxCounter);
    terminal.neuronCount = neuronCount;

    mTerminals.insert(std::make_pair(terminal.id, terminal));
    mTerminalNameToId.insert(std::make_pair(terminal.name, terminal.id));
    for (size_t i = 0; i < neuronCount; ++i) {
        mNeuronToTerminalId.insert(std::make_pair(
            GetNeuronId(BRAIN_REGION_INDEX, GetNewNeuronIndex()), terminal.id));
    }
}

void BrainBase::ConnectTerminal(const ConnectorName &name, const RemoteConnector &destination)
{
    auto itTerm = mTerminalNameToId.find(name);
    if (itTerm == mTerminalNameToId.end()) return;

    Terminal &terminal = mTerminals.find(itTerm->second)->second;
    terminal.connections.insert(destination);
}

void BrainBase::DisconnectTerminal(const ConnectorName &name, const RemoteConnector &destination)
{
    auto itTerm = mTerminalNameToId.find(name);
    if (itTerm == mTerminalNameToId.end()) return;

    Terminal &terminal = mTerminals.find(itTerm->second)->second;
    terminal.connections.erase(destination);
}

RegionIndex BrainBase::RequestRegionAddition(const RegionType &type, const RegionParams &params)
{
    RegionIndex regIdx = GetNewRegionIndex();
    mRegionAdditions.push_back(std::make_tuple(regIdx, type, params));
    return regIdx;
}

void BrainBase::RequestRegionRemoval(RegionIndex regIdx)
{
    mRegionRemovals.push_back(regIdx);
}

void BrainBase::RequestConnectorAddition(
    RegionIndex regIdx, Direction direction, const ConnectorName &name,
    const NeuronType &neuronType, const NeuronParams &neuronParams, size_t neuronCount)
{
    mConnectorAdditions.push_back(std::make_tuple(
        regIdx, direction, name, neuronType, neuronParams, neuronCount));
}

void BrainBase::RequestConnectorRemoval(RegionIndex regIdx, Direction direction, const ConnectorName &name)
{
    mConnectorRemovals.push_back(std::make_tuple(regIdx, direction, name));
}

void BrainBase::RequestConnectionAddition(Direction direction,
    RegionIndex srcRegIdx, const ConnectorName &srcConnectorName, 
    RegionIndex destRegIdx, const ConnectorName &destConnectorName)
{
    mConnectionAdditions.push_back(std::make_tuple(
        direction, srcRegIdx, srcConnectorName, destRegIdx, destConnectorName));
}

void BrainBase::RequestConnectionRemoval(Direction direction,
    RegionIndex srcRegIdx, const ConnectorName &srcConnectorName, 
    RegionIndex destRegIdx, const ConnectorName &destConnectorName)
{
    mConnectionRemovals.push_back(std::make_tuple(
        direction, srcRegIdx, srcConnectorName, destRegIdx, destConnectorName));
}

void BrainBase::PushSensoMotoricData(std::string &terminalName, std::vector<uint8_t> &data)
{
    auto itTerm = mTerminalNameToId.find(terminalName);
    if (itTerm == mTerminalNameToId.end()) return;

    Terminal &terminal = mTerminals.find(itTerm->second)->second;
    terminal.data.clear();
    std::swap(data, terminal.data);
}

void BrainBase::PullSensoMotoricData(std::string &terminalName, std::vector<uint8_t> &data)
{
    auto itTerm = mTerminalNameToId.find(terminalName);
    if (itTerm == mTerminalNameToId.end()) return;

    Terminal &terminal = mTerminals.find(itTerm->second)->second;
    std::swap(data, terminal.data);
    terminal.data.clear();
}

void BrainBase::ReceiveTerminalData(Spike::BrainSink &data)
{
    for (auto it = data.begin(); it != data.end(); ++it) {
        auto itTerm = mNeuronToTerminalId.find(it->first);
        if (itTerm == mNeuronToTerminalId.end()) continue;

        Terminal &terminal = mTerminals.find(itTerm->second)->second;

        size_t spikeByteCount = Spike::Edit(it->second)->AllBytes(it->second);
        size_t spikeOffset = (it->first - terminal.firstNeuron) * spikeByteCount;
        size_t requiredSize = spikeOffset + spikeByteCount;
        if (terminal.data.size() < requiredSize) terminal.data.resize(requiredSize);
        Spike::Edit(it->second)->ExportAll(it->second,
            terminal.data.data() + spikeOffset, spikeByteCount);
    }
}


void BrainBase::RunSimulation(size_t brainSteps, bool untilStopped)
{
    mDoSimulationProgress = true;
    mBrainStepsToRun = untilStopped ? SIZE_MAX : brainSteps;
    if (!mIsSimulationRunning) {
        thisProxy[thisIndex].Simulate();
    }
}

void BrainBase::StopSimulation()
{
    mBrainStepsToRun = 0;
}

void BrainBase::SetBrainStepsPerBodyStep(size_t steps)
{
    mBrainStepsPerBodyStep = steps;
}

void BrainBase::UpdateRegionOfInterest(Boxes &roiBoxes)
{
    mRoiBoxes = roiBoxes;
}

void BrainBase::RequestViewportUpdate(RequestId requestId, bool full)
{
    mViewportUpdateRequests.push_back(requestId);
    mDoFullViewportUpdate = full || mViewportUpdateFlushed;
    mViewportUpdateFlushed = false;
    if (!mIsSimulationRunning) {
        mDoSimulationProgress = false;
        thisProxy[thisIndex].Simulate();
    }
}

void BrainBase::Simulate()
{
    mIsSimulationRunning = true;
    this->SimulateBrainControl();
}

void BrainBase::SimulateBrainControl()
{
    if (mBrain && mDoSimulationProgress) mBrain->Control(mBrainStep);
    this->SimulateBrainControlDone();
}

void BrainBase::SimulateBrainControlDone()
{
    this->SimulateAddRegions();
}

void BrainBase::SimulateAddRegions()
{
    if (!mRegionAdditions.empty()) {
        gRegions.beginInserting();
        for (auto it = mRegionAdditions.begin(); it != mRegionAdditions.end(); ++it) {
            mRegionIndices.insert(std::get<0>(*it));
            gRegions[std::get<0>(*it)].insert(std::get<1>(*it), std::get<2>(*it));
        }
        gRegions.doneInserting();
    }

    this->SimulateAddRegionsDone();
}

void BrainBase::SimulateAddRegionsDone()
{
    this->SimulateAddConnectors();
}

void BrainBase::SimulateAddConnectors()
{
    if (!mConnectorAdditions.empty()) {
        gCompletionDetector.start_detection(1, CkCallback(), CkCallback(),
            CkCallback(CkIndex_BrainBase::SimulateAddConnectorsDone(), thisProxy[thisIndex]), 0);
        gCompletionDetector.ckLocalBranch()->produce(mConnectorAdditions.size());

        for (auto it = mConnectorAdditions.begin(); it != mConnectorAdditions.end(); ++it) {
            if (std::get<1>(*it) == Direction::Forward) {
                gRegions[std::get<0>(*it)].CreateInput(
                    std::get<2>(*it), std::get<3>(*it), std::get<4>(*it), std::get<5>(*it));
            } else {
                gRegions[std::get<0>(*it)].CreateOutput(
                    std::get<2>(*it), std::get<3>(*it), std::get<4>(*it), std::get<5>(*it));
            }
        }

        gCompletionDetector.ckLocalBranch()->done();
    } else {
        this->SimulateAddConnectorsDone();
    }
}

void BrainBase::SimulateAddConnectorsDone()
{
    this->SimulateAddRemoveConnections();
}

void BrainBase::SimulateAddRemoveConnections()
{
    if (!mConnectionAdditions.empty() || !mConnectionRemovals.empty()) {
        std::unordered_set<RegionIndex> touchedRegions;
        for (auto it = mConnectionAdditions.begin(); it != mConnectionAdditions.end(); ++it) {
            touchedRegions.insert(std::get<1>(*it));
            touchedRegions.insert(std::get<3>(*it));
        }
        for (auto it = mConnectionRemovals.begin(); it != mConnectionRemovals.end(); ++it) {
            touchedRegions.insert(std::get<1>(*it));
            touchedRegions.insert(std::get<3>(*it));
        }
        touchedRegions.erase(BRAIN_REGION_INDEX);

        gCompletionDetector.start_detection(1 + touchedRegions.size(), CkCallback(), CkCallback(),
            CkCallback(CkIndex_BrainBase::SimulateAddRemoveConnectionsDone(), thisProxy[thisIndex]), 0);

        for (auto it = mConnectionAdditions.begin(); it != mConnectionAdditions.end(); ++it) {
            RegionIndex outIdx =
                std::get<0>(*it) == Direction::Forward ? std::get<1>(*it) : std::get<3>(*it);
            ConnectorName outName(
                std::get<0>(*it) == Direction::Forward ? std::get<2>(*it) : std::get<4>(*it));
            RegionIndex inIdx =
                std::get<0>(*it) == Direction::Forward ? std::get<3>(*it) : std::get<1>(*it);
            ConnectorName inName(
                std::get<0>(*it) == Direction::Forward ? std::get<4>(*it) : std::get<2>(*it));
            
            if (outIdx == BRAIN_REGION_INDEX) {
                this->ConnectTerminal(outName, RemoteConnector(inIdx, inName));
            } else {
                gCompletionDetector.ckLocalBranch()->produce();
                gRegions[outIdx].ConnectOutput(outName, RemoteConnector(inIdx, inName), true);
            }

            if (inIdx == BRAIN_REGION_INDEX) {
                this->ConnectTerminal(inName, RemoteConnector(outIdx, outName));
            } else {
                gCompletionDetector.ckLocalBranch()->produce();
                gRegions[inIdx].ConnectInput(inName, RemoteConnector(outIdx, outName), true);
            }
        }

        for (auto it = mConnectionRemovals.begin(); it != mConnectionRemovals.end(); ++it) {
            RegionIndex outIdx =
                std::get<0>(*it) == Direction::Forward ? std::get<1>(*it) : std::get<3>(*it);
            ConnectorName outName(
                std::get<0>(*it) == Direction::Forward ? std::get<2>(*it) : std::get<4>(*it));
            RegionIndex inIdx =
                std::get<0>(*it) == Direction::Forward ? std::get<3>(*it) : std::get<1>(*it);
            ConnectorName inName(
                std::get<0>(*it) == Direction::Forward ? std::get<4>(*it) : std::get<2>(*it));

            if (outIdx == BRAIN_REGION_INDEX) {
                this->DisconnectTerminal(outName, RemoteConnector(inIdx, inName));
            } else {
                gCompletionDetector.ckLocalBranch()->produce();
                gRegions[outIdx].DisconnectOutput(outName, RemoteConnector(inIdx, inName), true);
            }

            if (inIdx == BRAIN_REGION_INDEX) {
                this->DisconnectTerminal(inName, RemoteConnector(outIdx, outName));
            } else {
                gCompletionDetector.ckLocalBranch()->produce();
                gRegions[inIdx].DisconnectInput(inName, RemoteConnector(outIdx, outName), true);
            }
        }

        gCompletionDetector.ckLocalBranch()->done();
    } else {
        this->SimulateAddRemoveConnectionsDone();
    }
}

void BrainBase::SimulateAddRemoveConnectionsDone()
{
    this->SimulateRemoveConnectors();
}

void BrainBase::SimulateRemoveConnectors()
{
    if (!mConnectorRemovals.empty()) {
        std::unordered_set<RegionIndex> touchedRegions;
        for (auto it = mConnectorRemovals.begin(); it != mConnectorRemovals.end(); ++it) {
            touchedRegions.insert(std::get<0>(*it));
        }

        gCompletionDetector.start_detection(1 + touchedRegions.size(), CkCallback(), CkCallback(),
            CkCallback(CkIndex_BrainBase::SimulateRemoveConnectorsDone(), thisProxy[thisIndex]), 0);
        gCompletionDetector.ckLocalBranch()->produce(mConnectorRemovals.size());

        for (auto it = mConnectorRemovals.begin(); it != mConnectorRemovals.end(); ++it) {
            if (std::get<1>(*it) == Direction::Forward) {
                gRegions[std::get<0>(*it)].DeleteInput(std::get<2>(*it));
            } else {
                gRegions[std::get<0>(*it)].DeleteOutput(std::get<2>(*it));
            }

            for (auto itTerm = mTerminalNameToId.begin(); itTerm != mTerminalNameToId.end(); ++itTerm) {
                DisconnectTerminal(itTerm->first, RemoteConnector(std::get<0>(*it), std::get<2>(*it)));
            }
        }

        gCompletionDetector.ckLocalBranch()->done();
    } else {
        this->SimulateRemoveConnectorsDone();
    }
}

void BrainBase::SimulateRemoveConnectorsDone()
{
    this->SimulateRemoveRegions();
}

void BrainBase::SimulateRemoveRegions()
{
    if (!mRegionRemovals.empty()) {

        gCompletionDetector.start_detection(1 + mRegionRemovals.size(), CkCallback(), CkCallback(),
            CkCallback(CkIndex_BrainBase::SimulateRemoveRegionsDone(), thisProxy[thisIndex]), 0);
        gCompletionDetector.ckLocalBranch()->produce(mRegionRemovals.size());

        for (auto it = mRegionRemovals.begin(); it != mRegionRemovals.end(); ++it) {
            gRegions[*it].Unlink();
        }

        gCompletionDetector.ckLocalBranch()->done();
    } else {
        this->SimulateRemoveRegionsDone();
    }
}

void BrainBase::SimulateRemoveRegionsDone()
{
    if (!mRegionRemovals.empty()) {
        for (auto it = mRegionRemovals.begin(); it != mRegionRemovals.end(); ++it) {
            mRegionIndices.erase(*it);
            gRegions[*it].ckDestroy();
        }
    }

    this->SimulateRegionPrepareTopologyChange();
}

void BrainBase::SimulateRegionPrepareTopologyChange()
{
    if (!mRegionIndices.empty()) {
        gRegions.PrepareTopologyChange(mBrainStep, mDoSimulationProgress);
    } else {
        this->SimulateRegionPrepareTopologyChangeDone(0);
    }
}

void BrainBase::SimulateRegionPrepareTopologyChangeDone(size_t deletedNeurons)
{
    mDeletedNeurons = deletedNeurons;
    this->SimulateRegionCommitTopologyChange();
}

void BrainBase::SimulateRegionCommitTopologyChange()
{
    if (!mRegionIndices.empty()) {
        if (mDeletedNeurons > 0) {
            gCompletionDetector.start_detection(mRegionIndices.size() + mDeletedNeurons, CkCallback(), CkCallback(),
                CkCallback(CkIndex_BrainBase::SimulateAllTopologyChangesDelivered(), thisProxy[thisIndex]), 0);
        } else {
            this->SimulateAllTopologyChangesDelivered();
        }

        gRegions.CommitTopologyChange();
    } else {
        this->SimulateAllTopologyChangesDelivered();
        this->SimulateRegionCommitTopologyChangeDone(0);
    }
}

void BrainBase::SimulateRegionCommitTopologyChangeDone(size_t triggeredNeurons)
{
    mTriggeredNeurons = triggeredNeurons;
    mRegionCommitTopologyChangeDone = true;
    if (mRegionCommitTopologyChangeDone && mAllTopologyChangesDelivered) {
        this->SimulateBodySimulate();
    }
}

void BrainBase::SimulateAllTopologyChangesDelivered()
{
    mAllSpikesDelivered = true;
    if (mRegionCommitTopologyChangeDone && mAllTopologyChangesDelivered) {
        this->SimulateBodySimulate();
    }
}

void BrainBase::SimulateBodySimulate()
{
    if (mBrainStep % mBrainStepsPerBodyStep == 0 && mDoSimulationProgress) {

        mBody->Simulate(
            std::bind(&BrainBase::PushSensoMotoricData, this, std::placeholders::_1, std::placeholders::_2),
            std::bind(&BrainBase::PullSensoMotoricData, this, std::placeholders::_1, std::placeholders::_2)
        );

        std::unordered_set<RegionIndex> touchedRegions;
        for (auto it = mTerminals.begin(); it != mTerminals.end(); ++it) {
            Terminal &terminal = it->second;
            if (!terminal.data.empty()) {
                for (auto itConn = terminal.connections.begin(); itConn != terminal.connections.end(); ++itConn) {
                    touchedRegions.insert(itConn->first);
                }
            }
        }

        if (!touchedRegions.empty()) {

            gCompletionDetector.start_detection(1 + touchedRegions.size(), CkCallback(), CkCallback(),
                CkCallback(CkIndex_BrainBase::SimulateBodySimulateDone(), thisProxy[thisIndex]), 0);

            std::unordered_set<RemoteConnector> touchedConnectors;
            for (auto it = mTerminals.begin(); it != mTerminals.end(); ++it) {

                Terminal &terminal = it->second;
                if (terminal.data.empty()) continue;

                size_t dataIdx = 0;
                Spike::BrainSource spikes(terminal.neuronCount);
                for (size_t i = 0; i < terminal.neuronCount; ++i) {
                    Spike::Data spike;
                    Spike::Initialize(terminal.spikeType, terminal.firstNeuron + i, spike);
                    size_t spikeByteCount = Spike::Edit(spike)->AllBytes(spike);
                    if (dataIdx + spikeByteCount <= terminal.data.size()) {
                        Spike::Edit(spike)->ImportAll(spike, terminal.data.data() + dataIdx, spikeByteCount);
                        dataIdx += spikeByteCount;
                        spikes.push_back(spike);
                    }
                }
                terminal.data.clear();

                Direction direction = terminal.isSensor ? Direction::Forward : Direction::Backward;
                for (auto itConn = terminal.connections.begin(); itConn != terminal.connections.end(); ++itConn) {
                    if (touchedConnectors.find(*itConn) == touchedConnectors.end()) {
                        touchedConnectors.insert(*itConn);
                        mTriggeredNeurons += spikes.size();
                    }
                    gCompletionDetector.ckLocalBranch()->produce();
                    gRegions[itConn->first].ReceiveSensoMotoricData(direction, itConn->second, spikes);
                }
            }

            gCompletionDetector.ckLocalBranch()->done();

        } else {
            this->SimulateBodySimulateDone();
        }

    } else {
        this->SimulateBodySimulateDone();
    }
}

void BrainBase::SimulateBodySimulateDone()
{
    this->SimulateRegionSimulate();
}

void BrainBase::SimulateRegionSimulate()
{
    if (mDoSimulationProgress) {
        if (mTriggeredNeurons > 0) {
            gCompletionDetector.start_detection(mTriggeredNeurons, CkCallback(), CkCallback(),
                CkCallback(CkIndex_BrainBase::SimulateAllSpikesDelivered(), thisProxy[thisIndex]), 0);
        } else {
            this->SimulateAllSpikesDelivered();
        }
    } else {
        this->SimulateAllSpikesDelivered();
    }

    if (!mRegionIndices.empty()) {
        SimulateMsg *msg = new SimulateMsg();
        msg->fullUpdate = mDoFullViewportUpdate;
        msg->doProgress = mDoSimulationProgress;
        msg->brainStep = mBrainStep;
        msg->roiBoxes = mRoiBoxes;

        gRegions.Simulate(msg);
    } else {
        this->SimulateRegionSimulateDone(nullptr);
    }
}

void BrainBase::SimulateRegionSimulateDone(CkReductionMsg *msg)
{
    // TODO

    if (msg) {
        CkReduction::setElement *current = (CkReduction::setElement *)msg->getData();
        while (current != nullptr) {
            int *result = (int *)&current->data;
            // Do something with result.
            current = current->next();
        }
        delete msg;
    }

    mRegionSimulateDone = true;
    if (mRegionSimulateDone && mAllSpikesDelivered) {
        this->SimulateDone();
    }
}

void BrainBase::SimulateAllSpikesDelivered()
{
    mAllSpikesDelivered = true;
    if (mRegionSimulateDone && mAllSpikesDelivered) {
        this->SimulateDone();
    }
}

void BrainBase::SimulateDone()
{
    mRegionSimulateDone = false;
    mAllSpikesDelivered = false;
    
    if (mDoSimulationProgress) {
        ++mBrainStep;
        if (mBrainStepsToRun != SIZE_MAX) {
            --mBrainStepsToRun;
        }
        if (mBrainStepsToRun > 0) {
            thisProxy[thisIndex].Simulate();
        } else {
            mIsSimulationRunning = false;
        }
    } else {
        mIsSimulationRunning = false;
    }
}

const char *ThresholdBrain::Type = "ThresholdBrain";

ThresholdBrain::ThresholdBrain(BrainBase &base, json &params) : Brain(base, params)
{
    // TODO
}

ThresholdBrain::~ThresholdBrain()
{
}

void ThresholdBrain::pup(PUP::er &p)
{
}

const char *ThresholdBrain::GetType() const
{
    return Type;
}

void ThresholdBrain::Control(size_t brainStep)
{
    // TODO
}

void ThresholdBrain::AcceptContributionFromRegion(
    RegionIndex regIdx, const uint8_t *contribution, size_t size)
{
    // TODO
}

#include "brain.def.h"
