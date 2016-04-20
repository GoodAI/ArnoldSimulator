using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Simulation;
using GoodAI.Arnold.Extensions;

namespace GoodAI.Arnold.Network
{
    public interface ICoreController
    {
        Task Command(CommandConversation conversation, Action<Response<StateResponse>> successAction,
            Func<TimeoutAction> timeoutAction, int timeoutMs = 0);
    }

    public enum TimeoutAction
    {
        Wait,
        Retry,
        Cancel
    }

    public class CoreController : ICoreController
    {
        private readonly ICoreLink m_coreLink;
        private Task<TimeoutResult<Response<StateResponse>>> m_runningCommand;
        private const int CommandTimeoutMs = 15 * 1000;

        public CoreController(ICoreLink coreLink)
        {
            m_coreLink = coreLink;
        }

        public async Task Command(CommandConversation conversation, Action<Response<StateResponse>> successAction,
            Func<TimeoutAction> timeoutCallback, int timeoutMs = CommandTimeoutMs)
        {
            if (m_runningCommand != null)
            {
                // TODO(HonzaS): Log this (warning).
                return;
            }

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
