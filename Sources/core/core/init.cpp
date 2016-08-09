#include "init.h"
#include "components.h"
#include "gen_spec_init.h"
#include "threshold_init.h"
#include "synapse.h"
#include "spike.h"

void RegisterCoreSynapseEditors(SynapseEditorCache *editorCache)
{
    Synapse::DefaultType = editorCache->Register("Empty", new Synapse::Editor());

    editorCache->Register("Weighted", new WeightedSynapse());
    editorCache->Register("Lagging", new LaggingSynapse());
    editorCache->Register("Conductive", new ConductiveSynapse());
    editorCache->Register("Probabilistic", new ProbabilisticSynapse());
    editorCache->Register("MultiWeighted", new MultiWeightedSynapse());
}

void RegisterCoreSpikeEditors(SpikeEditorCache *editorCache)
{
    Spike::DefaultType = editorCache->Register("Binary", new BinarySpike());

    editorCache->Register("Discrete", new DiscreteSpike());
    editorCache->Register("Continuous", new ContinuousSpike());
    editorCache->Register("Visual", new VisualSpike());
    editorCache->Register("Functional", new FunctionalSpike());
    editorCache->Register("MultiByte", new MultiByteSpike());
}

void InitializeComponents()
{
    NeuronFactory *neuronFactory = NeuronFactory::GetInstance();
    RegionFactory *regionFactory = RegionFactory::GetInstance();
    BrainFactory *brainFactory = BrainFactory::GetInstance();

    SynapseEditorCache *synapseEditorCache = SynapseEditorCache::GetInstance();
    SpikeEditorCache *spikeEditorCache = SpikeEditorCache::GetInstance();

    RegisterCoreSynapseEditors(synapseEditorCache);
    RegisterCoreSpikeEditors(spikeEditorCache);

    GenSpecModel::Init(neuronFactory, regionFactory, brainFactory);
    ThresholdModel::Init(neuronFactory, regionFactory, brainFactory);
}
