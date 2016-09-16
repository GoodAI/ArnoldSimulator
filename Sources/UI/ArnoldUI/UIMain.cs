using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Communication;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Forms;
using GoodAI.Arnold.Observation;
using GoodAI.Arnold.Project;
using GoodAI.Arnold.Properties;
using GoodAI.Arnold.UserSettings;
using GoodAI.Arnold.Visualization;
using GoodAI.Arnold.Visualization.Models;
using GoodAI.Logging;
using Newtonsoft.Json.Linq;
using Region = GoodAI.Arnold.Project.Region;

namespace GoodAI.Arnold
{
    public class UIMain : IDisposable
    {
        private string m_observerType = "FloatTensor";

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

        public event EventHandler<FileStatusChangedArgs> FileStatusChanged;

        public AgentBlueprint AgentBlueprint { get; }

        public IConductor Conductor { get; }
        public IDesigner Designer { get; }
        public ISet<ObserverHandle> Observers { get; set; }

        public FileStatus FileStatus { get; }

        public UIMain(IConductor conductor, IDesigner designer)
        {
            // TODO: This should move into the Designer.
            AgentBlueprint = new AgentBlueprint();
            AgentBlueprint.Brain.Regions.Add(new Region
            {
                Location = new PointF(100, 100)
            });

            FileStatus = new FileStatus();
            Conductor = conductor;
            Designer = designer;
            Observers = new HashSet<ObserverHandle>();

            Designer.BlueprintChanged += DesignerOnBlueprintChanged;
        }

        private void DesignerOnBlueprintChanged(object sender, BlueprintChangedArgs blueprintChangedArgs)
        {
            FileStatus.IsSaveNeeded = blueprintChangedArgs.ChangesMade;
            FileStatusChanged?.Invoke(this, new FileStatusChangedArgs(FileStatus));
        }

        public void Initialize()
        {
            var lastOpenedFile = Settings.Default.LastOpenedFile;
            if (!string.IsNullOrEmpty(lastOpenedFile))
                OpenBlueprint(lastOpenedFile);
            else
                Designer.SetBlueprint(Resources.DefaultBlueprint);
        }

        public void VisualizationClosed()
        {
            foreach (var handle in Observers.ToList())
                CloseObserver(handle.Definition);
        }

        public async Task ConnectToCoreAsync()
        {
            // TODO(HonzaS): endPoint = null means local.
            await Conductor.ConnectToCoreAsync(endPoint: null);
        }

        public async Task StartSimulationAsync()
        {
            if (Conductor.CoreState == CoreState.Empty)
                await LoadBlueprintAsync();

            await Conductor.StartSimulationAsync();
        }

        public async Task ClearBlueprintAsync()
        {
            await Conductor.ClearBlueprintAsync();
        }

        public async Task LoadBlueprintAsync()
        {
            try
            {
                await Conductor.LoadBlueprintAsync(Designer.Blueprint);
                await Conductor.UpdateConfigurationAsync(coreConfig => coreConfig.System.BrainStepsPerBodyStep = 10);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Blueprint loading failed.");
                throw;
            }
        }

        public async Task PauseSimulationAsync()
        {
            await Conductor.PauseSimulationAsync();
        }

        public async Task PerformBrainStepAsync()
        {
            await Conductor.PerformBrainStepAsync();
        }

        public async Task RunToBodyStepAsync()
        {
            await Conductor.RunToBodyStepAsync();
        }

        public async Task UpdateCoreConfig(Action<CoreConfiguration> updateConfig)
        {
            try
            {
                await Conductor.UpdateConfigurationAsync(updateConfig);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Core configuration update failed.");
                throw;
            }
        }

        public void Disconnect()
        {
            // TODO(HonzaS): Change this to Disconnect when we allow that.
            Conductor.ShutdownAsync();
        }

        public void Dispose()
        {
            Conductor.Dispose();
        }

        public void OpenObserver(NeuronModel neuron, Scene scene)
        {
            ObserverDefinition definition = CreateObserverDefinition(neuron);
            // TODO(HonzaS): Factory + injection.
            var observer = new CanvasObserver(definition, Conductor.ModelProvider);
            observer.Log = Log;
            var form = new ObserverForm(this, observer);
            form.Text = $"Neuron {neuron.Index}, region {neuron.RegionModel.Index} - '{definition.Type}'";

            var handle = new ObserverHandle(observer, form, scene);
            Observers.Add(handle);
            form.Show();

            RefreshObserverRequests();
        }

        private ObserverDefinition CreateObserverDefinition(NeuronModel neuron)
        {
            var definition = new ObserverDefinition(neuron.Index, neuron.RegionModel.Index, m_observerType);
            return definition;
        }

        public void CloseObserver(NeuronModel neuron)
        {
            ObserverDefinition definition = CreateObserverDefinition(neuron);
            CloseObserver(definition);
        }

        public void CloseObserver(ObserverDefinition definition)
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

        public void OpenBlueprint(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Log.Warn($"File '{fileName}' not found");
                Designer.SetBlueprint(Resources.DefaultBlueprint);
                return;
            }
            var fileContent = File.ReadAllText(fileName);

            OpenedFileChanged(fileName);
            // Blueprint validity should be checked inside the Designer.
            Designer.SetBlueprint(fileContent, reset: true);
        }

        public void SaveBlueprint(string fileName = null)
        {
            if (fileName == null)
            {
                if (FileStatus.FileName == null)
                {
                    Log.Warn("Cannot save: {reason}", "No file name was given.");
                    return;
                }
                fileName = FileStatus.FileName;
            }

            try
            {
                File.WriteAllText(fileName, Designer.Blueprint);
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Cannot save: {reason}", ex.Message);
                return;
            }

            OpenedFileChanged(fileName);
        }

        private void OpenedFileChanged(string fileName)
        {
            FileStatus.FileName = fileName;
            // If fileName is null, it's a new file, which is not saved.
            // Otherwise, a file was opened and doesn't need immediate saving.
            FileStatus.IsSaveNeeded = fileName == null;
            FileStatusChanged?.Invoke(this, new FileStatusChangedArgs(FileStatus));
            AppSettings.SaveSettings(settings => settings.LastOpenedFile = fileName);
        }

        public void NewBlueprint()
        {
            Designer.SetBlueprint("");
            OpenedFileChanged(null);
        }
    }
}
