#include "region.h"

#include "brain.h"

#include "core.decl.h"
#include "brain.decl.h"
#include "neuron.decl.h"

extern CkGroupID gMulticastGroupId;
extern CProxy_CompletionDetector gCompletionDetector;

extern CProxy_Core gCore;
extern CProxy_BrainBase gBrain;
extern CProxy_RegionBase gRegions;
extern CProxy_NeuronBase gNeurons;

Region::Region(RegionBase &base, json &params) : mBase(base)
{
}

Region *RegionBase::CreateRegion(const RegionType &type, RegionBase &base, json &params)
{
    if (type == ThresholdRegion::Type) {
        return new ThresholdRegion(base, params);
    } else {
        return nullptr;
    }
}

RegionBase::RegionBase(const RegionType &type, const RegionParams &params)
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

const char *RegionBase::GetType()
{
    return nullptr;
}

RegionIndex RegionBase::GetIndex() const
{
    return RegionIndex();
}

NeuronId RegionBase::GetNewNeuronId()
{
    return NeuronId();
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

const NeuronAdditions &RegionBase::GetNeuronAdditions() const
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

const ChildAdditions &RegionBase::GetChildAdditions() const
{
    return mChildAdditions;
}

const ChildRemovals &RegionBase::GetChildRemovals() const
{
    return mChildRemovals;
}

NeuronId RegionBase::RequestNeuronAddition(const NeuronType &type, const NeuronParams &params)
{
    return NeuronId();
}

void RegionBase::RequestNeuronRemoval(NeuronId neuronId)
{
}

void RegionBase::RequestSynapseAddition(Direction direction, NeuronId from, NeuronId to, const Synapse::Data &data)
{
}

void RegionBase::RequestSynapseRemoval(Direction direction, NeuronId from, NeuronId to)
{
}

void RegionBase::RequestChildAddition(NeuronId parent, NeuronId child)
{
}

void RegionBase::RequestChildRemoval(NeuronId parent, NeuronId child)
{
}

void RegionBase::CreateInput(const ConnectorName &name, Spike::Type spikeType, 
    const NeuronType &neuronType, const NeuronParams &neuronParams, size_t neuronCount)
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

void RegionBase::CreateOutput(const ConnectorName &name, Spike::Type spikeType, 
    const NeuronType &neuronType, const NeuronParams &neuronParams, size_t neuronCount)
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

void RegionBase::ReceiveSensoMotoricData(Direction direction, const ConnectorName &connectorName, Spike::BrainSource &data)
{
}

void RegionBase::EnqueueSensoMotoricSpike(NeuronId receiver, const Spike::Data &data)
{
}

void RegionBase::ChangeTopology()
{
}

void RegionBase::Simulate(SimulateMsg *msg)
{
    size_t brainStep = msg->brainStep;
    delete msg;

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

void RegionBase::NeuronSimulateDone(CkReductionMsg *msg)
{
    // Get the initial element in the set.
    CkReduction::setElement *current = (CkReduction::setElement *)msg->getData();
    while (current != NULL) // Loop over elements in set.
    {
        // Get the pointer to the packed int's.
        int *result = (int *)&current->data;
        // Do something with result.
        current = current->next(); // Iterate.
    }
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

void ThresholdRegion::pup(PUP::er &p)
{
}

const char *ThresholdRegion::GetType()
{
    return Type;
}

void ThresholdRegion::Control(size_t brainStep)
{
}

#include "region.def.h"
