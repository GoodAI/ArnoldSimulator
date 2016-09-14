#include <future>
#include <sstream>
#include <fstream>

#include "core.h"
#include "brain.h"
#include "log.h"
#include "data_utils.h"

#define CATCH_IMPL  // Unofficial macro that does not add the main() function.
#include "catch.hpp"

#include "core_tests.h"
#include "init.h"

CkGroupID gMulticastGroupId;
CProxy_CompletionDetector gCompletionDetector;

CProxy_Core gCore;
CProxy_BrainBase gBrain;
CProxy_RegionBase gRegions;
CProxy_NeuronBase gNeurons;

void CoreNodeInit()
{
    InitializeComponents();
}

void CoreProcInit()
{
    TurnManualLBOn();
}

Core *GetCoreLocalPtr()
{
    //return gCore.ckLocal();
    return static_cast<Core *>(gCore.ckGetChareID().objPtr);
}

Core::Core(CkArgMsg *msg) : 
    mStartTime(0.0), mBrainLoaded(false), mBrainIsUnloading(false), mIsShuttingDown(false),
    mRequestIdCounter(0), mKeyControlEnabled(false), mKeyControlRegularCheckpointsEnabled(false),
    mKeyControlRegularLoadBalancingEnabled(false),
    mKeyControlBrainStepsPerBodyStep(DEFAULT_BRAIN_STEPS_PER_BODY_STEP)
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

        std::vector<std::string> testArguments;
        for (auto i = 2; i < msg->argc; i++)
            testArguments.push_back(std::string(msg->argv[i]));

        thisProxy.RunTests(testArguments);

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

    auto blueprintString = blueprintContent.str();
    if (!blueprintString.empty() && TryLoadBrain(blueprintString)) {
        std::thread input(&Core::DetectKeyPress, this);
        mKeyControlEnabled = true;
        input.detach();
    }

    delete msg;
}

Core::Core(CkMigrateMessage *msg) :
    mStartTime(0.0), mBrainLoaded(false), mBrainIsUnloading(false), mIsShuttingDown(false), 
    mRequestIdCounter(0), mKeyControlEnabled(false), mKeyControlRegularCheckpointsEnabled(false),
    mKeyControlRegularLoadBalancingEnabled(false),
    mKeyControlBrainStepsPerBodyStep(DEFAULT_BRAIN_STEPS_PER_BODY_STEP)
{
    CkPrintf("Running on %d processors...\n", CkNumPes());

    gCore = thisProxy;

    CcsRegisterHandler("request", CkCallback(CkIndex_Core::HandleRequestFromClient(nullptr), thisProxy));
}

bool Core::TryLoadBrain(const std::string &blueprintString)
{
    // TODO(HonzaS): let the caller know why loading failed (parsing or wrong structure).
    json blueprint;
    try {
        blueprint = json::parse(blueprintString);
    } catch (std::invalid_argument &) {
        Log(LogLevel::Warn, "Invalid blueprint");
    }

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
                return true;
            }
        }
    }

    return false;
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
    p | mKeyControlEnabled;
    p | mKeyControlRegularCheckpointsEnabled;
    p | mKeyControlRegularLoadBalancingEnabled;
    p | mKeyControlBrainStepsPerBodyStep;

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

    if (p.isUnpacking() && mKeyControlEnabled) {
        std::thread input(&Core::DetectKeyPress, this);
        input.detach();
    }
}

void Core::Exit()
{
    CkPrintf("Exiting after %f seconds...\n", CmiWallTimer() - mStartTime);
    CkExit();
}

