using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Graphics.Models;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Network.Messages;
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

        private static RegionModel AddRegion(ModelDiffApplier applier, SimulationModel model, uint id)
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
                "Id",
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
            RegionModel addedRegion = AddRegion(m_applier, m_model, 1);

            Assert.Equal(1, m_model.Regions.Count());
            RegionModel generatedRegion = m_model.Regions.First();

            ComparisonResult result = CompareLogic(RegionComparisonConfig).Compare(addedRegion, generatedRegion);

            Assert.True(result.AreEqual, result.DifferencesString);
        }

        [Fact]
        public void RemovesRegion()
        {
            RegionModel addedRegion = AddRegion(m_applier, m_model, 1);

            ResponseMessage diff = ModelResponseBuilder.Build(removedRegions: new List<RegionModel> {addedRegion});

            m_applier.ApplyModelDiff(m_model, diff.GetResponse(new ModelResponse()));

            Assert.Empty(m_model.Regions);
        }

        [Fact]
        public void RepositionsRegion()
        {
            RegionModel addedRegion = AddRegion(m_applier, m_model, 1);

            addedRegion.Position = Vector3.UnitZ;
            ResponseMessage diff = ModelResponseBuilder.Build(repositionedRegions: new List<RegionModel> {addedRegion});

            m_applier.ApplyModelDiff(m_model, diff.GetResponse(new ModelResponse()));

            Assert.Equal(Vector3.UnitZ, m_model.Regions.First().Position);
        }

        [Fact]
        public void AddsNewConnector()
        {
            RegionModel addedRegion = AddRegion(m_applier, m_model, 1);

            ConnectorModel addedConnector1 = new InputConnectorModel(addedRegion, "input", 4);
            ConnectorModel addedConnector2 = new OutputConnectorModel(addedRegion, "output", 3);

            ResponseMessage diff =
                ModelResponseBuilder.Build(addedConnectors: new List<ConnectorModel> {addedConnector1, addedConnector2});
            m_applier.ApplyModelDiff(m_model, diff.GetResponse(new ModelResponse()));

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
            RegionModel addedRegion = AddRegion(m_applier, m_model, 1);

            ConnectorModel addedConnector = new InputConnectorModel(addedRegion, "input", 4);

            ResponseMessage diff =
                ModelResponseBuilder.Build(addedConnectors: new List<ConnectorModel> {addedConnector});

            m_applier.ApplyModelDiff(m_model, diff.GetResponse(new ModelResponse()));

            diff = ModelResponseBuilder.Build(removedConnectors: new List<ConnectorModel> {addedConnector});

            m_applier.ApplyModelDiff(m_model, diff.GetResponse(new ModelResponse()));

            var region = m_model.Regions.First();
            Assert.Empty(region.InputConnectors);
        }

        [Fact]
        public void AddsNewConnection()
        {
            RegionModel addedRegion1 = AddRegion(m_applier, m_model, 1);
            RegionModel addedRegion2 = AddRegion(m_applier, m_model, 2);

            var addedConnector1 = new OutputConnectorModel(addedRegion1, "output", 3);
            var addedConnector2 = new InputConnectorModel(addedRegion2, "input", 4);

            var addedConnection = new ConnectionModel(addedConnector1, addedConnector2);

            ResponseMessage diff =
                ModelResponseBuilder.Build(
                    addedConnectors: new List<ConnectorModel> {addedConnector1, addedConnector2},
                    addedConnections: new List<ConnectionModel> {addedConnection});
            m_applier.ApplyModelDiff(m_model, diff.GetResponse(new ModelResponse()));

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
            RegionModel addedRegion1 = AddRegion(m_applier, m_model, 1);
            RegionModel addedRegion2 = AddRegion(m_applier, m_model, 2);

            var addedConnector1 = new OutputConnectorModel(addedRegion1, "output", 3);
            var addedConnector2 = new InputConnectorModel(addedRegion2, "input", 4);

            var addedConnection = new ConnectionModel(addedConnector1, addedConnector2);

            ResponseMessage diff =
                ModelResponseBuilder.Build(
                    addedConnectors: new List<ConnectorModel> {addedConnector1, addedConnector2},
                    addedConnections: new List<ConnectionModel> {addedConnection});

            m_applier.ApplyModelDiff(m_model, diff.GetResponse(new ModelResponse()));

            diff = ModelResponseBuilder.Build(removedConnections: new List<ConnectionModel> {addedConnection});

            m_applier.ApplyModelDiff(m_model, diff.GetResponse(new ModelResponse()));

            var region1 = m_model.Regions.First();
            var region2 = m_model.Regions.Skip(1).First();

            Assert.Empty(region1.OutputConnectors.First().Connections);
            Assert.Empty(region2.InputConnectors.First().Connections);
            Assert.Empty(m_model.Connections);
        }

        [Fact]
        public void AddsNewNeuron()
        {
            RegionModel addedRegion = AddRegion(m_applier, m_model, 1);

            var addedNeuron = new NeuronModel(1, "neuronType", addedRegion, Vector3.One);

            ResponseMessage diff = ModelResponseBuilder.Build(addedNeurons: new List<NeuronModel> {addedNeuron});
            m_applier.ApplyModelDiff(m_model, diff.GetResponse(new ModelResponse()));

            RegionModel region = m_model.Regions.First();

            ComparisonResult result = CompareLogic(NeuronComparisonConfig)
                .Compare(addedNeuron, region.Neurons.First());

            Assert.True(result.AreEqual, result.DifferencesString);
        }

        [Fact]
        public void RepositionsNeuron()
        {
            RegionModel addedRegion = AddRegion(m_applier, m_model, 1);
            var addedNeuron = new NeuronModel(1, "neuronType", addedRegion, Vector3.One);

            ResponseMessage diff = ModelResponseBuilder.Build(addedNeurons: new List<NeuronModel> {addedNeuron});

            m_applier.ApplyModelDiff(m_model, diff.GetResponse(new ModelResponse()));

            addedNeuron.Position = Vector3.UnitZ;
            diff = ModelResponseBuilder.Build(repositionedNeurons: new List<NeuronModel> {addedNeuron});

            m_applier.ApplyModelDiff(m_model, diff.GetResponse(new ModelResponse()));

            Assert.Equal(Vector3.UnitZ, m_model.Regions[addedRegion.Index].Neurons[addedNeuron.Index].Position);
        }

        [Fact]
        public void RemovesNeuron()
        {
            RegionModel addedRegion = AddRegion(m_applier, m_model, 1);
            var addedNeuron = new NeuronModel(1, "neuronType", addedRegion, Vector3.One);

            ResponseMessage diff = ModelResponseBuilder.Build(addedNeurons: new List<NeuronModel> {addedNeuron});

            m_applier.ApplyModelDiff(m_model, diff.GetResponse(new ModelResponse()));

            diff = ModelResponseBuilder.Build(removedNeurons: new List<NeuronModel> {addedNeuron});

            m_applier.ApplyModelDiff(m_model, diff.GetResponse(new ModelResponse()));

            Assert.Empty(m_model.Regions[addedRegion.Index].Neurons);
        }

        [Fact]
        public void AddsNewSynapse()
        {
            RegionModel addedRegion = AddRegion(m_applier, m_model, 1);

            var addedNeuron1 = new NeuronModel(1, "neuronType", addedRegion, Vector3.One);
            var addedNeuron2 = new NeuronModel(2, "neuronType", addedRegion, Vector3.UnitY);

            var addedSynapse = new SynapseModel(addedRegion, addedNeuron1, addedRegion, addedNeuron2);

            ResponseMessage diff = ModelResponseBuilder.Build(
                addedNeurons: new List<NeuronModel> {addedNeuron1, addedNeuron2},
                addedSynapses: new List<SynapseModel> {addedSynapse});

            m_applier.ApplyModelDiff(m_model, diff.GetResponse(new ModelResponse()));

            RegionModel region = m_model.Regions.First();

            ComparisonResult result = CompareLogic(SynapseComparisonConfig)
                .Compare(addedSynapse, region.Synapses.First());

            Assert.True(result.AreEqual, result.DifferencesString);
        }

        [Fact]
        public void SpikesSynapse()
        {
            RegionModel addedRegion = AddRegion(m_applier, m_model, 1);

            var addedNeuron1 = new NeuronModel(1, "neuronType", addedRegion, Vector3.One);
            var addedNeuron2 = new NeuronModel(2, "neuronType", addedRegion, Vector3.UnitY);

            var addedSynapse = new SynapseModel(addedRegion, addedNeuron1, addedRegion, addedNeuron2);

            ResponseMessage diff = ModelResponseBuilder.Build(
                addedNeurons: new List<NeuronModel> {addedNeuron1, addedNeuron2},
                addedSynapses: new List<SynapseModel> {addedSynapse});

            m_applier.ApplyModelDiff(m_model, diff.GetResponse(new ModelResponse()));

            diff = ModelResponseBuilder.Build(spikedSynapses: new List<SynapseModel> {addedSynapse});

            m_applier.ApplyModelDiff(m_model, diff.GetResponse(new ModelResponse()));

            var region = m_model.Regions[addedRegion.Index];
            Assert.True(region.Synapses.First().IsSpiked);
        }

        [Fact]
        public void RemovesSynapse()
        {
            RegionModel addedRegion = AddRegion(m_applier, m_model, 1);

            var addedNeuron1 = new NeuronModel(1, "neuronType", addedRegion, Vector3.One);
            var addedNeuron2 = new NeuronModel(2, "neuronType", addedRegion, Vector3.UnitY);

            var addedSynapse = new SynapseModel(addedRegion, addedNeuron1, addedRegion, addedNeuron2);

            ResponseMessage diff = ModelResponseBuilder.Build(
                addedNeurons: new List<NeuronModel> {addedNeuron1, addedNeuron2},
                addedSynapses: new List<SynapseModel> {addedSynapse});

            m_applier.ApplyModelDiff(m_model, diff.GetResponse(new ModelResponse()));

            diff = ModelResponseBuilder.Build(removedSynapses: new List<SynapseModel> {addedSynapse});

            m_applier.ApplyModelDiff(m_model, diff.GetResponse(new ModelResponse()));

            var region = m_model.Regions[addedRegion.Index];
            Assert.Empty(region.Synapses);
            Assert.Empty(region.Neurons[addedNeuron1.Index].Outputs);
        }
    }
}
