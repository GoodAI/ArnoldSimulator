using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Simulation;
using GoodAI.Arnold.Extensions;
using GoodAI.Arnold.Project;
using GoodAI.Net.ConverseSharp;

namespace GoodAI.Arnold.Core
{
    public interface IConductor
    {
        ICoreProxy CoreProxy { get; }

        event EventHandler<StateChangeFailedEventArgs> StateChangeFailed;
        event EventHandler<StateUpdatedEventArgs> StateUpdated;

        void ConnectToCore(EndPoint endPoint = null);
        void Disconnect();
        void Shutdown();
        void LoadBlueprint(AgentBlueprint blueprint);
        void StartSimulation(uint stepsToRun = 0);
        void PauseSimulation();
        void KillSimulation();

        bool IsConnected { get; }

        CoreState CoreState { get; }
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
        public event EventHandler<StateUpdatedEventArgs> StateUpdated;
        public event EventHandler<StateChangeFailedEventArgs> StateChangeFailed;
        private bool m_shouldKill;

        private readonly ICoreProcessFactory m_coreProcessFactory;
        private ICoreProcess m_process;

        private readonly ICoreLinkFactory m_coreLinkFactory;

        private readonly ICoreProxyFactory m_coreProxyFactory;
        public ICoreProxy CoreProxy { get; private set; }

        private readonly ICoreControllerFactory m_coreControllerFactory;
        private ICoreController m_coreController;

        public Conductor(ICoreProcessFactory coreProcessFactory, ICoreLinkFactory coreLinkFactory,
            ICoreControllerFactory coreControllerFactory, ICoreProxyFactory coreProxyFactory)
        {
            m_coreProcessFactory = coreProcessFactory;
            m_coreLinkFactory = coreLinkFactory;
            m_coreControllerFactory = coreControllerFactory;
            m_coreProxyFactory = coreProxyFactory;
        }

        public void ConnectToCore(EndPoint endPoint = null)
        {
            if (CoreProxy != null)
            {
                throw new InvalidOperationException(
                    "There is still a core proxy present");
            }

            if (endPoint == null)
            {
                if (m_process == null)
                    m_process = m_coreProcessFactory.Create();

                endPoint = m_process.EndPoint;
            }

            var coreLink = m_coreLinkFactory.Create(endPoint);
            m_coreController = m_coreControllerFactory.Create(coreLink);

            CoreProxy = m_coreProxyFactory.Create(coreLink, m_coreController);

            RegisterCoreEvents();
        }

        private void RegisterCoreEvents()
        {
            CoreProxy.StateUpdated += OnCoreStateUpdated;
            CoreProxy.StateChangeFailed += OnCoreStateChangeFailed;
            CoreProxy.CommandTimedOut += OnCoreCommandTimedOut;
        }

        private void UnregisterCoreEvents()
        {
            CoreProxy.StateUpdated -= OnCoreStateUpdated;
            CoreProxy.StateChangeFailed -= OnCoreStateChangeFailed;
            CoreProxy.CommandTimedOut -= OnCoreCommandTimedOut;
        }

        public void Disconnect()
        {
            if (CoreProxy == null)
            {
                // TODO(HonzaS): logging.
                return;
            }

            if (m_process != null)
            {
                // TODO(HonzaS): We might want to ask the user if he wants to kill the local process when disconnecting.
            }

            FinishDisconnect();
        }

        public void Shutdown()
        {
            if (CoreProxy != null)
            {
                CoreProxy.Shutdown();
                return;
            }

            AfterShutdown();
        }

        private void OnCoreCommandTimedOut(object sender, TimeoutActionEventArgs args)
        {
            if (args.Command == CommandType.Shutdown)
            {
                args.Action = TimeoutAction.Cancel;

                AfterShutdown();
            }
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
            CoreProxy = null;

            StateUpdated?.Invoke(this, new StateUpdatedEventArgs(oldState, CoreState.Disconnected));
        }

        public void LoadBlueprint(AgentBlueprint blueprint)
        {
            // TODO(HonzaS): Add this when blueprints come into play.
            CoreProxy.LoadBlueprint(blueprint);
        }

        private void OnCoreStateUpdated(object sender, StateUpdatedEventArgs stateUpdatedEventArgs)
        {
            if (stateUpdatedEventArgs.CurrentState == CoreState.ShuttingDown)
                AfterShutdown();


            //if (m_shouldKill)
            //{
            //    if (stateUpdatedEventArgs.CurrentState == CoreState.Empty)
            //    {
            //        CleanupSimulation();
            //    }
            //    else
            //    {
            //        // TODO(HonzaS): Log an error/warning.
            //    }
            //}

            StateUpdated?.Invoke(this, stateUpdatedEventArgs);
        }

        private void OnCoreStateChangeFailed(object sender, StateChangeFailedEventArgs stateChangeFailedEventArgs)
        {
            StateChangeFailed?.Invoke(this, stateChangeFailedEventArgs);
        }

        public void StartSimulation(uint stepsToRun = 0)
        {
            if (CoreProxy == null)
                throw new InvalidOperationException("Simulation does not exist, cannot start.");

            m_shouldKill = false;
            CoreProxy.Run(stepsToRun);
        }

        public void PauseSimulation()
        {
            CoreProxy.Pause();
        }

        public void KillSimulation()
        {
            //m_shouldKill = true;
            //if (CoreProxy != null && CoreProxy.State != CoreState.Empty)
            //{
            //    CoreProxy?.Clear();
            //}
            //else
            //{
            //    CleanupSimulation();
            //}
        }

        public bool IsConnected => m_process != null && m_coreController != null;

        public CoreState CoreState => CoreProxy?.State ?? CoreState.Disconnected;

        // TODO(HonzaS): Clean up this when we start using it.
        //private void CleanupSimulation()
        //{
        //    CoreProxy.StateUpdated -= OnCoreStateUpdated;
        //    CoreProxy.StateChangeFailed -= OnCoreStateChangeFailed;

        //    CoreState previousState = CoreProxy.State;
        //    CoreProxy = null;

        //    StateUpdated?.Invoke(this, new StateUpdatedEventArgs(previousState, CoreState.Disconnected));
        //}
    }
}
