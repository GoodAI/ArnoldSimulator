using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Forms;
using GoodAI.Arnold.Observation;
using GoodAI.Arnold.Project;
using GoodAI.Arnold.Visualization.Models;
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
        public ISet<ObserverHandle> Observers { get; set; }

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
            Observers = new HashSet<ObserverHandle>();
        }

        public void VisualizationClosed()
        {
            //Simulation.Clear();
        }

        public async Task ConnectToCoreAsync()
        {
            // TODO(HonzaS): endPoint = null means local.
            await Conductor.ConnectToCoreAsync(endPoint: null);
        }

        public async Task StartSimulationAsync()
        {
            if (Conductor.CoreState == CoreState.Empty)
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

        public void ToggleObserver(NeuronModel neuron)
        {
            const string observerType = "greyscale";
            var definition = new ObserverDefinition(neuron.Index, neuron.RegionModel.Index, observerType);
            if (neuron.Picked)
                OpenObserver(definition);
            else
                CloseObserver(definition);
        }

        private void OpenObserver(ObserverDefinition definition)
        {
            // TODO(HonzaS): Factory + injection.
            var observer = new GreyscaleObserver(definition, Conductor.ModelProvider);
            observer.Log = Log;
            var form = new ObserverForm(observer);

            var handle = new ObserverHandle(observer, form);
            Observers.Add(handle);
            form.Show();

            RefreshObserverRequests();
        }

        private void CloseObserver(ObserverDefinition definition)
        {
            ObserverHandle handle = Observers.FirstOrDefault(observerForm => Equals(observerForm.Observer.Definition, definition));
            if (handle == null)
            {
                Log.Warn("Observer with {@definition} not found, cannot close", definition);
                return;
            }

            handle.Dispose();
            Observers.Remove(handle);

            RefreshObserverRequests();
        }

        private void RefreshObserverRequests()
        {
            Conductor.ModelProvider.ObserverRequests =
                Observers.Select(observerHandle => observerHandle.Definition).ToList();
        }
    }
}
