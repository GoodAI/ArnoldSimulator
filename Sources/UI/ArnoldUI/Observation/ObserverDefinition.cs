using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Visualization.Models;

namespace GoodAI.Arnold.Observation
{
    public class ObserverDefinition
    {
        public uint NeuronIndex { get; }
        public uint RegionIndex { get; }
        public string Type { get; }

        public ObserverDefinition(uint neuronIndex, uint regionIndex, string type)
        {
            NeuronIndex = neuronIndex;
            RegionIndex = regionIndex;
            Type = type;
        }
    }
}
