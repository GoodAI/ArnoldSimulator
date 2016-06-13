using System;
using System.Collections.Generic;
using System.Linq;
using GoodAI.Arnold.Observation;

namespace GoodAI.Arnold.Visualization.Models
{
    public sealed class SimulationModel : CompositeModel<IModel>
    {
        public CompositeModel<ConnectionModel> Connections { get; }
        public CompositeLookupModel<uint, RegionModel> Regions { get; }

        public IDictionary<ObserverDefinition, byte[]> Observers { get; }

        public SimulationModel()
        {
            Regions = new CompositeLookupModel<uint, RegionModel>();
            Connections = new CompositeModel<ConnectionModel>();
            Observers = new Dictionary<ObserverDefinition, byte[]>();

            AddChild(Regions);
            AddChild(Connections);
        }
    }
}