void Core::DetectKeyPress()
{
    while (true) {
        char c = getchar();
        if (c == 'b') {
            if (IsBrainLoaded()) {
                gBrain[0].RunSimulation(10, false, false);
            }
        } else if (c == 'r') {
            if (IsBrainLoaded()) {
                gBrain[0].RunSimulation(1, true, false);
            }
        } else if (c == 'p') {
            if (IsBrainLoaded()) {
                gBrain[0].PauseSimulation();
            }
        } else if (c == 's') {
            if (IsBrainLoaded()) {
                gBrain[0].RunSimulation(1, false, false);
            }
        } else if (c == 'c') {
            if (IsBrainLoaded()) {
                gBrain[0].RequestOneTimeCheckpoint(DEFAULT_CHECKPOINT_DIRECTORY);
            }
        } else if (c == 'h') {
            if (IsBrainLoaded()) {
                if (mKeyControlRegularCheckpointsEnabled) {
                    mKeyControlRegularCheckpointsEnabled = false;
                    CkPrintf("DisableRegularCheckpoints\n");
                    gBrain[0].DisableRegularCheckpoints();
                } else {
                    mKeyControlRegularCheckpointsEnabled = true;
                    CkPrintf("EnableRegularCheckpoints\n");
                    gBrain[0].EnableRegularCheckpoints(
                        DEFAULT_CHECKPOINT_DIRECTORY, DEFAULT_BRAIN_STEPS_PER_CHECKPOINT);
                }
            }
        } else if (c == 'l') {
            if (IsBrainLoaded()) {
                CkPrintf("RequestOneTimeLoadBalancing\n");
                gBrain[0].RequestOneTimeLoadBalancing();
            }
        } else if (c == 'n') {
            if (IsBrainLoaded()) {
                if (mKeyControlRegularLoadBalancingEnabled) {
                    mKeyControlRegularLoadBalancingEnabled = false;
                    CkPrintf("DisableRegularLoadBalancing\n");
                    gBrain[0].DisableRegularLoadBalancing();
                } else {
                    mKeyControlRegularLoadBalancingEnabled = true;
                    CkPrintf("EnableRegularLoadBalancing\n");
                    gBrain[0].EnableRegularLoadBalancing(DEFAULT_SECONDS_PER_LOAD_BALANCING);
                }
            }
        } else if (c == 'i') {
            if (IsBrainLoaded()) {
                if (mKeyControlBrainStepsPerBodyStep == 1) {
                    mKeyControlBrainStepsPerBodyStep = 5;
                } else {
                    mKeyControlBrainStepsPerBodyStep += 5;
                }
                CkPrintf("SetBrainStepsPerBodyStep: %u\n", mKeyControlBrainStepsPerBodyStep);
                gBrain[0].SetBrainStepsPerBodyStep(mKeyControlBrainStepsPerBodyStep);
            }
        } else if (c == 'd') {
            if (IsBrainLoaded()) {
                if (mKeyControlBrainStepsPerBodyStep <= 5) {
                    mKeyControlBrainStepsPerBodyStep = 1;
                } else {
                    mKeyControlBrainStepsPerBodyStep -= 5;
                }
                CkPrintf("SetBrainStepsPerBodyStep: %u\n", mKeyControlBrainStepsPerBodyStep);
                gBrain[0].SetBrainStepsPerBodyStep(mKeyControlBrainStepsPerBodyStep);
            }
        } else if (c == 'q') {
            mIsShuttingDown = true;
            if (IsBrainLoaded()) {
                UnloadBrain();
            } else {
                Exit();
            }
            break;
        }
    }
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
                SendCompleteStateResponse(getStateRequest, requestId);
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
    } catch (ShutdownRequestedException &) {
        mIsShuttingDown = true;
        if (IsBrainLoaded()) {
            gBrain[0].RequestSimulationState(requestId, true, false);
            UnloadBrain();
        } else {
            SendCompleteStateResponse(nullptr, requestId);

            Exit();
        }
    }
}

void Core::RunTests(std::vector<std::string> &args)
{
    CkPrintf("Running Catch tests...\n");

    std::unique_ptr<const char*[]> argvWrapper(new const char*[args.size() + 10]);
    auto argv = argvWrapper.get();
    int argc = 0;

    argv[argc++] = "tests";  // Process name.

    for (const auto &arg : args) {
        argv[argc++] = arg.c_str();
    }

    //argv[argc++] = "--success";  // Show also successful tests.
    //argv[argc++] = "--list-tests";  // Only list tests and exit.
    
    Catch::Session().run(argc, argv);

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

        if (mIsShuttingDown) Exit();
    }
}

void Core::SendEmptyMessage(RequestId requestId)
{
    flatbuffers::FlatBufferBuilder builder;
    SendResponseToClient(requestId, builder);
}

void Core::SendErrorResponse(RequestId requestId, const std::string &message)
{
    Log(LogLevel::Warn, message.c_str());

    flatbuffers::FlatBufferBuilder builder;
    BuildErrorResponse(message, builder);
    SendResponseToClient(requestId, builder);
}

