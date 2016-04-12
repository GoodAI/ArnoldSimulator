using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Project;
using GoodAI.Arnold.Simulation;
using Region = GoodAI.Arnold.Project.Region;

namespace ArnoldUI
{
    public class UIMain
    {
        public AgentBlueprint AgentBlueprint { get; }

        public SimulationProxy Simulation { get; set; }

        public UIMain()
        {
            AgentBlueprint = new AgentBlueprint();
            AgentBlueprint.Brain.Regions.Add(new Region
            {
                Location = new PointF(100, 100)
            });
        }

        public void VisualizationClosed()
        {
            Simulation.Clear();
        }

        public void StartSimulation()
        {
            Simulation.LoadBlueprint(AgentBlueprint);
        }
    }
}
