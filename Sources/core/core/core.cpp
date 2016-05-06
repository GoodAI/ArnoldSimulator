#include "core.h"

#include "brain.decl.h"
#include "region.decl.h"
#include "neuron.decl.h"
#include <future>

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
            default:
            {
                CkPrintf("Unknown request type %d\n", requestType);
            }
        }

    } catch (ShutdownRequestedException &exception) {
        Exit();
    }
}

void Core::SendViewportUpdate(
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
    ChildLinks &removedChildren)
{
    flatbuffers::FlatBufferBuilder builder;
    BuildViewportUpdateResponse(
        addedRegions, repositionedRegions, removedRegions,
        addedConnectors, removedConnectors, 
        addedConnections, removedConnections,
        addedNeurons, repositionedNeurons, removedNeurons,
        addedSynapses, spikedSynapses, removedSynapses,
        addedChildren, removedChildren, builder);
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

void Core::BuildStateResponse(Network::StateType state, flatbuffers::FlatBufferBuilder &builder)
{
    flatbuffers::Offset<Network::StateResponse> stateResponseOffset = CreateStateResponse(builder, state);
    auto responseMessage = Network::CreateResponseMessage(builder, Network::Response_StateResponse, stateResponseOffset.Union());
    builder.Finish(responseMessage);
}

void Core::BuildViewportUpdateResponse(
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
    flatbuffers::FlatBufferBuilder &builder)
{
    // TODO(HonzaS)
}



#include "core.def.h"
