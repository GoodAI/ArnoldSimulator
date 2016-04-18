#include "neuron.h"

#include "region.h"

const NeuronId RegionBase::DeletedNeuronId = DELETED_NEURON_ID;
const RegionId RegionBase::BrainRegionId = BRAIN_REGION_ID;

Region::Region(RegionBase &base, json &params) : mBase(base)
{
}

Region *RegionBase::CreateRegion(const std::string &type, RegionBase &base, json &params)
{
    if (type == ThresholdRegion::Type) {
        return new ThresholdRegion(base, params);
    } else {
        return nullptr;
    }
}

RegionBase::RegionBase(CProxy_BrainBase &brain, const std::string &type, const std::string &params) : mBrain(brain)
{
    json parsedParams;
    std::stringstream streamedParams(params);
    streamedParams >> parsedParams;

    mRegion = RegionBase::CreateRegion(type, *this, parsedParams);
}

RegionBase::RegionBase(CkMigrateMessage *msg)
{
}

RegionBase::~RegionBase()
{
}

void RegionBase::pup(PUP::er &p)
{
}

RegionId RegionBase::GetId() const
{
    return RegionId();
}

NeuronId RegionBase::GetNewNeuronId()
{
    return NeuronId();
}

std::vector<std::pair<ConnectorName, size_t>> RegionBase::GetInputs() const
{
    return std::vector<std::pair<ConnectorName, size_t>>();
}

std::vector<RemoteConnector> RegionBase::GetInputConnections(const ConnectorName &name) const
{
    return std::vector<RemoteConnector>();
}

void RegionBase::CreateInput(const ConnectorName &name, size_t size)
{
}

void RegionBase::DeleteInput(const ConnectorName &name)
{
}

void RegionBase::ConnectInput(const ConnectorName &name, const RemoteConnector &destination)
{
}

void RegionBase::DisconnectInput(const ConnectorName &name, const RemoteConnector &destination)
{
}

std::vector<std::pair<ConnectorName, size_t>> RegionBase::GetOutputs() const
{
    return std::vector<std::pair<ConnectorName, size_t>>();
}

std::vector<RemoteConnector> RegionBase::GetOutputConnections(const ConnectorName &name) const
{
    return std::vector<RemoteConnector>();
}

void RegionBase::CreateOutput(const ConnectorName &name, size_t size)
{
}

void RegionBase::DeleteOutput(const ConnectorName &name)
{
}

void RegionBase::ConnectOutput(const ConnectorName &name, const RemoteConnector &destination)
{
}

void RegionBase::DisconnectOutput(const ConnectorName &name, const RemoteConnector &destination)
{
}

void RegionBase::ReceiveSensoMotoricData(Direction direction, const ConnectorName &from, std::vector<unsigned char> &data)
{
}

void RegionBase::ReceiveSpikes(Direction direction, RegionId from, std::vector<Spike::Package> &spikes)
{
}

const tbb::concurrent_vector<NeuronId>& RegionBase::GetAddedNeurons() const
{
    return mNeuronsToAdd;
}

const tbb::concurrent_vector<NeuronId>& RegionBase::GetRemovedNeurons() const
{
    return mNeuronsToRemove;
}

const tbb::concurrent_vector<Synapse::Addition>& RegionBase::GetAddedSynapses() const
{
    return mSynapsesToAdd;
}

const tbb::concurrent_vector<Synapse::Removal>& RegionBase::GetRemovedSynapses() const
{
    return mSynapsesToRemove;
}

Neuron *RegionBase::GetNeuron(NeuronId neuronId)
{
    return nullptr;
}

void RegionBase::EnqueueSpike(Direction direction, RegionId destRegId, GateLaneIdx laneIdx, const Spike::Data &data)
{
}

void RegionBase::AddNeuron(Neuron *neuron)
{
}

void RegionBase::RemoveNeuron(NeuronId neuron)
{
}

void RegionBase::AddSynapse(Direction direction, NeuronId from, NeuronId to, Synapse::Data &data)
{
}

void RegionBase::RemoveSynapse(Direction direction, NeuronId from, NeuronId to)
{
}

void RegionBase::AddChildToParent(NeuronId parent, NeuronId child)
{
}

void RegionBase::RemoveChildFromParent(NeuronId parent, NeuronId child)
{
}

void RegionBase::TriggerNeuron(NeuronId sender, NeuronId receiver)
{
}

