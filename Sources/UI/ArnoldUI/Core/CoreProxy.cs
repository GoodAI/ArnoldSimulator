using System;
using System.Threading;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Extensions;
using GoodAI.Arnold.Project;
using GoodAI.Logging;

namespace GoodAI.Arnold.Core
{
    public enum CoreState
    {
        Disconnected,  // Only for event signaling - not a true state.
        Empty,  // The core is ready, but no blueprint has been loaded.
        Paused,  // There is a blueprint but the simulation is not running.
        Running,
        ShuttingDown
    }

    public class TimeoutActionEventArgs : EventArgs
    {
        public CommandType Command { get; }
        public TimeoutAction Action { get; set; }

        public TimeoutActionEventArgs(CommandType command, TimeoutAction action = TimeoutAction.Wait)
        {
            Command = command;
            Action = action;
        }
    }

    public interface ICoreProxy : IDisposable
    {
        event EventHandler<StateUpdatedEventArgs> StateUpdated;
        event EventHandler<StateChangeFailedEventArgs> StateChangeFailed;
        event EventHandler<TimeoutActionEventArgs> CommandTimedOut;

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
        void Run(uint stepsToRun = 0);

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
        /// Shut down the core.
        /// The core should shut down after it confirms the ShuttingDown state.
        /// </summary>
        void Shutdown();

        CoreState State { get; }

        ISimulationModel Model { get; }
    }


    public class WrongHandlerStateException : Exception
    {
        public WrongHandlerStateException(string message) : base(message) { }

        public WrongHandlerStateException(string methodName, CoreState state)
            : base($"Cannot run {methodName}, simulation is in {state} state.")
        { }
    }

    public class CoreProxy : ICoreProxy
    {
        // Injected.
        public ILog Log { get; set; } = NullLogger.Instance;

        public ISimulationModel Model { get; private set; }

        public event EventHandler<StateUpdatedEventArgs> StateUpdated;
        public event EventHandler<StateChangeFailedEventArgs> StateChangeFailed;
        public event EventHandler<TimeoutActionEventArgs> CommandTimedOut;

        public CoreState State
        {
            get { return m_state; }
            private set
            {
                CoreState oldState = m_state;
                m_state = value;
                StateUpdated?.Invoke(this, new StateUpdatedEventArgs(oldState, m_state));
            }
        }
        private CoreState m_state;

        private readonly ICoreLink m_coreLink;
        private readonly ICoreController m_controller;

        public CoreProxy(ICoreLink coreLink, ICoreController controller)
        {
            m_coreLink = coreLink;
            m_controller = controller;
            State = CoreState.Empty;

            m_controller.StartStateChecking(HandleKeepaliveStateResponse);

            Model = new SimulationModel();
        }

        public void Dispose()
        {
            m_controller.Dispose();
        }

        public void LoadBlueprint(AgentBlueprint agentBlueprint)
        {
            if (State != CoreState.Empty)
                throw new WrongHandlerStateException("LoadAgent", State);

            // TODO(HonzaS): Add the blueprint data.
            SendCommand(new CommandConversation(CommandType.Load));
        }

        public void Clear()
        {
            SendCommand(new CommandConversation(CommandType.Clear));
        }

        public void Shutdown()
        {
            SendCommand(new CommandConversation(CommandType.Shutdown));
        }

        public void Run(uint stepsToRun = 0)
        {
            if (State != CoreState.Paused && State != CoreState.Running)
                throw new WrongHandlerStateException("Run", State);

            if (State == CoreState.Running)
                return;

            SendCommand(new CommandConversation(CommandType.Run, stepsToRun));
        }

        public void Pause()
        {
            if (State != CoreState.Paused && State != CoreState.Running)
                throw new WrongHandlerStateException("Pause", State);

            if (State == CoreState.Paused)
                return;

            SendCommand(new CommandConversation(CommandType.Pause));
        }

        private void SendCommand(CommandConversation conversation)
        {
            m_controller.Command(conversation, HandleStateResponse, CreateTimeoutHandler(conversation.RequestData.Command));
        }

        private Func<TimeoutAction> CreateTimeoutHandler(CommandType type)
        {
            return () =>
            {
                var args = new TimeoutActionEventArgs(type);
                CommandTimedOut?.Invoke(this, args);

                return args.Action;
            };
        }

        private void HandleKeepaliveStateResponse(TimeoutResult<Response<StateResponse>> timeoutResult)
        {
            if (timeoutResult.TimedOut)
            {
                throw new NotImplementedException();
            }
            else
            {
                HandleStateResponse(timeoutResult.Result);
            }
        }

        private void HandleStateResponse(Response<StateResponse> response)
        {
            if (response.Error != null)
                HandleError(response.Error);
            else if (response.Data != null)
                State = ReadState(response.Data);
            else
                // This only happened so far when the request handler was misspelled.
                // Keep it as warning for a while and switch to debug later?
                Log.Warn("The server rejected the message.");
        }

        private static CoreState ReadState(StateResponse stateData)
        {
            switch (stateData.State)
            {
                case StateType.Empty:
                    return CoreState.Empty;
                case StateType.Running:
                    return CoreState.Running;
                case StateType.Paused:
                    return CoreState.Paused;
                case StateType.ShuttingDown:
                    return CoreState.ShuttingDown;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleError(ErrorResponse error)
        {
            StateChangeFailed?.Invoke(this, new StateChangeFailedEventArgs(error));
        }

        public void RefreshState()
        {
            var conversation = new GetStateConversation();

            m_coreLink.Request(conversation).ContinueWith(task =>
            {
                TimeoutResult<Response<StateResponse>> timeoutResult = task.Result;
                if (!timeoutResult.TimedOut && timeoutResult.Result.Data != null)
                    State = ReadState(timeoutResult.Result.Data);
            });
        }
    }
}
