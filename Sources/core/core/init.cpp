#include "init.h"
#include "components.h"
#include "gen_spec_init.h"
#include "threshold_init.h"
#include "synapse.h"
#include "spike.h"

void registerCoreSynapseEditors(SynapseEditorCache *editorCache)
{
    Synapse::DefaultType = editorCache->Register("Empty", new SynapseEditor());

    editorCache->Register("Weighted", new WeightedSynapse());
    editorCache->Register("Lagging", new LaggingSynapse());
    editorCache->Register("Conductive", new ConductiveSynapse());
    editorCache->Register("Probabilistic", new ProbabilisticSynapse());
    editorCache->Register("MultiWeighted", new MultiWeightedSynapse());
}

void registerCoreSpikeEditors(SpikeEditorCache *editorCache)
{
    Spike::DefaultType = editorCache->Register("Binary", new BinarySpike());

    editorCache->Register("Discrete", new DiscreteSpike());
    editorCache->Register("Continuous", new ContinuousSpike());
    editorCache->Register("Visual", new VisualSpike());
    editorCache->Register("Functional", new FunctionalSpike());
    editorCache->Register("MultiByte", new MultiByteSpike());
}

void initializeComponents()
{
    NeuronFactory *neuronFactory = NeuronFactory::GetInstance();
    RegionFactory *regionFactory = RegionFactory::GetInstance();
    BrainFactory *brainFactory = BrainFactory::GetInstance();

    SynapseEditorCache *synapseEditorCache = SynapseEditorCache::GetInstance();
    SpikeEditorCache *spikeEditorCache = SpikeEditorCache::GetInstance();

    registerCoreSynapseEditors(synapseEditorCache);
    registerCoreSpikeEditors(spikeEditorCache);

    GenSpecModel::init(neuronFactory, regionFactory, brainFactory);
    ThresholdModel::init(neuronFactory, regionFactory, brainFactory);
}
