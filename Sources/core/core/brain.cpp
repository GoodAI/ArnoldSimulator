#include "core.h"
#include "brain.h"

extern CkGroupID gMulticastGroupId;
extern CProxy_CompletionDetector gCompletionDetector;

extern CProxy_Core gCore;
extern CProxy_BrainBase gBrain;
extern CProxy_RegionBase gRegions;
extern CProxy_NeuronBase gNeurons;

ViewportUpdate::ViewportUpdate() : isFull(false), sinceBrainStep(0), brainStepCount(0)
{
}

void ViewportUpdate::pup(PUP::er &p)
{
    p | isFull;
    p | sinceBrainStep;
    p | brainStepCount;
    p | addedRegions;
    p | repositionedRegions;
    p | removedRegions;
    p | addedConnectors;
    p | removedConnectors;
    p | addedConnections;
    p | removedConnections;
    p | addedNeurons;
    p | repositionedNeurons;
    p | removedNeurons;
    p | addedSynapses;
    p | spikedSynapses;
    p | removedSynapses;
    p | addedChildren;
    p | removedChildren;
}

void *SimulateMsg::pack(SimulateMsg *msg)
{
    size_t boxCnt = msg->roiBoxes.size();
    size_t boxLastCnt = msg->roiBoxesLast.size();
    size_t size = (sizeof(bool) * 3) + (sizeof(size_t) * 2) + (sizeof(Box3D) * (boxCnt + boxLastCnt));
    char *buf = static_cast<char *>(CkAllocBuffer(msg, size));
    char *cur = buf;

    std::memcpy(cur, &msg->doUpdate, sizeof(bool));
    cur += sizeof(bool);

    std::memcpy(cur, &msg->doFullUpdate, sizeof(bool));
    cur += sizeof(bool);

    std::memcpy(cur, &msg->doProgress, sizeof(bool));
    cur += sizeof(bool);

    std::memcpy(cur, &msg->brainStep, sizeof(size_t));
    cur += sizeof(size_t);

    std::memcpy(cur, &boxCnt, sizeof(size_t));
    cur += sizeof(size_t);

    std::memcpy(cur, msg->roiBoxes.data(), sizeof(Box3D) * boxCnt);
    cur += sizeof(Box3D) * boxCnt;

    std::memcpy(cur, &boxLastCnt, sizeof(size_t));
    cur += sizeof(size_t);

    std::memcpy(cur, msg->roiBoxesLast.data(), sizeof(Box3D) * boxLastCnt);
    //cur += sizeof(Box3D) * boxLastCnt;

    delete msg;
    return static_cast<void *>(buf);
}

SimulateMsg *SimulateMsg::unpack(void *buf)
{
    char* cur = static_cast<char *>(buf);
    SimulateMsg *msg = static_cast<SimulateMsg *>(CkAllocBuffer(buf, sizeof(SimulateMsg)));
    msg = new (static_cast<void *>(msg)) SimulateMsg();

    int num_nodes;
    memcpy(&num_nodes, cur, sizeof(int));
    cur = cur + sizeof(int);

    std::memcpy(&msg->doUpdate, cur, sizeof(bool));
    cur += sizeof(bool);

    std::memcpy(&msg->doFullUpdate, cur, sizeof(bool));
    cur += sizeof(bool);

    std::memcpy(&msg->doProgress, cur, sizeof(bool));
    cur += sizeof(bool);

    std::memcpy(&msg->brainStep, cur, sizeof(size_t));
    cur += sizeof(size_t);

    size_t boxCnt = 0;
    std::memcpy(&boxCnt, cur, sizeof(size_t));
    cur += sizeof(size_t);

    msg->roiBoxes.reserve(boxCnt);
    std::memcpy(msg->roiBoxes.data(), cur, sizeof(Box3D) * boxCnt);
    cur += sizeof(Box3D) * boxCnt;

    size_t boxLastCnt = 0;
    std::memcpy(&boxLastCnt, cur, sizeof(size_t));
    cur += sizeof(size_t);

    msg->roiBoxesLast.reserve(boxLastCnt);
    std::memcpy(msg->roiBoxesLast.data(), cur, sizeof(Box3D) * boxLastCnt);
    //cur += sizeof(Box3D) * boxLastCnt;

    CkFreeMsg(buf);
    return msg;
}

BrainMap::BrainMap()
{
}

BrainMap::BrainMap(CkMigrateMessage *msg)
{
}

int BrainMap::procNum(int arrayHdl, const CkArrayIndex &index)
{
    return 0;
}

Brain::Brain(BrainBase &base, json &params) : mBase(base)
{
}

void BrainBase::Terminal::pup(PUP::er &p)
{
    p | isSensor;
    p | id;
    p | name;
    p | spikeType;
    p | firstNeuron;
    p | neuronCount;
    p | data;

    if (p.isUnpacking()) {
        size_t connectionCount; p | connectionCount;
        connections.reserve(connectionCount);
        for (size_t i = 0; i < connectionCount; ++i) {
            RemoteConnector connector; p | connector;
            connections.insert(connector);
        }
    } else {
        size_t connectionCount = connections.size(); p | connectionCount;
        for (auto it = connections.begin(); it != connections.end(); ++it) {
            RemoteConnector connector(it->first, it->second);
            p | connector;
        }
    }
}

Body *BrainBase::CreateBody(const std::string &type, const std::string &params)
{
    json p = json::parse(params);

    if (!p.empty()) {
        for (auto itParams = p.begin(); itParams != p.end(); ++itParams) {

            if (itParams.key() == "sensors" && itParams->is_array()) {

                for (auto itSensor = itParams.value().begin(); itSensor != itParams.value().end(); ++itSensor) {
                    if (itSensor->is_object()) {

                        std::string sensorName, spikeType;
                        size_t sensorSize = 0;
                        for (auto it = itSensor->begin(); it != itSensor->end(); ++it) {
                            if (it.key() == "name" && it.value().is_string()) {
                                sensorName = it.value().get<std::string>();
                            } else if (it.key() == "spikeType" && it.value().is_string()) {
                                spikeType = it.value().get<std::string>();
                            } else if (it.key() == "size" && it.value().is_number_integer()) {
                                sensorSize = it.value().get<size_t>();
                            }
                        }

                        if (!sensorName.empty()) {
                            CreateTerminal(sensorName, Spike::ParseType(spikeType), sensorSize, true);
                        }
                    }
                }

            } else if (itParams.key() == "actuators" && itParams->is_array()) {

                for (auto itActuator = itParams.value().begin(); itActuator != itParams.value().end(); ++itActuator) {
                    if (itActuator->is_object()) {

                        std::string actuatorName, spikeType;
                        size_t actuatorSize = 0;
                        for (auto it = itActuator->begin(); it != itActuator->end(); ++it) {
                            if (it.key() == "name" && it.value().is_string()) {
                                actuatorName = it.value().get<std::string>();
                            } else if (it.key() == "spikeType" && it.value().is_string()) {
                                spikeType = it.value().get<std::string>();
                            } else if (it.key() == "size" && it.value().is_number_integer()) {
                                actuatorSize = it.value().get<size_t>();
                            }
                        }

                        if (!actuatorName.empty()) {
                            CreateTerminal(actuatorName, Spike::ParseType(spikeType), actuatorSize, false);
                        }
                    }
                }

            }
        }
    }

    return Body::CreateBody(type, p);
}

