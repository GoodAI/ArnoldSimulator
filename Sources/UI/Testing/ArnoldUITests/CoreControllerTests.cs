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
        private CoreController m_controller;
        private Mock<ICoreLink> m_coreLinkMock;

        private const int TimeoutMs = 30;

        public CoreControllerTests()
        {
            m_coreLinkMock = new Mock<ICoreLink>();
            m_coreLink = m_coreLinkMock.Object;

            m_controller = new CoreController(m_coreLink);
        }

        [Fact]
        public async void OnlyAllowsOneCommand()
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

            bool first = false;
            bool second = false;

            Task firstTask = m_controller.Command(conversation, response => first = true, () => TimeoutAction.Wait);
            await m_controller.Command(conversation, response => second = true, () => TimeoutAction.Wait);

            firstTask.Wait();

            Assert.True(first);
            Assert.False(second);
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

            await m_controller.Command(conversation, response => { }, () => TimeoutAction.Retry, TimeoutMs);

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

            await m_controller.Command(conversation, response => { }, () => TimeoutAction.Wait, TimeoutMs);

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

            bool successfulResult = false;
            await m_controller.Command(conversation, response => { successfulResult = true; }, () => TimeoutAction.Cancel, TimeoutMs);

            Assert.Equal(1, noOfRuns);
            Assert.False(successfulResult);
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
