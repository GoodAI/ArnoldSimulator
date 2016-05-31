#include <future>
#include <sstream>
#include <fstream>

#include "core.h"
#include "brain.h"

CkGroupID gMulticastGroupId;
CProxy_CompletionDetector gCompletionDetector;

CProxy_Core gCore;
CProxy_BrainBase gBrain;
CProxy_RegionBase gRegions;
CProxy_NeuronBase gNeurons;

void CoreNodeInit()
{
}

void CoreProcInit()
{
    TurnManualLBOn();
}

Core::Core(CkArgMsg *msg) : mState(Network::StateType_Empty), mRequestIdCounter(0)
{
    mStartTime = CmiWallTimer();
    CkPrintf("Running on %d processors...\n", CkNumPes());

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

    std::ifstream blueprintFile;
    std::stringstream blueprintFilePath;
    if (msg->argc > 1) {
        blueprintFilePath << msg->argv[1];
        if (!blueprintFilePath.str().empty()) {
            blueprintFile.open(blueprintFilePath.str());
        }
    }

    std::stringstream blueprintContent;
    if (blueprintFile.is_open()) {
        blueprintContent << blueprintFile.rdbuf();
        blueprintFile.close();
    }

    if (!blueprintContent.str().empty()) {
        json blueprint = json::parse(blueprintContent.str());
        if (!blueprint.empty()) {
            if (blueprint.begin().key() == "brain" && blueprint.begin()->is_object()) {

                json brain = blueprint.begin().value();
                std::string brainName, brainType, brainParams;
                for (auto it = brain.begin(); it != brain.end(); ++it) {
                    if (it.key() == "name" && it.value().is_string()) {
                        brainName = it.value().get<std::string>();
                    } else if (it.key() == "type" && it.value().is_string()) {
                        brainType = it.value().get<std::string>();
                    } else if (it.key() == "params" && it.value().is_object()) {
                        brainParams = it.value().dump();
                    }
                }

                if (!brainType.empty()) {
                    gBrain[0].insert(brainName, brainType, brainParams);
                    gBrain.doneInserting();
                }
            }
        }
    }

    // TODO(Premek): remove
    // assume hardcoder blueprint for now
    mState = Network::StateType_Paused;

    delete msg;
}

