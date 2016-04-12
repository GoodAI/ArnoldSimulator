using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ArnoldUI.Network;
using ArnoldUI.Simulation;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Simulation;
using GoodAI.Net.ConverseSharp;

namespace ArnoldUI.Core
{
    public interface IConductor
    {
        ICoreLink CoreLink { get; }
        ISimulation Simulation { get; }

        event EventHandler<StateChangeFailedEventArgs> SimulationStateChangeFailed;
        event EventHandler<StateUpdatedEventArgs> SimulationStateUpdated;

        void KillSimulation();
        void Setup();
        void Teardown();
        void StartSimulation(int stepsToRun = 0);
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
        private const int TcpTimeoutMs = 5000;

        public event EventHandler<StateUpdatedEventArgs> SimulationStateUpdated;
        public event EventHandler<StateChangeFailedEventArgs> SimulationStateChangeFailed;

        private ICoreProxy m_proxy;
        private EndPoint m_endPoint;
        private bool m_shouldKill;
        public ICoreLink CoreLink { get; private set; }
        public ISimulation Simulation { get; private set; }

        public void Setup()
        {
            if (Simulation != null)
                throw new InvalidOperationException("There is still a Simulation present. It must be cleaned up before Setup().");

            if (m_proxy != null)
                throw new InvalidOperationException("There is still a Core present. It must be cleaned up before Setup().");

            m_proxy = new LocalCoreProxy();

            m_endPoint = m_proxy.Start();

            // TODO(HonzaS): How to better handle resolution here?
            CoreLink = new CoreLink(
                new ConverseProtoBufClient(new TcpConnector(m_endPoint.Hostname, m_endPoint.Port, TcpTimeoutMs)));

            // TODO(HonzaS): Simulation should only be present after there has been a blueprint upload.
            Simulation = new SimulationProxy(CoreLink);

            Simulation.StateUpdated += SimulationOnStateUpdated;
            Simulation.StateChangeFailed += SimulationOnStateChangeFailed;

            // Handshake.
            Simulation.RefreshState();
        }

        public void Teardown()
        {
            var conversation = new CommandConversation
            {
                Request = { Command = CommandRequest.Types.CommandType.Shutdown }
            };

            var task = CoreLink.Request(conversation);
            task.ContinueWith(AfterTeardown);
        }

        private void AfterTeardown(Task<StateResponse> finishedTask)
        {
            StateResponse result = finishedTask.Result;
            if (result.ResponseOneofCase == StateResponse.ResponseOneofOneofCase.Error)
            {
                // TODO(HonzaS): Logging.
            }

            // This will kill the local process.
            m_proxy.Dispose();
            m_proxy = null;
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
