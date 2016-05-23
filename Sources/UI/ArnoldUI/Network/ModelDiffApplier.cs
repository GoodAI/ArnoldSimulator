using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Graphics.Models;
using GoodAI.Logging;
using OpenTK;

namespace GoodAI.Arnold.Network
{
    public interface IModelDiffApplier
    {
        void ApplyModelDiff(SimulationModel model, ModelResponse diff);
    }

    public class ModelDiffApplier : IModelDiffApplier
    {
        // Injected.
        public ILog Log { get; set; } = NullLogger.Instance;

        public void ApplyModelDiff(SimulationModel model, ModelResponse diff)
        {
            ApplyAddedRegions(model, diff);
            ApplyAddedNeurons(model, diff);
        }

        private static void ApplyAddedRegions(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.AddedRegionsLength; i++)
            {
                Region addedRegion = diff.GetAddedRegions(i);

                Position lowerBound = addedRegion.LowerBound;
                Position upperBound = addedRegion.UpperBound;

                var size = new Vector3(upperBound.X - lowerBound.X, upperBound.Y - lowerBound.Y, upperBound.Z - lowerBound.Z);

                Vector3 position = lowerBound.ToVector3() + size/2;

                model.AddChild(new RegionModel(addedRegion.Index, addedRegion.Name, addedRegion.Type, position, size));
            }
        }

        private void ApplyAddedNeurons(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.AddedNeuronsLength; i++)
            {
                Neuron neuron = diff.GetAddedNeurons(i);

                // TODO(HonzaS): Add a lookup table instead of this.
                RegionModel targetRegionModel = model.Models.FirstOrDefault(region => region.Index == neuron.RegionIndex);
                if (targetRegionModel == null)
                {
                    Log.Warn("Cannot add neuron {neuronId}, region with index {regionIndex} was not found", neuron.Id,
                        neuron.RegionIndex);
                    continue;
                }

                targetRegionModel.AddExpert(new ExpertModel(targetRegionModel, neuron.Position.ToVector3()));
            }
        }
    }
}
