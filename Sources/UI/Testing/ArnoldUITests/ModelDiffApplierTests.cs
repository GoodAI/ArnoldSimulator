using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Visualization.Models;
using GoodAI.Arnold.Communication;
using GoodAI.Arnold.Observation;
using KellermanSoftware.CompareNetObjects;
using OpenTK;
using Xunit;

namespace GoodAI.Arnold.UI.Tests
{
    public class ModelDiffApplierTests
    {
        private SimulationModel m_model;
        private ModelDiffApplier m_applier;

        public const int MaxDifferences = 20;

        public ModelDiffApplierTests()
        {
            m_model = new SimulationModel();
            m_applier = new ModelDiffApplier();
        }

        private static RegionModel SetupRegion(ModelDiffApplier applier, SimulationModel model, uint id)
        {
            var sourceRegion = new RegionModel(id, "foo", "bar", new Vector3(1, 2, 3), new Vector3(6, 5, 4));

            ResponseMessage diff = ModelResponseBuilder.Build(addedRegions: new List<RegionModel> {sourceRegion});
            applier.ApplyModelDiff(model, diff.GetResponse(new ModelResponse()));
            return sourceRegion;
        }

        private static ComparisonConfig RegionComparisonConfig => new ComparisonConfig
        {
            MaxDifferences = MaxDifferences,
            SkipInvalidIndexers = true,
            MembersToInclude = new List<string>
            {
                "Index",
                "Name",
                "Type",
                "Position",
                "Size"
            }
        };

        private static ComparisonConfig ConnectorComparisonConfig => new ComparisonConfig
        {
            MaxDifferences = MaxDifferences,
            SkipInvalidIndexers = true,
            MembersToInclude = new List<string>
            {
                "Name",
                "Direction",
                "SlotCount"
            }
        };

        private static ComparisonConfig ConnectionComparisonConfig => new ComparisonConfig
        {
            MaxDifferences = MaxDifferences,
            SkipInvalidIndexers = true,
            MembersToInclude = new List<string>
            {
                "From",
                "To",
                "Direction"
            }
        };

        private static ComparisonConfig NeuronComparisonConfig => new ComparisonConfig
        {
            MaxDifferences = MaxDifferences,
            SkipInvalidIndexers = true,
            MembersToInclude = new List<string>
            {
                "Index",
                "Type",
                "RegionModel",
                "Position"
            }
        };

        private static ComparisonConfig SynapseComparisonConfig => new ComparisonConfig
        {
            MaxDifferences = MaxDifferences,
            SkipInvalidIndexers = true,
            MembersToInclude = new List<string>
            {
                "FromRegion",
                "ToRegion",
                "FromNeuron",
                "ToNeuron",
                "Position",
                "Target"
            }
        };

        private static CompareLogic CompareLogic(ComparisonConfig config)
        {
            return new CompareLogic {Config = config};
        }

        [Fact]
        public void AddsNewRegion()
        {
            RegionModel addedRegion = SetupRegion(m_applier, m_model, 1);

            Assert.Equal(1, m_model.Regions.Count());
            RegionModel generatedRegion = m_model.Regions.First();

            ComparisonResult result = CompareLogic(RegionComparisonConfig).Compare(addedRegion, generatedRegion);

            Assert.True(result.AreEqual, result.DifferencesString);
        }

        [Fact]
        public void RemovesRegion()
        {
            RegionModel addedRegion = SetupRegion(m_applier, m_model, 1);

            ResponseMessage diff = ModelResponseBuilder.Build(removedRegions: new List<RegionModel> {addedRegion});

            ApplyModelDiff(diff);

            Assert.Empty(m_model.Regions);
        }

        [Fact]
        public void RepositionsRegion()
        {
            RegionModel addedRegion = SetupRegion(m_applier, m_model, 1);

            addedRegion.Position = Vector3.UnitZ;
            ResponseMessage diff = ModelResponseBuilder.Build(repositionedRegions: new List<RegionModel> {addedRegion});

            ApplyModelDiff(diff);

            Assert.Equal(Vector3.UnitZ, m_model.Regions.First().Position);
        }

        [Fact]
        public void AddsNewConnector()
        {
            RegionModel addedRegion = SetupRegion(m_applier, m_model, 1);

            ConnectorModel addedConnector1 = new InputConnectorModel(addedRegion, "input", 4);
            ConnectorModel addedConnector2 = new OutputConnectorModel(addedRegion, "output", 3);

            ResponseMessage diff =
                ModelResponseBuilder.Build(addedConnectors: new List<ConnectorModel> {addedConnector1, addedConnector2});
            ApplyModelDiff(diff);

            RegionModel region = m_model.Regions.First();

            Assert.Equal(1, region.InputConnectors.Models.Count());
            Assert.Equal(1, region.OutputConnectors.Models.Count());

            ComparisonResult result1 = CompareLogic(ConnectorComparisonConfig)
                .Compare(addedConnector1, region.InputConnectors.First());
            ComparisonResult result2 = CompareLogic(ConnectorComparisonConfig)
                .Compare(addedConnector2, region.OutputConnectors.First());

            Assert.True(result1.AreEqual, result1.DifferencesString);
            Assert.True(result2.AreEqual, result2.DifferencesString);
        }

