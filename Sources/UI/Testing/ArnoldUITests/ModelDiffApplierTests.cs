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
        public const int MaxDifferences = 20;

        [Fact]
        public void AddsNewRegion()
        {
            var model = new SimulationModel();
            var applier = new ModelDiffApplier();

            var sourceRegion = new RegionModel(1, "foo", "bar", new Vector3(1, 2, 3), new Vector3(6, 5, 4));

            ResponseMessage diff = ModelResponseBuilder.Build(new List<RegionModel> {sourceRegion});
            applier.ApplyModelDiff(model, diff.GetResponse(new ModelResponse()));

            Assert.Equal(1, model.Models.Count());
            RegionModel generatedRegion = model.Models.First();

            CompareLogic compareLogic = new CompareLogic
            {
                Config = {
                    MaxDifferences = MaxDifferences,
                    MembersToInclude = new List<string>
                    {
                        "Name",
                        "Type",
                        "Position",
                        "Size"
                    }
                }
            };

            ComparisonResult result = compareLogic.Compare(sourceRegion, generatedRegion);

            Assert.True(result.AreEqual, result.DifferencesString);
        }
    }
}
