#include <future>
#include <sstream>
#include <fstream>

#include "core.h"
#include "brain.h"

#define CATCH_IMPL  // Unofficial macro that does not add the main() function.
#include "catch.hpp"
#include "core_tests.h"

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

Core::Core(CkArgMsg *msg) : 
    mStartTime(0.0), mBrainLoaded(false),
    mBrainIsUnloading(false), mIsShuttingDown(false), mRequestIdCounter(0)
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

    // Experimental Catch tests
    if ((msg->argc > 1) && (strcmp(msg->argv[1], "--test") == 0)) {
        CkPrintf("Setting up Catch tests...\n");
        SetupCharmTests();

        thisProxy.RunTests();

        delete msg;
        return;
    }
    // TODO(Premek): Add position independent argument processing.

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
                    LoadBrain(brainName, brainType, brainParams);
                }
            }
        }
    }

    delete msg;
}

Core::Core(CkMigrateMessage *msg) :
    mStartTime(0.0), mBrainLoaded(false),
    mBrainIsUnloading(false), mIsShuttingDown(false), mRequestIdCounter(0)
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
    p | mStartTime;
    p | mBrainLoaded;
    p | mBrainIsUnloading;
    p | mIsShuttingDown;
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
    // TODO(HonzaS): Handle graceful exit later (saving of the snapshot etc).
    CkPrintf("Exiting after %lf...\n", CmiWallTimer() - mStartTime);
    CkExit();
}

void Core::HandleRequestFromClient(CkCcsRequestMsg *msg)
{
    RequestId requestId = mRequestIdCounter++;
    mRequests.insert(std::make_pair(requestId, msg));

    const Communication::RequestMessage *requestMessage = Communication::GetRequestMessage(msg->data);
    Communication::Request requestType = requestMessage->request_type();

    try {
        switch (requestType) {
            case Communication::Request_CommandRequest:
            {
                auto commandRequest = static_cast<const Communication::CommandRequest*>(requestMessage->request());
                ProcessCommandRequest(commandRequest, requestId);
                break;
            }
            case Communication::Request_GetStateRequest:
            {
                auto getStateRequest = static_cast<const Communication::GetStateRequest*>(requestMessage->request());
                ProcessGetStateRequest(getStateRequest, requestId);
                break;
            }
            case Communication::Request_GetModelRequest:
            {
                auto getModelRequest = static_cast<const Communication::GetModelRequest*>(requestMessage->request());
                ProcessGetModelRequest(getModelRequest, requestId);
                break;
            }
            default:
            {
                CkPrintf("Unknown request type %d\n", requestType);
            }
        }
    } catch (ShutdownRequestedException &exception) {
        mIsShuttingDown = true;
        if (IsBrainLoaded()) {
            gBrain[0].RequestSimulationState(requestId, true, false);
            UnloadBrain();
        } else {
            ProcessGetStateRequest(nullptr, requestId);

            Exit();
        }
    }
}

void Core::RunTests()
{
    CkPrintf("Running Catch tests...\n");

    char *arg = "tests";
    Catch::Session().run(1, &arg);

    CkPrintf("Testing done. Exiting.\n");
    Exit();
}

void Core::LoadBrain(const BrainName &name, const BrainType &type, const BrainParams &params)
{
    if (!mBrainLoaded) {
        gBrain[0].insert(name, type, params);
        gBrain.doneInserting();
        mBrainLoaded = true;
    }
}

bool Core::IsBrainLoaded() const
{
    return (mBrainLoaded && !mBrainIsUnloading);
}

void Core::UnloadBrain()
{
    if (mBrainLoaded && !mBrainIsUnloading) {
        mBrainIsUnloading = true;
        gBrain[0].Unload();
    }
}

void Core::BrainUnloaded()
{
    if (mBrainIsUnloading) {
        gBrain[0].ckDestroy();
        mBrainLoaded = false;
        mBrainIsUnloading = false;

        if (mIsShuttingDown)
            Exit();
    }
}

void Core::SendEmptyMessage(RequestId requestId)
{
    flatbuffers::FlatBufferBuilder builder;
    SendResponseToClient(requestId, builder);
}

void Core::SendSimulationState(RequestId requestId, bool isSimulationRunning, 
    size_t atBrainStep, size_t atBodyStep, size_t brainStepsPerBodyStep)
{
    flatbuffers::FlatBufferBuilder builder;
    BuildStateResponse(isSimulationRunning, atBrainStep, atBodyStep, brainStepsPerBodyStep, builder);
    SendResponseToClient(requestId, builder);
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
    } else {
        CcsSendDelayedReply(requestMessage->reply, 0, nullptr);
    }

    mRequests.erase(requestId);
    delete requestMessage;
}

