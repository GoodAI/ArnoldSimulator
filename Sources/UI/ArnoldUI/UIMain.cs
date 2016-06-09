using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Project;
using GoodAI.Logging;
using Region = GoodAI.Arnold.Project.Region;

namespace ArnoldUI
{
    // TODO(HonzaS): This class will only start making real sense once there's also Designer besides Conductor.
    public class UIMain : IDisposable
    {
        // Injected.
        public ILog Log { get; set; } = NullLogger.Instance;

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

        public IConductor Conductor { get; }
        public IDesigner Designer { get; }

        public UIMain(IConductor conductor, IDesigner designer)
        {
            // TODO: This should move into the Designer.
            AgentBlueprint = new AgentBlueprint();
            AgentBlueprint.Brain.Regions.Add(new Region
            {
                Location = new PointF(100, 100)
            });

            Conductor = conductor;
            Designer = designer;
        }

        public void VisualizationClosed()
        {
            //Simulation.Clear();
        }

        public async void ConnectToCoreAsync()
        {
            // TODO(HonzaS): endPoint = null means local.
            await Conductor.ConnectToCoreAsync(endPoint: null);
        }

        public async Task StartSimulationAsync()
        {
            try
            {
                await Conductor.LoadBlueprintAsync(Designer.Blueprint);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Blueprint loading failed.");
                throw;
            }

            await Conductor.StartSimulationAsync();
        }

        public async Task PauseSimulationAsync()
        {
            await Conductor.PauseSimulationAsync();
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
            Conductor.ShutdownAsync();
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
