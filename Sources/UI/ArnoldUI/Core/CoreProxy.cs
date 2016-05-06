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
        event EventHandler<StateChangedEventArgs> StateChanged;
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
        /// <param name="brainStepsToRun">The number of steps to run. 0 is infinity.</param>
        void Run(uint brainStepsToRun = 0);

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

        public event EventHandler<StateChangedEventArgs> StateChanged;
        public event EventHandler<StateChangeFailedEventArgs> StateChangeFailed;
        public event EventHandler<TimeoutActionEventArgs> CommandTimedOut;

        public CoreState State
        {
            get { return m_state; }
            private set
            {
                if (value == m_state)  // Pass only changes, not updates keeping the state unchanged.
                    return;

                CoreState oldState = m_state;
                m_state = value;
                StateChanged?.Invoke(this, new StateChangedEventArgs(oldState, m_state));
            }
        }
        private CoreState m_state = CoreState.Empty;

        private readonly ICoreLink m_coreLink;
        private readonly ICoreController m_controller;

        public CoreProxy(ICoreLink coreLink, ICoreController controller)
        {
            m_coreLink = coreLink;
            m_controller = controller;

            Log.Debug("Starting periodic core state checking");
            m_controller.StartStateChecking(HandleKeepaliveStateResponse);

            Model = new SimulationModel();
        }

        public void Dispose()
        {
            Log.Debug("Disposing");
            m_controller.Dispose();
        }

        public void LoadBlueprint(AgentBlueprint agentBlueprint)
        {
            if (State != CoreState.Empty)
            {
                Log.Warn("Loading of blueprint failed - the core is in state: {state}", State);
                throw new WrongHandlerStateException("LoadBlueprint", State);
            }

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

        public void Run(uint brainStepsToRun = 0)
        {
            if (State != CoreState.Paused && State != CoreState.Running)
            {
                Log.Warn("Run failed - the core is in state: {State} state", State);
                throw new WrongHandlerStateException("Run", State);
            }

            if (State == CoreState.Running)
                return;

            SendCommand(new CommandConversation(CommandType.Run, brainStepsToRun));
        }

        public void Pause()
        {
            if (State != CoreState.Paused && State != CoreState.Running)
            {
                Log.Warn("Pause failed - the core is in state: {State} state", State);
                throw new WrongHandlerStateException("Pause", State);
            }

            if (State == CoreState.Paused)
                return;

            SendCommand(new CommandConversation(CommandType.Pause));
        }

        private void SendCommand(CommandConversation conversation)
        {

            try
            {
                m_controller.Command(conversation, HandleStateResponse, CreateTimeoutHandler(conversation.RequestData.Command));
            }
            catch (RemoteCoreException ex)
            {
                HandleError(ex.Message);
            }
            // TODO(Premek): Handle timeout here...
        }

        private Func<TimeoutAction> CreateTimeoutHandler(CommandType type)
        {
            return () =>
            {
                Log.Debug("Timeout occured for command: {command}", type);
                var args = new TimeoutActionEventArgs(type);
                CommandTimedOut?.Invoke(this, args);

                return args.Action;
            };
        }

        private void HandleKeepaliveStateResponse(StateResponse response)
        {
            HandleStateResponse(response);
        }

        private void HandleStateResponse(StateResponse response)
        {
            State = ReadState(response);
            
            // TODO(Premek): check that this is handled elsewhere...
            //else
            //    // This only happened so far when the request handler was misspelled.
            //    // Keep it as warning for a while and switch to debug later?
            //    Log.Warn("The server rejected the message.");
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

        private void HandleError(string errorMessage)
        {
            Log.Warn("Core error: {error}", errorMessage);
            StateChangeFailed?.Invoke(this, new StateChangeFailedEventArgs(errorMessage));
        }
    }
}
