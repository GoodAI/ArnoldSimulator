#include <future>

#include "core.h"
#include "brain.h"

#include "brain.decl.h"
#include "region.decl.h"
#include "neuron.decl.h"

CkGroupID gMulticastGroupId;
CProxy_CompletionDetector gCompletionDetector;

CProxy_Core gCore;
CProxy_BrainBase gBrain;
CProxy_RegionBase gRegions;
CProxy_NeuronBase gNeurons;

Core::Core(CkArgMsg *msg) : mState(Network::StateType_Empty), mRequestIdCounter(0)
{
    mStartTime = CmiWallTimer();
    CkPrintf("Running on %d processors...\n", CkNumPes());

    //if (msg->argc > 1) someParam1 = atoi(msg->argv[1]);
    //if (msg->argc > 2) someParam2 = atoi(msg->argv[2]);
    //if (msg->argc > 3) someParam3 = atoi(msg->argv[3]);

    delete msg;
    msg = nullptr;

    CcsRegisterHandler("request", CkCallback(CkIndex_Core::HandleRequestFromClient(nullptr), thisProxy));

    gMulticastGroupId = CProxy_CkMulticastMgr::ckNew();
    gCompletionDetector = CProxy_CompletionDetector::ckNew();

    CProxy_BrainMap brainMap = CProxy_BrainMap::ckNew();
    CkArrayOptions brainOpts;
    brainOpts.setMap(brainMap);

    CProxy_RegionMap regionMap = CProxy_RegionMap::ckNew();
    CkArrayOptions regionOpts;
    regionOpts.setMap(regionMap);

    CProxy_NeuronMap neuronMap = CProxy_NeuronMap::ckNew();
    CkArrayOptions neuronOpts;
    neuronOpts.setMap(neuronMap);

    gCore = thisProxy;
    gBrain = CProxy_BrainBase::ckNew(brainOpts);
    gRegions = CProxy_RegionBase::ckNew(regionOpts);
    gNeurons = CProxy_NeuronBase::ckNew(neuronOpts);

    // TODO(Premek): remove
    // assume hardcoder blueprint for now
    mState = Network::StateType_Paused;
}

Core::Core(CkMigrateMessage *msg)
{
}

Core::~Core()
{
    for (auto it = mRequests.begin(); it != mRequests.end(); ++it) {
        CkCcsRequestMsg *requestMessage = it->second;
        delete requestMessage;
    }
}

void Core::pup(PUP::er &p)
{
    // TODO(PetrK)
}

void Core::Exit()
{
    CkPrintf("Exitting after %lf...\n", CmiWallTimer() - mStartTime);
    CkExit();
}

void Core::HandleRequestFromClient(CkCcsRequestMsg *msg)
{
    RequestId requestId = mRequestIdCounter++;
    mRequests.insert(std::make_pair(requestId, msg));

    const Network::RequestMessage *requestMessage = Network::GetRequestMessage(msg->data);
    Network::Request requestType = requestMessage->request_type();

    try {
        switch (requestType) {
            case Network::Request_CommandRequest:
            {
                const Network::CommandRequest *commandRequest =
                    static_cast<const Network::CommandRequest*>(requestMessage->request());
                ProcessCommandRequest(commandRequest, requestId);
                break;
            }
            case Network::Request_GetStateRequest:
            {
                const Network::GetStateRequest *getStateRequest =
                    static_cast<const Network::GetStateRequest*>(requestMessage->request());
                ProcessGetStateRequest(getStateRequest, requestId);
                break;
            }
            case Network::Request_GetModelRequest:
            {
                const Network::GetModelRequest *getModelRequest =
                    static_cast<const Network::GetModelRequest*>(requestMessage->request());
                ProcessGetModelRequest(getModelRequest, requestId);
                break;
            }
            default:
            {
                CkPrintf("Unknown request type %d\n", requestType);
            }
        }

    } catch (ShutdownRequestedException &exception) {
        Exit();
    }
}

void Core::SendViewportUpdate(RequestId requestId, const ViewportUpdate &update)
{
    flatbuffers::FlatBufferBuilder builder;
    BuildViewportUpdateResponse(update, builder);
    SendResponseToClient(requestId, builder);
}

void Core::SendResponseToClient(RequestId requestId, flatbuffers::FlatBufferBuilder &builder)
{
    CkCcsRequestMsg *requestMessage = mRequests[requestId];

    if (builder.GetSize() > 0) {
        CcsSendDelayedReply(requestMessage->reply, 
            builder.GetSize(), builder.GetBufferPointer());
    }

    mRequests.erase(requestId);
    delete requestMessage;
}

void Core::ProcessCommandRequest(const Network::CommandRequest *commandRequest, RequestId requestId)
{
    flatbuffers::FlatBufferBuilder builder;
    Network::CommandType commandType = commandRequest->command();

    if (commandType == Network::CommandType_Shutdown) {

        BuildStateResponse(Network::StateType_ShuttingDown, builder);
        SendResponseToClient(requestId, builder);
        throw ShutdownRequestedException("Shutdown requested by the client");
    }

    if (commandType == Network::CommandType_Run) {
        if (mState != Network::StateType_Paused) {
            // TODO(Premek): return error response
            CkPrintf("Run command failed: invalid state\n");
        }

        mState = Network::StateType_Running;  // TODO(): Add actual logic here.

        uint32_t runSteps = commandRequest->stepsToRun();
        if (runSteps != 0)
        {
            // TODO(HonzaS): Handle the exact number of brain steps here.
            // For now, we'll just schedule a delayed state change.
            std::thread{ [this]()
            {
                std::this_thread::sleep_for(std::chrono::seconds{ 1 });
                mState = Network::StateType_Paused;
            } }.detach();
        }

    } else if (commandType == Network::CommandType_Pause) {
        if (mState != Network::StateType_Running) {
            // TODO(Premek): return error response
            CkPrintf("Pause command failed: invalid state\n");
        }

        mState = Network::StateType_Paused;  // TODO(): Add actual logic here.
    }

    BuildStateResponse(mState, builder);
    SendResponseToClient(requestId, builder);
}

