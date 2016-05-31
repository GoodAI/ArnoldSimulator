#include "random.h"
#include "region.h"

#include "brain.h"

extern CkGroupID gMulticastGroupId;
extern CProxy_CompletionDetector gCompletionDetector;

extern CProxy_Core gCore;
extern CProxy_BrainBase gBrain;
extern CProxy_RegionBase gRegions;
extern CProxy_NeuronBase gNeurons;

RegionMap::RegionMap()
{
}

RegionMap::RegionMap(CkMigrateMessage *msg)
{
}

int RegionMap::procNum(int arrayHdl, const CkArrayIndex &index)
{
    int regionIndex = index.data()[0];

    int nodeNum = regionIndex % CkNumNodes();
    int rankNum = regionIndex % CkNodeSize(nodeNum);
    int peNum = rankNum + CkNodeFirst(nodeNum);

    return peNum;
}

Region::Region(RegionBase &base, json &params) : mBase(base)
{
}

void RegionBase::Connector::pup(PUP::er &p)
{
    p | name;
    p | neurons;

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

Region *RegionBase::CreateRegion(const RegionType &type, RegionBase &base, json &params)
{
    if (type == ThresholdRegion::Type) {
        return new ThresholdRegion(base, params);
    } else {
        return nullptr;
    }
}

RegionBase::RegionBase(const RegionName &name, const RegionType &type, const Box3D &box, const RegionParams &params) :
    mName(name), mBoxChanged(false), mPosition(box.first), mSize(box.second),
    mUnlinking(false), mDoUpdate(false), mDoFullUpdate(false), mDoProgress(false), mBrainStep(0),
    mNeuronIdxCounter(NEURON_INDEX_MIN), mNeuronSectionFilled(false), mRegion(nullptr)
{
    json p = json::parse(params);

    auto engine = Random::GetThreadEngine();
    std::unordered_map<std::string, std::vector<NeuronId>> clusters;

    if (!p.empty()) {
        for (auto itParams = p.begin(); itParams != p.end(); ++itParams) {

            if (itParams.key() == "clusters" && itParams->is_array()) {

                for (auto itCluster = itParams.value().begin(); itCluster != itParams.value().end(); ++itCluster) {
                    if (itCluster->is_object()) {

                        std::string clusterName, neuronType, neuronParams, synapseType;
                        size_t neuronCount = 0;
                        size_t synapseCount = 0;
                        for (auto it = itCluster->begin(); it != itCluster->end(); ++it) {
                            if (it.key() == "name" && it.value().is_string()) {
                                clusterName = it.value().get<std::string>();
                            } else if (it.key() == "neuronType" && it.value().is_string()) {
                                neuronType = it.value().get<std::string>();
                            } else if (it.key() == "neuronParams" && it.value().is_object()) {
                                neuronParams = it.value().dump();
                            } else if (it.key() == "neuronCount" && it.value().is_number_integer()) {
                                neuronCount = it.value().get<size_t>();
                            } else if (it.key() == "synapseType" && it.value().is_string()) {
                                synapseType = it.value().get<std::string>();
                            } else if (it.key() == "synapseCount" && it.value().is_number_integer()) {
                                synapseCount = it.value().get<size_t>();
                            }
                        }

                        if (!clusterName.empty() && neuronCount > 0) {
                            std::vector<NeuronId> cluster;
                            cluster.reserve(neuronCount);
                            std::uniform_int_distribution<size_t> randClusterIdx(0, neuronCount - 1);
                            for (size_t i = 0; i < neuronCount; ++i) {
                                cluster.push_back(RequestNeuronAddition(neuronType, neuronParams));
                            }
                            for (size_t i = 0; i < synapseCount; ++i) {
                                Synapse::Data synapse;
                                Synapse::Initialize(Synapse::ParseType(synapseType), synapse);
                                NeuronId from = cluster.at(randClusterIdx(engine));
                                NeuronId to = cluster.at(randClusterIdx(engine));
                                RequestSynapseAddition(Direction::Forward, from, to, synapse);
                            }
                        }
                    }
                }

            } else if (itParams.key() == "webs" && itParams->is_array()) {

                for (auto itActuator = itParams.value().begin(); itActuator != itParams.value().end(); ++itActuator) {
                    if (itActuator->is_object()) {

                        std::string fromCluster, toCluster, synapseType;
                        size_t synapseCount = 0;
                        for (auto it = itActuator->begin(); it != itActuator->end(); ++it) {
                            if (it.key() == "from" && it.value().is_string()) {
                                fromCluster = it.value().get<std::string>();
                            } else if (it.key() == "to" && it.value().is_string()) {
                                toCluster = it.value().get<std::string>();
                            } else if (it.key() == "synapseType" && it.value().is_string()) {
                                synapseType = it.value().get<std::string>();
                            } else if (it.key() == "synapseCount" && it.value().is_number_integer()) {
                                synapseCount = it.value().get<size_t>();
                            }
                        }

                        if (!fromCluster.empty() && !toCluster.empty()) {
                            
                            Direction srcDirection = Direction::Forward;
                            Direction dstDirection = Direction::Forward;
                            std::vector<NeuronId> *srcCluster = nullptr;
                            std::vector<NeuronId> *dstCluster = nullptr;

                            if (clusters.find(fromCluster) != clusters.end()) {
                                srcCluster = &clusters[fromCluster];
                            } else if (mInputConnectors.find(fromCluster) != mInputConnectors.end()) {
                                srcCluster = &mInputConnectors[fromCluster].neurons;
                            } else if (mOutputConnectors.find(fromCluster) != mOutputConnectors.end()) {
                                srcDirection = Direction::Backward;
                                srcCluster = &mOutputConnectors[fromCluster].neurons;
                            }
                            
                            if (clusters.find(toCluster) != clusters.end()) {
                                dstCluster = &clusters[toCluster];
                            } else if (mInputConnectors.find(toCluster) != mInputConnectors.end()) {
                                dstDirection = Direction::Backward;
                                dstCluster = &mInputConnectors[toCluster].neurons;
                            } else if (mOutputConnectors.find(toCluster) != mOutputConnectors.end()) {
                                dstCluster = &mOutputConnectors[toCluster].neurons;
                            }

                            if (srcCluster && dstCluster && srcDirection == dstDirection) {
                                std::uniform_int_distribution<size_t> randSrcClusterIdx(0, srcCluster->size() - 1);
                                std::uniform_int_distribution<size_t> randDstClusterIdx(0, dstCluster->size() - 1);
                                for (size_t i = 0; i < synapseCount; ++i) {
                                    Synapse::Data synapse;
                                    Synapse::Initialize(Synapse::ParseType(synapseType), synapse);
                                    NeuronId from = srcCluster->at(randSrcClusterIdx(engine));
                                    NeuronId to = dstCluster->at(randDstClusterIdx(engine));
                                    RequestSynapseAddition(srcDirection, from, to, synapse);
                                }
                            }
                        }
                    }
                }

            }
        }
    }

    mRegion = RegionBase::CreateRegion(type, *this, p);
}

RegionBase::RegionBase(CkMigrateMessage *msg) :
    mBoxChanged(false), mUnlinking(false), mDoUpdate(false), mDoFullUpdate(false), mDoProgress(false),
    mBrainStep(0), mNeuronIdxCounter(0), mNeuronSectionFilled(false), mRegion(nullptr)
{
}

RegionBase::~RegionBase()
{
    if (mRegion) delete mRegion;
}

void RegionBase::pup(PUP::er &p)
{
    p | mName;
    p | mBoxChanged;
    p | mPosition;
    p | mSize;

    p | mUnlinking;

    p | mDoUpdate;
    p | mDoFullUpdate;
    p | mDoProgress;
    p | mBrainStep;
    p | mRoiBoxes;

    p | mNeuronIdxCounter;

    p | mNeuronAdditions;
    p | mNeuronRemovals;
    p | mSynapseAdditions;
    p | mSynapseRemovals;
    p | mChildAdditions;
    p | mChildRemovals;

    p | mBrainSink;

    if (p.isUnpacking()) {
        size_t neuronIndicesCount; p | neuronIndicesCount;
        mNeuronIndices.reserve(neuronIndicesCount);
        for (size_t i = 0; i < neuronIndicesCount; ++i) {
            NeuronIndex index; p | index;
            mNeuronIndices.insert(index);
        }

        size_t inputConnectorsCount; p | inputConnectorsCount;
        mInputConnectors.reserve(inputConnectorsCount);
        for (size_t i = 0; i < inputConnectorsCount; ++i) {
            Connector connector; p | connector;
            mInputConnectors.insert(std::make_pair(connector.name, connector));
        }

        size_t outputConnectorsCount; p | outputConnectorsCount;
        mOutputConnectors.reserve(outputConnectorsCount);
        for (size_t i = 0; i < outputConnectorsCount; ++i) {
            Connector connector; p | connector;
            mOutputConnectors.insert(std::make_pair(connector.name, connector));
        }

        size_t triggeredCount; p | triggeredCount;
        mNeuronsTriggered.reserve(triggeredCount);
        for (size_t i = 0; i < triggeredCount; ++i) {
            NeuronId triggered; p | triggered;
            mNeuronsTriggered.insert(triggered);
        }

        json regionParams;
        RegionType regionType;
        p | regionType;
        mRegion = CreateRegion(regionType, *this, regionParams);
        if (mRegion) mRegion->pup(p);
    } else {
        size_t neuronIndicesCount = mNeuronIndices.size(); p | neuronIndicesCount;
        for (auto it = mNeuronIndices.begin(); it != mNeuronIndices.end(); ++it) {
            NeuronIndex index = *it; p | index;
        }

        size_t inputConnectorsCount = mInputConnectors.size(); p | inputConnectorsCount;
        for (auto it = mInputConnectors.begin(); it != mInputConnectors.end(); ++it) {
            Connector connector = it->second; p | connector;
        }

        size_t outputConnectorsCount = mOutputConnectors.size(); p | outputConnectorsCount;
        for (auto it = mOutputConnectors.begin(); it != mOutputConnectors.end(); ++it) {
            Connector connector = it->second; p | connector;
        }

        size_t triggeredCount = mNeuronsTriggered.size(); p | triggeredCount;
        for (auto it = mNeuronsTriggered.begin(); it != mNeuronsTriggered.end(); ++it) {
            NeuronId triggered = *it; p | triggered;
        }

        RegionType regionType;
        if (mRegion) regionType = mRegion->GetType();
        p | regionType;
        if (mRegion) mRegion->pup(p);
    }
}

const char *RegionBase::GetType() const
{
    if (mRegion) {
        return mRegion->GetType();
    } else {
        return "";
    }
}

const char *RegionBase::GetName() const
{
    return mName.c_str();
}

RegionIndex RegionBase::GetIndex() const
{
    return thisIndex;
}

NeuronIndex RegionBase::GetNewNeuronIndex()
{
    if (mNeuronIdxCounter == NEURON_INDEX_MAX) CkAbort("Neuron indices depleted.");
    return mNeuronIdxCounter++;
}

const RegionBase::Connectors &RegionBase::GetInputs() const
{
    return mInputConnectors;
}

const RegionBase::Connector &RegionBase::GetInput(const ConnectorName &name) const
{
    return mInputConnectors.at(name);
}

const RegionBase::Connectors &RegionBase::GetOutputs() const
{
    return mOutputConnectors;
}

const RegionBase::Connector &RegionBase::GetOutput(const ConnectorName &name) const
{
    return mOutputConnectors.at(name);
}

const NeuronAdditionRequests &RegionBase::GetNeuronAdditions() const
{
    return mNeuronAdditions;
}

const NeuronRemovals &RegionBase::GetNeuronRemovals() const
{
    return mNeuronRemovals;
}

const Synapse::Additions &RegionBase::GetSynapseAdditions() const
{
    return mSynapseAdditions;
}

const Synapse::Removals &RegionBase::GetSynapseRemovals() const
{
    return mSynapseRemovals;
}

const ChildLinks &RegionBase::GetChildAdditions() const
{
    return mChildAdditions;
}

const ChildLinks &RegionBase::GetChildRemovals() const
{
    return mChildRemovals;
}

const NeuronsTriggered &RegionBase::GetTriggeredNeurons() const
{
    return mNeuronsTriggered;
}

void RegionBase::TriggerNeurons(const NeuronsTriggered &neurons)
{
    mNeuronsTriggered.insert(neurons.begin(), neurons.end());
}

void RegionBase::UntriggerNeurons(const NeuronsTriggered &neurons)
{
    for (auto it = neurons.begin(); it != neurons.end(); ++it) {
        mNeuronsTriggered.erase(*it);
    }
}

NeuronId RegionBase::RequestNeuronAddition(const NeuronType &type, const NeuronParams &params)
{
    NeuronId neuronId = GetNeuronId(thisIndex, GetNewNeuronIndex());
    mNeuronAdditions.push_back(std::make_tuple(neuronId, type, params));
    return neuronId;
}

void RegionBase::RequestNeuronRemoval(NeuronId neuronId)
{
    mNeuronRemovals.push_back(neuronId);
}

void RegionBase::RequestSynapseAddition(Direction direction, NeuronId from, NeuronId to, const Synapse::Data &data)
{
    mSynapseAdditions.push_back(std::make_tuple(direction, from, to, data));
}

void RegionBase::RequestSynapseRemoval(Direction direction, NeuronId from, NeuronId to)
{
    mSynapseRemovals.push_back(std::make_tuple(direction, from, to));
}

void RegionBase::RequestChildAddition(NeuronId parent, NeuronId child)
{
    mChildAdditions.push_back(std::make_pair(parent, child));
}

void RegionBase::RequestChildRemoval(NeuronId parent, NeuronId child)
{
    mChildRemovals.push_back(std::make_pair(parent, child));
}

void RegionBase::CreateInput(const ConnectorName &name,
    const NeuronType &neuronType, const NeuronParams &neuronParams, size_t neuronCount)
{
    Connector connector;
    connector.name = name;
    for (size_t i = 0; i < neuronCount; ++i) {
        connector.neurons.push_back(
            RequestNeuronAddition(neuronType, neuronParams));
    }
    mInputConnectors.insert(std::make_pair(name, connector));

    gCompletionDetector.ckLocalBranch()->consume();
}

void RegionBase::DeleteInput(const ConnectorName &name)
{
    auto itConn = mInputConnectors.find(name);
    if (itConn != mInputConnectors.end()) {
        Connector &connector = itConn->second;

        for (auto it = connector.connections.begin(); it != connector.connections.end(); ++it) {
            if (it->first != BRAIN_REGION_INDEX) {
                gCompletionDetector.ckLocalBranch()->produce();
                gRegions[it->first].DisconnectOutput(it->second, RemoteConnector(thisIndex, name), false);
                if (!connector.neurons.empty()) {
                    gCompletionDetector.ckLocalBranch()->produce();
                    gRegions[it->first].DisconnectOutputNeurons(
                        it->second, connector.neurons.at(0));
                }
            }
        }

        for (auto it = connector.neurons.begin(); it != connector.neurons.end(); ++it) {
            RequestNeuronRemoval(*it);
        }

        mInputConnectors.erase(name);
    }

    if (!mUnlinking) {
        gCompletionDetector.ckLocalBranch()->done();
        gCompletionDetector.ckLocalBranch()->consume();
    }
}

void RegionBase::ConnectInput(const ConnectorName &name, const RemoteConnector &destination, bool syncSynapses)
{
    auto itConn = mInputConnectors.find(name);
    if (itConn != mInputConnectors.end()) {
        Connector &connector = itConn->second;
        connector.connections.insert(destination);
        if (syncSynapses && !connector.neurons.empty()) {
            if (destination.first != BRAIN_REGION_INDEX) {
                gCompletionDetector.ckLocalBranch()->produce();
                gRegions[destination.first].ConnectOutputNeurons(
                    destination.second, connector.neurons.at(0));
            }
            gCompletionDetector.ckLocalBranch()->done();
        }
    }

    gCompletionDetector.ckLocalBranch()->consume();
}

void RegionBase::DisconnectInput(const ConnectorName &name, const RemoteConnector &destination, bool syncSynapses)
{
    auto itConn = mInputConnectors.find(name);
    if (itConn != mInputConnectors.end()) {
        Connector &connector = itConn->second;
        connector.connections.erase(destination);
        if (syncSynapses && !connector.neurons.empty()) {
            if (destination.first != BRAIN_REGION_INDEX) {
                gCompletionDetector.ckLocalBranch()->produce();
                gRegions[destination.first].DisconnectOutputNeurons(
                    destination.second, connector.neurons.at(0));
            }
            gCompletionDetector.ckLocalBranch()->done();
        }
    }

    gCompletionDetector.ckLocalBranch()->consume();
}

void RegionBase::ConnectInputNeurons(const ConnectorName &name, NeuronId destFirstNeuron)
{
    auto itConn = mInputConnectors.find(name);
    if (itConn != mInputConnectors.end()) {
        Connector &connector = itConn->second;
        Synapse::Data synapse;
        Synapse::Initialize(Synapse::Type::Weighted, synapse);
        NeuronId destNeuronId = destFirstNeuron;
        for (auto it = connector.neurons.begin(); it != connector.neurons.end(); ++it) {
            RequestSynapseAddition(Direction::Forward, destNeuronId++, *it, synapse);
        }
    }

    gCompletionDetector.ckLocalBranch()->consume();
}

void RegionBase::DisconnectInputNeurons(const ConnectorName &name, NeuronId destFirstNeuron)
{
    auto itConn = mInputConnectors.find(name);
    if (itConn != mInputConnectors.end()) {
        Connector &connector = itConn->second;
        NeuronId destNeuronId = destFirstNeuron;
        for (auto it = connector.neurons.begin(); it != connector.neurons.end(); ++it) {
            RequestSynapseRemoval(Direction::Forward, destNeuronId++, *it);
        }
    }

    gCompletionDetector.ckLocalBranch()->consume();
}

void RegionBase::CreateOutput(const ConnectorName &name,
    const NeuronType &neuronType, const NeuronParams &neuronParams, size_t neuronCount)
{
    Connector connector;
    connector.name = name;
    for (size_t i = 0; i < neuronCount; ++i) {
        connector.neurons.push_back(
            RequestNeuronAddition(neuronType, neuronParams));
    }
    mOutputConnectors.insert(std::make_pair(name, connector));

    gCompletionDetector.ckLocalBranch()->consume();
}

void RegionBase::DeleteOutput(const ConnectorName &name)
{
    auto itConn = mInputConnectors.find(name);
    if (itConn != mInputConnectors.end()) {
        Connector &connector = itConn->second;

        for (auto it = connector.connections.begin(); it != connector.connections.end(); ++it) {
            if (it->first != BRAIN_REGION_INDEX) {
                gCompletionDetector.ckLocalBranch()->produce();
                gRegions[it->first].DisconnectInput(it->second, RemoteConnector(thisIndex, name), false);
                if (!connector.neurons.empty()) {
                    gCompletionDetector.ckLocalBranch()->produce();
                    gRegions[it->first].DisconnectInputNeurons(
                        it->second, connector.neurons.at(0));
                }
            }
        }

        for (auto it = connector.neurons.begin(); it != connector.neurons.end(); ++it) {
            RequestNeuronRemoval(*it);
        }

        mInputConnectors.erase(name);
    }

    if (!mUnlinking) {
        gCompletionDetector.ckLocalBranch()->done();
        gCompletionDetector.ckLocalBranch()->consume();
    }
}

void RegionBase::ConnectOutput(const ConnectorName &name, const RemoteConnector &destination, bool syncSynapses)
{
    auto itConn = mOutputConnectors.find(name);
    if (itConn != mOutputConnectors.end()) {
        Connector &connector = itConn->second;
        connector.connections.insert(destination);
        if (syncSynapses && !connector.neurons.empty()) {
            if (destination.first != BRAIN_REGION_INDEX) {
                gCompletionDetector.ckLocalBranch()->produce();
                gRegions[destination.first].ConnectInputNeurons(
                    destination.second, connector.neurons.at(0));
            }
            gCompletionDetector.ckLocalBranch()->done();
        }
    }

    gCompletionDetector.ckLocalBranch()->consume();
}

void RegionBase::DisconnectOutput(const ConnectorName &name, const RemoteConnector &destination, bool syncSynapses)
{
    auto itConn = mOutputConnectors.find(name);
    if (itConn != mOutputConnectors.end()) {
        Connector &connector = itConn->second;
        connector.connections.erase(destination);
        if (syncSynapses && !connector.neurons.empty()) {
            if (destination.first != BRAIN_REGION_INDEX) {
                gCompletionDetector.ckLocalBranch()->produce();
                gRegions[destination.first].DisconnectInputNeurons(
                    destination.second, connector.neurons.at(0));
            }
            gCompletionDetector.ckLocalBranch()->done();
        }
    }

    gCompletionDetector.ckLocalBranch()->consume();
}

void RegionBase::ConnectOutputNeurons(const ConnectorName &name, NeuronId destFirstNeuron)
{
    auto itConn = mOutputConnectors.find(name);
    if (itConn != mOutputConnectors.end()) {
        Connector &connector = itConn->second;
        Synapse::Data synapse;
        Synapse::Initialize(Synapse::Type::Weighted, synapse);
        NeuronId destNeuronId = destFirstNeuron;
        for (auto it = connector.neurons.begin(); it != connector.neurons.end(); ++it) {
            RequestSynapseAddition(Direction::Forward, *it, destNeuronId++, synapse);
        }
    }

    gCompletionDetector.ckLocalBranch()->consume();
}

void RegionBase::DisconnectOutputNeurons(const ConnectorName &name, NeuronId destFirstNeuron)
{
    auto itConn = mOutputConnectors.find(name);
    if (itConn != mOutputConnectors.end()) {
        Connector &connector = itConn->second;
        NeuronId destNeuronId = destFirstNeuron;
        for (auto it = connector.neurons.begin(); it != connector.neurons.end(); ++it) {
            RequestSynapseRemoval(Direction::Forward, *it, destNeuronId++);
        }
    }

    gCompletionDetector.ckLocalBranch()->consume();
}

void RegionBase::ReceiveSensoMotoricData(Direction direction, const ConnectorName &connectorName, Spike::BrainSource &data)
{
    Connector *connector = nullptr;
    
    if (direction == Direction::Forward) {
        auto itConn = mInputConnectors.find(connectorName);
        if (itConn != mInputConnectors.end()) {
            connector = &itConn->second;
        }
    } else {
        auto itConn = mOutputConnectors.find(connectorName);
        if (itConn != mOutputConnectors.end()) {
            connector = &itConn->second;
        }
    }

    if (connector) {
        if (data.size() == connector->neurons.size()) {
            gCompletionDetector.ckLocalBranch()->produce(data.size());
            for (size_t i = 0; i < data.size(); ++i) {
                NeuronId neuronId = connector->neurons[i];
                gNeurons(GetRegionIndex(neuronId), GetNeuronIndex(neuronId)).EnqueueSpike(direction, data[i]);
            }
        }
    }

    gCompletionDetector.ckLocalBranch()->done();
    gCompletionDetector.ckLocalBranch()->consume();
}

void RegionBase::EnqueueSensoMotoricSpike(NeuronId receiver, const Spike::Data &data)
{
    mBrainSink.push_back(std::make_pair(receiver, data));
    gCompletionDetector.ckLocalBranch()->consume();
}

void RegionBase::SetBox(Box3D &box)
{
    mBoxChanged = true;
    mPosition = box.first;
    mSize = box.second;
    gCompletionDetector.ckLocalBranch()->consume();
}

void RegionBase::Unlink()
{
    mUnlinking = true;
    while (!mInputConnectors.empty()) {
        DeleteInput(mInputConnectors.begin()->first);
    }
    while (!mOutputConnectors.empty()) {
        DeleteOutput(mOutputConnectors.begin()->first);
    }
    mUnlinking = false;

    gCompletionDetector.ckLocalBranch()->done();
    gCompletionDetector.ckLocalBranch()->consume();

    for (auto it = mNeuronIndices.begin(); it != mNeuronIndices.end(); ++it) {
        gNeurons(thisIndex, *it).ckDestroy();
    }
    mNeuronIndices.clear();
}

void RegionBase::PrepareTopologyChange(size_t brainStep, bool doProgress)
{
    if (mRegion && doProgress) mRegion->Control(brainStep);

    CkCallback cb(CkReductionTarget(BrainBase, SimulateRegionPrepareTopologyChangeDone), gBrain[0]);
    std::unordered_set<NeuronId> deletedNeurons;
    deletedNeurons.insert(mNeuronRemovals.begin(), mNeuronRemovals.end());
    size_t deletedNeuronCount = deletedNeurons.size();
    contribute(sizeof(size_t), &deletedNeuronCount, CkReduction::sum_ulong_long, cb);
}

void RegionBase::CommitTopologyChange()
{
    if (!mNeuronAdditions.empty()) {
        gNeurons.beginInserting();
        for (auto it = mNeuronAdditions.begin(); it != mNeuronAdditions.end(); ++it) {
            mNeuronIndices.insert(std::get<0>(*it));
            gNeurons(thisIndex, std::get<0>(*it)).insert(std::get<1>(*it), std::get<2>(*it));
        }
        gNeurons.doneInserting();
    }

    if (!mSynapseAdditions.empty()) {
        for (auto it = mSynapseAdditions.begin(); it != mSynapseAdditions.end(); ++it) {
            gCompletionDetector.ckLocalBranch()->produce(2);
            NeuronId from = std::get<0>(*it) == Direction::Forward ? std::get<1>(*it) : std::get<2>(*it);
            NeuronId to = std::get<0>(*it) == Direction::Forward ? std::get<2>(*it) : std::get<1>(*it);
            gNeurons(GetRegionIndex(from), GetNeuronIndex(from)).AddOutputSynapse(to, std::get<3>(*it));
            gNeurons(GetRegionIndex(to), GetNeuronIndex(to)).AddInputSynapse(from, std::get<3>(*it));
        }
    }

    if (!mChildAdditions.empty()) {
        for (auto it = mChildAdditions.begin(); it != mChildAdditions.end(); ++it) {
            gCompletionDetector.ckLocalBranch()->produce(2);
            NeuronId parent = std::get<0>(*it);
            NeuronId child = std::get<1>(*it);
            gNeurons(GetRegionIndex(parent), GetNeuronIndex(parent)).AddChild(child);
            gNeurons(GetRegionIndex(child), GetNeuronIndex(child)).SetParent(parent);
        }
    }

    if (!mChildRemovals.empty()) {
        for (auto it = mChildRemovals.begin(); it != mChildRemovals.end(); ++it) {
            gCompletionDetector.ckLocalBranch()->produce(2);
            NeuronId parent = std::get<0>(*it);
            NeuronId child = std::get<1>(*it);
            gNeurons(GetRegionIndex(parent), GetNeuronIndex(parent)).RemoveChild(child);
            gNeurons(GetRegionIndex(child), GetNeuronIndex(child)).UnsetParent();
        }
    }

    if (!mSynapseRemovals.empty()) {
        for (auto it = mSynapseRemovals.begin(); it != mSynapseRemovals.end(); ++it) {
            gCompletionDetector.ckLocalBranch()->produce(2);
            NeuronId from = std::get<0>(*it) == Direction::Forward ? std::get<1>(*it) : std::get<2>(*it);
            NeuronId to = std::get<0>(*it) == Direction::Forward ? std::get<2>(*it) : std::get<1>(*it);
            gNeurons(GetRegionIndex(from), GetNeuronIndex(from)).RemoveOutputSynapse(to);
            gNeurons(GetRegionIndex(to), GetNeuronIndex(to)).RemoveInputSynapse(from);
        }
    }

    if (!mNeuronRemovals.empty()) {
        gCompletionDetector.ckLocalBranch()->produce(mNeuronRemovals.size());
        CkVec<CkArrayIndex2D> deletedNeuronIndices;
        {
            std::unordered_set<NeuronId> deletedNeurons;
            deletedNeurons.insert(mNeuronRemovals.begin(), mNeuronRemovals.end());
            for (auto it = deletedNeurons.begin(); it != deletedNeurons.end(); ++it) {
                CkArrayIndex2D index(GetRegionIndex(*it), GetNeuronIndex(*it));
                deletedNeuronIndices.push_back(index);
            }
        }
        CProxySection_NeuronBase neuronSection = CProxySection_NeuronBase::ckNew(
            gNeurons.ckGetArrayID(), deletedNeuronIndices.getVec(), deletedNeuronIndices.size());
        neuronSection.ckSectionDelegate(CProxy_CkMulticastMgr(gMulticastGroupId).ckLocalBranch());
        neuronSection.Unlink();
    }

    gCompletionDetector.ckLocalBranch()->done();

    CkCallback cb(CkReductionTarget(BrainBase, SimulateRegionCommitTopologyChangeDone), gBrain[0]);
    size_t triggeredNeuronCount = mNeuronsTriggered.size();
    contribute(sizeof(size_t), &triggeredNeuronCount, CkReduction::sum_ulong_long, cb);
}

void RegionBase::Simulate(SimulateMsg *msg)
{
    mDoUpdate = msg->doUpdate;
    mDoFullUpdate = msg->doFullUpdate;
    mDoProgress = msg->doProgress;
    mBrainStep = msg->brainStep;
    mRoiBoxes = msg->roiBoxes;
    
    if (!mNeuronRemovals.empty()) {
        std::unordered_set<NeuronId> deletedNeurons;
        deletedNeurons.insert(mNeuronRemovals.begin(), mNeuronRemovals.end());
        for (auto it = deletedNeurons.begin(); it != deletedNeurons.end(); ++it) {
            mNeuronIndices.erase(*it);
            gNeurons(GetRegionIndex(*it), GetNeuronIndex(*it)).ckDestroy();
        }
    }

    CkVec<CkArrayIndex2D> sectionNeuronIndices;
    if (mDoFullUpdate && !mNeuronIndices.empty()) {
        for (auto it = mNeuronIndices.begin(); it != mNeuronIndices.end(); ++it) {
            CkArrayIndex2D index(GetRegionIndex(*it), GetNeuronIndex(*it));
            sectionNeuronIndices.push_back(index);
        }

    } else if (mDoProgress && !mNeuronsTriggered.empty()) {
        for (auto it = mNeuronsTriggered.begin(); it != mNeuronsTriggered.end(); ++it) {
            CkArrayIndex2D index(GetRegionIndex(*it), GetNeuronIndex(*it));
            sectionNeuronIndices.push_back(index);
        }
    }

    if (sectionNeuronIndices.size() > 0) {
        mNeuronSectionFilled = true;
        CProxySection_NeuronBase mNeuronSection = CProxySection_NeuronBase::ckNew(
            gNeurons.ckGetArrayID(), sectionNeuronIndices.getVec(), sectionNeuronIndices.size());
        mNeuronSection.ckSectionDelegate(CProxy_CkMulticastMgr(gMulticastGroupId).ckLocalBranch());
    }

    if (mDoProgress && mNeuronSectionFilled) {
        EmptyMsg *emptyMsg = new EmptyMsg();
        mNeuronSection.FlipSpikeQueues(emptyMsg);
    } else {
        NeuronFlipSpikeQueuesDone(nullptr);
    }

    delete msg;
}

void RegionBase::NeuronFlipSpikeQueuesDone(CkReductionMsg *msg)
{
    if (mNeuronSectionFilled) {

        SimulateMsg *simulateMsg = new SimulateMsg();
        simulateMsg->doUpdate = mDoUpdate;
        simulateMsg->doFullUpdate = mDoFullUpdate;
        simulateMsg->doProgress = mDoProgress;
        simulateMsg->brainStep = mBrainStep;
        simulateMsg->roiBoxes = mRoiBoxes;

        mNeuronSection.Simulate(simulateMsg);

    } else {
        NeuronSimulateDone(nullptr);
    }

    if (msg) delete msg;
}

void RegionBase::NeuronSimulateDone(CkReductionMsg *msg)
{
    if (mNeuronSectionFilled) {
        mNeuronSectionFilled = false;
        mNeuronSection = CProxySection_NeuronBase();
    }

    RegionIndex regionIndex = thisIndex;

    RegionAdditionReports addedRegions;
    ConnectorAdditionReports addedConnectors;
    Connections addedConnections;

    if (mDoUpdate && mDoFullUpdate) {
        RegionAdditionReport additionReport(
            regionIndex, GetName(), GetType(), Box3D(mPosition, mSize));
        if (!mBoxChanged) {
            addedRegions.push_back(additionReport);
        }

        for (auto itConnector = mInputConnectors.begin(); itConnector != mInputConnectors.end(); ++itConnector) {
            addedConnectors.push_back(ConnectorAdditionReport(regionIndex, Direction::Backward,
                itConnector->second.name, itConnector->second.neurons.size()));
            for (auto itConnection = itConnector->second.connections.begin();
                    itConnection != itConnector->second.connections.end(); ++itConnection) {
                addedConnections.push_back(Connection(Direction::Forward,
                    itConnection->first, itConnection->second, regionIndex, itConnector->second.name));
            }
        }

        for (auto itConnector = mOutputConnectors.begin(); itConnector != mOutputConnectors.end(); ++itConnector) {
            addedConnectors.push_back(ConnectorAdditionReport(regionIndex, Direction::Forward,
                itConnector->second.name, itConnector->second.neurons.size()));
            for (auto itConnection = itConnector->second.connections.begin();
                itConnection != itConnector->second.connections.end(); ++itConnection) {
                addedConnections.push_back(Connection(Direction::Forward,
                    regionIndex, itConnector->second.name, itConnection->first, itConnection->second));
            }
        }
    }

    RegionAdditionReports repositionedRegions;

    if (mDoUpdate && mBoxChanged) {
        mBoxChanged = false;
        RegionAdditionReport additionReport(
            regionIndex, GetName(), GetType(), Box3D(mPosition, mSize));
        repositionedRegions.push_back(additionReport);
    }

    NeuronAdditionReports addedNeurons;
    NeuronAdditionReports repositionedNeurons;
    NeuronRemovals removedNeurons;
    Synapse::Links addedSynapses;
    Synapse::Links spikedSynapses;
    Synapse::Links removedSynapses;
    ChildLinks addedChildren;
    ChildLinks removedChildren;

    if (mDoUpdate && !mNeuronRemovals.empty()) {
        std::unordered_set<NeuronId> removedNeuronSet(
            mNeuronRemovals.begin(), mNeuronRemovals.end());
        removedNeurons.insert(removedNeurons.begin(), mNeuronRemovals.begin(), mNeuronRemovals.end());
        for (auto it = mSynapseRemovals.begin(); it != mSynapseRemovals.end(); ++it) {
            bool intersectsRemovedNeuron =
                (removedNeuronSet.find(std::get<1>(*it)) != removedNeuronSet.end()) ||
                (removedNeuronSet.find(std::get<2>(*it)) != removedNeuronSet.end());
            if (intersectsRemovedNeuron) removedSynapses.push_back(Synapse::Link(std::get<1>(*it), std::get<2>(*it)));
        }
        for (auto it = mChildRemovals.begin(); it != mChildRemovals.end(); ++it) {
            bool intersectsRemovedNeuron =
                (removedNeuronSet.find(std::get<0>(*it)) != removedNeuronSet.end()) ||
                (removedNeuronSet.find(std::get<1>(*it)) != removedNeuronSet.end());
            if (intersectsRemovedNeuron) removedChildren.push_back(Synapse::Link(std::get<0>(*it), std::get<1>(*it)));
        }
    }

    mNeuronAdditions.clear();
    mNeuronRemovals.clear();
    mSynapseAdditions.clear();
    mSynapseRemovals.clear();
    mChildAdditions.clear();
    mChildRemovals.clear();

    mNeuronsTriggered.clear();

    if (msg) {
        CkReduction::setElement *neuronResult = static_cast<CkReduction::setElement *>(msg->getData());
        while (neuronResult != nullptr) {
            uint8_t *neuronResultPtr = reinterpret_cast<uint8_t *>(&neuronResult->data);

            PUP::fromMem p(static_cast<void *>(neuronResultPtr));

            NeuronId neuronId; p | neuronId;

            size_t triggeredCount; p | triggeredCount;
            mNeuronsTriggered.reserve(mNeuronsTriggered.size() + triggeredCount);
            for (size_t i = 0; i < triggeredCount; ++i) {
                NeuronId triggered; p | triggered;
                mNeuronsTriggered.insert(triggered);
            }

            uint8_t *neuronContributionPtr = nullptr;
            size_t neuronContributionSize = 0;
            p(neuronContributionPtr, neuronContributionSize);
            if (mRegion && neuronContributionSize > 0) {
                mRegion->AcceptContributionFromNeuron(neuronId,
                    neuronContributionPtr, neuronContributionSize);
            }

            bool skipDynamicityReport; p | skipDynamicityReport;
            if (!skipDynamicityReport) {
                std::unordered_map<NeuronId, NeuronId> tempNeuronIdMap;

                NeuronAdditionRequests tmpNeuronAdditionRequests; p | tmpNeuronAdditionRequests;
                mNeuronAdditions.reserve(mNeuronAdditions.size() + tmpNeuronAdditionRequests.size());
                for (auto it = tmpNeuronAdditionRequests.begin(); it != tmpNeuronAdditionRequests.end(); ++it) {
                    NeuronId localNeuronId = std::get<0>(*it);
                    if (GetRegionIndex(localNeuronId) == TEMP_REGION_INDEX) {
                        NeuronId globalNeuronId = GetNeuronId(thisIndex, GetNewNeuronIndex());
                        tempNeuronIdMap.insert(std::make_pair(localNeuronId, globalNeuronId));
                        std::get<0>(*it) = globalNeuronId;
                    }
                    mNeuronAdditions.push_back(*it);
                }

                NeuronRemovals tmpNeuronRemovals; p | tmpNeuronRemovals;
                mNeuronRemovals.reserve(mNeuronRemovals.size() + tmpNeuronRemovals.size());
                for (auto it = tmpNeuronRemovals.begin(); it != tmpNeuronRemovals.end(); ++it) {
                    bool validRequest = true;
                    for (auto itConn = mInputConnectors.begin(); itConn != mInputConnectors.end(); ++itConn) {
                        size_t connectorSize = itConn->second.neurons.size();
                        if (connectorSize > 0) {
                            NeuronId firstNeuronId = itConn->second.neurons.at(0);
                            bool isWithinConnector = ((*it >= firstNeuronId) && (*it < firstNeuronId + connectorSize));
                            validRequest = validRequest && !isWithinConnector;
                        }
                    }
                    if (validRequest) tmpNeuronRemovals.push_back(*it);
                }

                Synapse::Additions tmpSynapseAdditions; p | tmpSynapseAdditions;
                mSynapseAdditions.reserve(mSynapseAdditions.size() + tmpSynapseAdditions.size());
                for (auto it = tmpSynapseAdditions.begin(); it != tmpSynapseAdditions.end(); ++it) {
                    NeuronId fromNeuronId = std::get<1>(*it);
                    NeuronId toNeuronId = std::get<2>(*it);
                    if (GetRegionIndex(fromNeuronId) == TEMP_REGION_INDEX) {
                        std::get<1>(*it) = tempNeuronIdMap[fromNeuronId];
                    }
                    if (GetRegionIndex(toNeuronId) == TEMP_REGION_INDEX) {
                        std::get<2>(*it) = tempNeuronIdMap[toNeuronId];
                    }
                    mSynapseAdditions.push_back(*it);
                }

                Synapse::Removals tmpSynapseRemovals; p | tmpSynapseRemovals;
                mSynapseRemovals.reserve(mSynapseRemovals.size() + tmpSynapseRemovals.size());
                mSynapseRemovals.insert(mSynapseRemovals.begin(),
                    tmpSynapseRemovals.begin(), tmpSynapseRemovals.end());

                ChildLinks tmpChildAdditions; p | tmpChildAdditions;
                mChildAdditions.reserve(mChildAdditions.size() + tmpChildAdditions.size());
                for (auto it = tmpChildAdditions.begin(); it != tmpChildAdditions.end(); ++it) {
                    NeuronId parentNeuronId = std::get<0>(*it);
                    NeuronId childNeuronId = std::get<1>(*it);
                    if (GetRegionIndex(parentNeuronId) == TEMP_REGION_INDEX) {
                        std::get<0>(*it) = tempNeuronIdMap[parentNeuronId];
                    }
                    if (GetRegionIndex(childNeuronId) == TEMP_REGION_INDEX) {
                        std::get<1>(*it) = tempNeuronIdMap[childNeuronId];
                    }
                    mChildAdditions.push_back(*it);
                }

                ChildLinks tmpChildRemovals; p | tmpChildRemovals;
                mChildRemovals.reserve(mChildRemovals.size() + tmpChildRemovals.size());
                mChildRemovals.insert(mChildRemovals.begin(),
                    tmpChildRemovals.begin(), tmpChildRemovals.end());
            }

            bool skipTopologyReport; p | skipTopologyReport;
            if (!skipTopologyReport) {
                NeuronAdditionReports tmpAddedNeurons; p | tmpAddedNeurons;
                addedNeurons.reserve(addedNeurons.size() + tmpAddedNeurons.size());
                for (auto it = tmpAddedNeurons.begin(); it != tmpAddedNeurons.end(); ++it) {
                    NeuronAdditionReport report = *it;
                    TranslateAndScale(std::get<2>(report), Box3D(mPosition, mSize));
                    addedNeurons.push_back(report);
                }

                NeuronAdditionReports tmpRepositionedNeurons; p | tmpRepositionedNeurons;
                repositionedNeurons.reserve(repositionedNeurons.size() + tmpRepositionedNeurons.size());
                for (auto it = tmpRepositionedNeurons.begin(); it != tmpRepositionedNeurons.end(); ++it) {
                    NeuronAdditionReport report = *it;
                    TranslateAndScale(std::get<2>(report), Box3D(mPosition, mSize));
                    repositionedNeurons.push_back(report);
                }

                NeuronRemovals tmpRemovedNeurons; p | tmpRemovedNeurons;
                removedNeurons.reserve(removedNeurons.size() + tmpRemovedNeurons.size());
                removedNeurons.insert(removedNeurons.begin(),
                    tmpRemovedNeurons.begin(), tmpRemovedNeurons.end());

                Synapse::Links tmpAddedSynapses; p | tmpAddedSynapses;
                addedSynapses.reserve(addedSynapses.size() + tmpAddedSynapses.size());
                addedSynapses.insert(addedSynapses.begin(),
                    tmpAddedSynapses.begin(), tmpAddedSynapses.end());

                Synapse::Links tmpSpikedSynapses; p | tmpSpikedSynapses;
                spikedSynapses.reserve(spikedSynapses.size() + tmpSpikedSynapses.size());
                spikedSynapses.insert(spikedSynapses.begin(),
                    tmpSpikedSynapses.begin(), tmpSpikedSynapses.end());

                Synapse::Links tmpRemovedSynapses; p | tmpRemovedSynapses;
                removedSynapses.reserve(removedSynapses.size() + tmpRemovedSynapses.size());
                removedSynapses.insert(removedSynapses.begin(),
                    tmpRemovedSynapses.begin(), tmpRemovedSynapses.end());

                ChildLinks tmpAddedChildren; p | tmpAddedChildren;
                addedChildren.reserve(addedChildren.size() + tmpAddedChildren.size());
                addedChildren.insert(addedChildren.begin(),
                    tmpAddedChildren.begin(), tmpAddedChildren.end());

                ChildLinks tmpRemovedChildren; p | tmpRemovedChildren;
                removedChildren.reserve(removedChildren.size() + tmpRemovedChildren.size());
                removedChildren.insert(removedChildren.begin(),
                    tmpRemovedChildren.begin(), tmpRemovedChildren.end());
            }

            neuronResult = neuronResult->next();
        }

        delete msg;
    }

    uint8_t *customContribution = nullptr;
    size_t customContributionSize = 0;

    if (mDoProgress) {
        customContributionSize = mRegion->ContributeToBrain(customContribution);
    }

    uint8_t *resultPtr = nullptr;
    size_t resultSize = 0;

    for (size_t i = 0; i < 2; ++i) {

        PUP::sizer sizer;
        PUP::toMem toMem(resultPtr);
        PUP::er *p = (i == 0) ? static_cast<PUP::er *>(&sizer) : static_cast<PUP::er *>(&toMem);

        *p | regionIndex;

        *p | mBrainSink;

        (*p)(customContribution, customContributionSize);

        if (mDoUpdate) {
            if (mDoFullUpdate) {
                *p | addedRegions;
                *p | addedConnectors;
                *p | addedConnections;
            }

            *p | repositionedRegions;

            *p | addedNeurons;
            *p | repositionedNeurons;
            *p | removedNeurons;
            *p | addedSynapses;
            *p | spikedSynapses;
            *p | removedSynapses;
            *p | addedChildren;
            *p | removedChildren;
        }

        if (i == 0) {
            resultSize = sizer.size();
            resultPtr = new uint8_t[resultSize];
        }
    }

    CkCallback cb(CkReductionTarget(BrainBase, SimulateRegionSimulateDone), gBrain[0]);
    contribute(resultSize, resultPtr, CkReduction::set, cb);

    mBrainSink.clear();
    delete[] resultPtr;
    if (customContributionSize > 0) delete customContribution;
}

const char *ThresholdRegion::Type = "ThresholdRegion";

ThresholdRegion::ThresholdRegion(RegionBase &base, json &params) : Region(base, params)
{
    // TODO
}

ThresholdRegion::~ThresholdRegion()
{
}

void ThresholdRegion::pup(PUP::er &p)
{
}

const char *ThresholdRegion::GetType() const
{
    return Type;
}

void ThresholdRegion::Control(size_t brainStep)
{
    // TODO
}

void ThresholdRegion::AcceptContributionFromNeuron(
    NeuronId neuronId, const uint8_t *contribution, size_t size)
{
    // TODO
}

size_t ThresholdRegion::ContributeToBrain(uint8_t *&contribution)
{
    // TODO
    contribution = nullptr;
    return 0;
}

#include "region.def.h"
