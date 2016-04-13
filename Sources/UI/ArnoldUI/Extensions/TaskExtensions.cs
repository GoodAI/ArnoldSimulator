using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Extensions
{
    public class TimeoutResult<TResult>
    {
        public TResult Result { get; set; }
        public bool TimedOut { get; set; }
        public Task<TResult> OriginalTask { get; set; }
    }

    public static class TaskExtensions
    {
        public static async Task<TimeoutResult<TResult>> TimeoutAfter<TResult>(this Task<TResult> task,
            int timeoutMs)
        {
            var result = new TimeoutResult<TResult> {OriginalTask = task};

            if (timeoutMs <= 0)
            {
                result.Result = await task;
            }
            else
            {
                if (task == await Task.WhenAny(task, Task.Delay(timeoutMs)))
                {
                    await task;
                    result.Result = task.Result;
                }
                else
                {
                    result.TimedOut = true;
                }
            }

            return result;
        }
    }
}
