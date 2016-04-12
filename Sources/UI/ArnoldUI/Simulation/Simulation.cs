using System;
using System.Threading.Tasks;
using ArnoldUI.Network;
using ArnoldUI.Simulation;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Project;
using Google.Protobuf;

namespace GoodAI.Arnold.Simulation
{
    public interface ISimulation
    {
        event EventHandler<StateUpdatedEventArgs> StateUpdated;
        event EventHandler<StateChangeFailedEventArgs> StateChangeFailed;

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
        /// Pauses the running simulation. If the simulation is not running, this does nothing.
        /// This moves the simulation from Running state to Paused.
        /// </summary>
        void Pause();

        /// <summary>
        /// This moves the simulation from Running or Paused state to Empty.
        /// </summary>
        void Clear();

        /// <summary>
        /// Requests a state refresh.
        /// </summary>
        void RefreshState();

        SimulationState State { get; }

        ISimulationModel Model { get; }
    }

    public enum SimulationState
    {
        Null,  // The simulation doesn't exist. Don't set this as Simulation.State!
        Empty,  // The core is ready, but no blueprint has been loaded.
        Paused,  // The simulation is ready but not running.
        Running  // The simulation is running.
    }

    public class WrongHandlerStateException : Exception
    {
        public WrongHandlerStateException(string message) : base(message) { }

        public WrongHandlerStateException(string methodName, SimulationState state)
            : base($"Cannot run {methodName}, simulation is in {state} state.")
        { }
    }

    public class SimulationProxy : ISimulation
    {
        public ISimulationModel Model { get; private set; }

        public event EventHandler<StateUpdatedEventArgs> StateUpdated;
        public event EventHandler<StateChangeFailedEventArgs> StateChangeFailed;

        public SimulationState State
        {
            get { return m_state; }
            private set
            {
                if (value == SimulationState.Null)
                    throw new InvalidOperationException("The simulation state cannot be Null.");

                SimulationState oldState = m_state;
                m_state = value;
                StateUpdated?.Invoke(this, new StateUpdatedEventArgs(oldState, m_state));
            }
        }

        private readonly ICoreLink m_coreLink;
        private SimulationState m_state;

        public SimulationProxy(ICoreLink coreLink)
        {
            m_coreLink = coreLink;
            State = SimulationState.Empty;

            Model = new SimulationModel();
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