Brain *BrainBase::CreateBrain(const BrainType &type, BrainBase &base, json &params)
{
    if (type == ThresholdBrain::Type) {
        return new ThresholdBrain(base, params);
    } else {
        return nullptr;
    }
}

BrainBase::BrainBase(const BrainType &name, const BrainType &type, const BrainParams &params) :
    mName(name), mDoViewportUpdate(true), mDoFullViewportUpdate(false), mDoFullViewportUpdateNext(false),
    mDoSimulationProgress(false), mDoSimulationProgressNext(false), mViewportUpdateOverflowed(false),
    mIsSimulationRunning(false), mRegionCommitTopologyChangeDone(false), mRegionSimulateDone(false),
    mAllTopologyChangesDelivered(false), mAllSpikesDelivered(false),
    mDeletedNeurons(0), mTriggeredNeurons(0), mBodyStep(0), mBrainStep(0), mBrainStepsToRun(0),
    mBrainStepsPerBodyStep(DEFAULT_BRAIN_STEPS_PER_BODY_STEP),
    mNeuronIdxCounter(NEURON_INDEX_MIN), mRegionIdxCounter(REGION_INDEX_MIN), mTerminalIdCounter(0),
    mBody(nullptr), mBrain(nullptr)
{
    setMigratable(false);

    mNeuronToTerminalId.set_deleted_key(DELETED_NEURON_ID);

    json p = json::parse(params);
    std::unordered_map<std::string, RegionIndex> regionNameToIndex;

    if (!p.empty()) {
        for (auto itParams = p.begin(); itParams != p.end(); ++itParams) {

            if (itParams.key() == "body" && itParams->is_object()) {

                json body = itParams.value();
                std::string bodyType, bodyParams;
                for (auto it = body.begin(); it != body.end(); ++it) {
                    if (it.key() == "type" && it.value().is_string()) {
                        bodyType = it.value().get<std::string>();
                    } else if (it.key() == "params" && it.value().is_object()) {
                        bodyParams = it.value().dump();
                    }
                }

                if (!bodyType.empty()) {
                    mBody = CreateBody(bodyType, bodyParams);
                }
            
            } else if (itParams.key() == "regions" && itParams->is_array()) {

                
                
                for (auto itRegion = itParams.value().begin(); itRegion != itParams.value().end(); ++itRegion) {
                    if (itRegion->is_object()) {

                        std::string regionName, regionType, regionParams;
                        bool haveRegionPosition = false;
                        Point3D regionPosition;
                        bool haveRegionSize = false;;
                        Size3D regionSize;
                        json inputs, outputs;

                        for (auto it = itRegion->begin(); it != itRegion->end(); ++it) {
                            if (it.key() == "name" && it.value().is_string()) {
                                regionName = it.value().get<std::string>();
                            } else if (it.key() == "type" && it.value().is_string()) {
                                regionType = it.value().get<std::string>();
                            } else if (it.key() == "position" && it.value().is_array()) {
                                if (it.value().size() == 3) {
                                    haveRegionPosition = it.value().at(0).is_number_float() &&
                                        it.value().at(1).is_number_float() && 
                                        it.value().at(2).is_number_float();
                                    if (haveRegionPosition) {
                                        regionPosition = Point3D(it.value().at(0).get<float>(), 
                                            it.value().at(1).get<float>(), it.value().at(2).get<float>());
                                    }
                                }
                            } else if (it.key() == "size" && it.value().is_array()) {
                                if (it.value().size() == 3) {
                                    haveRegionSize = it.value().at(0).is_number_float() &&
                                        it.value().at(1).is_number_float() &&
                                        it.value().at(2).is_number_float();
                                    if (haveRegionSize) {
                                        regionSize = Size3D(it.value().at(0).get<float>(),
                                            it.value().at(1).get<float>(), it.value().at(2).get<float>());
                                    }
                                }
                            } else if (it.key() == "params" && it.value().is_object()) {
                                regionParams = it.value().dump();
                                for (auto itRegParams = it.value().begin(); itRegParams != it.value().end(); ++itRegParams) {
                                    if (itRegParams.key() == "inputs" && itRegParams.value().is_array()) {
                                        inputs = itRegParams.value();
                                    } else if (itRegParams.key() == "outputs" && itRegParams.value().is_array()) {
                                        outputs = itRegParams.value();
                                    }
                                }
                            }
                        }

                        if (!regionName.empty() && !regionType.empty()) {

                            RegionIndex regionIndex = RequestRegionAddition(
                                regionName, regionType, regionParams);

                            regionNameToIndex.insert(std::make_pair(regionName, regionIndex));
                            if (haveRegionPosition && haveRegionSize) {
                                Box3D box(regionPosition, regionSize);
                                mRegionBoxes.insert(std::make_pair(regionIndex, box));
                            }

                            for (auto itInput = inputs.begin(); itInput != inputs.end(); ++itInput) {
                                if (itInput->is_object()) {

                                    std::string inputName, neuronType, neuronParams;
                                    size_t neuronCount = 0;
                                    for (auto it = itInput->begin(); it != itInput->end(); ++it) {
                                        if (it.key() == "name" && it.value().is_string()) {
                                            inputName = it.value().get<std::string>();
                                        } else if (it.key() == "neuronType" && it.value().is_string()) {
                                            neuronType = it.value().get<std::string>();
                                        } else if (it.key() == "neuronParams" && it.value().is_object()) {
                                            neuronParams = it.value().dump();
                                        } else if (it.key() == "neuronCount" && it.value().is_number_integer()) {
                                            neuronCount = it.value().get<size_t>();
                                        }
                                    }

                                    if (!inputName.empty()) {
                                        RequestConnectorAddition(regionIndex, Direction::Backward,
                                            inputName, neuronType, neuronParams, neuronCount);
                                    }
                                }
                            }

                            for (auto itOutput = outputs.begin(); itOutput != outputs.end(); ++itOutput) {
                                if (itOutput->is_object()) {

                                    std::string outputName, neuronType, neuronParams;
                                    size_t neuronCount = 0;
                                    for (auto it = itOutput->begin(); it != itOutput->end(); ++it) {
                                        if (it.key() == "name" && it.value().is_string()) {
                                            outputName = it.value().get<std::string>();
                                        } else if (it.key() == "neuronType" && it.value().is_string()) {
                                            neuronType = it.value().get<std::string>();
                                        } else if (it.key() == "neuronParams" && it.value().is_object()) {
                                            neuronParams = it.value().dump();
                                        } else if (it.key() == "neuronCount" && it.value().is_number_integer()) {
                                            neuronCount = it.value().get<size_t>();
                                        }
                                    }

                                    if (!outputName.empty()) {
                                        RequestConnectorAddition(regionIndex, Direction::Forward,
                                            outputName, neuronType, neuronParams, neuronCount);
                                    }
                                }
                            }  

                        }
                    }
                }

            } else if (itParams.key() == "connections" && itParams->is_array()) {

                for (auto itConnection = itParams.value().begin(); itConnection != itParams.value().end(); ++itConnection) {
                    if (itConnection->is_object()) {

                        std::string fromRegion, fromConnector, toRegion, toConnector;
                        for (auto it = itConnection->begin(); it != itConnection->end(); ++it) {
                            if (it.key() == "fromRegion" && it.value().is_string()) {
                                fromRegion = it.value().get<std::string>();
                            } else if (it.key() == "fromConnector" && it.value().is_string()) {
                                fromConnector = it.value().get<std::string>();
                            } else if (it.key() == "toRegion" && it.value().is_string()) {
                                toRegion = it.value().get<std::string>();
                            } else if (it.key() == "toConnector" && it.value().is_string()) {
                                toConnector = it.value().get<std::string>();
                            }
                        }

                        RegionIndex fromRegIdx = BRAIN_REGION_INDEX;
                        if (!fromRegion.empty()) {
                            if (regionNameToIndex.find(fromRegion) != regionNameToIndex.end()) {
                                fromRegIdx = regionNameToIndex[fromRegion];
                            } else {
                                fromRegIdx = TEMP_REGION_INDEX;
                            }
                        }

                        RegionIndex toRegIdx = BRAIN_REGION_INDEX;
                        if (!toRegion.empty()) {
                            if (regionNameToIndex.find(toRegion) != regionNameToIndex.end()) {
                                toRegIdx = regionNameToIndex[toRegion];
                            } else {
                                toRegIdx = TEMP_REGION_INDEX;
                            }
                        }

                        bool valid = (fromRegIdx != TEMP_REGION_INDEX) &&
                            (toRegIdx != TEMP_REGION_INDEX) &&
                            !fromConnector.empty() && !toConnector.empty();

                        if (valid) {
                            RequestConnectionAddition(Direction::Forward,
                                fromRegIdx, fromConnector, toRegIdx, toConnector);
                        }
                    }
                }

            }
        }
    }

    mBrain = BrainBase::CreateBrain(type, *this, p);
}