void Core::ProcessCommandRequest(const Communication::CommandRequest *commandRequest, RequestId requestId)
{
    Communication::CommandType commandType = commandRequest->command();

    if (commandType == Communication::CommandType_Shutdown)
        throw ShutdownRequestedException("Shutdown requested by the client");

    if (commandType == Communication::CommandType_Run) {
        if (!IsBrainLoaded()) {
            // TODO(Premek): return error response
            CkPrintf("Run command failed: invalid state\n");
        } else {
            uint32_t runSteps = commandRequest->stepsToRun();
            gBrain[0].RunSimulation(runSteps, runSteps == 0);
        }
    } else if (commandType == Communication::CommandType_Pause) {
        if (!IsBrainLoaded()) {
            // TODO(Premek): return error response
            CkPrintf("Pause command failed: invalid state\n");
        } else {
            gBrain[0].PauseSimulation();
        }
    }

    // TODO(HonzaS): Refactor (or at least rename).
    ProcessGetStateRequest(nullptr, requestId);
}

void Core::ProcessGetStateRequest(const Communication::GetStateRequest *getStateRequest, RequestId requestId)
{
    flatbuffers::FlatBufferBuilder builder;

    if (IsBrainLoaded()) {
        gBrain[0].RequestSimulationState(requestId, true, false);
    } else {
        BuildStateResponse(false, 0, 0, 0, builder);
        SendResponseToClient(requestId, builder);
    }
}

// TODO(HonzaS): Remove this.
bool chance()
{
    return rand() % 60;
}

template <typename TResponse>
void BuildResponseMessage(flatbuffers::FlatBufferBuilder &builder, Communication::Response responseType, flatbuffers::Offset<TResponse> &responseOffset)
{
    auto responseMessage = Communication::CreateResponseMessage(builder, responseType, responseOffset.Union());
    builder.Finish(responseMessage);
}

void Core::ProcessGetModelRequest(const Communication::GetModelRequest *getModelRequest, RequestId requestId)
{
    if (IsBrainLoaded())
        gBrain[0].RequestViewportUpdate(requestId, getModelRequest->full(), true);

    // TODO(HonzaS): Remove.
    //SendStubModel(requestId);
}

