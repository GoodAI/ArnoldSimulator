using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArnoldUI.Core;
using ArnoldUI.Network;
using ArnoldUI.Simulation;
using GoodAI.Arnold.Project;
using GoodAI.Arnold.Simulation;
using Region = GoodAI.Arnold.Project.Region;

namespace ArnoldUI
{
    // TODO(HonzaS): This class will only start making real sense once there's also Designer besides Conductor.
    public class UIMain
    {
        public event EventHandler<StateUpdatedEventArgs> SimulationStateUpdated
        {
            add { Conductor.SimulationStateUpdated += value; }
            remove { Conductor.SimulationStateUpdated -= value; }
        }
        public event EventHandler<StateChangeFailedEventArgs> SimulationStateChangeFailed
        {
            add { Conductor.SimulationStateChangeFailed += value; }
            remove { Conductor.SimulationStateChangeFailed -= value; }
        }

        public AgentBlueprint AgentBlueprint { get; }

        public IConductor Conductor { get; set; }

        public ISimulation Simulation => Conductor.Simulation;

        public UIMain()
        {
            // TODO: This should move into the Designer.
            AgentBlueprint = new AgentBlueprint();
            AgentBlueprint.Brain.Regions.Add(new Region
            {
                Location = new PointF(100, 100)
            });

            // TODO(HonzaS): Resolve from container.
            Conductor = new Conductor(new CoreProxyFactory(), new CoreLinkFactory(), new SimulationFactory());
        }

        public void VisualizationClosed()
        {
            //Simulation.Clear();
        }

        public void StartSimulation()
        {
            // TODO(HonzaS): Here will be some logic governing local/remote core setup.
            if (Conductor.Simulation == null)
                Conductor.Setup();

            // The play button has been pushed.
            Conductor.StartSimulation();
        }

        public void KillSimulation()
        {
            if (Conductor.Simulation == null)
                return;

            Conductor.KillSimulation();
        }
    }
}
