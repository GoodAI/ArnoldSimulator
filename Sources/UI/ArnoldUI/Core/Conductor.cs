using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Project;
using GoodAI.Logging;

namespace GoodAI.Arnold.Core
{
    public interface IConductor : IDisposable
    {
        ICoreProxy CoreProxy { get; }

        event EventHandler<StateChangeFailedEventArgs> StateChangeFailed;
        event EventHandler<StateChangedEventArgs> StateChanged;

        void ConnectToCore(EndPoint endPoint = null);
        void Disconnect();
        void Shutdown();
        void LoadBlueprint(AgentBlueprint blueprint);
        void StartSimulation(uint stepsToRun = 0);
        void PauseSimulation();
        void KillSimulation();

        bool IsConnected { get; }

        CoreState CoreState { get; }
        ICoreLink CoreLink { get; }

        IModelProvider ModelProvider { get; }

        void PerformBrainStep();
    }

    public class SimulationInstanceEventArgs : EventArgs
    {
        private ICoreProxy CoreProxy { get; set; }

        public SimulationInstanceEventArgs(ICoreProxy coreProxy)
        {
            CoreProxy = coreProxy;
        }
    }

    public class Conductor : IConductor
    {
        // Injected.
        public ILog Log { get; set; } = NullLogger.Instance;

        public event EventHandler<StateChangedEventArgs> StateChanged;
        public event EventHandler<StateChangeFailedEventArgs> StateChangeFailed;

        private readonly ICoreProcessFactory m_coreProcessFactory;
        private ICoreProcess m_process;

        private readonly ICoreLinkFactory m_coreLinkFactory;

        private readonly ICoreProxyFactory m_coreProxyFactory;
        private readonly IModelUpdaterFactory m_modelUpdaterFactory;

        public ICoreProxy CoreProxy { get; private set; }
        public IModelProvider ModelProvider { get; }

        private readonly ICoreControllerFactory m_coreControllerFactory;

        public Conductor(ICoreProcessFactory coreProcessFactory, ICoreLinkFactory coreLinkFactory,
            ICoreControllerFactory coreControllerFactory, ICoreProxyFactory coreProxyFactory,
            IModelUpdaterFactory modelUpdaterFactory, IModelProviderFactory modelProviderFactory)
        {
            m_coreProcessFactory = coreProcessFactory;
            m_coreLinkFactory = coreLinkFactory;
            m_coreControllerFactory = coreControllerFactory;
            m_coreProxyFactory = coreProxyFactory;
            m_modelUpdaterFactory = modelUpdaterFactory;

            ModelProvider = modelProviderFactory.Create(this);
        }

        public void ConnectToCore(EndPoint endPoint = null)
        {
            if (CoreProxy != null)
            {
                Log.Error("Cannot connect to core, there is still a core proxy present");
                throw new InvalidOperationException(
                    "There is still a core proxy present");
            }

            if (endPoint == null)
            {
                if (m_process == null)
                {
                    // TODO(HonzaS): Move this elsewhere when we have finer local process control.
                    Log.Info("Starting a local core process");
                    m_process = m_coreProcessFactory.Create();
                }

                endPoint = m_process.EndPoint;
            }

            if (endPoint == null)
            {
                Log.Error("Endpoint not set up, cannot connect to core");
                throw new InvalidOperationException("Endpoint not set");
            }

            Log.Info("Connecting to Core running at {hostname:l}:{port}", endPoint.Hostname, endPoint.Port);
            ICoreLink coreLink = m_coreLinkFactory.Create(endPoint);
            // TODO(HonzaS): Check here if we can connect to the core so that we could abort immediatelly.
            CoreLink = coreLink;

            // TODO(HonzaS): Move these inside the factory method.
            ICoreController coreController = m_coreControllerFactory.Create(coreLink);
            IModelUpdater modelUpdater = m_modelUpdaterFactory.Create(coreLink, coreController);

            CoreProxy = m_coreProxyFactory.Create(coreLink, coreController, modelUpdater);

            RegisterCoreEvents();
        }

        // TODO(HonzaS): Remove this at a later stage, it's here only for testing purposes.
        public ICoreLink CoreLink { get; private set; }