BrainBase::BrainBase(CkMigrateMessage *msg) :
    mDoViewportUpdate(true), mDoFullViewportUpdate(false), mDoFullViewportUpdateNext(false),
    mDoSimulationProgress(false), mDoSimulationProgressNext(false), mViewportUpdateOverflowed(false),
    mIsSimulationRunning(false), mRegionCommitTopologyChangeDone(false), mRegionSimulateDone(false),
    mAllTopologyChangesDelivered(false), mAllSpikesDelivered(false),
    mDeletedNeurons(0), mTriggeredNeurons(0), mBodyStep(0), mBrainStep(0), mBrainStepsToRun(0),
    mBrainStepsPerBodyStep(DEFAULT_BRAIN_STEPS_PER_BODY_STEP),
    mNeuronIdxCounter(NEURON_INDEX_MIN), mRegionIdxCounter(REGION_INDEX_MIN), mTerminalIdCounter(0),
    mBody(nullptr), mBrain(nullptr)
{
    mNeuronToTerminalId.set_deleted_key(DELETED_NEURON_ID);
}

BrainBase::~BrainBase()
{
    if (mBrain) delete mBrain;
    if (mBody) delete mBody;
}

void BrainBase::pup(PUP::er &p)
{
    p | mName;

    p | mDoViewportUpdate;
    p | mDoFullViewportUpdate;
    p | mDoFullViewportUpdateNext;
    p | mDoSimulationProgress;
    p | mDoSimulationProgressNext;
    p | mViewportUpdateOverflowed;
    p | mIsSimulationRunning;

    p | mRegionCommitTopologyChangeDone;
    p | mRegionSimulateDone;
    p | mAllTopologyChangesDelivered;
    p | mAllSpikesDelivered;

    p | mDeletedNeurons;
    p | mTriggeredNeurons;

    p | mBodyStep;
    p | mBrainStep;
    p | mBrainStepsToRun;
    p | mBrainStepsPerBodyStep;

    p | mNeuronIdxCounter;
    p | mRegionIdxCounter;
    p | mTerminalIdCounter;

    p | mRoiBoxes;
    p | mViewportUpdateRequests;

    p | mViewportUpdateAccumulator;

    p | mRegionAdditions;
    p | mRegionRepositions;
    p | mRegionRemovals;
    p | mConnectorAdditions;
    p | mConnectorRemovals;
    p | mConnectionAdditions;
    p | mConnectionRemovals;

    if (p.isUnpacking()) {
        size_t regionIndicesCount; p | regionIndicesCount;
        mRegionIndices.reserve(regionIndicesCount);
        for (size_t i = 0; i < regionIndicesCount; ++i) {
            RegionIndex index; p | index;
            mRegionIndices.insert(index);
        }

        size_t regionBoxesCount; p | regionBoxesCount;
        mRegionBoxes.reserve(regionBoxesCount);
        for (size_t i = 0; i < regionBoxesCount; ++i) {
            RegionIndex index; p | index;
            Box3D box; p | box;
            mRegionBoxes.insert(std::make_pair(index, box));
        }

        size_t terminalsCount; p | terminalsCount;
        mTerminals.reserve(terminalsCount);
        for (size_t i = 0; i < terminalsCount; ++i) {
            Terminal terminal; p | terminal;
            mTerminals.insert(std::make_pair(terminal.id, terminal));
        }

        size_t terminalNameToIdCount; p | terminalNameToIdCount;
        mTerminalNameToId.reserve(terminalNameToIdCount);
        for (size_t i = 0; i < terminalNameToIdCount; ++i) {
            ConnectorName name; p | name;
            TerminalId terminalId; p | terminalId;
            mTerminalNameToId.insert(std::make_pair(name, terminalId));
        }

        size_t meuronToTerminalIdCount; p | meuronToTerminalIdCount;
        for (size_t i = 0; i < meuronToTerminalIdCount; ++i) {
            NeuronId neuronId; p | neuronId;
            TerminalId terminalId; p | terminalId;
            mNeuronToTerminalId.insert(std::make_pair(neuronId, terminalId));
        }

        json brainParams;
        BrainType brainType;
        p | brainType;
        mBrain = CreateBrain(brainType, *this, brainParams);
        if (mBrain) mBrain->pup(p);

        json bodyParams;
        std::string bodyType;
        p | bodyType;
        mBody = Body::CreateBody(bodyType, bodyParams);
        if (mBody) mBody->pup(p);
    } else {
        size_t regionIndicesCount = mRegionIndices.size(); p | regionIndicesCount;
        for (auto it = mRegionIndices.begin(); it != mRegionIndices.end(); ++it) {
            RegionIndex index = *it; p | index;
        }

        size_t regionBoxesCount = mRegionBoxes.size(); p | regionBoxesCount;
        for (auto it = mRegionBoxes.begin(); it != mRegionBoxes.end(); ++it) {
            RegionIndex index = it->first; p | index;
            Box3D box = it->second; p | box;
        }

        size_t terminalsCount = mTerminals.size(); p | terminalsCount;
        for (auto it = mTerminals.begin(); it != mTerminals.end(); ++it) {
            Terminal terminal = it->second; p | terminal;
        }

        size_t terminalNameToIdCount = mTerminalNameToId.size(); p | terminalNameToIdCount;
        for (auto it = mTerminalNameToId.begin(); it != mTerminalNameToId.end(); ++it) {
            ConnectorName name = it->first; p | name;
            TerminalId terminalId = it->second; p | terminalId;
        }

        size_t meuronToTerminalIdCount = mNeuronToTerminalId.size(); p | meuronToTerminalIdCount;
        for (auto it = mNeuronToTerminalId.begin(); it != mNeuronToTerminalId.end(); ++it) {
            NeuronId neuronId = it->first; p | neuronId;
            TerminalId terminalId = it->second; p | terminalId;
        }

        BrainType brainType;
        if (mBrain) brainType = mBrain->GetType();
        p | brainType;
        if (mBrain) mBrain->pup(p);

        std::string bodyType;
        if (mBody) bodyType = mBody->GetType();
        p | bodyType;
        if (mBody) mBody->pup(p);
    }
}

