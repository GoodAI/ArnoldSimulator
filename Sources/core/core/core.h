#pragma once

#include <unordered_map>

#include <json.hpp>

#include <pup.h>
#include <pup_stl.h>
#include <completion.h>
#include <ckmulticast.h>

#include "common_messages_generated.h"
#include "request_messages_generated.h"
#include "response_messages_generated.h"

#include "common.h"

#include "core.decl.h"

using namespace GoodAI::Arnold;

struct ViewportUpdate;

inline void operator|(PUP::er &p, Communication::StateType &state)
{
    pup_bytes(&p, static_cast<void *>(&state), sizeof(Communication::StateType));
}

class ShutdownRequestedException : public std::runtime_error
{
public:
    explicit ShutdownRequestedException(const char *reason) : runtime_error(reason) { }
};

void CoreNodeInit();
void CoreProcInit();

Core *GetCoreLocalPtr();

class Core : public CBase_Core
{
public:
    explicit Core(CkArgMsg *msg);
    explicit Core(CkMigrateMessage *msg);
    ~Core();

    Core(const Core &other) = delete;
    Core &operator=(const Core &other) = delete;

    virtual void pup(PUP::er &p) override;

    void Exit();

    void DetectKeyPress();

    void HandleRequestFromClient(CkCcsRequestMsg *msg);
    
    void RunTests();

    void LoadBrain(const BrainName &name, const BrainType &type, const BrainParams &params);
    bool IsBrainLoaded() const;
    void UnloadBrain();
    void BrainUnloaded();
    
    void SendEmptyMessage(RequestId requestId);
    void SendSimulationState(RequestId requestId, bool isSimulationRunning, 
        size_t atBrainStep, size_t atBodyStep, size_t brainStepsPerBodyStep);
    void SendViewportUpdate(RequestId requestId, const ViewportUpdate &update);

protected:
    void SendResponseToClient(RequestId requestId, flatbuffers::FlatBufferBuilder &builder);

    void ProcessCommandRequest(const Communication::CommandRequest *commandRequest, RequestId requestId);
    void ProcessGetStateRequest(const Communication::GetStateRequest *getStateRequest, RequestId requestId);
    void ProcessGetModelRequest(const Communication::GetModelRequest *getModelRequest, RequestId requestId);

    bool TryLoadBrain(const std::string &blueprintString);

    void SendErrorResponse(RequestId requestId, const std::string &message);

    flatbuffers::Offset<Communication::Position> CreatePosition(flatbuffers::FlatBufferBuilder &builder, Point3D lowerBound);
    flatbuffers::Offset<Communication::NeuronId> Core::CommunicationNeuronId(flatbuffers::FlatBufferBuilder &builder, NeuronId neuronId) const;

    void BuildStateResponse(bool isSimulationRunning, size_t atBrainStep, 
        size_t atBodyStep, size_t brainStepsPerBodyStep, flatbuffers::FlatBufferBuilder &builder) const;

    void BuildErrorResponse(const std::string &message, flatbuffers::FlatBufferBuilder &builder) const;

    void BuildRegionOffsets(
        flatbuffers::FlatBufferBuilder &builder,
        const RegionAdditionReports &regions,
        std::vector<flatbuffers::Offset<Communication::Region>> &regionOffsets) const;

    Communication::Direction CommunicationDirection(Direction direction) const;

    void Core::BuildConnectorOffsets(
        flatbuffers::FlatBufferBuilder &builder,
        const ConnectorAdditionReports &connectors,
        std::vector<flatbuffers::Offset<Communication::Connector>> &connectorOffsets) const;

    void Core::BuildConnectorRemovalOffsets(
        flatbuffers::FlatBufferBuilder &builder,
        const ConnectorRemovals &connectors,
        std::vector<flatbuffers::Offset<Communication::ConnectorRemoval>> &connectorOffsets) const;

    void Core::BuildConnectionOffsets(
        flatbuffers::FlatBufferBuilder &builder,
        const Connections &connections,
        std::vector<flatbuffers::Offset<Communication::Connection>> &connectionOffsets) const;
    
    void BuildNeuronOffsets(
        flatbuffers::FlatBufferBuilder &builder,
        const NeuronAdditionReports &regions,
        std::vector<flatbuffers::Offset<Communication::Neuron>> &regionOffsets) const;

    void Core::BuildSynapseOffsets(
        flatbuffers::FlatBufferBuilder &builder,
        const Synapse::Links &synapses,
        std::vector<flatbuffers::Offset<Communication::Synapse>> &synapseOffsets) const;

    void BuildStateResponse(const Communication::StateType state, size_t atBrainStep, 
        size_t atBodyStep, size_t brainStepsPerBodyStep, flatbuffers::FlatBufferBuilder &builder) const;
    void BuildViewportUpdateResponse(const ViewportUpdate &update, flatbuffers::FlatBufferBuilder &builder) const;

private:
    double mStartTime;

    bool mBrainLoaded;
    bool mBrainIsUnloading;

    bool mIsShuttingDown;

    RequestId mRequestIdCounter;
    std::unordered_map<RequestId, CkCcsRequestMsg *> mRequests;

    // TODO(HonzaS): Remove this stub once it's not needed.
    void SendStubModel(RequestId requestId);
};
