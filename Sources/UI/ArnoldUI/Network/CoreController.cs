using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Simulation;
using GoodAI.Arnold.Extensions;
using GoodAI.Logging;

namespace GoodAI.Arnold.Network
{
    public interface ICoreController : IDisposable
    {
        Task Command(CommandConversation conversation, Action<Response<StateResponse>> successAction,
            Func<TimeoutAction> timeoutAction, int timeoutMs = 0);

        bool IsCommandInProgress { get; }
        void StartStateChecking(Action<TimeoutResult<Response<StateResponse>>> action);
    }

    public enum TimeoutAction
    {
        Wait,
        Retry,
        Cancel
    }

    public class CoreController : ICoreController
    {
        public ILog Log { get; set; } = NullLogger.Instance;

        private readonly ICoreLink m_coreLink;
        private Task<TimeoutResult<Response<StateResponse>>> m_runningCommand;
        private Action<TimeoutResult<Response<StateResponse>>> m_action;
        private CancellationTokenSource m_cancellationTokenSource;
        private const int CommandTimeoutMs = 15 * 1000;

        public bool IsCommandInProgress => m_runningCommand != null;

        public CoreController(ICoreLink coreLink)
        {
            m_coreLink = coreLink;

            m_cancellationTokenSource = new CancellationTokenSource();
        }

        public void StartStateChecking(Action<TimeoutResult<Response<StateResponse>>> action)
        {
            if (action != null)
            {
                m_action = action;
                try
                {
                    RepeatGetStateAsync(2000);
                }
                catch (AggregateException exception)
                {
                    Log.Warn(exception, "Error in state checking.");
                }
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
                    // TODO(HonzaS): Handle timeout here.
                    m_action(await m_coreLink.Request(new GetStateConversation()).ConfigureAwait(false));
                }
                await Task.Delay(repeatMillis, m_cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            m_cancellationTokenSource.Cancel();
        }

        public async Task Command(CommandConversation conversation, Action<Response<StateResponse>> successAction,
            Func<TimeoutAction> timeoutCallback, int timeoutMs = CommandTimeoutMs)
        {
            if (m_runningCommand != null)
            {
                Log.Info("A command is already running: {commandType}", conversation.RequestData.Command);
                return;
            }

            m_cancellationTokenSource.Cancel();

            var retry = true;

            while (true)
            {
                TimeoutResult<Response<StateResponse>> result;

                if (retry)
                {
                    m_runningCommand = m_coreLink.Request(conversation, timeoutMs);
                    result = await m_runningCommand.ConfigureAwait(false);

                    retry = false;
                }
                else
                {
                    result = await m_runningCommand.Result.OriginalTask.TimeoutAfter(CommandTimeoutMs).ConfigureAwait(false);
                }

                if (result.TimedOut)
                {
                    TimeoutAction timeoutAction = timeoutCallback();
                    if (timeoutAction == TimeoutAction.Cancel)
                        break;

                    if (timeoutAction == TimeoutAction.Retry)
                        retry = true;

                    // Redundant.
                    //if (timeoutAction == TimeoutAction.Wait)
                    //    continue;
                }
                else
                {
                    successAction(result.Result);
                    break;
                }
            }

            m_runningCommand = null;
        }
    }
}
