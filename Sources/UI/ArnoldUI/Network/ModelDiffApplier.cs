using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Graphics.Models;
using OpenTK;

namespace GoodAI.Arnold.Network
{
    public interface IModelDiffApplier
    {
        void ApplyModelDiff(SimulationModel model, ModelResponse diff);
    }

    public class ModelDiffApplier : IModelDiffApplier
    {
        public void ApplyModelDiff(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.AddedRegionsLength; i++)
            {
                Region addedRegion = diff.GetAddedRegions(i);

                Position lowerBound = addedRegion.LowerBound;
                Position upperBound = addedRegion.UpperBound;

                var size = new Vector3(upperBound.X-lowerBound.X, upperBound.Y-lowerBound.Y, upperBound.Z-lowerBound.Z);

                Vector3 position = new Vector3(lowerBound.X, lowerBound.Y, lowerBound.Z) + size/2;

                model.Regions.Add(new RegionModel(addedRegion.Name, addedRegion.Type, position, size));
            }
        }
    }
}
