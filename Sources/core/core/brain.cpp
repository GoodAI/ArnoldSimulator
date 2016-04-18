#include "brain.h"

extern CProxy_Core gCore;

Brain::Brain(BrainBase &base, json &params) : mBase(base)
{
}

Brain *BrainBase::CreateBrain(const std::string &type, BrainBase &base, json &params)
{
    if (type == ThresholdBrain::Type) {
        return new ThresholdBrain(base, params);
    } else {
        return nullptr;
    }
}

BrainBase::BrainBase(const std::string &type, const std::string &params)
{
    json parsedParams;
    std::stringstream streamedParams(params);
    streamedParams >> parsedParams;

    mBrain = BrainBase::CreateBrain(type, *this, parsedParams);
}

BrainBase::BrainBase(CkMigrateMessage *msg)
{
}

BrainBase::~BrainBase()
{
}

void BrainBase::EnqueueClientRequest(RequestId token, std::vector<unsigned char> &request)
{
}

const std::unordered_map<ConnectorName, BrainBase::Terminal> &BrainBase::GetTerminals() const
{
    return mTerminals;
}

void BrainBase::AddRegion(RegionId regId, const std::string &type, const std::string &params)
{
}

void BrainBase::RemoveRegion(RegionId regId)
{
}

void BrainBase::AddConnector(RegionId regId, Direction direction, const ConnectorName &name, size_t size)
{
}

void BrainBase::RemoveConnector(RegionId regId, Direction direction, const ConnectorName &name)
{
}

void BrainBase::AddConnection(Direction direction,
    RegionId srcRegId, const ConnectorName &srcConnectorName,
    RegionId destRegId, const ConnectorName &destConnectorName)
{
}

void BrainBase::RemoveConnection(Direction direction,
    RegionId srcRegId, const ConnectorName &srcConnectorName,
    RegionId destRegId, const ConnectorName &destConnectorName)
{
}

void BrainBase::ReceiveTerminalData(RegionId from, const ConnectorName &to, std::vector<unsigned char> &data)
{
    
}

void BrainBase::TriggerRegion(RegionId regId)
{
    /*
    interactionsConfirmedCnt++;
    regionsTriggeredNext->insert(regId);
    ProgressSimulation();
    */
}

void BrainBase::RegionSimulated(RegionId regId, size_t regionsTriggeredCnt)
{
    /*
    interactionsToBeConfirmedCnt += interactionsTriggeredCnt;
    regionsSimulatedCnt++;
    ProgressSimulation();
    */
}

void BrainBase::InteractionConfirmed()
{
    /*
    interactionsConfirmedCnt++;
    ProgressSimulation();
    */
}

void BrainBase::PushSensoMotoricData(std::string &terminalName, std::vector<unsigned char> &data)
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

void BrainBase::PullSensoMotoricData(std::string &terminalName, std::vector<unsigned char> &data)
{
    /*
    RegionTerminalCtx &terminal = regionTerminals[regionTerminalsIdMap[from]];
    std::swap(data, terminal.data); // or possibly reduce if more than 1 binding
    */
}

void BrainBase::ProgressSimulation()
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

const char *ThresholdBrain::Type = "ThresholdBrain";

ThresholdBrain::ThresholdBrain(BrainBase &base, json &params) : Brain(base, params)
{
}

ThresholdBrain::~ThresholdBrain()
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