const char *BrainBase::GetType() const
{
    if (mBrain) {
        return mBrain->GetType();
    } else {
        return "";
    }
}

const char *BrainBase::GetName() const
{
    return mName.c_str();
}

RegionIndex BrainBase::GetNewNeuronIndex()
{
    if (mNeuronIdxCounter == NEURON_INDEX_MAX) CkAbort("Neuron indices depleted.");
    return mNeuronIdxCounter++;
}

RegionIndex BrainBase::GetNewRegionIndex()
{
    if (mNeuronIdxCounter == REGION_INDEX_MAX) CkAbort("Region indices depleted.");
    return mRegionIdxCounter++;
}

Box3D BrainBase::GetBoxForRegion(RegionIndex regIdx)
{
    auto itBox = mRegionBoxes.find(regIdx);
    if (itBox != mRegionBoxes.end()) {
        return itBox->second;
    } else {
        float xMax = 0.0f;
        for (auto it = mRegionBoxes.begin(); it != mRegionBoxes.end(); ++it) {
            float x = std::get<0>(it->second.first);
            xMax = (x > xMax) ? x : xMax;
        }
        xMax += BOX_DEFAULT_MARGIN;

        Box3D box;
        box.first = Point3D(xMax, 0.0f, 0.0f);
        box.second = Size3D(BOX_DEFAULT_SIZE_X, BOX_DEFAULT_SIZE_Y, BOX_DEFAULT_SIZE_Z);

        mRegionBoxes.insert(std::make_pair(regIdx, box));
        return box;
    }
}

const BrainBase::Terminals &BrainBase::GetTerminals() const
{
    return mTerminals;
}

void BrainBase::CreateTerminal(const ConnectorName &name, Spike::Type spikeType, size_t neuronCount, bool isSensor)
{
    Terminal terminal;
    terminal.isSensor = isSensor;
    terminal.id = mTerminalIdCounter++;
    terminal.name = name;
    terminal.spikeType = spikeType;
    terminal.firstNeuron = GetNeuronId(BRAIN_REGION_INDEX, mNeuronIdxCounter);
    terminal.neuronCount = neuronCount;

    mTerminals.insert(std::make_pair(terminal.id, terminal));
    mTerminalNameToId.insert(std::make_pair(terminal.name, terminal.id));
    for (size_t i = 0; i < neuronCount; ++i) {
        mNeuronToTerminalId.insert(std::make_pair(
            GetNeuronId(BRAIN_REGION_INDEX, GetNewNeuronIndex()), terminal.id));
    }
}

void BrainBase::ConnectTerminal(const ConnectorName &name, const RemoteConnector &destination)
{
    auto itTerm = mTerminalNameToId.find(name);
    if (itTerm == mTerminalNameToId.end()) return;

    Terminal &terminal = mTerminals.find(itTerm->second)->second;
    terminal.connections.insert(destination);
}

void BrainBase::DisconnectTerminal(const ConnectorName &name, const RemoteConnector &destination)
{
    auto itTerm = mTerminalNameToId.find(name);
    if (itTerm == mTerminalNameToId.end()) return;

    Terminal &terminal = mTerminals.find(itTerm->second)->second;
    terminal.connections.erase(destination);
}

RegionIndex BrainBase::RequestRegionAddition(const RegionName &name, const RegionType &type, const RegionParams &params)
{
    RegionIndex regIdx = GetNewRegionIndex();
    mRegionAdditions.push_back(std::make_tuple(regIdx, name, type, params));
    return regIdx;
}

void BrainBase::RequestRegionRemoval(RegionIndex regIdx)
{
    mRegionRemovals.push_back(regIdx);
}

void BrainBase::RequestConnectorAddition(
    RegionIndex regIdx, Direction direction, const ConnectorName &name,
    const NeuronType &neuronType, const NeuronParams &neuronParams, size_t neuronCount)
{
    mConnectorAdditions.push_back(std::make_tuple(
        regIdx, direction, name, neuronType, neuronParams, neuronCount));
}

void BrainBase::RequestConnectorRemoval(RegionIndex regIdx, Direction direction, const ConnectorName &name)
{
    mConnectorRemovals.push_back(std::make_tuple(regIdx, direction, name));
}

void BrainBase::RequestConnectionAddition(Direction direction,
    RegionIndex srcRegIdx, const ConnectorName &srcConnectorName, 
    RegionIndex destRegIdx, const ConnectorName &destConnectorName)
{
    mConnectionAdditions.push_back(std::make_tuple(
        direction, srcRegIdx, srcConnectorName, destRegIdx, destConnectorName));
}

void BrainBase::RequestConnectionRemoval(Direction direction,
    RegionIndex srcRegIdx, const ConnectorName &srcConnectorName, 
    RegionIndex destRegIdx, const ConnectorName &destConnectorName)
{
    mConnectionRemovals.push_back(std::make_tuple(
        direction, srcRegIdx, srcConnectorName, destRegIdx, destConnectorName));
}

void BrainBase::PushSensoMotoricData(const std::string &terminalName, std::vector<uint8_t> &data)
{
    auto itTerm = mTerminalNameToId.find(terminalName);
    if (itTerm == mTerminalNameToId.end()) return;

    Spike::Data dummySpike;
    Terminal &terminal = mTerminals.find(itTerm->second)->second;
    Spike::Initialize(terminal.spikeType, 0, dummySpike);
    size_t spikeSize = Spike::Edit(dummySpike)->AllBytes(dummySpike);

    terminal.data.clear();
    std::swap(data, terminal.data);
    terminal.data.resize(terminal.neuronCount * spikeSize, 0);
}

void BrainBase::PullSensoMotoricData(const std::string &terminalName, std::vector<uint8_t> &data)
{
    auto itTerm = mTerminalNameToId.find(terminalName);
    if (itTerm == mTerminalNameToId.end()) return;

    Spike::Data dummySpike;
    Terminal &terminal = mTerminals.find(itTerm->second)->second;
    Spike::Initialize(terminal.spikeType, 0, dummySpike);
    size_t spikeSize = Spike::Edit(dummySpike)->AllBytes(dummySpike);
    
    terminal.data.resize(terminal.neuronCount * spikeSize, 0);
    std::swap(data, terminal.data);
    terminal.data.clear();
    terminal.data.reserve(terminal.neuronCount * spikeSize);
}