void Core::SendStubModel(RequestId requestId)
{
    flatbuffers::FlatBufferBuilder builder;

    std::vector<flatbuffers::Offset<Communication::Region>> addedRegionsOffsets;

    static size_t mDummyTimestep = 0;

    if (mDummyTimestep == 0) {
        auto regionName = builder.CreateString("testname");
        auto regionType = builder.CreateString("testtype");
        auto lowerBound = Communication::CreatePosition(builder, 30.0f, 00.0f, 10.0f);
        auto upperBound = Communication::CreatePosition(builder, 82.0f, 22.0f, 32.0f);
        auto regionOffset = Communication::CreateRegion(builder, 1, regionName, regionType, lowerBound, upperBound);

        auto regionName2 = builder.CreateString("testname 2");
        auto regionType2 = builder.CreateString("testtype 2");
        auto lowerBound2 = Communication::CreatePosition(builder, 110.0f, 00.0f, 10.0f);
        auto upperBound2 = Communication::CreatePosition(builder, 162.0f, 22.0f, 32.0f);
        auto regionOffset2 = Communication::CreateRegion(builder, 2, regionName2, regionType2, lowerBound2, upperBound2);

        addedRegionsOffsets.push_back(regionOffset);
        addedRegionsOffsets.push_back(regionOffset2);
    }

    std::vector<uint32_t> removedRegions;

    if (mDummyTimestep % 2 == 0) {
        auto regionName3 = builder.CreateString("testname 3");
        auto regionType3 = builder.CreateString("testtype 3");
        auto lowerBound3 = Communication::CreatePosition(builder, 210.0f, 00.0f, 10.0f);
        auto upperBound3 = Communication::CreatePosition(builder, 252.0f, 22.0f, 32.0f);
        auto regionOffset3 = Communication::CreateRegion(builder, 3, regionName3, regionType3, lowerBound3, upperBound3);

        addedRegionsOffsets.push_back(regionOffset3);
    } else {
        removedRegions.push_back(3);
    }

    auto addedRegionsVector = builder.CreateVector(addedRegionsOffsets);
    auto removedRegionsVector = builder.CreateVector(removedRegions);

    std::vector<flatbuffers::Offset<Communication::Connector>> addedConnectorsOffsets;
    std::vector<flatbuffers::Offset<Communication::Connection>> addedConnectionsOffsets;

    if (mDummyTimestep == 0) {
        auto connectorName1 = builder.CreateString("connector 1");
        auto connectorName2 = builder.CreateString("connector 2");

        auto connectorName3 = builder.CreateString("connector 3");
        auto connectorName4 = builder.CreateString("connector 4");
        auto connectorName5 = builder.CreateString("connector 5");

        auto connectorName6 = builder.CreateString("connector 6");
        auto connectorName7 = builder.CreateString("connector 7");

        auto connectorOffset1 = Communication::CreateConnector(builder, 1, connectorName1, Communication::Direction_Forward, 5);
        auto connectorOffset2 = Communication::CreateConnector(builder, 1, connectorName2, Communication::Direction_Forward, 15);

        auto connectorOffset3 = Communication::CreateConnector(builder, 1, connectorName3, Communication::Direction_Backward, 5);
        auto connectorOffset4 = Communication::CreateConnector(builder, 1, connectorName4, Communication::Direction_Backward, 8);
        auto connectorOffset5 = Communication::CreateConnector(builder, 1, connectorName5, Communication::Direction_Backward, 2);

        auto connectorOffset6 = Communication::CreateConnector(builder, 2, connectorName6, Communication::Direction_Backward, 5);
        auto connectorOffset7 = Communication::CreateConnector(builder, 2, connectorName7, Communication::Direction_Backward, 5);

        addedConnectorsOffsets.push_back(connectorOffset1);
        addedConnectorsOffsets.push_back(connectorOffset2);

        addedConnectorsOffsets.push_back(connectorOffset3);
        addedConnectorsOffsets.push_back(connectorOffset4);
        addedConnectorsOffsets.push_back(connectorOffset5);

        addedConnectorsOffsets.push_back(connectorOffset6);
        addedConnectorsOffsets.push_back(connectorOffset7);

        auto connectionOffset1 = Communication::CreateConnection(builder, 1, connectorName1, 2, connectorName6);
        auto connectionOffset2 = Communication::CreateConnection(builder, 1, connectorName1, 2, connectorName7);

        addedConnectionsOffsets.push_back(connectionOffset1);
        addedConnectionsOffsets.push_back(connectionOffset2);
    }

    auto addedConnectorsVector = builder.CreateVector(addedConnectorsOffsets);

    auto addedConnectionsVector = builder.CreateVector(addedConnectionsOffsets);

    std::vector<flatbuffers::Offset<Communication::Neuron>> addedNeuronsOffsets;

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

        auto neuronPosition = Communication::CreatePosition(builder,
            5.f * static_cast<float>(x),
            2.f * static_cast<float>(y),
            2.f * static_cast<float>(z));
        auto neuronId = Communication::CreateNeuronId(builder, addedNeuronCount + 1, 1);
        auto neuronOffset = Communication::CreateNeuron(builder, neuronId, neuronType, neuronPosition);

        addedNeuronsOffsets.push_back(neuronOffset);
        addedNeuronCount++;
    }

    auto addedNeuronsVector = builder.CreateVector(addedNeuronsOffsets);

    auto synapseAddInterval = 20;

    std::vector<flatbuffers::Offset<Communication::Synapse>> addedSynapsesOffsets;

    static std::vector<std::pair<uint32_t, uint32_t>> addedSynapses;

    if (mDummyTimestep % synapseAddInterval == 0) {
        int fromNeuron = (rand() % addedNeuronCount) + 1;
        int nextLayerStart = ((fromNeuron / layerSize) + 1) * layerSize;
        if (nextLayerStart < addedNeuronCount) {
            int toNeuron = (rand() % (addedNeuronCount - nextLayerStart)) + nextLayerStart;

            auto fromNeuronId = Communication::CreateNeuronId(builder, fromNeuron, 1);
            auto toNeuronId = Communication::CreateNeuronId(builder, toNeuron, 1);
            auto synapseOffset = Communication::CreateSynapse(builder, fromNeuronId, toNeuronId);

            addedSynapsesOffsets.push_back(synapseOffset);

            std::pair<int32_t, int32_t> synapse(fromNeuron, toNeuron);
            addedSynapses.push_back(synapse);
        }
    }

    auto addedSynapsesVector = builder.CreateVector(addedSynapsesOffsets);

    std::vector<flatbuffers::Offset<Communication::Synapse>> spikedSynapsesOffsets;

    for (auto synapse : addedSynapses) {
        if (rand() % 100 == 0) {
            auto fromNeuronId = Communication::CreateNeuronId(builder, synapse.first, 1);
            auto toNeuronId = Communication::CreateNeuronId(builder, synapse.second, 1);

            auto synapseOffset = Communication::CreateSynapse(builder, fromNeuronId, toNeuronId);
            spikedSynapsesOffsets.push_back(synapseOffset);
        }
    }

    auto spikedSynapsesVector = builder.CreateVector(spikedSynapsesOffsets);

    Communication::ModelResponseBuilder responseBuilder(builder);
    // Added items.
    responseBuilder.add_addedRegions(addedRegionsVector);
    responseBuilder.add_addedConnectors(addedConnectorsVector);
    responseBuilder.add_addedConnections(addedConnectionsVector);
    responseBuilder.add_addedNeurons(addedNeuronsVector);
    responseBuilder.add_addedSynapses(addedSynapsesVector);
    responseBuilder.add_spikedSynapses(spikedSynapsesVector);

    // Removed items.
    responseBuilder.add_removedRegions(removedRegionsVector);

    auto modelResponseOffset = responseBuilder.Finish();

    BuildResponseMessage(builder, Communication::Response_ModelResponse, modelResponseOffset);
    SendResponseToClient(requestId, builder);

    mDummyTimestep++;
}

