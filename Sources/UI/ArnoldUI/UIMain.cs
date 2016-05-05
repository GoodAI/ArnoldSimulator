using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Project;
using Region = GoodAI.Arnold.Project.Region;

namespace ArnoldUI
{
    // TODO(HonzaS): This class will only start making real sense once there's also Designer besides Conductor.
    public class UIMain : IDisposable
    {
        public event EventHandler<StateChangedEventArgs> SimulationStateChanged
        {
            add { Conductor.StateChanged += value; }
            remove { Conductor.StateChanged -= value; }
        }
        public event EventHandler<StateChangeFailedEventArgs> SimulationStateChangeFailed
        {
            add { Conductor.StateChangeFailed += value; }
            remove { Conductor.StateChangeFailed -= value; }
        }

        public AgentBlueprint AgentBlueprint { get; }

        public IConductor Conductor { get; set; }

        public ICoreProxy CoreProxy => Conductor.CoreProxy;

        public UIMain(IConductor conductor)
        {
            // TODO: This should move into the Designer.
            AgentBlueprint = new AgentBlueprint();
            AgentBlueprint.Brain.Regions.Add(new Region
            {
                Location = new PointF(100, 100)
            });

            Conductor = conductor;
        }

        public void VisualizationClosed()
        {
            //Simulation.Clear();
        }

        public void ConnectToCore()
        {
            // TODO(HonzaS): endPoint = null means local.
            Conductor.ConnectToCore(endPoint: null);
        }

        public void StartSimulation()
        {
            Conductor.StartSimulation();
        }

        public void PauseSimulation()
        {
            Conductor.PauseSimulation();
        }

        //public void KillSimulation()
        //{
        //    if (Conductor.CoreProxy == null)
        //        return;

        //    Conductor.KillSimulation();
        //}

        public void Disconnect()
        {
            // TODO(HonzaS): Change this to Disconnect when we allow that.
            Conductor.Shutdown();
        }

        public void Dispose()
        {
            Conductor.Dispose();
        }

        public void PerformBrainStep()
        {
            Conductor.PerformBrainStep();
        }
    }
}