void BrainBase::ReceiveTerminalData(Spike::BrainSink &data)
{
    for (auto it = data.begin(); it != data.end(); ++it) {
        auto itTerm = mNeuronToTerminalId.find(it->first);
        if (itTerm == mNeuronToTerminalId.end()) continue;

        Terminal &terminal = mTerminals.find(itTerm->second)->second;

        size_t spikeByteCount = Spike::Edit(it->second)->AllBytes(it->second);
        size_t spikeOffset = (it->first - terminal.firstNeuron) * spikeByteCount;
        size_t requiredSize = spikeOffset + spikeByteCount;
        if (terminal.data.size() < requiredSize) terminal.data.resize(requiredSize, 0);
        Spike::Edit(it->second)->ExportAll(it->second,
            terminal.data.data() + spikeOffset, spikeByteCount);
    }
}


void BrainBase::RunSimulation(size_t brainSteps, bool untilStopped)
{
    mDoSimulationProgressNext = true;
    mBrainStepsToRun = untilStopped ? SIZE_MAX : brainSteps;
    if (!mIsSimulationRunning) {
        thisProxy[thisIndex].Simulate();
    }
}

void BrainBase::StopSimulation()
{
    mBrainStepsToRun = 0;
}

void BrainBase::SetBrainStepsPerBodyStep(size_t steps)
{
    mBrainStepsPerBodyStep = steps;
}

void BrainBase::UpdateRegionOfInterest(Boxes &roiBoxes)
{
    mRoiBoxes = roiBoxes;
}

void BrainBase::UpdateRegionBox(RegionIndex regIdx, Box3D &box)
{
    mRegionRepositions.push_back(RegionRepositionRequest(regIdx, box));
}

void BrainBase::RequestSimulationState(RequestId requestId)
{
    if (mIsSimulationRunning) {
        mSimulationStateRequests.push_back(requestId);  
    } else {
        gCore.ckLocal()->SendSimulationState(requestId, mIsSimulationRunning,
            mBrainStep, mBodyStep, mBrainStepsPerBodyStep);
    }
}

void BrainBase::RequestViewportUpdate(RequestId requestId, bool full)
{
    mViewportUpdateRequests.push_back(requestId);
    mDoFullViewportUpdateNext = full || mViewportUpdateOverflowed;
    mViewportUpdateOverflowed = false;
    if (!mIsSimulationRunning) {
        mDoSimulationProgressNext = false;
        thisProxy[thisIndex].Simulate();
    }
}

void BrainBase::Simulate()
{
    mDoSimulationProgress = mDoSimulationProgressNext;
    mDoFullViewportUpdate = mDoFullViewportUpdateNext;
    mDoFullViewportUpdateNext = false;
    mDoViewportUpdate = mDoFullViewportUpdate || !mViewportUpdateOverflowed;
    mIsSimulationRunning = true;
    this->SimulateBrainControl();
}

void BrainBase::SimulateBrainControl()
{
    if (mBrain && mDoSimulationProgress) mBrain->Control(mBrainStep);
    this->SimulateBrainControlDone();
}

void BrainBase::SimulateBrainControlDone()
{
    this->SimulateAddRegions();
}

void BrainBase::SimulateAddRegions()
{
    if (!mRegionAdditions.empty()) {
        gRegions.beginInserting();
        for (auto it = mRegionAdditions.begin(); it != mRegionAdditions.end(); ++it) {
            mRegionIndices.insert(std::get<0>(*it));
            gRegions[std::get<0>(*it)].insert(
                std::get<1>(*it), std::get<2>(*it), GetBoxForRegion(std::get<0>(*it)), std::get<3>(*it));
        }
        gRegions.doneInserting();
    }

    this->SimulateAddRegionsDone();
}

void BrainBase::SimulateAddRegionsDone()
{
    this->SimulateRepositionRegions();
}

void BrainBase::SimulateRepositionRegions()
{
    if (!mRegionRepositions.empty()) {
        gCompletionDetector.start_detection(1, CkCallback(), CkCallback(),
            CkCallback(CkIndex_BrainBase::SimulateRepositionRegionsDone(), thisProxy[thisIndex]), 0);
        gCompletionDetector.ckLocalBranch()->produce(mRegionRepositions.size());

        for (auto it = mRegionRepositions.begin(); it != mRegionRepositions.end(); ++it) {
            gRegions[std::get<0>(*it)].SetBox(std::get<1>(*it));
        }

        gCompletionDetector.ckLocalBranch()->done();
    } else {
        this->SimulateRepositionRegionsDone();
    }
}

void BrainBase::SimulateRepositionRegionsDone()
{
    this->SimulateAddConnectors();
}

void BrainBase::SimulateAddConnectors()
{
    if (!mConnectorAdditions.empty()) {
        gCompletionDetector.start_detection(1, CkCallback(), CkCallback(),
            CkCallback(CkIndex_BrainBase::SimulateAddConnectorsDone(), thisProxy[thisIndex]), 0);
        gCompletionDetector.ckLocalBranch()->produce(mConnectorAdditions.size());

        for (auto it = mConnectorAdditions.begin(); it != mConnectorAdditions.end(); ++it) {
            if (std::get<1>(*it) == Direction::Forward) {
                gRegions[std::get<0>(*it)].CreateInput(
                    std::get<2>(*it), std::get<3>(*it), std::get<4>(*it), std::get<5>(*it));
            } else {
                gRegions[std::get<0>(*it)].CreateOutput(
                    std::get<2>(*it), std::get<3>(*it), std::get<4>(*it), std::get<5>(*it));
            }
        }

        gCompletionDetector.ckLocalBranch()->done();
    } else {
        this->SimulateAddConnectorsDone();
    }
}

void BrainBase::SimulateAddConnectorsDone()
{
    this->SimulateAddRemoveConnections();
}

