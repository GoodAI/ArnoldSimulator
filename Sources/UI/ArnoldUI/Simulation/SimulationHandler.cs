using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArnoldUI.Network;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Project;
using Google.Protobuf;

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
        /// This moves the simulation from Paused to Running. When the requested number
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
        /// Pauses the running simulation. If the simulation is not running, this does nothing.
        /// This moves the simulation from Running state to Paused.
        /// </summary>
        void Pause();

        /// <summary>
        /// This moves the simulation from Paused state to Empty.
        /// </summary>
        void Clear();

        /// <summary>
        /// Requests a state refresh.
        /// </summary>
        void RefreshState();

        SimulationState State { get; }
    }

    public enum SimulationState
    {
        Empty,
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

    public class StateChangedEventArgs : EventArgs
    {
        public SimulationState PreviousState { get; set; }
        public SimulationState CurrentState { get; set; }

        public StateChangedEventArgs(SimulationState previousState, SimulationState currentState)
        {
            PreviousState = previousState;
            CurrentState = currentState;
        }
    }

    public class StateChangeFailedEventArgs : EventArgs
    {
        public StateChangeFailedEventArgs(Error error)
        {
            Error = error;
        }

        public Error Error { get; set; }
    }

    public class RemoteSimulation : ISimulation
    {
        public Model Model { get; private set; }

        public event EventHandler<StateChangedEventArgs> StateChanged;
        public event EventHandler<StateChangeFailedEventArgs> StateChangeFailed;

        public SimulationState State
        {
            get { return m_state; }
            private set
            {
                SimulationState oldState = m_state;
                m_state = value;
                StateChanged?.Invoke(this, new StateChangedEventArgs(oldState, m_state));
            }
        }

        private ICoreLink m_coreLink;
        private SimulationState m_state;

        public RemoteSimulation(ICoreLink coreLink)
        {
            m_coreLink = coreLink;
            State = SimulationState.Empty;

            Model = new Model();
        }

        public void LoadBlueprint(AgentBlueprint agentBlueprint)
        {
            if (State != SimulationState.Empty)
                throw new WrongHandlerStateException("LoadAgent", State);

            // TODO(HonzaS): Add the blueprint data.
            var conversation = new CommandConversation
            {
                Request =
                {
                    Command = CommandRequest.Types.CommandType.Load,
                    Blueprint = new BlueprintData()
                }
            };

            RequestAndHandleState(conversation);
        }

        public void Clear()
        {
            if (State != SimulationState.Empty && State != SimulationState.Paused)
                throw new WrongHandlerStateException("Reset", State);

            var conversation = new CommandConversation
            {
                Request =
                {
                    Command = CommandRequest.Types.CommandType.Clear
                }
            };

            RequestAndHandleState(conversation);
        }

        public void Run(int stepsToRun = 0)
        {
            if (State != SimulationState.Paused && State != SimulationState.Running)
                throw new WrongHandlerStateException("Run", State);

            RunSimulation(stepsToRun);
        }

        public void Step()
        {
            if (State != SimulationState.Paused && State != SimulationState.Running)
                throw new WrongHandlerStateException("Step", State);

            if (State == SimulationState.Running)
            {
                Pause();
                return;
            }

            RunSimulation(1);
        }

        public void Pause()
        {
            if (State != SimulationState.Paused && State != SimulationState.Running)
                throw new WrongHandlerStateException("Pause", State);

            if (State == SimulationState.Paused)
                return;

            var conversation = new CommandConversation
            {
                Request =
                {
                    Command = CommandRequest.Types.CommandType.Pause
                }
            };

            RequestAndHandleState(conversation);

            // TODO(HonzaS): Signal the Core.
            // TODO(HonzaS): Logging!
            //if (!signalled)
            //    deal with it
        }

        private void RunSimulation(int stepsToRun)
        {
            if (State == SimulationState.Running)
                return;

            var conversation = new CommandConversation
            {
                Request =
                {
                    Command = CommandRequest.Types.CommandType.Run,
                    StepsToRun = stepsToRun
                }
            };

            RequestAndHandleState(conversation);
        }

        private void RequestAndHandleState<TRequest>(IConversation<TRequest, StateResponse> conversation)
            where TRequest : class, IMessage
        {
            Task<StateResponse> response = m_coreLink.Request(conversation);

            response.ContinueWith(HandleStateResponse);
        }

        private void HandleStateResponse(Task<StateResponse> task)
        {
            StateResponse result = task.Result;
            if (result.ResponseOneofCase == StateResponse.ResponseOneofOneofCase.Error)
            {
                ProcessError(result.Error);
            }
            else
            {
                switch (result.Data.State)
                {
                    case StateData.Types.StateType.Empty:
                        State = SimulationState.Empty;
                        break;
                    case StateData.Types.StateType.Paused:
                        State = SimulationState.Paused;
                        break;
                    case StateData.Types.StateType.Running:
                        State = SimulationState.Running;
                        break;
                    case StateData.Types.StateType.Invalid:
                        ProcessError(new Error {Message = "Invalid simulation state"});
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void ProcessError(Error error)
        {
            StateChangeFailed?.Invoke(this, new StateChangeFailedEventArgs(error));
        }

        public void RefreshState()
        {
            var conversation = new GetStateConversation();

            RequestAndHandleState(conversation);
        }
    }
}
