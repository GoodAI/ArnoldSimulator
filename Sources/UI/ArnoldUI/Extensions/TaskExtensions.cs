using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Extensions
{
    public class TaskTimeoutException<TResult> : TimeoutException
    {
        public Task<TResult> OriginalTask { get; set; }

        public TaskTimeoutException(Task<TResult> originalTask)
        {
            OriginalTask = originalTask;
        }
    }
    
    public static class TaskExtensions
    {
        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task,
            int timeoutMs)
        {
            if (timeoutMs <= 0)
                return await task;

            if (task == await Task.WhenAny(task, Task.Delay(timeoutMs)))
                return await task;

            throw new TaskTimeoutException<TResult>(task);
        }
    }
}
