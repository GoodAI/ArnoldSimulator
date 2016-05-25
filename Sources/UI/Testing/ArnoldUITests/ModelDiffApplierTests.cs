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
            MembersToInclude = new List<string>
            {
                "Name",
                "Type",
                "Position",
                "Size"
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

            Assert.Equal((uint) 4, region.InputConnectors.First().SlotCount);
            Assert.Equal((uint) 3, region.OutputConnectors.First().SlotCount);
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

            Assert.Equal(region1.OutputConnectors.First().Connection, region2.InputConnectors.First().Connection);
            Assert.Equal(region2.InputConnectors.First().Connection.From, region1.OutputConnectors.First());
        }
    }
}
