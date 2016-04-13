using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoodAI.Arnold.Extensions;
using Xunit;

namespace GoodAI.Arnold.UI.Tests
{
    public class TaskExtensionTests
    {
        [Fact]
        public void TimeoutAfterFinishesBeforeTheLimit()
        {
            Task<TimeoutResult<bool>> task = Task<bool>.Factory.StartNew(() =>
            {
                Thread.Sleep(50);
                return true;
            }).TimeoutAfter(100);
            TimeoutResult<bool> result = task.Result;

            Assert.False(result.TimedOut);
            Assert.Equal(true, result.Result);
        }

        [Fact]
        public void TimeoutAfterTimesOut()
        {
            Task<TimeoutResult<bool>> task = Task<bool>.Factory.StartNew(() =>
            {
                Thread.Sleep(100);
                return true;
            }).TimeoutAfter(50);
            TimeoutResult<bool> result = task.Result;

            Assert.True(result.TimedOut);

            // Ensure that the task doesn't set the result even after the inner task finished.
            Thread.Sleep(100);
            Assert.False(result.Result);
        }
    }
}