void BrainBase::SimulateAddRemoveConnections()
{
    if (!mConnectionAdditions.empty() || !mConnectionRemovals.empty()) {
        std::unordered_set<RegionIndex> touchedRegions;
        for (auto it = mConnectionAdditions.begin(); it != mConnectionAdditions.end(); ++it) {
            touchedRegions.insert(std::get<1>(*it));
            touchedRegions.insert(std::get<3>(*it));
        }
        for (auto it = mConnectionRemovals.begin(); it != mConnectionRemovals.end(); ++it) {
            touchedRegions.insert(std::get<1>(*it));
            touchedRegions.insert(std::get<3>(*it));
        }
        touchedRegions.erase(BRAIN_REGION_INDEX);

        gCompletionDetector.start_detection(1 + touchedRegions.size(), CkCallback(), CkCallback(),
            CkCallback(CkIndex_BrainBase::SimulateAddRemoveConnectionsDone(), thisProxy[thisIndex]), 0);

        for (auto it = mConnectionAdditions.begin(); it != mConnectionAdditions.end(); ++it) {
            RegionIndex outIdx =
                std::get<0>(*it) == Direction::Forward ? std::get<1>(*it) : std::get<3>(*it);
            ConnectorName outName(
                std::get<0>(*it) == Direction::Forward ? std::get<2>(*it) : std::get<4>(*it));
            RegionIndex inIdx =
                std::get<0>(*it) == Direction::Forward ? std::get<3>(*it) : std::get<1>(*it);
            ConnectorName inName(
                std::get<0>(*it) == Direction::Forward ? std::get<4>(*it) : std::get<2>(*it));
            
            if (outIdx == BRAIN_REGION_INDEX) {
                this->ConnectTerminal(outName, RemoteConnector(inIdx, inName));
            } else {
                gCompletionDetector.ckLocalBranch()->produce();
                gRegions[outIdx].ConnectOutput(outName, RemoteConnector(inIdx, inName), true);
            }

            if (inIdx == BRAIN_REGION_INDEX) {
                this->ConnectTerminal(inName, RemoteConnector(outIdx, outName));
            } else {
                gCompletionDetector.ckLocalBranch()->produce();
                gRegions[inIdx].ConnectInput(inName, RemoteConnector(outIdx, outName), true);
            }
        }

        for (auto it = mConnectionRemovals.begin(); it != mConnectionRemovals.end(); ++it) {
            RegionIndex outIdx =
                std::get<0>(*it) == Direction::Forward ? std::get<1>(*it) : std::get<3>(*it);
            ConnectorName outName(
                std::get<0>(*it) == Direction::Forward ? std::get<2>(*it) : std::get<4>(*it));
            RegionIndex inIdx =
                std::get<0>(*it) == Direction::Forward ? std::get<3>(*it) : std::get<1>(*it);
            ConnectorName inName(
                std::get<0>(*it) == Direction::Forward ? std::get<4>(*it) : std::get<2>(*it));

            if (outIdx == BRAIN_REGION_INDEX) {
                this->DisconnectTerminal(outName, RemoteConnector(inIdx, inName));
            } else {
                gCompletionDetector.ckLocalBranch()->produce();
                gRegions[outIdx].DisconnectOutput(outName, RemoteConnector(inIdx, inName), true);
            }

            if (inIdx == BRAIN_REGION_INDEX) {
                this->DisconnectTerminal(inName, RemoteConnector(outIdx, outName));
            } else {
                gCompletionDetector.ckLocalBranch()->produce();
                gRegions[inIdx].DisconnectInput(inName, RemoteConnector(outIdx, outName), true);
            }
        }

        gCompletionDetector.ckLocalBranch()->done();
    } else {
        this->SimulateAddRemoveConnectionsDone();
    }
}

void BrainBase::SimulateAddRemoveConnectionsDone()
{
    this->SimulateRemoveConnectors();
}

void BrainBase::SimulateRemoveConnectors()
{
    if (!mConnectorRemovals.empty()) {
        std::unordered_set<RegionIndex> touchedRegions;
        for (auto it = mConnectorRemovals.begin(); it != mConnectorRemovals.end(); ++it) {
            touchedRegions.insert(std::get<0>(*it));
        }

        gCompletionDetector.start_detection(1 + touchedRegions.size(), CkCallback(), CkCallback(),
            CkCallback(CkIndex_BrainBase::SimulateRemoveConnectorsDone(), thisProxy[thisIndex]), 0);
        gCompletionDetector.ckLocalBranch()->produce(mConnectorRemovals.size());

        for (auto it = mConnectorRemovals.begin(); it != mConnectorRemovals.end(); ++it) {
            if (std::get<1>(*it) == Direction::Forward) {
                gRegions[std::get<0>(*it)].DeleteInput(std::get<2>(*it));
            } else {
                gRegions[std::get<0>(*it)].DeleteOutput(std::get<2>(*it));
            }

            for (auto itTerm = mTerminalNameToId.begin(); itTerm != mTerminalNameToId.end(); ++itTerm) {
                DisconnectTerminal(itTerm->first, RemoteConnector(std::get<0>(*it), std::get<2>(*it)));
            }
        }

        gCompletionDetector.ckLocalBranch()->done();
    } else {
        this->SimulateRemoveConnectorsDone();
    }
}

void BrainBase::SimulateRemoveConnectorsDone()
{
    this->SimulateRemoveRegions();
}

void BrainBase::SimulateRemoveRegions()
{
    if (!mRegionRemovals.empty()) {

        gCompletionDetector.start_detection(1 + mRegionRemovals.size(), CkCallback(), CkCallback(),
            CkCallback(CkIndex_BrainBase::SimulateRemoveRegionsDone(), thisProxy[thisIndex]), 0);
        gCompletionDetector.ckLocalBranch()->produce(mRegionRemovals.size());

        for (auto it = mRegionRemovals.begin(); it != mRegionRemovals.end(); ++it) {
            gRegions[*it].Unlink();
        }

        gCompletionDetector.ckLocalBranch()->done();
    } else {
        this->SimulateRemoveRegionsDone();
    }
}

void BrainBase::SimulateRemoveRegionsDone()
{
    if (!mRegionRemovals.empty()) {
        for (auto it = mRegionRemovals.begin(); it != mRegionRemovals.end(); ++it) {
            mRegionIndices.erase(*it);
            gRegions[*it].ckDestroy();
        }
    }

    this->SimulateRegionPrepareTopologyChange();
}

void BrainBase::SimulateRegionPrepareTopologyChange()
{
    if (!mRegionIndices.empty()) {
        gRegions.PrepareTopologyChange(mBrainStep, mDoSimulationProgress);
    } else {
        this->SimulateRegionPrepareTopologyChangeDone(0);
    }
}

void BrainBase::SimulateRegionPrepareTopologyChangeDone(size_t deletedNeurons)
{
    mDeletedNeurons = deletedNeurons;
    this->SimulateRegionCommitTopologyChange();
}

void BrainBase::SimulateRegionCommitTopologyChange()
{
    if (!mRegionIndices.empty()) {
        if (mDeletedNeurons > 0) {
            gCompletionDetector.start_detection(mRegionIndices.size() + mDeletedNeurons, CkCallback(), CkCallback(),
                CkCallback(CkIndex_BrainBase::SimulateAllTopologyChangesDelivered(), thisProxy[thisIndex]), 0);
        } else {
            this->SimulateAllTopologyChangesDelivered();
        }

        gRegions.CommitTopologyChange();
    } else {
        this->SimulateAllTopologyChangesDelivered();
        this->SimulateRegionCommitTopologyChangeDone(0);
    }
}

void BrainBase::SimulateRegionCommitTopologyChangeDone(size_t triggeredNeurons)
{
    mTriggeredNeurons = triggeredNeurons;
    mRegionCommitTopologyChangeDone = true;
    if (mRegionCommitTopologyChangeDone && mAllTopologyChangesDelivered) {
        this->SimulateBodySimulate();
    }
}

void BrainBase::SimulateAllTopologyChangesDelivered()
{
    mAllSpikesDelivered = true;
    if (mRegionCommitTopologyChangeDone && mAllTopologyChangesDelivered) {
        this->SimulateBodySimulate();
    }
}