        [Fact]
        public void RemovesConnector()
        {
            RegionModel addedRegion = SetupRegion(m_applier, m_model, 1);

            ConnectorModel addedConnector = new InputConnectorModel(addedRegion, "input", 4);

            ResponseMessage diff =
                ModelResponseBuilder.Build(addedConnectors: new List<ConnectorModel> {addedConnector});
            ApplyModelDiff(diff);

            diff = ModelResponseBuilder.Build(removedConnectors: new List<ConnectorModel> {addedConnector});

            ApplyModelDiff(diff);

            var region = m_model.Regions.First();
            Assert.Empty(region.InputConnectors);
        }

        [Fact]
        public void AddsNewConnection()
        {
            ConnectionModel addedConnection = SetupRegionsWithConnectors();

            RegionModel region1 = m_model.Regions.First();
            RegionModel region2 = m_model.Regions.Skip(1).First();

            Assert.Equal(region1.OutputConnectors.First().Connections.First(), region2.InputConnectors.First().Connections.First());
            Assert.Equal(region2.InputConnectors.First().Connections.First().From, region1.OutputConnectors.First());

            var result = CompareLogic(ConnectionComparisonConfig)
                .Compare(addedConnection, region1.OutputConnectors.First().Connections.First());

            Assert.True(result.AreEqual, result.DifferencesString);
        }

        [Fact]
        public void RemovesConnection()
        {
            ConnectionModel addedConnection = SetupRegionsWithConnectors();

            ResponseMessage diff = ModelResponseBuilder.Build(removedConnections: new List<ConnectionModel> {addedConnection});

            ApplyModelDiff(diff);

            RegionModel region1 = m_model.Regions.First();
            RegionModel region2 = m_model.Regions.Skip(1).First();

            Assert.Empty(region1.OutputConnectors.First().Connections);
            Assert.Empty(region2.InputConnectors.First().Connections);
            Assert.Empty(m_model.Connections);
        }

        private ConnectionModel SetupRegionsWithConnectors()
        {
            RegionModel addedRegion1 = SetupRegion(m_applier, m_model, 1);
            RegionModel addedRegion2 = SetupRegion(m_applier, m_model, 2);

            var addedConnector1 = new OutputConnectorModel(addedRegion1, "output", 3);
            var addedConnector2 = new InputConnectorModel(addedRegion2, "input", 4);

            var addedConnection = new ConnectionModel(addedConnector1, addedConnector2);

            ResponseMessage diff =
                ModelResponseBuilder.Build(
                    addedConnectors: new List<ConnectorModel> {addedConnector1, addedConnector2},
                    addedConnections: new List<ConnectionModel> {addedConnection});

            ApplyModelDiff(diff);
            return addedConnection;
        }

        [Fact]
        public void AddsNewNeuron()
        {
            RegionModel addedRegion;
            NeuronModel addedNeuron;
            SetupRegionWithNeuron(out addedRegion, out addedNeuron);

            RegionModel region = m_model.Regions.First();

            ComparisonResult result = CompareLogic(NeuronComparisonConfig)
                .Compare(addedNeuron, region.Neurons.First());

            Assert.True(result.AreEqual, result.DifferencesString);
        }

        [Fact]
        public void RepositionsNeuron()
        {
            RegionModel addedRegion;
            NeuronModel addedNeuron;
            SetupRegionWithNeuron(out addedRegion, out addedNeuron);

            addedNeuron.Position = Vector3.UnitZ;
            ResponseMessage diff = ModelResponseBuilder.Build(repositionedNeurons: new List<NeuronModel> {addedNeuron});

            ApplyModelDiff(diff);

            Assert.Equal(Vector3.UnitZ, m_model.Regions[addedRegion.Index].Neurons[addedNeuron.Index].Position);
        }

        [Fact]
        public void RemovesNeuron()
        {
            RegionModel addedRegion;
            NeuronModel addedNeuron;
            SetupRegionWithNeuron(out addedRegion, out addedNeuron);

            ResponseMessage diff = ModelResponseBuilder.Build(removedNeurons: new List<NeuronModel> {addedNeuron});

            ApplyModelDiff(diff);

            Assert.Empty(m_model.Regions[addedRegion.Index].Neurons);
        }

