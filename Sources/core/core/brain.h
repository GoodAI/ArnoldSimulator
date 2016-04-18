#pragma once

#include <tuple>
#include <vector>
#include <set>
#include <string>
#include <unordered_map>

#include <json.hpp>

#include <pup.h>
#include <pup_stl.h>

#include "common.h"
#include "spike.h"
#include "synapse.h"
#include "body.h"
#include "core.decl.h"
#include "brain.decl.h"
#include "region.decl.h"

using namespace nlohmann;

class BrainBase;

class Brain
{
public:
    Brain(BrainBase &base, json &params);
    virtual ~Brain() = default;

    Brain(const Brain &other) = delete;
    Brain &operator=(const Brain &other) = delete;

    virtual const char *GetType() = 0;

    virtual void Control(size_t brainStep) = 0;

protected:
    BrainBase &mBase;
};

class BrainBase : public CBase_BrainBase
{
public:
    struct Terminal
    {
        ConnectorName name;
        std::vector<unsigned char> data;
        std::vector<RegionId> bindings;
    };

    static Brain *CreateBrain(const std::string &type, BrainBase &base, json &params);

    BrainBase(const std::string &type, const std::string &params);
    BrainBase(CkMigrateMessage *msg);
    ~BrainBase();

    BrainBase(const BrainBase &other) = delete;
    BrainBase &operator=(const BrainBase &other) = delete;

    void EnqueueClientRequest(RequestId token, std::vector<unsigned char> &request);

    const std::unordered_map<ConnectorName, Terminal> &GetTerminals() const;

    void AddRegion(RegionId regId, const std::string &type, const std::string &params);
    void RemoveRegion(RegionId regId);
    void AddConnector(RegionId regId, Direction direction, const ConnectorName &name, size_t size);
    void RemoveConnector(RegionId regId, Direction direction, const ConnectorName &name);
    void AddConnection(Direction direction,
        RegionId srcRegId, const ConnectorName &srcConnectorName,
        RegionId destRegId, const ConnectorName &destConnectorName);
    void RemoveConnection(Direction direction,
        RegionId srcRegId, const ConnectorName &srcConnectorName,
        RegionId destRegId, const ConnectorName &destConnectorName);
    
    void ReceiveTerminalData(RegionId from, const ConnectorName &to, std::vector<unsigned char> &data);

    void TriggerRegion(RegionId regId);
    void RegionSimulated(RegionId regId, size_t regionsTriggeredCnt);
    void InteractionConfirmed();

    void PushSensoMotoricData(std::string &terminalName, std::vector<unsigned char> &data);
    void PullSensoMotoricData(std::string &terminalName, std::vector<unsigned char> &data);

    void ProgressSimulation();

private:
    std::vector<std::pair<RequestId, std::vector<unsigned char>>> mClientRequests;

    Body *mBody;
    Brain *mBrain;
    CProxy_RegionBase mRegionsAll;

    bool mShouldStop;
    size_t mBrainStep;
    size_t mBrainStepsPerBodyStep;
    bool mBodySimulated;
    size_t mRegionsSimulated;
    size_t mRegionsToBeSimulated;
    size_t mInteractionsConfirmed;
    size_t mInteractionsToBeConfirmed;

    std::unordered_map<ConnectorName, Terminal> mTerminals;

    std::vector<std::tuple<RegionId, std::string, std::string>> mRegionsToAdd;
    std::vector<RegionId> mRegionsToRemove;
    std::vector<ConnectorAddition> mConnectorsToAdd;
    std::vector<ConnectorRemoval> mConnectorsToRemove;
    std::vector<Connection> mConnectionsToAdd;
    std::vector<Connection> mConnectionsToRemove;

    std::set<RegionId> *mRegionsTriggeredCurrent;
    std::set<RegionId> *mRegionsTriggeredNext;
};

class ThresholdBrain : public Brain
{
public:
    static const char *Type;

    ThresholdBrain(BrainBase &base, json &params);
    virtual ~ThresholdBrain();

    virtual const char *GetType() override;

    virtual void Control(size_t brainStep) override;

    /*
    void SomeInternalFunction(SomeType1 someArg1, SomeType2 someArg2);

    entry void SomeFunctionForRegions(RegionId caller, SomeType1 someArg1, SomeType2 someArg2);
    */
};