void Core::BuildStateResponse(const Communication::StateType state, size_t atBrainStep, 
    size_t atBodyStep, size_t brainStepsPerBodyStep, flatbuffers::FlatBufferBuilder &builder) const
{
    auto stateResponseOffset = Communication::CreateStateResponse(builder, state, atBrainStep, atBodyStep, brainStepsPerBodyStep);
    BuildResponseMessage(builder, Communication::Response_StateResponse, stateResponseOffset);
}

flatbuffers::Offset<Communication::Position> Core::CreatePosition(flatbuffers::FlatBufferBuilder& builder, Point3D point)
{
    return Communication::CreatePosition(builder, std::get<0>(point), std::get<1>(point), std::get<2>(point));
}

void Core::BuildStateResponse(bool isSimulationRunning, size_t atBrainStep, 
    size_t atBodyStep, size_t brainStepsPerBodyStep, flatbuffers::FlatBufferBuilder &builder) const
{
    Communication::StateType state;
    if (mIsShuttingDown) {
        state = Communication::StateType_ShuttingDown;
    } else if (isSimulationRunning) {
        state = Communication::StateType_Running;
    } else if (IsBrainLoaded()) {
        state = Communication::StateType_Paused;
    } else {
        state = Communication::StateType_Empty;
    }

    BuildStateResponse(state, atBrainStep, atBodyStep, brainStepsPerBodyStep, builder);
}

void Core::BuildRegionOffsets(
    flatbuffers::FlatBufferBuilder &builder,
    const RegionAdditionReports &regions,
    std::vector<flatbuffers::Offset<Communication::Region>> &regionOffsets) const
{
    for (auto region : regions) {
        RegionIndex index = std::get<0>(region);

        auto regionName = builder.CreateString(std::get<1>(region));
        auto regionType = builder.CreateString(std::get<2>(region));

        Box3D box3d = std::get<3>(region);

        auto lowerBound = Communication::CreatePosition(builder, std::get<0>(box3d.first), std::get<1>(box3d.first), std::get<2>(box3d.first));
        auto upperBound = Communication::CreatePosition(builder, std::get<0>(box3d.second), std::get<1>(box3d.second), std::get<2>(box3d.second));

        auto regionOffset = Communication::CreateRegion(builder, index, regionName, regionType, lowerBound, upperBound);

        regionOffsets.push_back(regionOffset);
    }
}

Communication::Direction Core::CommunicationDirection(Direction direction) const
{
    return direction == Direction::Forward
        ? Communication::Direction::Direction_Forward
        : Communication::Direction::Direction_Backward;
}

void Core::BuildConnectorOffsets(
    flatbuffers::FlatBufferBuilder &builder,
    const ConnectorAdditionReports &connectors,
    std::vector<flatbuffers::Offset<Communication::Connector>> &connectorOffsets) const
{
    for (auto connector : connectors) {
        RegionIndex regionIndex = std::get<0>(connector);

        auto direction = CommunicationDirection(std::get<1>(connector));
        auto connectorName = builder.CreateString(std::get<2>(connector));
        auto size = std::get<3>(connector);

        auto connectorOffset = Communication::CreateConnector(builder, regionIndex, connectorName, direction, size);

        connectorOffsets.push_back(connectorOffset);
    }
}

