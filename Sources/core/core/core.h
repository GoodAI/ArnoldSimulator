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

using namespace GoodAI::Arnold::Network;

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
    
    void SendSynapticTransfers(RequestId requestId, Synapse::Transfers &transfers);

protected:
    void SendResponseToClient(RequestId requestId, flatbuffers::FlatBufferBuilder &builder);

    void ProcessCommandRequest(const CommandRequest *commandRequest, RequestId requestId);
    void ProcessGetStateRequest(const GetStateRequest *getStateRequest, RequestId requestId);

    void BuildStateResponse(const StateType state, flatbuffers::FlatBufferBuilder &builder);
    void BuildSynapticTransfersResponse(Synapse::Transfers &transfers, flatbuffers::FlatBufferBuilder &builder);

private:
    StateType mState;

    double mStartTime;

    RequestId mRequestIdCounter;
    std::unordered_map<RequestId, CkCcsRequestMsg *> mRequests;
};
