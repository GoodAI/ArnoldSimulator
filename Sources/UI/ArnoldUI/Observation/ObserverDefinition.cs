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

        public override bool Equals(object obj)
        {
            var other = obj as ObserverDefinition;
            if (other != null)
                return GetHashCode().Equals(other.GetHashCode());

            return ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;

                hash = hash*31 + NeuronIndex.GetHashCode();
                hash = hash*31 + RegionIndex.GetHashCode();
                hash = hash*31 + Type.GetHashCode();

                return hash;
            }
        }
    }
}
