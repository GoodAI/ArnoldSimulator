#pragma once

#include <atomic>
#include <vector>
#include <unordered_map>

#include <json.hpp>

#include <pup.h>
#include <pup_stl.h>
#include <completion.h>
#include <ckmulticast.h>

#include "requests_generated.h"
#include "responses_generated.h"

#include "common.h"

#include "core.decl.h"

using namespace GoodAI::Arnold;

class ShutdownRequestedException : public std::runtime_error
{
public:
    explicit ShutdownRequestedException(const char *reason) : runtime_error(reason) { }
};

class Core : public CBase_Core
{
public:
    Core(CkArgMsg *msg);
    Core(CkMigrateMessage *msg);
    ~Core();

    Core(const Core &other) = delete;
    Core &operator=(const Core &other) = delete;

    void pup(PUP::er &p);

    void Exit();

    void HandleRequestFromClient(CkCcsRequestMsg *msg);
    
    void SendViewportUpdate(
        RequestId requestId,
        RegionAdditionReports &addedRegions,
        RegionAdditionReports &repositionedRegions,
        RegionRemovals &removedRegions,
        ConnectorAdditionReports &addedConnectors,
        ConnectorRemovals &removedConnectors,
        Connections &addedConnections,
        Connections &removedConnections,
        NeuronAdditionReports &addedNeurons,
        NeuronAdditionReports &repositionedNeurons,
        NeuronRemovals &removedNeurons,
        Synapse::Links &addedSynapses,
        Synapse::Links &spikedSynapses,
        Synapse::Links &removedSynapses,
        ChildLinks &addedChildren,
        ChildLinks &removedChildren
    );

protected:
    void SendResponseToClient(RequestId requestId, flatbuffers::FlatBufferBuilder &builder);

    void ProcessCommandRequest(const Network::CommandRequest *commandRequest, RequestId requestId);
    void ProcessGetStateRequest(const Network::GetStateRequest *getStateRequest, RequestId requestId);

    void BuildViewportUpdateResponse(
        const RegionAdditionReports &addedRegions,
        const RegionAdditionReports &repositionedRegions,
        const RegionRemovals &removedRegions,
        const ConnectorAdditionReports &addedConnectors,
        const ConnectorRemovals &removedConnectors,
        const Connections &addedConnections,
        const Connections &removedConnections,
        const NeuronAdditionReports &addedNeurons,
        const NeuronAdditionReports &repositionedNeurons,
        const NeuronRemovals &removedNeurons,
        const Synapse::Links &addedSynapses,
        const Synapse::Links &spikedSynapses,
        const Synapse::Links &removedSynapses,
        const ChildLinks &addedChildren,
        const ChildLinks &removedChildren,
        flatbuffers::FlatBufferBuilder &builder);
    void BuildStateResponse(const Network::StateType state, flatbuffers::FlatBufferBuilder &builder);

private:
    Network::StateType mState;

    double mStartTime;

    RequestId mRequestIdCounter;
    std::unordered_map<RequestId, CkCcsRequestMsg *> mRequests;
};