        private void RegisterCoreEvents()
        {
            CoreProxy.StateChanged += OnCoreStateChanged;
            CoreProxy.StateChangeFailed += OnCoreStateChangeFailed;
            CoreProxy.CommandTimedOut += OnCoreCommandTimedOut;
            CoreProxy.Disconnected += OnDisconnected;
        }

        private void UnregisterCoreEvents()
        {
            CoreProxy.StateChanged -= OnCoreStateChanged;
            CoreProxy.StateChangeFailed -= OnCoreStateChangeFailed;
            CoreProxy.CommandTimedOut -= OnCoreCommandTimedOut;
            CoreProxy.Disconnected -= OnDisconnected;
        }

        public void Disconnect()
        {
            if (CoreProxy == null)
                return;

            // TODO(HonzaS): We might want to ask the user if he wants to kill the local process when disconnecting.
            //if (m_process != null)
            //{
            //}

            FinishDisconnect();
        }

        public void Shutdown()
        {
            if (CoreProxy != null)
            {
                Log.Info("Shutting down the core");
                CoreProxy.Shutdown();
                return;
            }

            AfterShutdown();
        }

        private void OnCoreCommandTimedOut(object sender, TimeoutActionEventArgs args)
        {
            Log.Debug("Core command {command} timed out", args.Command);
            if (args.Command == CommandType.Shutdown)
            {
                args.Action = TimeoutAction.Cancel;

                AfterShutdown();
            }
        }

        private void OnDisconnected(object sender, EventArgs args)
        {
            Disconnect();
        }

        private void AfterShutdown()
        {
            if (m_process != null)
            {
                m_process.Dispose();
                m_process = null;
            }

            FinishDisconnect();
        }

        private void FinishDisconnect()
        {
            CoreState oldState = CoreProxy.State;

            UnregisterCoreEvents();

            CoreProxy.Dispose();
            CoreProxy = null;

            StateChanged?.Invoke(this, new StateChangedEventArgs(oldState, CoreState.Disconnected));
            Log.Info("Disconnected from core");
        }

        public void LoadBlueprint(AgentBlueprint blueprint)
        {
            // TODO(HonzaS): Add this when blueprints come into play.
            Log.Info("Loading blueprint");
            CoreProxy.LoadBlueprint(blueprint);
        }

        private void OnCoreStateChanged(object sender, StateChangedEventArgs stateChangedEventArgs)
        {
            if (stateChangedEventArgs.CurrentState == CoreState.ShuttingDown)
                AfterShutdown();

            Log.Debug("Core state changed: {previousState} -> {currentState}", stateChangedEventArgs.PreviousState, stateChangedEventArgs.CurrentState);

            StateChanged?.Invoke(this, stateChangedEventArgs);
        }

        private void OnCoreStateChangeFailed(object sender, StateChangeFailedEventArgs stateChangeFailedEventArgs)
        {
            Log.Warn("Core state change failed with: {error}", stateChangeFailedEventArgs.ErrorMessage);
            StateChangeFailed?.Invoke(this, stateChangeFailedEventArgs);
        }

        public void StartSimulation(uint stepsToRun = 0)
        {
            if (CoreProxy == null)
            {
                Log.Error("Cannot start simulation, not connected to a core");
                throw new InvalidOperationException("Core proxy does not exist, cannot start");
            }

            Log.Info("Starting simulation");
            CoreProxy.Run(stepsToRun);
        }

        public void PauseSimulation()
        {
            Log.Info("Pausing simulation");
            CoreProxy.Pause();
        }

        public void KillSimulation()
        {
            throw new NotImplementedException();
        }

        public bool IsConnected => CoreProxy != null;

        public CoreState CoreState => CoreProxy?.State ?? CoreState.Disconnected;

        public void PerformBrainStep()
        {
            if (CoreProxy == null)
            {
                Log.Error("Cannot perform brain step, not connected to a core");
                throw new InvalidOperationException("Core proxy does not exist, cannot step");
            }

            Log.Info("Performing brain step");
            CoreProxy.Run(1);
        }

        public void Dispose()
        {
            Log.Debug("Disposing conductor");
            if (m_process != null)
            {
                Shutdown();
                // Process will wait for a while to let the core process finish gracefully.
                // TODO(HonzaS): Wait for the core to finish before killing.
                // When the local simulation is auto-saving before closing, it might take a long time.
                m_process.Dispose();
            }
            else
            {
                Disconnect();
            }
        }
    }
}