void Core::SendSimulationState(RequestId requestId, bool isSimulationRunning, 
    size_t atBrainStep, size_t atBodyStep, size_t brainStepsPerBodyStep)
{
    flatbuffers::FlatBufferBuilder builder;
    BuildCompleteStateResponse(isSimulationRunning, atBrainStep, atBodyStep, brainStepsPerBodyStep, builder);
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
    bool sendCommandInProgress = false;

    if (commandType == Communication::CommandType_Shutdown)
        throw ShutdownRequestedException("Shutdown requested by the client");

    if (commandType == Communication::CommandType_Load) {
        if (IsBrainLoaded()) {
            // TODO(Premek): refactor using exceptions?
            SendErrorResponse(requestId, "Load command failed: brain already loaded\n");
            return;
        }

        const flatbuffers::String *blueprint = commandRequest->blueprint();
        if (blueprint == nullptr) {
            SendErrorResponse(requestId, "Load command failed: blueprint missing\n");
            return;
        }

        if (!TryLoadBrain(blueprint->str())) {
            SendErrorResponse(requestId, "Load command failed: invalid blueprint\n");
            return;
        }

        // TODO(Premek): ensure this is consistent with default state of UI controls
        gBrain[0].EnableRegularLoadBalancing(DEFAULT_SECONDS_PER_LOAD_BALANCING);
        gBrain[0].EnableRegularCheckpoints(
            DEFAULT_CHECKPOINT_DIRECTORY, DEFAULT_BRAIN_STEPS_PER_CHECKPOINT);
    } else if (commandType == Communication::CommandType_Run) {
        if (!IsBrainLoaded()) {
            SendErrorResponse(requestId, "Run command failed: brain not loaded\n");
            return;
        }

        uint32_t runSteps = commandRequest->stepsToRun();

        gBrain[0].RunSimulation(runSteps, runSteps == 0, commandRequest->runToBodyStep());
        sendCommandInProgress = true;
    } else if (commandType == Communication::CommandType_Pause) {
        if (!IsBrainLoaded()) {
            SendErrorResponse(requestId, "Pause command failed: brain not loaded\n");
            return;
        }

        gBrain[0].PauseSimulation();
        sendCommandInProgress = true;
    } else if (commandType == Communication::CommandType_Configure) {
        json configuration;
        try {
            configuration = json::parse(commandRequest->configuration()->systemConfiguration()->str());
        }
        catch (std::invalid_argument &) {
            CkPrintf("Invalid configuration.");
        }

        uint32_t brainStepsPerBodyStep = configuration["brainStepsPerBodyStep"].get<uint32_t>();
        gBrain[0].SetBrainStepsPerBodyStep(brainStepsPerBodyStep);
    } else if (commandType == Communication::CommandType_Clear) {
        UnloadBrain();
    }

    if (sendCommandInProgress) {
        SendCommandInProgress(requestId);
    } else {
        // TODO(HonzaS): Refactor (or at least rename).
        SendCompleteStateResponse(nullptr, requestId);
    }
}

void Core::SendCompleteStateResponse(const Communication::GetStateRequest *getStateRequest, RequestId requestId)
{
    flatbuffers::FlatBufferBuilder builder;

    if (IsBrainLoaded()) {
        gBrain[0].RequestSimulationState(requestId, false, false);
    } else {
        BuildCompleteStateResponse(false, 0, 0, 0, builder);
        SendResponseToClient(requestId, builder);
    }
}

template <typename TResponse>
void BuildResponseMessage(flatbuffers::FlatBufferBuilder &builder, Communication::Response responseType, flatbuffers::Offset<TResponse> &responseOffset)
{
    auto responseMessage = Communication::CreateResponseMessage(builder, responseType, responseOffset.Union());
    builder.Finish(responseMessage);
}

void Core::SendCommandInProgress(RequestId requestId)
{
    flatbuffers::FlatBufferBuilder builder;

    Communication::StateResponseBuilder responseBuilder(builder);
    responseBuilder.add_state(Communication::StateType_CommandInProgress);
    auto responseOffset = responseBuilder.Finish();
    BuildResponseMessage(builder, Communication::Response_StateResponse, responseOffset);
    SendResponseToClient(requestId, builder);
}

void Core::ProcessGetModelRequest(const Communication::GetModelRequest *getModelRequest, RequestId requestId)
{
    //SendStubModel(requestId);
    //return;

    if (!IsBrainLoaded()) {
        SendErrorResponse(requestId, "Get model failed: brain not loaded.");
        return;
    }

    if (getModelRequest->filter() != nullptr) {
        Boxes roiBoxes;
        auto filterSize = getModelRequest->filter()->boxes()->size();
        roiBoxes.reserve(filterSize);

        for (int i = 0; i < filterSize; i++) {
            auto box = getModelRequest->filter()->boxes()->Get(i);
            Point3D boxPosition(box->x(), box->y(), box->z());
            Size3D boxSize(box->sizeX(), box->sizeY(), box->sizeZ());
            Box3D roiBox(boxPosition, boxSize);
            roiBoxes.push_back(roiBox);
        }

        gBrain[0].UpdateRegionOfInterest(roiBoxes);
    }

    if (getModelRequest->observers() != nullptr) {
        Observers observers;
        auto observationSize = getModelRequest->observers()->size();
        observers.reserve(observationSize);

        for (int i = 0; i < observationSize; i++) {
            auto observer = getModelRequest->observers()->Get(i);
            NeuronId neuronId = GetNeuronId(observer->neuronId()->region(), observer->neuronId()->neuron());
            ObserverType observerType = ParseObserverType(observer->type()->str());

            observers.push_back(Observer(neuronId, observerType));
        }

        gBrain[0].UpdateObservers(observers);
    }


    gBrain[0].RequestViewportUpdate(requestId, getModelRequest->full(), true);
}

