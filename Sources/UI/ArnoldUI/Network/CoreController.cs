using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Extensions;
using GoodAI.Logging;
using TaskExtensions = GoodAI.Arnold.Extensions.TaskExtensions;

namespace GoodAI.Arnold.Network
{
    public interface ICoreController : IDisposable
    {
        Task Command(CommandConversation conversation, Action<StateResponse> successAction,
            Func<TimeoutAction> timeoutAction, int timeoutMs = 0);

        bool IsCommandInProgress { get; }
        void StartStateChecking(Action<StateResponse> stateResultAction);
    }

    public enum TimeoutAction
    {
        Wait,
        Retry,
        Cancel
    }

    public class CoreController : ICoreController
    {
        // Injected.
        public ILog Log { get; set; } = NullLogger.Instance;

        private readonly ICoreLink m_coreLink;
        private Task<StateResponse> m_runningCommand;
        private Action<StateResponse> m_stateResultAction;
        private CancellationTokenSource m_cancellationTokenSource;

        private const int CommandTimeoutMs = 15*1000;
        private const int KeepaliveIntervalMs = 2*1000;
        private const int KeepaliveTimeoutMs = KeepaliveIntervalMs;

        public bool IsCommandInProgress => m_runningCommand != null;

        public CoreController(ICoreLink coreLink)
        {
            m_coreLink = coreLink;
            m_cancellationTokenSource = new CancellationTokenSource();
        }

        public void StartStateChecking(Action<StateResponse> stateResultAction)
        {
            if (stateResultAction == null)
                throw new ArgumentNullException(nameof(stateResultAction));

            m_stateResultAction = stateResultAction;
            try
            {
                m_cancellationTokenSource = new CancellationTokenSource();
#pragma warning disable 4014 // This is supposed to start a parallel task and continue.
                RepeatGetStateAsync(KeepaliveIntervalMs);
#pragma warning restore 4014
            }
            catch (AggregateException exception)
            {
                Log.Warn(exception, "Error in state checking.");
            }
        }

        private async Task RepeatGetStateAsync(int repeatMillis)
        {
            while (true)
            {
                if (m_cancellationTokenSource.IsCancellationRequested)
                    return;

                if (!IsCommandInProgress)
                {
                    // TODO(): Handle timeout and other exceptions here.
                    StateResponse stateCheckResult =
                        await m_coreLink.Request(new GetStateConversation(), KeepaliveTimeoutMs)
                        .ConfigureAwait(false);

                    // Check this again - the cancellation could have come during the request.
                    if (!m_cancellationTokenSource.IsCancellationRequested)
                        m_stateResultAction(stateCheckResult);
                }
                await Task.Delay(repeatMillis, m_cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            Log.Debug("Disposing");
            m_cancellationTokenSource.Cancel();
        }

        public async Task Command(CommandConversation conversation, Action<StateResponse> successAction,
            Func<TimeoutAction> timeoutCallback, int timeoutMs = CommandTimeoutMs)
        {
            if (m_runningCommand != null)
            {
                Log.Info("A command is already running: {commandType}", conversation.RequestData.Command);
                return;
            }

            m_cancellationTokenSource.Cancel();

            var retry = true;  // Count the first try as a retry.

            while (true)
            {
                StateResponse result;

                try
                {
                    if (retry)
                    {
                        retry = false;
                        m_runningCommand = m_coreLink.Request(conversation, timeoutMs);
                    }

                    result = await m_runningCommand.ConfigureAwait(false);
                }
                catch (TaskTimeoutException<StateResponse> ex)
                {
                    TimeoutAction timeoutAction = timeoutCallback();
                    Log.Info("Command {command} timed out, {action} requested", conversation.RequestData.Command,
                        timeoutAction);
                    if (timeoutAction == TimeoutAction.Cancel)
                        break;

                    if (timeoutAction == TimeoutAction.Retry)
                        retry = true;

                    if (timeoutAction == TimeoutAction.Wait)
                        m_runningCommand = ex.OriginalTask.TimeoutAfter(CommandTimeoutMs);

                    continue;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Request failed");
                    throw;
                }

                Log.Debug("Successful command {command}", conversation.RequestData.Command);
                successAction(result);
                break;
            }

            m_runningCommand = null;

            if (m_stateResultAction != null)
            {
                Log.Debug("Restarting regular state checking");
                StartStateChecking(m_stateResultAction);
            }
        }
    }
}
