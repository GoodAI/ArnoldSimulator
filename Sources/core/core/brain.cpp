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

BrainBase::BrainBase(const BrainType &type, const BrainParams &params)
{
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
}

const char *BrainBase::GetType()
{
    return nullptr;
}

const BrainBase::Terminals &BrainBase::GetTerminals() const
{
    return mTerminals;
}

void BrainBase::CreateTerminal(const ConnectorName &name, Spike::Type spikeType, NeuronId firstNeuron, size_t neuronCount)
{
}

void BrainBase::DeleteTerminal(const ConnectorName &name)
{
}

void BrainBase::ConnectTerminal(const ConnectorName &name, const RemoteConnector &destination)
{
}

void BrainBase::DisconnectTerminal(const ConnectorName &name, const RemoteConnector &destination)
{
}

void BrainBase::AddRegion(RegionIndex regIdx, const RegionType &type, const RegionParams &params)
{
}

void BrainBase::RemoveRegion(RegionIndex regIdx)
{
}

void BrainBase::AddConnector(RegionIndex regIdx, Direction direction, const ConnectorName &name, size_t size)
{
}

void BrainBase::RemoveConnector(RegionIndex regIdx, Direction direction, const ConnectorName &name)
{
}

void BrainBase::AddConnection(Direction direction, 
    RegionIndex srcRegIdx, const ConnectorName &srcConnectorName, 
    RegionIndex destRegIdx, const ConnectorName &destConnectorName)
{
}

void BrainBase::RemoveConnection(Direction direction, 
    RegionIndex srcRegIdx, const ConnectorName &srcConnectorName, 
    RegionIndex destRegIdx, const ConnectorName &destConnectorName)
{
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

void BrainBase::StartSimulation()
{
}

void BrainBase::StopSimulation()
{
}

void BrainBase::SetBrainStepsPerBodyStep(size_t steps)
{
}

void BrainBase::RequestSynapticTransfers(RequestId requestId)
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

const char *ThresholdBrain::GetType()
{
    return Type;
}

void ThresholdBrain::Control(size_t brainStep)
{
}

#include "brain.def.h"