void BrainBase::SimulateBodySimulate()
{
    if (mBrainStep % mBrainStepsPerBodyStep == 0 && mDoSimulationProgress) {

        ++mBodyStep;
        mBody->Simulate(
            std::bind(&BrainBase::PushSensoMotoricData, this, std::placeholders::_1, std::placeholders::_2),
            std::bind(&BrainBase::PullSensoMotoricData, this, std::placeholders::_1, std::placeholders::_2)
        );

        std::unordered_set<RegionIndex> touchedRegions;
        for (auto it = mTerminals.begin(); it != mTerminals.end(); ++it) {
            Terminal &terminal = it->second;
            if (!terminal.data.empty()) {
                for (auto itConn = terminal.connections.begin(); itConn != terminal.connections.end(); ++itConn) {
                    touchedRegions.insert(itConn->first);
                }
            }
        }

        if (!touchedRegions.empty()) {

            gCompletionDetector.start_detection(1 + touchedRegions.size(), CkCallback(), CkCallback(),
                CkCallback(CkIndex_BrainBase::SimulateBodySimulateDone(), thisProxy[thisIndex]), 0);

            std::unordered_set<RemoteConnector> touchedConnectors;
            for (auto it = mTerminals.begin(); it != mTerminals.end(); ++it) {

                Terminal &terminal = it->second;
                if (terminal.data.empty()) continue;

                size_t dataIdx = 0;
                Spike::BrainSource spikes(terminal.neuronCount);
                for (size_t i = 0; i < terminal.neuronCount; ++i) {
                    Spike::Data spike;
                    Spike::Initialize(terminal.spikeType, terminal.firstNeuron + i, spike);
                    size_t spikeByteCount = Spike::Edit(spike)->AllBytes(spike);
                    if (dataIdx + spikeByteCount <= terminal.data.size()) {
                        Spike::Edit(spike)->ImportAll(spike, terminal.data.data() + dataIdx, spikeByteCount);
                        dataIdx += spikeByteCount;
                        spikes.push_back(spike);
                    }
                }
                terminal.data.clear();

                Direction direction = terminal.isSensor ? Direction::Forward : Direction::Backward;
                for (auto itConn = terminal.connections.begin(); itConn != terminal.connections.end(); ++itConn) {
                    if (touchedConnectors.find(*itConn) == touchedConnectors.end()) {
                        touchedConnectors.insert(*itConn);
                        mTriggeredNeurons += spikes.size();
                    }
                    gCompletionDetector.ckLocalBranch()->produce();
                    gRegions[itConn->first].ReceiveSensoMotoricData(direction, itConn->second, spikes);
                }
            }

            gCompletionDetector.ckLocalBranch()->done();

        } else {
            this->SimulateBodySimulateDone();
        }

    } else {
        this->SimulateBodySimulateDone();
    }
}

void BrainBase::SimulateBodySimulateDone()
{
    this->SimulateRegionSimulate();
}

void BrainBase::SimulateRegionSimulate()
{
    if (mDoSimulationProgress) {
        if (mTriggeredNeurons > 0) {
            gCompletionDetector.start_detection(mTriggeredNeurons, CkCallback(), CkCallback(),
                CkCallback(CkIndex_BrainBase::SimulateAllSpikesDelivered(), thisProxy[thisIndex]), 0);
        } else {
            this->SimulateAllSpikesDelivered();
        }
    } else {
        this->SimulateAllSpikesDelivered();
    }

    if (!mRegionIndices.empty()) {
        SimulateMsg *simulateMsg = new SimulateMsg();
        simulateMsg->doUpdate = mDoViewportUpdate;
        simulateMsg->doFullUpdate = mDoFullViewportUpdate;
        simulateMsg->doProgress = mDoSimulationProgress;
        simulateMsg->brainStep = mBrainStep;
        simulateMsg->roiBoxes = mRoiBoxes;

        gRegions.Simulate(simulateMsg);
    } else {
        this->SimulateRegionSimulateDone(nullptr);
    }
}

