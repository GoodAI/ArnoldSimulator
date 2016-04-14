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
        ICoreLink CoreLink { get; }
        ISimulation Simulation { get; }

        event EventHandler<StateChangeFailedEventArgs> SimulationStateChangeFailed;
        event EventHandler<StateUpdatedEventArgs> SimulationStateUpdated;

        void Setup(EndPoint endPoint = null);
        void Teardown();
        void LoadBlueprint(AgentBlueprint blueprint);
        void StartSimulation(int stepsToRun = 0);
        void PauseSimulation();
        void KillSimulation();

        SimulationState SimulationState { get; }
    }

    public class SimulationInstanceEventArgs : EventArgs
    {
        private ISimulation Simulation { get; set; }

        public SimulationInstanceEventArgs(ISimulation simulation)
        {
            Simulation = simulation;
        }
    }

    public class Conductor : IConductor
    {
        public event EventHandler<StateUpdatedEventArgs> SimulationStateUpdated;
        public event EventHandler<StateChangeFailedEventArgs> SimulationStateChangeFailed;

        private bool m_shouldKill;

        private readonly ICoreProxyFactory m_coreProxyFactory;
        private ICoreProxy m_proxy;

        private readonly ICoreLinkFactory m_coreLinkFactory;
        public ICoreLink CoreLink { get; private set; }

        private readonly ISimulationFactory m_simulationFactory;
        public ISimulation Simulation { get; private set; }

        private ICoreControllerFactory m_coreControllerFactory;
        public ICoreController CoreController { get; private set; }

        public Conductor(ICoreProxyFactory coreProxyFactory, ICoreLinkFactory coreLinkFactory,
            ICoreControllerFactory coreControllerFactory, ISimulationFactory simulationFactory)
        {
            m_coreProxyFactory = coreProxyFactory;
            m_coreLinkFactory = coreLinkFactory;
            m_coreControllerFactory = coreControllerFactory;
            m_simulationFactory = simulationFactory;
        }

        public void Setup(EndPoint endPoint = null)
        {
            if (Simulation != null)
            {
                throw new InvalidOperationException(
                    "There is still a Simulation present. It must be cleaned up before Setup().");
            }

            if (m_proxy != null)
            {
                throw new InvalidOperationException(
                    "There is still a Core present. It must be cleaned up before Setup().");
            }

            m_proxy = m_coreProxyFactory.Create(endPoint);

            endPoint = m_proxy.Start();

            CoreLink = m_coreLinkFactory.Create(endPoint);

            CoreController = m_coreControllerFactory.Create(CoreLink);

            // TODO(HonzaS): Simulation should only be present after there has been a blueprint upload.
            Simulation = m_simulationFactory.Create(CoreLink, CoreController);

            Simulation.StateUpdated += SimulationOnStateUpdated;
            Simulation.StateChangeFailed += SimulationOnStateChangeFailed;

            // Handshake.
            Simulation.RefreshState();
        }

        public void Teardown()
        {
            if (m_proxy == null)
                throw new InvalidOperationException("Not set up, cannot tear down.");

            if (CoreLink == null)
            {
                Reset();
                return;
            }

            var conversation = new CommandConversation
            {
                Request = { Command = CommandRequest.Types.CommandType.Shutdown }
            };

            CoreController.Command(conversation, AfterTeardown, TeardownTimeout);
        }

        private TimeoutAction TeardownTimeout()
        {
            // TODO(HonzaS): Ask the user what to do.
            return TimeoutAction.Wait;
        }

        public void LoadBlueprint(AgentBlueprint blueprint)
        {
            // TODO(HonzaS): Add this when blueprints come into play.
            Simulation.LoadBlueprint(blueprint);
        }

        private void AfterTeardown(StateResponse result)
        {
            if (result.ResponseOneofCase == StateResponse.ResponseOneofOneofCase.Error)
            {
                // TODO(HonzaS): Logging.
            }

            // TODO(HonzaS): This will force-reset, should we give the user a say in that?
            Reset();
        }

        private void Reset()
        {
            CoreLink = null;

            // This will kill the local process.
            m_proxy.Dispose();
            m_proxy = null;

            // TODO(HonzaS): Is the previousState correct?
            SimulationStateUpdated?.Invoke(this, new StateUpdatedEventArgs(SimulationState.Empty, SimulationState.Null));
        }

        private void SimulationOnStateUpdated(object sender, StateUpdatedEventArgs stateUpdatedEventArgs)
        {
            if (m_shouldKill)
            {
                if (stateUpdatedEventArgs.CurrentState == SimulationState.Empty)
                {
                    CleanupSimulation();
                }
                else
                {
                    // TODO(HonzaS): Log an error/warning.
                }
            }

            SimulationStateUpdated?.Invoke(this, stateUpdatedEventArgs);
        }

        private void SimulationOnStateChangeFailed(object sender, StateChangeFailedEventArgs stateChangeFailedEventArgs)
        {
            SimulationStateChangeFailed?.Invoke(this, stateChangeFailedEventArgs);
        }

        public void StartSimulation(int stepsToRun = 0)
        {
            if (Simulation == null)
                throw new InvalidOperationException("Simulation does not exist, cannot start.");

            m_shouldKill = false;
            Simulation.Run(stepsToRun);
        }

        public void PauseSimulation()
        {
            Simulation.Pause();
        }

        public void KillSimulation()
        {
            m_shouldKill = true;
            if (Simulation != null && Simulation.State != SimulationState.Empty)
            {
                Simulation?.Clear();
            }
            else
            {
                CleanupSimulation();
            }
        }

        public SimulationState SimulationState
        {
            get
            {
                if (Simulation != null)
                    return Simulation.State;

                return SimulationState.Null;
            }
        }

        private void CleanupSimulation()
        {
            //Simulation.Dispose();
            Simulation.StateUpdated -= SimulationOnStateUpdated;
            Simulation.StateChangeFailed -= SimulationOnStateChangeFailed;

            SimulationState previousState = Simulation.State;
            Simulation = null;

            SimulationStateUpdated?.Invoke(this, new StateUpdatedEventArgs(previousState, SimulationState.Null));
        }
    }
}
