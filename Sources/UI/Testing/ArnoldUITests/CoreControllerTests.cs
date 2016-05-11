using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Extensions;
using Moq;
using Xunit;

namespace GoodAI.Arnold.UI.Tests
{
    public class CoreControllerTests
    {
        private ICoreLink m_coreLink;
        private readonly CoreController m_controller;
        private readonly Mock<ICoreLink> m_coreLinkMock;

        private const int TimeoutMs = 50;

        public CoreControllerTests()
        {
            m_coreLinkMock = new Mock<ICoreLink>();
            m_coreLink = m_coreLinkMock.Object;

            m_controller = new CoreController(m_coreLink, TimeoutMs/5);
        }

        [Fact]
        public void OnlyAllowsOneCommand()
        {
            m_coreLinkMock.Setup(link => link.Request(It.IsAny<CommandConversation>(), It.IsAny<int>()))
                .Returns(() =>
                {
                    var task = new Task<StateResponse>(() =>
                    {
                        Thread.Sleep(100);
                        return new StateResponse();
                    });
                    task.Start();
                    return task;
                });

            var conversation = new CommandConversation(CommandType.Run);

            var firstTask = m_controller.Command(conversation, () => TimeoutAction.Wait);

            var exception = Assert.Throws<AggregateException>(() => m_controller.Command(conversation, () => TimeoutAction.Wait).Wait());
            Assert.Contains(exception.InnerExceptions, ex => ex is InvalidOperationException);

            Assert.NotNull(firstTask.Result);
        }

        [Fact]
        public async void RetriesCommands()
        {
            var noOfRuns = 0;
            m_coreLinkMock.Setup(link => link.Request(It.IsAny<CommandConversation>(), It.IsAny<int>()))
                .Returns(() =>
                {
                    var task = new Task<StateResponse>(() =>
                    {
                        if (noOfRuns++ == 0)
                        {
                            throw new TaskTimeoutException<StateResponse>(null);
                        }

                        return new StateResponse();
                    });
                    task.Start();
                    return task;
                });

            var conversation = new CommandConversation(CommandType.Run);

            await m_controller.Command(conversation, () => TimeoutAction.Retry, timeoutMs: TimeoutMs);

            Assert.Equal(2, noOfRuns);
        }

        [Fact]
        public async void WaitsForCommands()
        {
            var noOfRuns = 0;
            m_coreLinkMock.Setup(link => link.Request(It.IsAny<CommandConversation>(), It.IsAny<int>()))
                .Returns(() =>
                {
                    var task = new Task<StateResponse>(() =>
                    {
                        // Set up the "continuation" task.
                        var result = new Task<StateResponse>(() => new StateResponse());
                        result.Start();
                        
                        // This should only get called once, because the OriginalTask will be called on the second go.
                        noOfRuns++;

                        // If this is the first run, simulate timeout.
                        if (noOfRuns == 1)
                            throw new TaskTimeoutException<StateResponse>(result);

                        return result.Result;
                    });
                    task.Start();
                    return task;
                });

            var conversation = new CommandConversation(CommandType.Run);

            await m_controller.Command(conversation, () => TimeoutAction.Wait, timeoutMs: TimeoutMs);

            Assert.Equal(1, noOfRuns);
        }

        [Fact]
        public async void CancelsOnTimeout()
        {
            var noOfRuns = 0;
            m_coreLinkMock.Setup(link => link.Request(It.IsAny<CommandConversation>(), It.IsAny<int>()))
                .Returns(() =>
                {
                    var task = new Task<StateResponse>(() =>
                    {
                        // Set up the "continuation" task.
                        var result = new Task<StateResponse>(() => new StateResponse());
                        result.Start();
                        
                        // This should only get called once, because the OriginalTask will be called on the second go.
                        noOfRuns++;

                        // If this is the first run, simulate timeout.
                        if (noOfRuns == 1)
                            throw new TaskTimeoutException<StateResponse>(result);

                        return result.Result;
                    });
                    task.Start();
                    return task;
                });

            var conversation = new CommandConversation(CommandType.Run);

            var successfulResult = await m_controller.Command(conversation, () => TimeoutAction.Cancel, timeoutMs: TimeoutMs);

            Assert.Equal(1, noOfRuns);
            Assert.Null(successfulResult);
        }

        [Fact]
        public void ChecksState()
        {
            m_coreLinkMock.Setup(link => link.Request(It.IsAny<GetStateConversation>(), It.IsAny<int>())).Returns(
                () =>
                {
                    var task = new Task<StateResponse>(() => new StateResponse());
                    task.Start();
                    return task;
                });

            var stateUpdatedEvent = new AutoResetEvent(false);
            m_controller.StartStateChecking(timeoutResult => stateUpdatedEvent.Set());
            Assert.True(stateUpdatedEvent.WaitOne(TimeoutMs));
        }
    }
}