void Core::ProcessGetStateRequest(const Network::GetStateRequest *getStateRequest, RequestId requestId)
{
    // TODO(HonzaS): Add actual logic here.
    flatbuffers::FlatBufferBuilder builder;
    BuildStateResponse(mState, builder);
    SendResponseToClient(requestId, builder);
}

bool chance()
{
    return rand() % 60;
}

template <typename TResponse>
void BuildResponseMessage(flatbuffers::FlatBufferBuilder &builder, Network::Response responseType, flatbuffers::Offset<TResponse> &responseOffset)
{
    auto responseMessage = Network::CreateResponseMessage(builder, responseType, responseOffset.Union());
    builder.Finish(responseMessage);
}

void Core::ProcessGetModelRequest(const Network::GetModelRequest *getModelRequest, RequestId requestId)
{
    // TODO(HonzaS): Add actual logic here.
    flatbuffers::FlatBufferBuilder builder;

    /*RegionIndex regionIndex = 1;
    RegionName regionName("FooRegion");
    RegionName regionType("BasicRegion");
    Point3D origin(10.f, 20.f, 30.f);
    Size3D size(40.f, 10.f, 20.f);
    Box3D regionBounds(origin, size);

    ViewportUpdate update;
    update.addedRegions.push_back(RegionAdditionReport(regionIndex, regionName, regionType, regionBounds));

    SendViewportUpdate(requestId, update);*/

    std::vector<flatbuffers::Offset<Network::Region>> addedRegionsOffsets;

    auto regionName = builder.CreateString("testname");
    auto regionType = builder.CreateString("testtype");
    auto lowerBound = Network::CreatePosition(builder, 10.0f, 20.0f, 30.0f);
    auto upperBound = Network::CreatePosition(builder, 50.0f, 40.0f, 45.0f);
    auto regionOffset = Network::CreateRegion(builder, 1, regionName, regionType, lowerBound, upperBound);

    addedRegionsOffsets.push_back(regionOffset);
    auto addedRegionsVector = builder.CreateVector(addedRegionsOffsets);

    Network::ModelResponseBuilder responseBuilder(builder);
    responseBuilder.add_addedRegions(addedRegionsVector);
    auto modelResponseOffset = responseBuilder.Finish();

    BuildResponseMessage(builder, Network::Response_ModelResponse, modelResponseOffset);
    SendResponseToClient(requestId, builder);
}

void Core::BuildStateResponse(Network::StateType state, flatbuffers::FlatBufferBuilder &builder) const
{
    flatbuffers::Offset<Network::StateResponse> stateResponseOffset = Network::CreateStateResponse(builder, state);
    BuildResponseMessage(builder, Network::Response_StateResponse, stateResponseOffset);
}

flatbuffers::Offset<Network::Position> Core::CreatePosition(flatbuffers::FlatBufferBuilder& builder, Point3D point)
{
    return Network::CreatePosition(builder, std::get<0>(point), std::get<1>(point), std::get<2>(point));
}

void Core::BuildViewportUpdateResponse(const ViewportUpdate &update, flatbuffers::FlatBufferBuilder &builder) const
{
    std::vector<flatbuffers::Offset<Network::Region>> addedRegionOffsets;

    for (auto addedRegion : update.addedRegions) {
        RegionIndex index = std::get<0>(addedRegion);

        auto regionName = builder.CreateString(std::get<1>(addedRegion));
        auto regionType = builder.CreateString(std::get<2>(addedRegion));

        Box3D box3d = std::get<3>(addedRegion);

        auto lowerBound = Network::CreatePosition(builder, std::get<0>(box3d.first), std::get<1>(box3d.first), std::get<2>(box3d.first));
        auto upperBound = Network::CreatePosition(builder, std::get<0>(box3d.second), std::get<1>(box3d.second), std::get<2>(box3d.second));

        auto regionOffset = Network::CreateRegion(builder, index, regionName, regionType, lowerBound, upperBound);

        addedRegionOffsets.push_back(regionOffset);
    }

    auto regionName = builder.CreateString("testname");
    auto regionType = builder.CreateString("testtype");
    auto lowerBound = Network::CreatePosition(builder, 10.0f, 20.0f, 30.0f);
    auto upperBound = Network::CreatePosition(builder, 40.0f, 20.0f, 15.0f);
    auto regionOffset = Network::CreateRegion(builder, 1, regionName, regionType, lowerBound, upperBound);
    addedRegionOffsets.push_back(regionOffset);

    auto addedRegionsOffset = builder.CreateVector(addedRegionOffsets);

    Network::ModelResponseBuilder responseBuilder(builder);
    responseBuilder.add_addedRegions(addedRegionsOffset);
    auto modelResponseOffset = responseBuilder.Finish();

    BuildResponseMessage(builder, Network::Response_ModelResponse, modelResponseOffset);
}



#include "core.def.h"
