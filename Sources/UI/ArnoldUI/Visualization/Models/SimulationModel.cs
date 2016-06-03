using System;
using System.Collections.Generic;
using System.Linq;

namespace GoodAI.Arnold.Visualization.Models
{
    public sealed class SimulationModel : CompositeModel<IModel>
    {
        public CompositeModel<ConnectionModel> Connections { get; }
        public CompositeLookupModel<uint, RegionModel> Regions { get; }

        public SimulationModel()
        {
            Regions = new CompositeLookupModel<uint, RegionModel>();
            Connections = new CompositeModel<ConnectionModel>();

            AddChild(Regions);
            AddChild(Connections);
        }
    }
}