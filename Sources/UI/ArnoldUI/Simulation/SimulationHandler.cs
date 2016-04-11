using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoodAI.Arnold.Project;

namespace GoodAI.Arnold.Simulation
{
    public interface ISimulation
    {
        /// <summary>
        /// Loads an agent into the handler, creates a new simulation.
        /// This moves the simulation from Empty state to Paused.
        /// </summary>
        void LoadBlueprint(AgentBlueprint agentBlueprint);

        /// <summary>
        /// Runs the given number of steps.
        /// This moves the simulation from Stopped to Running. When the requested number
        /// of steps are performed, moves to state Paused.
        /// </summary>
        /// <param name="stepsToRun">The number of steps to run. 0 is infinity.</param>
        void Run(int stepsToRun = 0);

        /// <summary>
        /// Runs one step. Alternative to Run(1).
        /// This briefly moves the simulation from Paused to Running and then back to Paused.
        /// </summary>
        void Step();

        /// <summary>
        /// Stops the running simulation. If the simulation is not running, this does nothing.
        /// This moves the simulation from Running state to Paused.
        /// </summary>
        void Pause();

        /// <summary>
        /// This moves the simulation from Paused state to Clear.
        /// </summary>
        void Reset();

        SimulationState State { get; }
    }

    public enum SimulationState
    {
        Stopped,
        Paused,
        Running
    }

    public class WrongHandlerStateException : Exception
    {
        public WrongHandlerStateException(string message) : base(message) { }

        public WrongHandlerStateException(string methodName, SimulationState state)
            : base($"Cannot run {methodName}, simulation is in {state} state.")
        { }
    }

    public class RemoteSimulation : ISimulation
    {
        private const int SimulationStopTimeoutMs = 30000;

        public Model Model { get; private set; }
        private CancellationTokenSource m_cancellationTokenSource;

        public SimulationState State { get; private set; }

        private readonly ManualResetEvent m_simulationPaused = new ManualResetEvent(true);

        public RemoteSimulation()
        {
            State = SimulationState.Stopped;
        }

        public void LoadBlueprint(AgentBlueprint agentBlueprint)
        {
            if (State != SimulationState.Stopped)
                throw new WrongHandlerStateException("LoadAgent", State);

            Model = new Model(agentBlueprint);

            State = SimulationState.Paused;
        }

        public void Reset()
        {
            if (State != SimulationState.Paused)
                throw new WrongHandlerStateException("Reset", State);

            Model = null;

            State = SimulationState.Stopped;
        }

        public void Run(int stepsToRun = 0)
        {
            if (State != SimulationState.Paused)
                throw new WrongHandlerStateException("Run", State);

            RunSimulation(stepsToRun);
        }

        public void Step()
        {
            if (State != SimulationState.Paused)
                throw new WrongHandlerStateException("Step", State);

            RunSimulation(1);
        }

        public void Pause()
        {
            if (State != SimulationState.Paused && State != SimulationState.Running)
                throw new WrongHandlerStateException("Pause", State);

            m_cancellationTokenSource.Cancel();
            bool signalled = m_simulationPaused.WaitOne(SimulationStopTimeoutMs);
            // TODO(HonzaS): Logging!
            //if (!signalled)
            //    deal with it
        }



        private void RunSimulation(int stepsToRun)
        {
            m_cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = m_cancellationTokenSource.Token;
            m_simulationPaused.Reset();
            Task.Factory.StartNew(() => RunSimulationSteps(stepsToRun, token), TaskCreationOptions.LongRunning)
                .ContinueWith(task => m_simulationPaused.Set(), token);
        }

        private void RunSimulationSteps(int stepsToRun, CancellationToken token)
        {
            State = SimulationState.Running;

            int i = 0;
            while(stepsToRun == 0 || i < stepsToRun)
            {
                if (token.IsCancellationRequested)
                    break;

                //Model.Step();

                i++;
            }

            State = SimulationState.Paused;
        }
    }
}
