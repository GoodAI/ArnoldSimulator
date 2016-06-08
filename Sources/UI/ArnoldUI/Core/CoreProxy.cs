using System;
using System.Threading;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Communication;
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
        event EventHandler<EventArgs> Disconnected;

        /// <summary>
        /// Loads an agent into the handler, creates a new simulation.
        /// This moves the simulation from Empty state to Paused.
        /// </summary>
        Task LoadBlueprintAsync(string blueprint);

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
        Task PauseAsync();

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

        IModelUpdater ModelUpdater { get; }
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

        public event EventHandler<StateChangedEventArgs> StateChanged;
        public event EventHandler<StateChangeFailedEventArgs> StateChangeFailed;
        public event EventHandler<TimeoutActionEventArgs> CommandTimedOut;
        public event EventHandler<EventArgs> Disconnected;

        public CoreState State
        {
            get { return m_state; }
            internal set
            {
                if (value == m_state)  // Pass only changes, not updates keeping the state unchanged.
                    return;

                CoreState oldState = m_state;
                m_state = value;
                StateChanged?.Invoke(this, new StateChangedEventArgs(oldState, m_state));
            }
        }
        private CoreState m_state = CoreState.Disconnected;

        private readonly ICoreLink m_coreLink;
        private readonly ICoreController m_controller;

        private const int FailCountBeforeDisconnect = 3;

        private int m_failCount;

        public IModelUpdater ModelUpdater { get; }

        public CoreProxy(ICoreLink coreLink, ICoreController controller, IModelUpdater modelUpdater)
        {
            m_coreLink = coreLink;
            m_controller = controller;
            ModelUpdater = modelUpdater;

            m_controller.StartStateChecking(HandleKeepaliveStateResponse);
            modelUpdater.Start();
        }

        public void Dispose()
        {
            Log.Debug("Disposing");
            m_controller.Dispose();
            ModelUpdater.Dispose();
        }

        public async Task LoadBlueprintAsync(string blueprint)
        {
            if (State != CoreState.Empty)
            {
                Log.Warn("Loading of blueprint failed - the core is in state: {state}", State);
                throw new WrongHandlerStateException("LoadBlueprint", State);
            }

            await SendCommandAsync(new CommandConversation(CommandType.Load, blueprint: blueprint));
        }

        public void Clear()
        {
            SendCommandAsync(new CommandConversation(CommandType.Clear));
        }

        public void Shutdown()
        {
            // Prevent the state checking from being restarted after the shutdown completes.
            SendCommandAsync(new CommandConversation(CommandType.Shutdown), stopCheckingCoreState: true);
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

            SendCommandAsync(new CommandConversation(CommandType.Run, brainStepsToRun));
        }

        public async Task PauseAsync()
        {
            if (State != CoreState.Paused && State != CoreState.Running)
            {
                Log.Warn("Pause failed - the core is in state: {State} state", State);
                throw new WrongHandlerStateException("Pause", State);
            }

            if (State == CoreState.Paused)
                return;

            await SendCommandAsync(new CommandConversation(CommandType.Pause));
        }

        private async Task SendCommandAsync(CommandConversation conversation, bool stopCheckingCoreState = false)
        {
            try
            {
                StateResponse response = await m_controller.Command(
                    conversation, CreateTimeoutHandler(conversation.RequestData.Command),
                    restartKeepaliveOnSuccess: !stopCheckingCoreState);

                HandleStateResponse(response);
            }
            catch (Exception ex)
            {
                HandleError(ex.Message);
                throw;
            }
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

        private void HandleKeepaliveStateResponse(KeepaliveResult keepaliveResult)
        {
            if (keepaliveResult.RequestFailed)
            {
                m_failCount++;

                if (m_failCount < FailCountBeforeDisconnect) return;

                m_failCount = 0;
                Disconnected?.Invoke(this, EventArgs.Empty);

                return;
            }

            m_failCount = 0;
            HandleStateResponse(keepaliveResult.StateResponse);
        }

        private void HandleStateResponse(StateResponse response)
        {
            State = ReadState(response);
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
            // TODO(HonzaS): move this up to the clients of CoreProxy.
            Log.Warn("Core error: {error}", errorMessage);
            StateChangeFailed?.Invoke(this, new StateChangeFailedEventArgs(errorMessage));
        }
    }
}