        private void SetupRegionWithNeuron(out RegionModel addedRegion, out NeuronModel addedNeuron)
        {
            addedRegion = SetupRegion(m_applier, m_model, 1);
            addedNeuron = new NeuronModel(1, "neuronType", addedRegion, Vector3.One);

            ResponseMessage diff = ModelResponseBuilder.Build(addedNeurons: new List<NeuronModel> {addedNeuron});
            ApplyModelDiff(diff);
        }

        [Fact]
        public void AddsNewSynapse()
        {
            RegionModel addedRegion;
            SynapseModel addedSynapse;
            SetupRegionWithSynapse(out addedRegion, out addedSynapse);

            RegionModel region = m_model.Regions.First();

            ComparisonResult result = CompareLogic(SynapseComparisonConfig)
                .Compare(addedSynapse, region.Synapses.First());

            Assert.True(result.AreEqual, result.DifferencesString);

            Assert.Equal(region.Synapses.First(), region.Neurons.First().Outputs.Values.First());
            Assert.Equal(region.Synapses.First(), region.Neurons.Last().Inputs.Values.First());
        }

        [Fact]
        public void SpikesSynapse()
        {
            RegionModel addedRegion;
            SynapseModel addedSynapse;

            SetupRegionWithSynapse(out addedRegion, out addedSynapse);

            ResponseMessage diff = ModelResponseBuilder.Build(spikedSynapses: new List<SynapseModel> {addedSynapse});

            ApplyModelDiff(diff);

            var region = m_model.Regions[addedRegion.Index];
            Assert.True(region.Synapses.First().IsSpiked);
        }

        [Fact]
        public void RemovesSynapse()
        {
            RegionModel addedRegion;
            SynapseModel addedSynapse;

            SetupRegionWithSynapse(out addedRegion, out addedSynapse);

            ResponseMessage diff = ModelResponseBuilder.Build(removedSynapses: new List<SynapseModel> {addedSynapse});

            ApplyModelDiff(diff);

            var region = m_model.Regions[addedRegion.Index];
            Assert.Empty(region.Synapses);
            Assert.Empty(region.Neurons[1].Outputs);
            Assert.Empty(region.Neurons[2].Inputs);
        }

        // Check that synapses are repositioned when one of their neuron is.
        [Fact]
        public void RepositionsSynapse()
        {
            RegionModel addedRegion;
            SynapseModel addedSynapse;

            SetupRegionWithSynapse(out addedRegion, out addedSynapse);

            NeuronModel neuron1 = m_model.Regions.First().Neurons.First();
            NeuronModel neuron2 = m_model.Regions.First().Neurons.Last();

            SynapseModel synapse = m_model.Regions.First().Synapses.First();

            neuron1.Position *= 2;
            ResponseMessage diff = ModelResponseBuilder.Build(repositionedNeurons: new List<NeuronModel> {neuron1});
            ApplyModelDiff(diff);

            Assert.Equal(neuron1.Position, synapse.Position);

            neuron2.Position *= 3;
            diff = ModelResponseBuilder.Build(repositionedNeurons: new List<NeuronModel> {neuron2});
            ApplyModelDiff(diff);

            Assert.Equal(neuron2.Position, neuron1.Position + synapse.TargetPosition);
        }

        [Fact]
        public void LoadsObserverData()
        {
            var observerDefinition = new ObserverDefinition(1, 1, "foo");
            var metadata = new int[] {28, 28, 3};
            var plainData = new byte[] {1, 2, 3};
            var floatData = new float[] {4.5f, 6.7f};

            var observer = new ObserverDataContainer(
                observerDefinition, new ObserverData(metadata: metadata, plainData: plainData, floatData: floatData));

            ResponseMessage diff = ModelResponseBuilder.Build(observers: new List<ObserverDataContainer> {observer});
            ApplyModelDiff(diff);

            var observerData = m_model.Observers.Values.First();
            Assert.Equal(metadata, observerData.Metadata);
            Assert.Equal(plainData, observerData.PlainData);
            Assert.Equal(floatData, observerData.FloatData);
        }

        private void SetupRegionWithSynapse(out RegionModel addedRegion, out SynapseModel addedSynapse)
        {
            addedRegion = SetupRegion(m_applier, m_model, 1);

            var addedNeuron1 = new NeuronModel(1, "neuronType", addedRegion, Vector3.One);
            var addedNeuron2 = new NeuronModel(2, "neuronType", addedRegion, Vector3.UnitY);

            addedSynapse = new SynapseModel(addedRegion, addedNeuron1, addedRegion, addedNeuron2);

            ResponseMessage diff = ModelResponseBuilder.Build(
                addedNeurons: new List<NeuronModel> {addedNeuron1, addedNeuron2},
                addedSynapses: new List<SynapseModel> {addedSynapse});

            ApplyModelDiff(diff);
        }

        private void ApplyModelDiff(ResponseMessage diff)
        {
            m_applier.ApplyModelDiff(m_model, diff.GetResponse(new ModelResponse()));
        }
    }
}
