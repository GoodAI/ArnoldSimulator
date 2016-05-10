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

Brain *BrainBase::CreateBrain(const BrainType &type, BrainBase &base, json &params)
{
    if (type == ThresholdBrain::Type) {
        return new ThresholdBrain(base, params);
    } else {
        return nullptr;
    }
}

BrainBase::BrainBase(const BrainType &type, const BrainParams &params) : 
    mShouldStop(true), mShouldRunUntilStopped(true),
    mBrainStepsToRun(0), mBrainStepsPerBodyStep(10), mBrainStep(0),
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
    auto it = mTerminalNameToId.find(name);
    if (it == mTerminalNameToId.end()) return;

    Terminal &terminal = mTerminals.find(it->second)->second;
    for (size_t i = 0; i < terminal.neuronCount; ++i) {
        mNeuronToTerminalId.erase(terminal.firstNeuron + i);
    }
    mTerminalNameToId.erase(terminal.name);
    mTerminals.erase(terminal.id);
}

void BrainBase::ConnectTerminal(const ConnectorName &name, const RemoteConnector &destination)
{
    auto it = mTerminalNameToId.find(name);
    if (it == mTerminalNameToId.end()) return;

    Terminal &terminal = mTerminals.find(it->second)->second;
    terminal.connections.insert(destination);
}

void BrainBase::DisconnectTerminal(const ConnectorName &name, const RemoteConnector &destination)
{
    auto it = mTerminalNameToId.find(name);
    if (it == mTerminalNameToId.end()) return;

    Terminal &terminal = mTerminals.find(it->second)->second;
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
    /*
    RegionTerminalCtx &terminal = regionTerminals[regionTerminalsNameMap[sensorName]];
    for each binding in terminal.bindings {
    std::vector<std::pair<GateLaneIdx, PUP::able>> densePayload;
    densePayload.push_back(make_pair(binding.third, (PUP::able)data);
    regionsAll[binding.second].ReceivePayloads(binding.first, -1, densePayload, true);
    interactionsToBeConfirmed++;
    }
    */
}

void BrainBase::PullSensoMotoricData(std::string &terminalName, std::vector<uint8_t> &data)
{
    /*
    RegionTerminalCtx &terminal = regionTerminals[regionTerminalsIdMap[from]];
    std::swap(data, terminal.data); // or possibly reduce if more than 1 binding
    */
}

void BrainBase::RunSimulation(size_t brainSteps, bool untilStopped)
{
}

void BrainBase::StopSimulation()
{
}

void BrainBase::SetBrainStepsPerBodyStep(size_t steps)
{
}

void BrainBase::UpdateRegionOfInterest(Boxes &roiBoxes)
{
}

void BrainBase::RequestViewportUpdate(RequestId requestId, bool full)
{
}

void BrainBase::Simulate()
{
    // TODO(HonzaS): Incorporate the message handling into the simulation code when it's done.

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
