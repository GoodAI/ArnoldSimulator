using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using GoodAI.Arnold.Graphics;
using GoodAI.Arnold.Graphics.Models;
using GoodAI.Arnold.Project;
using OpenTK;
using OpenTK.Platform.Windows;

namespace GoodAI.Arnold.Core
{
    public sealed class SimulationModel : CompositeModel<IModel>
    {
        public CompositeModel<ConnectionModel> Connections { get; set; }
        public CompositeModel<RegionModel> Regions { get; set; }

        public SimulationModel()
        {
            Regions = new CompositeModel<RegionModel>();
            Connections = new CompositeModel<ConnectionModel>();

            AddChild(Regions);
            AddChild(Connections);
        }
    }
}