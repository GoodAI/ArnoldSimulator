using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Visualization.Models;
using OpenTK;
using Xunit;

namespace GoodAI.Arnold.UI.Tests
{
    public class ModelTests
    {
        [Fact]
        public void CorrectlyRepositionsNeurons()
        {
            var size = new Vector3(10, 20, 30);
            var region = new RegionModel(1, "foo", "bar", Vector3.Zero, size);
            var neuron1 = new NeuronModel(1, "foo", region, new Vector3(0, 0, 0));
            var neuron2 = new NeuronModel(1, "foo", region, new Vector3(1, 1, 1));

            Assert.Equal(-size/2 + new Vector3(RegionModel.RegionMargin), neuron1.Position);
            Assert.Equal(size/2 - new Vector3(RegionModel.RegionMargin), neuron2.Position);
        }
    }
}