void Core::SendStubModel(const Communication::GetModelRequest *getModelRequest, RequestId requestId)
{
    flatbuffers::FlatBufferBuilder builder;

    std::vector<flatbuffers::Offset<Communication::Region>> addedRegionsOffsets;

    static size_t mDummyTimestep = 0;

    if (mDummyTimestep == 0) {
        auto regionName = builder.CreateString("testname");
        auto regionType = builder.CreateString("testtype");
        auto lowerBound = Communication::CreatePosition(builder, 30.0f, 00.0f, 10.0f);
        auto upperBound = Communication::CreatePosition(builder, 50.0f, 20.0f, 20.0f);
        auto regionOffset = Communication::CreateRegion(builder, 1, regionName, regionType, lowerBound, upperBound);

        auto regionName2 = builder.CreateString("testname 2");
        auto regionType2 = builder.CreateString("testtype 2");
        auto lowerBound2 = Communication::CreatePosition(builder, 110.0f, 00.0f, 10.0f);
        auto upperBound2 = Communication::CreatePosition(builder, 50.0f, 20.0f, 20.0f);
        auto regionOffset2 = Communication::CreateRegion(builder, 2, regionName2, regionType2, lowerBound2, upperBound2);

        addedRegionsOffsets.push_back(regionOffset);
        addedRegionsOffsets.push_back(regionOffset2);
    }

    std::vector<uint32_t> removedRegions;

    if (mDummyTimestep % 2 == 0) {
        auto regionName3 = builder.CreateString("testname 3");
        auto regionType3 = builder.CreateString("testtype 3");
        auto lowerBound3 = Communication::CreatePosition(builder, 210.0f, 00.0f, 10.0f);
        auto upperBound3 = Communication::CreatePosition(builder, 50.0f, 20.0f, 20.0f);
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
            static_cast<float>(x) / layerSizeX,
            static_cast<float>(y) / layerSizeX,
            static_cast<float>(z) / layerSizeX);
        auto neuronId = Communication::CreateNeuronId(builder, addedNeuronCount + 1, 1);
        auto neuronOffset = Communication::CreateNeuron(builder, neuronId, neuronType, neuronPosition);

        addedNeuronsOffsets.push_back(neuronOffset);
        addedNeuronCount++;
    }

    std::vector<flatbuffers::Offset<Communication::ObserverResult>> observersOffsets;

    auto observers = getModelRequest->observers();
    if (observers != nullptr) {
        for (int i = 0; i < observers->size(); i++) {
            auto observer = observers->Get(i);

            auto neuronId = observer->neuronId();

            uint32_t neuronIndex = neuronId->neuron();
            uint32_t regionIndex = neuronId->region();
            std::string observerType = observer->type()->str();

            auto neuronIdOffset = Communication::CreateNeuronId(builder, neuronIndex, regionIndex);
            auto observerTypeOffset = builder.CreateString(observerType);

            auto observerOffset = Communication::CreateObserver(builder, neuronIdOffset, observerTypeOffset);
            std::vector<uint8_t> data;
            data.push_back(255);
            data.push_back(0);
            data.push_back(128);
            data.push_back(255);
            data.push_back(0);
            data.push_back(128);
            data.push_back(255);
            data.push_back(0);
            data.push_back(128);
            data.push_back(255);
            data.push_back(0);
            data.push_back(128);
            auto dataVectorOffset = builder.CreateVector(data);

            auto observerDataOffset = Communication::CreateObserverResult(builder, observerOffset, 0 /* dimensions */, dataVectorOffset);
            observersOffsets.push_back(observerDataOffset);
        }    
    }

    auto observersVector = builder.CreateVector(observersOffsets);

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

    // Observers.
    responseBuilder.add_observerResults(observersVector);

    auto modelResponseOffset = responseBuilder.Finish();

    BuildResponseMessage(builder, Communication::Response_ModelResponse, modelResponseOffset);
    SendResponseToClient(requestId, builder);

    mDummyTimestep++;
}

