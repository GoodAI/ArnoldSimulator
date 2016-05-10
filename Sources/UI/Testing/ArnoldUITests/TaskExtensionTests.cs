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
            Task<bool> task = Task<bool>.Factory.StartNew(() =>
            {
                Thread.Sleep(50);
                return true;
            }).TimeoutAfter(100);

            bool result = task.Result;

            Assert.True(result);
        }

        [Fact]
        public async Task TimeoutAfterTimesOut()
        {
            Task<bool> task = Task<bool>.Factory.StartNew(() =>
            {
                Thread.Sleep(100);
                return true;
            }).TimeoutAfter(50);

            await Assert.ThrowsAsync<TaskTimeoutException<bool>>(() => task);

            // Ensure that the task doesn't set the result even after the inner task finished. TODO(Premek)
            //Thread.Sleep(100);
            //Assert.False(result.Result);
        }

        // TODO(Premek): add also test for TimeoutAfter(0) ?
    }
}
