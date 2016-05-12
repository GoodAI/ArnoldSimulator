#include "brain.h"

#include "core.decl.h"
#include "region.decl.h"
#include "neuron.decl.h"

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
    mBrainStep(0), mBrainStepsToRun(0), mBrainStepsPerBodyStep(10),
    mNeuronIdCounter(0), mRegionIdxCounter(0), mTerminalIdCounter(0),
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
}

void BrainBase::pup(PUP::er &p)
{
    // TODO
}

const char *BrainBase::GetType() const
{
    return mBrain->GetType();
}

const char *BrainBase::GetName() const
{
    return mName.c_str();
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
    terminal.firstNeuron = mNeuronIdCounter;
    terminal.neuronCount = neuronCount;

    mTerminals.insert(std::make_pair(terminal.id, terminal));
    mTerminalNameToId.insert(std::make_pair(terminal.name, terminal.id));
    for (size_t i = 0; i < neuronCount; ++i) {
        mNeuronToTerminalId.insert(std::make_pair(mNeuronIdCounter++, terminal.id));
    }
}

void BrainBase::DeleteTerminal(const ConnectorName &name)
{
    auto itTerm = mTerminalNameToId.find(name);
    if (itTerm == mTerminalNameToId.end()) return;

    Terminal &terminal = mTerminals.find(itTerm->second)->second;
    for (size_t i = 0; i < terminal.neuronCount; ++i) {
        mNeuronToTerminalId.erase(terminal.firstNeuron + i);
    }
    mTerminalNameToId.erase(terminal.name);
    mTerminals.erase(terminal.id);
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
    RegionIndex regIdx = mRegionIdxCounter++;
    mRegionAdditions.push_back(std::make_tuple(regIdx, type, params));
    return regIdx;
}

void BrainBase::RequestRegionRemoval(RegionIndex regIdx)
{
    mRegionsRemovals.push_back(regIdx);
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
    
    size_t dataIdx = 0;
    Spike::BrainSource spikes(terminal.neuronCount);
    for (size_t i = 0; i < terminal.neuronCount; ++i) {
        Spike::Data spike;
        Spike::Initialize(terminal.spikeType, terminal.firstNeuron + i, spike);
        size_t spikeByteCount = Spike::Edit(spike)->AllBytes(spike);
        if (dataIdx + spikeByteCount <= data.size()) {
            Spike::Edit(spike)->ImportAll(spike, data.data() + dataIdx, spikeByteCount);
            dataIdx += spikeByteCount;
            spikes.push_back(spike);
        }
    }

    Direction direction = terminal.isSensor ? Direction::Forward : Direction::Backward;
    for (auto it = terminal.connections.begin(); it != terminal.connections.end(); ++it) {
        gRegions[it->first].ReceiveSensoMotoricData(direction, it->second, spikes);
    }
}

void BrainBase::PullSensoMotoricData(std::string &terminalName, std::vector<uint8_t> &data)
{
    auto itTerm = mTerminalNameToId.find(terminalName);
    if (itTerm == mTerminalNameToId.end()) return;

    Terminal &terminal = mTerminals.find(itTerm->second)->second;
    std::swap(data, terminal.data);
}

void BrainBase::RunSimulation(size_t brainSteps, bool untilStopped)
{
    mDoSimulationProgress = true;
    mBrainStepsToRun = untilStopped ? SIZE_MAX : brainSteps;
    if (!mIsSimulationRunning) {
        thisProxy.Simulate();
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
        thisProxy.Simulate();
    }
}

void BrainBase::Simulate()
{
    /*
    bool allSimulated = regionsSimulatedCnt == regionsToBeSimulatedCnt;
    bool allConfirmed = interactionsConfirmedCnt == interactionsToBeConfirmedCnt;
    if (allSimulated && allConfirmed) {
        if (bodySimulated) {
            // respond to any delayed client requests
            // go through the client requests, adapt internal state
            if (shouldStop) return;

            triggeredRegionsCurrent.clear();
            swap(triggeredRegionsCurrent, triggeredRegionsNext);
            
            regionsSimulatedCnt = 0;
            regionsToBeSimulatedCnt = triggeredRegionsCurrent.size();
            interactionsConfirmedCnt = interactionsToBeConfirmedCnt = 0;
    
            BrainControl(brainStep);

            // add new regions
            // resize region connections
            // remove regions, remove also from triggered set

            CProxySection_Region triggeredRegions = 
                  CProxySection_Region::ckNew(regionsTriggeredCurrent);
            triggeredRegions.Simulate(brainStep);
           
            brainStep++;
            if (brainStep % brainStepsPerBodyStep == 0) {
                bodySimulated = false;
            }
        } else {
            body.Simulate(PushSensoMotoricData, PullSensoMotoricData);
            bodySimulated = true;
        }
    }
    */
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

void BrainBase::ChangeTopologyDone(long triggeredNeurons)
{
}

void BrainBase::RegionSimulateDone(CkReductionMsg *msg)
{
}

const char *ThresholdBrain::Type = "ThresholdBrain";

ThresholdBrain::ThresholdBrain(BrainBase &base, json &params) : Brain(base, params)
{
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
}

#include "brain.def.h"