void RegionBase::Simulate(size_t brainStep)
{

    /*
    expertsTriggeredCurrent->clear();
    swap(expertsTriggeredCurrent, expertsTriggeredNext);

    tbb::parallel_for(ExpertSet::range_type(*expertsTriggeredCurrent), [&](ExpertSet::range_type &expers) {
        ExpertSet::iterator it;
        for (it = expers.begin(); it != expers.end(); ++it) {
            it->first->FlipClosureQueues();
        }
    });

    tbb::parallel_for(ExpertSet::range_type(*expertsTriggeredCurrent), [&](ExpertSet::range_type &expers) {
        ExpertSet::iterator it;
            for (it = expers.begin(); it != expers.end(); ++it) {
        it->first->Simulate();
        }
    });

    CkPrintf("Report from region [%d]: %d experts simulated, %d experts triggered \n",
    thisIndex, expertsTriggeredCurrent->size(), expertsTriggeredNext->size());
    mainProxy.done();
    */

    /*
    for each input in regionInputs{
        parallel_for each payload in input.receivedSparsePayloads
             input.gateLanes[payload.First]->HandlePayload(Direction::Forward, null, payload.Second);
        input.receivedSparsePayloads.clear();
        if (input.receivedDensePayloads != NULL) {
            for each payload in input.receivedDensePayload
                parallel_for each elemPayload in payload.second
                     input.gateLanes[payload.First + rank]->HandlePayload(Direction::Forward, null, elemPayload);
            input.receivedDensePayloads.clear();
        }
    }
    for each output in regionOutputs{
        parallel_for each payload in output.receivedSparsePayloads
             output.gateLanes[payload.First]->HandlePayload(Direction::Backward, null, payload.Second);
        output.receivedSparsePayloads.clear();
        if (output.receivedDensePayloads != NULL) {
            for each payload in output.receivedDensePayload
                parallel_for each elemPayload in payload.second
                     output.gateLanes[payload.First + rank]->HandlePayload(Direction::Backward, null, elemPayload);
            output.receivedDensePayloads.clear();
        }
    }
  
    triggeredNeuronsCurrent->clear();
    swap(triggeredNeuronsCurrent, triggeredNeuronsNext);
    parallel_for each neuron in triggeredNeuronsCurrent
        neuron->FlipClosureQueues();

    size_t interactionsTriggeredCnt = 0;
    RegionControl(brainStep, interactionsTriggeredCnt);

    // remove connections between neurons
    // remove neurons, remove also from triggered set
    // add new neurons
    // add new connections between neurons

    parallel_for each neuron in triggeredNeuronsCurrent
        neuron->Simulate(brainStep);

    for each input in regionInputs{
        if not input.Val.queuedPayloads.empty() {
            interactionsTriggeredCnt++;
            thisProxy[intput.Key].ReceivePayloads(Direction::Backward, thisIndex, input.Val.queuedPayloads);
            input.Val.queuedPayloads.clear();
        }
    }
    for each output in regionOutputs{
        if not output.Val.queuedPayloads.empty() {
            interactionsTriggeredCnt++;
            thisProxy[output.Key].ReceivePayloads(Direction::Forward, thisIndex, output.Val.queuedPayloads);
            output.Val.queuedPayloads.clear();
        }
    }

    if (!triggeredNeuronsNext.empty()) {
        interactionsTriggeredCnt++;
        brainTriggerRegion(thisIndex);
    }
    brain.RegionSimulated(thisIndex, interactionsTriggeredCnt);
    */
}

const char *ThresholdRegion::Type = "ThresholdRegion";

ThresholdRegion::ThresholdRegion(RegionBase &base, json &params) : Region(base, params)
{
    // TODO

    /*
    CkPrintf("TestRegion %d initializing...\n", thisIndex);

    randEngine.seed(std::chrono::high_resolution_clock::now().time_since_epoch().count());

    expertsTriggeredCurrent = new ExpertSet();
    expertsTriggeredNext = new ExpertSet();

    size_t expertCount = 1000000;
    expertsAll.reserve(expertCount);
    for (size_t i = 0; i < expertCount; ++i) {
        expertsAll.insert(std::make_pair((uint32_t)i, new TestExpert(this)));
    }

    std::uniform_int_distribution<int> randExpert(0, expertCount - 1);
    for (size_t i = 0; i < expertCount * 10; ++i) {
        size_t from = randExpert(randEngine);
        size_t to = randExpert(randEngine);
        if (from == to) continue;
        expertsAll[from]->AddConnection(Direction::Forward, expertsAll[to]);
    }

    size_t initialSpikes = 10000;
    for (size_t i = 0; i < initialSpikes; ++i) {
        size_t chosenOne = randExpert(randEngine);
        expertsAll[chosenOne]->EnqueueClosure(Direction::Forward, nullptr, [=](Direction direction, TestExpert *caller, TestExpert *callee) {
            callee->ReceiveSpike(direction, caller);
        });
    }

    CkPrintf("TestRegion %d initialized\n", thisIndex);
    */
}

ThresholdRegion::~ThresholdRegion()
{
}

const char *ThresholdRegion::GetType()
{
    return Type;
}

Spike::Editor *ThresholdRegion::GetInputSpikeEditor(const ConnectorName &name)
{
    return nullptr;
}

Spike::Editor *ThresholdRegion::GetOutputSpikeEditor(const ConnectorName &name)
{
    return nullptr;
}

Neuron *ThresholdRegion::CreateNewInputNeuron(const ConnectorName &name)
{
    return nullptr;
}

Neuron *ThresholdRegion::CreateNewOutputNeuron(const ConnectorName &name)
{
    return nullptr;
}

void ThresholdRegion::Control(size_t brainStep, size_t &interactionsTriggeredCnt)
{
}

#include "region.def.h"
