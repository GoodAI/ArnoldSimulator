#pragma once

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

class ViewportUpdate;

class ShutdownRequestedException : public std::runtime_error
{
public:
    explicit ShutdownRequestedException(const char *reason) : runtime_error(reason) { }
};

void CoreNodeInit();
void CoreProcInit();

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
    
    void SendViewportUpdate(RequestId requestId, const ViewportUpdate &update);

protected:
    void SendResponseToClient(RequestId requestId, flatbuffers::FlatBufferBuilder &builder);

    void ProcessCommandRequest(const Network::CommandRequest *commandRequest, RequestId requestId);
    void ProcessGetStateRequest(const Network::GetStateRequest *getStateRequest, RequestId requestId);
    void ProcessGetModelRequest(const Network::GetModelRequest *getModelRequest, RequestId requestId);

    static flatbuffers::Offset<Network::Position> CreatePosition(flatbuffers::FlatBufferBuilder &builder, Point3D lowerBound);

    void BuildViewportUpdateResponse(const ViewportUpdate &update, flatbuffers::FlatBufferBuilder &builder) const;
    void BuildStateResponse(const Network::StateType state, flatbuffers::FlatBufferBuilder &builder) const;
private:
    Network::StateType mState;

    double mStartTime;

    RequestId mRequestIdCounter;
    std::unordered_map<RequestId, CkCcsRequestMsg *> mRequests;

    // TODO(Premek): Remove.
    int mDummyTimestep;
};