void BrainBase::SimulateRegionSimulateDone(CkReductionMsg *msg)
{
    ViewportUpdate fullViewportUpdateAccumulator;

    if (msg) {
        CkReduction::setElement *regionResult = static_cast<CkReduction::setElement *>(msg->getData());
        while (regionResult != nullptr) {
            uint8_t *regionResultPtr = reinterpret_cast<uint8_t *>(&regionResult->data);

            PUP::fromMem p(static_cast<void *>(regionResultPtr));

            RegionIndex regionIndex; p | regionIndex;

            Spike::BrainSink brainSink; p | brainSink;
            ReceiveTerminalData(brainSink);

            uint8_t *regionContributionPtr = nullptr;
            size_t regionContributionSize = 0;
            p(regionContributionPtr, regionContributionSize);
            if (mBrain && regionContributionSize > 0) {
                mBrain->AcceptContributionFromRegion(regionIndex,
                    regionContributionPtr, regionContributionSize);
            }

            ViewportUpdate *accum = mDoFullViewportUpdate ?
                &fullViewportUpdateAccumulator : &mViewportUpdateAccumulator;

            if (mDoViewportUpdate) {

                if (mDoFullViewportUpdate) {

                    RegionAdditionReports tmpAddedRegions; p | tmpAddedRegions;
                    accum->addedRegions.reserve(accum->addedRegions.size() + tmpAddedRegions.size());
                    accum->addedRegions.insert(accum->addedRegions.begin(),
                        tmpAddedRegions.begin(), tmpAddedRegions.end());

                    ConnectorAdditionReports tmpAddedConnectors; p | tmpAddedConnectors;
                    accum->addedConnectors.reserve(accum->addedConnectors.size() + tmpAddedConnectors.size());
                    accum->addedConnectors.insert(accum->addedConnectors.begin(),
                        tmpAddedConnectors.begin(), tmpAddedConnectors.end());

                    Connections tmpAddedConnections; p | tmpAddedConnections;
                    accum->addedConnections.reserve(accum->addedConnections.size() + tmpAddedConnections.size());
                    accum->addedConnections.insert(accum->addedConnections.begin(),
                        tmpAddedConnections.begin(), tmpAddedConnections.end());

                } else {

                    accum->addedRegions.reserve(accum->addedRegions.size() + mRegionAdditions.size());
                    for (auto it = mRegionAdditions.begin(); it != mRegionAdditions.end(); ++it) {
                        accum->addedRegions.push_back(RegionAdditionReport(
                            std::get<0>(*it), std::get<1>(*it), std::get<2>(*it), GetBoxForRegion(std::get<0>(*it))));
                    }

                    accum->removedRegions.reserve(accum->removedRegions.size() + mRegionRemovals.size());
                    accum->removedRegions.insert(accum->removedRegions.begin(),
                        mRegionRemovals.begin(), mRegionRemovals.end());

                    accum->addedConnectors.reserve(accum->addedConnectors.size() + mConnectorAdditions.size());
                    for (auto it = mConnectorAdditions.begin(); it != mConnectorAdditions.end(); ++it) {
                        accum->addedConnectors.push_back(ConnectorAdditionReport(
                            std::get<0>(*it), std::get<1>(*it), std::get<2>(*it), std::get<5>(*it)));
                    }

                    accum->removedConnectors.reserve(accum->removedConnectors.size() + mConnectorRemovals.size());
                    accum->removedConnectors.insert(accum->removedConnectors.begin(),
                        mConnectorRemovals.begin(), mConnectorRemovals.end());

                    accum->addedConnections.reserve(accum->addedConnections.size() + mConnectionAdditions.size());
                    accum->addedConnections.insert(accum->addedConnections.begin(),
                        mConnectionAdditions.begin(), mConnectionAdditions.end());

                    accum->removedConnections.reserve(accum->removedConnections.size() + mConnectionRemovals.size());
                    accum->removedConnections.insert(accum->removedConnections.begin(),
                        mConnectionRemovals.begin(), mConnectionRemovals.end());
                }

                RegionAdditionReports tmpRepositionedRegions; p | tmpRepositionedRegions;
                accum->repositionedRegions.reserve(accum->repositionedRegions.size() + tmpRepositionedRegions.size());
                accum->repositionedRegions.insert(accum->repositionedRegions.begin(),
                    tmpRepositionedRegions.begin(), tmpRepositionedRegions.end());

                NeuronAdditionReports tmpAddedNeurons; p | tmpAddedNeurons;
                accum->addedNeurons.reserve(accum->addedNeurons.size() + tmpAddedNeurons.size());
                accum->addedNeurons.insert(accum->addedNeurons.begin(),
                    tmpAddedNeurons.begin(), tmpAddedNeurons.end());

                NeuronAdditionReports tmpRepositionedNeurons; p | tmpRepositionedNeurons;
                accum->repositionedNeurons.reserve(accum->repositionedNeurons.size() + tmpRepositionedNeurons.size());
                accum->repositionedNeurons.insert(accum->repositionedNeurons.begin(),
                    tmpRepositionedNeurons.begin(), tmpRepositionedNeurons.end());

                NeuronRemovals tmpRemovedNeurons; p | tmpRemovedNeurons;
                accum->removedNeurons.reserve(accum->removedNeurons.size() + tmpRemovedNeurons.size());
                accum->removedNeurons.insert(accum->removedNeurons.begin(),
                    tmpRemovedNeurons.begin(), tmpRemovedNeurons.end());

                Synapse::Links tmpAddedSynapses; p | tmpAddedSynapses;
                accum->addedSynapses.reserve(accum->addedSynapses.size() + tmpAddedSynapses.size());
                accum->addedSynapses.insert(accum->addedSynapses.begin(),
                    tmpAddedSynapses.begin(), tmpAddedSynapses.end());

                Synapse::Links tmpSpikedSynapses; p | tmpSpikedSynapses;
                accum->spikedSynapses.reserve(accum->spikedSynapses.size() + tmpSpikedSynapses.size());
                accum->spikedSynapses.insert(accum->spikedSynapses.begin(),
                    tmpSpikedSynapses.begin(), tmpSpikedSynapses.end());

                Synapse::Links tmpRemovedSynapses; p | tmpRemovedSynapses;
                accum->removedSynapses.reserve(accum->removedSynapses.size() + tmpRemovedSynapses.size());
                accum->removedSynapses.insert(accum->removedSynapses.begin(),
                    tmpRemovedSynapses.begin(), tmpRemovedSynapses.end());

                ChildLinks tmpAddedChildren; p | tmpAddedChildren;
                accum->addedChildren.reserve(accum->addedChildren.size() + tmpAddedChildren.size());
                accum->addedChildren.insert(accum->addedChildren.begin(),
                    tmpAddedChildren.begin(), tmpAddedChildren.end());

                ChildLinks tmpRemovedChildren; p | tmpRemovedChildren;
                accum->removedChildren.reserve(accum->removedChildren.size() + tmpRemovedChildren.size());
                accum->removedChildren.insert(accum->removedChildren.begin(),
                    tmpRemovedChildren.begin(), tmpRemovedChildren.end());
            }

            regionResult = regionResult->next();
        }

        delete msg;
    }

    if (mDoFullViewportUpdate) {
        fullViewportUpdateAccumulator.isFull = true;
        fullViewportUpdateAccumulator.sinceBrainStep = mBrainStep;
        fullViewportUpdateAccumulator.brainStepCount = 1;
        mViewportUpdateAccumulator = std::move(fullViewportUpdateAccumulator);
    } else {
        if (mBrainStep == 0) {
            fullViewportUpdateAccumulator.isFull = true;
        }
        if (mDoSimulationProgress) {
            ++mViewportUpdateAccumulator.brainStepCount;
        }
    }

    bool flushViewportAccumulator = false;;
    bool cannotRespond = mViewportUpdateRequests.empty() || 
        (!mDoFullViewportUpdate && mDoFullViewportUpdateNext);

    if (cannotRespond) {
        if (mViewportUpdateAccumulator.brainStepCount > mBrainStepsPerBodyStep) {
            mViewportUpdateOverflowed = true;
            flushViewportAccumulator = true;
        }
    } else {
        RequestId requestId = mViewportUpdateRequests.front();
        mViewportUpdateRequests.pop_front();

        gCore.ckLocal()->SendViewportUpdate(requestId, mViewportUpdateAccumulator);
        flushViewportAccumulator = true;
    }

    if (flushViewportAccumulator) {
        mViewportUpdateAccumulator = ViewportUpdate();
        mViewportUpdateAccumulator.isFull = false;
        mViewportUpdateAccumulator.sinceBrainStep = mBrainStep;
        mViewportUpdateAccumulator.brainStepCount = 0;
    }

    mRegionAdditions.clear();
    mRegionRepositions.clear();
    mRegionRemovals.clear();
    mConnectorAdditions.clear();
    mConnectorRemovals.clear();
    mConnectionAdditions.clear();
    mConnectionRemovals.clear();

    mRegionSimulateDone = true;
    if (mRegionSimulateDone && mAllSpikesDelivered) {
        this->SimulateDone();
    }
}

void BrainBase::SimulateAllSpikesDelivered()
{
    mAllSpikesDelivered = true;
    if (mRegionSimulateDone && mAllSpikesDelivered) {
        this->SimulateDone();
    }
}

void BrainBase::SimulateDone()
{
    mRegionSimulateDone = false;
    mAllSpikesDelivered = false;
    
    if (mDoSimulationProgress) {
        ++mBrainStep;
        if (mBrainStepsToRun != SIZE_MAX) {
            --mBrainStepsToRun;
        }
        if (mBrainStepsToRun > 0) {
            thisProxy[thisIndex].Simulate();
        } else {
            mIsSimulationRunning = false;
        }
    } else {
        mIsSimulationRunning = false;
    }

    if (!mSimulationStateRequests.empty()) {
        RequestId requestId = mSimulationStateRequests.front();
        mSimulationStateRequests.pop_front();
        gCore.ckLocal()->SendSimulationState(requestId, mIsSimulationRunning, 
            mBrainStep, mBodyStep, mBrainStepsPerBodyStep);
    }
}

const char *ThresholdBrain::Type = "ThresholdBrain";

ThresholdBrain::ThresholdBrain(BrainBase &base, json &params) : Brain(base, params)
{
    // TODO
}

ThresholdBrain::~ThresholdBrain()
{
}

void ThresholdBrain::pup(PUP::er &p)
{
}

const char *ThresholdBrain::GetType() const
{
    return Type;
}

void ThresholdBrain::Control(size_t brainStep)
{
    // TODO
}

void ThresholdBrain::AcceptContributionFromRegion(
    RegionIndex regIdx, const uint8_t *contribution, size_t size)
{
    // TODO
}

#include "brain.def.h"