Core::Core(CkMigrateMessage *msg) :
    mState(Network::StateType::StateType_Empty), mStartTime(0.0), mRequestIdCounter(0)
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
    p | mState;
    p | mStartTime;
    p | mRequestIdCounter;

    if (p.isUnpacking()) {
        size_t requestsCount; p | requestsCount;
        for (size_t i = 0; i < requestsCount; ++i) {
            RequestId requestId; p | requestId;
            CkCcsRequestMsg *messagePtr = new CkCcsRequestMsg(); messagePtr->pup(p);
            mRequests.insert(std::make_pair(requestId, messagePtr));
        }
    } else {
        size_t requestsCount = mRequests.size(); p | requestsCount;
        for (auto it = mRequests.begin(); it != mRequests.end(); ++it) {
            RequestId requestId = it->first; p | requestId;
            CkCcsRequestMsg *messagePtr = it->second; messagePtr->pup(p);
        }
    }
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
                auto commandRequest = static_cast<const Network::CommandRequest*>(requestMessage->request());
                ProcessCommandRequest(commandRequest, requestId);
                break;
            }
            case Network::Request_GetStateRequest:
            {
                auto getStateRequest = static_cast<const Network::GetStateRequest*>(requestMessage->request());
                ProcessGetStateRequest(getStateRequest, requestId);
                break;
            }
            case Network::Request_GetModelRequest:
            {
                auto getModelRequest = static_cast<const Network::GetModelRequest*>(requestMessage->request());
                ProcessGetModelRequest(getModelRequest, requestId);
                break;
            }
            default:
            {
                CkPrintf("Unknown request type %d\n", requestType);
            }
        }

    } catch (ShutdownRequestedException &exception) {
        CkPrintf("ShutdownRequestedException: %s\n", exception.what());
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

    static size_t mDummyTimestep = 0;

    if (mDummyTimestep == 0) {
        auto regionName = builder.CreateString("testname");
        auto regionType = builder.CreateString("testtype");
        auto lowerBound = Network::CreatePosition(builder, 30.0f, 00.0f, 10.0f);
        auto upperBound = Network::CreatePosition(builder, 82.0f, 22.0f, 32.0f);
        auto regionOffset = Network::CreateRegion(builder, 1, regionName, regionType, lowerBound, upperBound);

        auto regionName2 = builder.CreateString("testname 2");
        auto regionType2 = builder.CreateString("testtype 2");
        auto lowerBound2 = Network::CreatePosition(builder, 110.0f, 00.0f, 10.0f);
        auto upperBound2 = Network::CreatePosition(builder, 162.0f, 22.0f, 32.0f);
        auto regionOffset2 = Network::CreateRegion(builder, 2, regionName2, regionType2, lowerBound2, upperBound2);

        addedRegionsOffsets.push_back(regionOffset);
        addedRegionsOffsets.push_back(regionOffset2);
    }

    auto addedRegionsVector = builder.CreateVector(addedRegionsOffsets);

    std::vector<flatbuffers::Offset<Network::Connector>> addedConnectorsOffsets;
    std::vector<flatbuffers::Offset<Network::Connection>> addedConnectionsOffsets;

    if (mDummyTimestep == 0) {
        auto connectorName1 = builder.CreateString("connector 1");
        auto connectorName2 = builder.CreateString("connector 2");

        auto connectorName3 = builder.CreateString("connector 3");
        auto connectorName4 = builder.CreateString("connector 4");
        auto connectorName5 = builder.CreateString("connector 5");

        auto connectorName6 = builder.CreateString("connector 6");
        auto connectorName7 = builder.CreateString("connector 7");

        auto connectorOffset1 = Network::CreateConnector(builder, 1, connectorName1, Network::Direction_Forward, 5);
        auto connectorOffset2 = Network::CreateConnector(builder, 1, connectorName2, Network::Direction_Forward, 15);

        auto connectorOffset3 = Network::CreateConnector(builder, 1, connectorName3, Network::Direction_Backward, 5);
        auto connectorOffset4 = Network::CreateConnector(builder, 1, connectorName4, Network::Direction_Backward, 8);
        auto connectorOffset5 = Network::CreateConnector(builder, 1, connectorName5, Network::Direction_Backward, 2);

        auto connectorOffset6 = Network::CreateConnector(builder, 2, connectorName6, Network::Direction_Backward, 5);
        auto connectorOffset7 = Network::CreateConnector(builder, 2, connectorName7, Network::Direction_Backward, 5);

        addedConnectorsOffsets.push_back(connectorOffset1);
        addedConnectorsOffsets.push_back(connectorOffset2);

        addedConnectorsOffsets.push_back(connectorOffset3);
        addedConnectorsOffsets.push_back(connectorOffset4);
        addedConnectorsOffsets.push_back(connectorOffset5);

        addedConnectorsOffsets.push_back(connectorOffset6);
        addedConnectorsOffsets.push_back(connectorOffset7);

        auto connectionOffset1 = Network::CreateConnection(builder, 1, connectorName1, 2, connectorName6);
        auto connectionOffset2 = Network::CreateConnection(builder, 1, connectorName1, 2, connectorName7);

        addedConnectionsOffsets.push_back(connectionOffset1);
        addedConnectionsOffsets.push_back(connectionOffset2);
    }

    auto addedConnectorsVector = builder.CreateVector(addedConnectorsOffsets);

    auto addedConnectionsVector = builder.CreateVector(addedConnectionsOffsets);

    std::vector<flatbuffers::Offset<Network::Neuron>> addedNeuronsOffsets;

    const auto neuronAddInterval = 1;
    const auto maxNeuronCount = 1000;
    static auto addedNeuronCount = 0;

    const auto layerSizeX = 10;
    const auto layerSizeY = 10;
    const auto layerSize = layerSizeX * layerSizeY;

    if ((mDummyTimestep % neuronAddInterval == 0) && (addedNeuronCount < maxNeuronCount)) {
        auto neuronType = builder.CreateString("neurotype");
        // This is relative to region lower bound in the UI (?)

        auto x = addedNeuronCount / layerSize;
        auto y = (addedNeuronCount / 10) % layerSizeY;
        auto z = addedNeuronCount % layerSizeX;

        auto neuronPosition = Network::CreatePosition(builder,
            5.f * static_cast<float>(x),
            2.f * static_cast<float>(y),
            2.f * static_cast<float>(z));
        auto neuronOffset = Network::CreateNeuron(builder, addedNeuronCount+1, 1, neuronType, neuronPosition);

        addedNeuronsOffsets.push_back(neuronOffset);
        addedNeuronCount++;
    }

    auto addedNeuronsVector = builder.CreateVector(addedNeuronsOffsets);

    auto synapseAddInterval = 20;

    std::vector<flatbuffers::Offset<Network::Synapse>> addedSynapsesOffsets;

    static std::vector<std::pair<uint32_t, uint32_t>> addedSynapses;

    if (mDummyTimestep % synapseAddInterval == 0) {
        int fromNeuron = (rand() % addedNeuronCount) + 1;
        int nextLayerStart = ((fromNeuron / layerSize) + 1) * layerSize;
        if (nextLayerStart < addedNeuronCount) {
            int toNeuron = (rand() % (addedNeuronCount - nextLayerStart)) + nextLayerStart;

            auto synapseOffset = Network::CreateSynapse(builder, 1, fromNeuron, 1, toNeuron);

            addedSynapsesOffsets.push_back(synapseOffset);

            std::pair<int32_t, int32_t> synapse(fromNeuron, toNeuron);
            addedSynapses.push_back(synapse);
        }
    }

    auto addedSynapsesVector = builder.CreateVector(addedSynapsesOffsets);

    std::vector<flatbuffers::Offset<Network::Synapse>> spikedSynapsesOffsets;

    for (auto synapse : addedSynapses) {
        if (rand() % 100 == 0) {
            auto synapseOffset = Network::CreateSynapse(builder, 1, synapse.first, 1, synapse.second);
            spikedSynapsesOffsets.push_back(synapseOffset);
        }
    }

    auto spikedSynapsesVector = builder.CreateVector(spikedSynapsesOffsets);

    Network::ModelResponseBuilder responseBuilder(builder);
    responseBuilder.add_addedRegions(addedRegionsVector);
    responseBuilder.add_addedConnectors(addedConnectorsVector);
    responseBuilder.add_addedConnections(addedConnectionsVector);
    responseBuilder.add_addedNeurons(addedNeuronsVector);
    responseBuilder.add_addedSynapses(addedSynapsesVector);
    responseBuilder.add_spikedSynapses(spikedSynapsesVector);
    auto modelResponseOffset = responseBuilder.Finish();

    BuildResponseMessage(builder, Network::Response_ModelResponse, modelResponseOffset);
    SendResponseToClient(requestId, builder);

    mDummyTimestep++;
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