void Core::BuildConnectorRemovalOffsets(
    flatbuffers::FlatBufferBuilder &builder,
    const ConnectorRemovals &connectors,
    std::vector<flatbuffers::Offset<Communication::ConnectorRemoval>> &connectorOffsets) const
{
    for (auto connector : connectors) {
        RegionIndex regionIndex = std::get<0>(connector);

        auto direction = CommunicationDirection(std::get<1>(connector));
        auto connectorName = builder.CreateString(std::get<2>(connector));

        auto connectorOffset = Communication::CreateConnectorRemoval(builder, regionIndex, connectorName, direction);

        connectorOffsets.push_back(connectorOffset);
    }
}

void Core::BuildConnectionOffsets(
    flatbuffers::FlatBufferBuilder &builder,
    const Connections &connections,
    std::vector<flatbuffers::Offset<Communication::Connection>> &connectionOffsets) const
{
    for (auto connection : connections) {
        auto direction = CommunicationDirection(std::get<0>(connection));

        RegionIndex fromRegion = std::get<1>(connection);
        auto fromConnector = builder.CreateString(std::get<2>(connection));

        RegionIndex toRegion = std::get<3>(connection);
        auto toConnector = builder.CreateString(std::get<4>(connection));

        auto connectionOffset = Communication::CreateConnection(builder, fromRegion, fromConnector, toRegion, toConnector, direction);

        connectionOffsets.push_back(connectionOffset);
    }
}

void Core::BuildNeuronOffsets(
    flatbuffers::FlatBufferBuilder &builder,
    const NeuronAdditionReports &neurons,
    std::vector<flatbuffers::Offset<Communication::Neuron>> &neuronOffsets) const
{
    for (auto neuron : neurons) {
        NeuronId id = std::get<0>(neuron);

        auto idOffset = CommunicationNeuronId(builder, id);
        auto typeOffset = builder.CreateString(std::get<1>(neuron));

        auto position = std::get<2>(neuron);
        auto positionOffset = Communication::CreatePosition(builder, std::get<0>(position), std::get<1>(position), std::get<2>(position));

        auto neuronOffset = Communication::CreateNeuron(builder, idOffset, typeOffset, positionOffset);

        neuronOffsets.push_back(neuronOffset);
    }
}

flatbuffers::Offset<Communication::NeuronId> Core::CommunicationNeuronId(flatbuffers::FlatBufferBuilder &builder, NeuronId neuronId) const
{
    return Communication::CreateNeuronId(builder, GetNeuronIndex(neuronId), GetRegionIndex(neuronId));
}

void Core::BuildSynapseOffsets(
    flatbuffers::FlatBufferBuilder &builder,
    const Synapse::Links &synapses,
    std::vector<flatbuffers::Offset<Communication::Synapse>> &synapseOffsets) const
{
    for (auto synapse : synapses) {
        auto fromNeuronId = std::get<0>(synapse);
        auto toNeuronId = std::get<1>(synapse);

        auto fromOffset = CommunicationNeuronId(builder, fromNeuronId);
        auto toOffset = CommunicationNeuronId(builder, toNeuronId);
        auto synapseOffset = Communication::CreateSynapse(builder, fromOffset, toOffset);

        synapseOffsets.push_back(synapseOffset);
    }
}