flatbuffers::Offset<Communication::Position> Core::CreatePosition(flatbuffers::FlatBufferBuilder &builder, Point3D point)
{
    return Communication::CreatePosition(builder, std::get<0>(point), std::get<1>(point), std::get<2>(point));
}

void Core::BuildCompleteStateResponse(const Communication::StateType state, size_t atBrainStep, 
    size_t atBodyStep, size_t brainStepsPerBodyStep, flatbuffers::FlatBufferBuilder &builder) const
{
    auto stats = Communication::CreateSimulationStats(builder, atBrainStep, atBodyStep, brainStepsPerBodyStep);
    auto stateResponseOffset = Communication::CreateStateResponse(builder, state, stats);
    BuildResponseMessage(builder, Communication::Response_StateResponse, stateResponseOffset);
}

void Core::BuildCompleteStateResponse(bool isSimulationRunning, size_t atBrainStep, 
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

    BuildCompleteStateResponse(state, atBrainStep, atBodyStep, brainStepsPerBodyStep, builder);
}

void Core::BuildErrorResponse(const std::string &message, flatbuffers::FlatBufferBuilder &builder) const
{
    auto messageOffset = builder.CreateString(message);
    auto errorResponseOffest = Communication::CreateErrorResponse(builder, messageOffset);
    BuildResponseMessage(builder, Communication::Response_ErrorResponse, errorResponseOffest);
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
        auto direction = std::get<0>(connection);

        RegionIndex fromRegion = std::get<1>(connection);
        auto fromConnector = builder.CreateString(std::get<2>(connection));

        RegionIndex toRegion = std::get<3>(connection);
        auto toConnector = builder.CreateString(std::get<4>(connection));

        if (direction == Direction::Backward) {
            auto tmpConnector = fromConnector;
            fromConnector = toConnector;
            toConnector = tmpConnector;
        }

        auto connectionOffset = Communication::CreateConnection(builder, fromRegion, fromConnector, toRegion, toConnector);

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

void Core::BuildObserverResults(const ViewportUpdate &update, flatbuffers::FlatBufferBuilder &builder,
    std::vector<flatbuffers::Offset<Communication::ObserverResult>> &observerResultOffsets) const
{
    for (auto observerResult : update.observerResults) {

        auto observerDefinition = std::get<0>(observerResult);
        auto neuronId = std::get<0>(observerDefinition);

        ObserverType observerType = std::get<1>(observerDefinition);

        std::vector<int32_t> observerMetadata = std::get<1>(observerResult);
        std::vector<uint8_t> observerData = std::get<2>(observerResult);

        auto neuronIdOffset = CommunicationNeuronId(builder, neuronId);

        auto observerStringType = SerializeObserverType(observerType);
        auto observerTypeOffset = builder.CreateString(observerStringType);

        auto observerOffset = Communication::CreateObserver(builder, neuronIdOffset, observerTypeOffset);

        auto observerMetadataOffset = builder.CreateVector(observerMetadata);

        flatbuffers::Offset<flatbuffers::Vector<uint8_t>> observerPlainDataOffset;
        flatbuffers::Offset<flatbuffers::Vector<float>> observerFloatDataOffset;
        
        if (observerType == ObserverType::FloatTensor) {
            std::vector<float> floatDataVector;
            ConvertByteToFloatVector(observerData, floatDataVector);

            observerFloatDataOffset = builder.CreateVector(floatDataVector);
        } else {
            observerPlainDataOffset = builder.CreateVector(observerData);
        }

        Communication::ObserverResultBuilder observerResultBuilder(builder);
        observerResultBuilder.add_observer(observerOffset);
        observerResultBuilder.add_metadata(observerMetadataOffset);

        if (observerType == ObserverType::FloatTensor) {
            observerResultBuilder.add_floatData(observerFloatDataOffset);
        } else {
            observerResultBuilder.add_plainData(observerPlainDataOffset);
        }

        auto observerResultOffset = observerResultBuilder.Finish();

        observerResultOffsets.push_back(observerResultOffset);
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

    // Observers.
    std::vector<flatbuffers::Offset<Communication::ObserverResult>> observerResultOffsets;
    BuildObserverResults(update, builder, observerResultOffsets);
    auto observerResultsVectorOffset = builder.CreateVector(observerResultOffsets);

    // Finalize the message.
    Communication::ModelResponseBuilder responseBuilder(builder);
    responseBuilder.add_isFull(update.isFull);

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

    responseBuilder.add_observerResults(observerResultsVectorOffset);

    auto modelResponseOffset = responseBuilder.Finish();

    BuildResponseMessage(builder, Communication::Response_ModelResponse, modelResponseOffset);
}

#include "core.def.h"