void Core::BuildViewportUpdateResponse(const ViewportUpdate &update, flatbuffers::FlatBufferBuilder &builder) const
{
    // Regions.
    std::vector<flatbuffers::Offset<Communication::Region>> addedRegionOffsets;
    BuildRegionOffsets(builder, update.addedRegions, addedRegionOffsets);
    auto addedRegionsVectorOffset = builder.CreateVector(addedRegionOffsets);

    std::vector<flatbuffers::Offset<Communication::Region>> repositionedRegionOffsets;
    BuildRegionOffsets(builder, update.repositionedRegions, repositionedRegionOffsets);
    auto repositionedRegionsVectorOffset = builder.CreateVector(repositionedRegionOffsets);

    std::vector<uint32_t> removedRegions;
    for (auto regionIndex : update.removedRegions)
        removedRegions.push_back(regionIndex);
    auto removedRegionsVectorOffset = builder.CreateVector(removedRegions);

    // Connectors.
    std::vector<flatbuffers::Offset<Communication::Connector>> addedConnectorOffsets;
    BuildConnectorOffsets(builder, update.addedConnectors, addedConnectorOffsets);
    auto addedConnectorsVectorOffset = builder.CreateVector(addedConnectorOffsets);

    std::vector<flatbuffers::Offset<Communication::ConnectorRemoval>> removedConnectorOffsets;
    BuildConnectorRemovalOffsets(builder, update.removedConnectors, removedConnectorOffsets);
    auto removedConnectorsVectorOffset = builder.CreateVector(removedConnectorOffsets);

    // Connections.
    std::vector<flatbuffers::Offset<Communication::Connection>> addedConnectionOffsets;
    BuildConnectionOffsets(builder, update.addedConnections, addedConnectionOffsets);
    auto addedConnectionsVectorOffset = builder.CreateVector(addedConnectionOffsets);

    std::vector<flatbuffers::Offset<Communication::Connection>> removedConnectionOffsets;
    BuildConnectionOffsets(builder, update.removedConnections, removedConnectionOffsets);
    auto removedConnectionsVectorOffset = builder.CreateVector(removedConnectionOffsets);

    // Neurons.
    std::vector<flatbuffers::Offset<Communication::Neuron>> addedNeuronOffsets;
    BuildNeuronOffsets(builder, update.addedNeurons, addedNeuronOffsets);
    auto addedNeuronsVectorOffset = builder.CreateVector(addedNeuronOffsets);

    std::vector<flatbuffers::Offset<Communication::Neuron>> repositionedNeuronOffsets;
    BuildNeuronOffsets(builder, update.repositionedNeurons, repositionedNeuronOffsets);
    auto repositionedNeuronsVectorOffset = builder.CreateVector(repositionedNeuronOffsets);

    std::vector<flatbuffers::Offset<Communication::NeuronId>> removedNeurons;
    for (auto neuronId : update.removedNeurons) {
        auto neuronIndex = GetNeuronIndex(neuronId);
        auto regionIndex = GetRegionIndex(neuronId);

        auto idOffset = Communication::CreateNeuronId(builder, neuronIndex, regionIndex);
        removedNeurons.push_back(idOffset);
    }
    auto removedNeuronsVectorOffset = builder.CreateVector(removedNeurons);

    // Synapses.
    std::vector<flatbuffers::Offset<Communication::Synapse>> addedSynapseOffsets;
    BuildSynapseOffsets(builder, update.addedSynapses, addedSynapseOffsets);
    auto addedSynapsesVectorOffset = builder.CreateVector(addedSynapseOffsets);

    std::vector<flatbuffers::Offset<Communication::Synapse>> spikedSynapseOffsets;
    BuildSynapseOffsets(builder, update.spikedSynapses, spikedSynapseOffsets);
    auto spikedSynapsesVectorOffset = builder.CreateVector(spikedSynapseOffsets);

    std::vector<flatbuffers::Offset<Communication::Synapse>> removedSynapseOffsets;
    BuildSynapseOffsets(builder, update.removedSynapses, removedSynapseOffsets);
    auto removedSynapsesVectorOffset = builder.CreateVector(removedSynapseOffsets);

    Communication::ModelResponseBuilder responseBuilder(builder);
    responseBuilder.add_addedRegions(addedRegionsVectorOffset);
    responseBuilder.add_repositionedRegions(repositionedRegionsVectorOffset);
    responseBuilder.add_removedRegions(removedRegionsVectorOffset);

    responseBuilder.add_addedConnectors(addedConnectorsVectorOffset);
    responseBuilder.add_removedConnectors(removedConnectorsVectorOffset);

    responseBuilder.add_addedConnections(addedConnectionsVectorOffset);
    responseBuilder.add_removedConnections(removedConnectionsVectorOffset);

    responseBuilder.add_addedNeurons(addedNeuronsVectorOffset);
    responseBuilder.add_repositionedNeurons(repositionedNeuronsVectorOffset);
    responseBuilder.add_removedNeurons(removedNeuronsVectorOffset);

    responseBuilder.add_addedSynapses(addedSynapsesVectorOffset);
    responseBuilder.add_spikedSynapses(spikedSynapsesVectorOffset);
    responseBuilder.add_removedSynapses(removedSynapsesVectorOffset);

    auto modelResponseOffset = responseBuilder.Finish();

    BuildResponseMessage(builder, Communication::Response_ModelResponse, modelResponseOffset);
}

#include "core.def.h"
