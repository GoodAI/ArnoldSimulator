using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArnoldUI.Network;
using GoodAI.Arnold.Extensions;
using GoodAI.Arnold.Network;
using Moq;
using Xunit;

namespace GoodAI.Arnold.UI.Tests
{
    public class CoreControllerTests
    {
        private ICoreLink m_coreLink;
        private CoreController m_controller;
        private Mock<ICoreLink> m_coreLinkMock;

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
                    var task = new Task<TimeoutResult<StateResponse>>(() =>
                    {
                        Thread.Sleep(100);
                        return new TimeoutResult<StateResponse>();
                    });
                    task.Start();
                    return task;
                });

            var conversation = new CommandConversation
            {
                Request =
                {
                    Command = CommandRequest.Types.CommandType.Run
                }
            };

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
                    var task = new Task<TimeoutResult<StateResponse>>(() =>
                    {
                        var result = new TimeoutResult<StateResponse>();
                        if (noOfRuns == 0)
                        {
                            result.TimedOut = true;
                        } else
                        {
                            result.Result = new StateResponse();
                        }
                        noOfRuns++;
                        return result;
                    });
                    task.Start();
                    return task;
                });

            var conversation = new CommandConversation
            {
                Request =
                {
                    Command = CommandRequest.Types.CommandType.Run
                }
            };

            await m_controller.Command(conversation, response => { }, () => TimeoutAction.Retry, 30);

            Assert.Equal(2, noOfRuns);
        }

        [Fact]
        public async void WaitsForCommands()
        {
            var noOfRuns = 0;
            m_coreLinkMock.Setup(link => link.Request(It.IsAny<CommandConversation>(), It.IsAny<int>()))
                .Returns(() =>
                {
                    var task = new Task<TimeoutResult<StateResponse>>(() =>
                    {
                        // Set up the "continuation" task.
                        var result = new TimeoutResult<StateResponse>
                        {
                            OriginalTask = new Task<StateResponse>(() => new StateResponse())
                        };
                        result.OriginalTask.Start();
                        
                        // If this is the first run, simulate timeout.
                        if (noOfRuns == 0)
                            result.TimedOut = true;

                        // This should only get called once, because the OriginalTask will be called on the second go.
                        noOfRuns++;
                        return result;
                    });
                    task.Start();
                    return task;
                });

            var conversation = new CommandConversation
            {
                Request =
                {
                    Command = CommandRequest.Types.CommandType.Run
                }
            };

            await m_controller.Command(conversation, response => { }, () => TimeoutAction.Wait, 30);

            Assert.Equal(1, noOfRuns);
        }

        [Fact]
        public async void CancelsOnTimeout()
        {
            bool successCalled = false;

            m_coreLinkMock.Setup(link => link.Request(It.IsAny<CommandConversation>(), It.IsAny<int>()))
                .Returns(() =>
                {
                    var task = new Task<TimeoutResult<StateResponse>>(() =>
                    {
                        var result = new TimeoutResult<StateResponse>
                        {
                            OriginalTask = new Task<StateResponse>(() => new StateResponse())
                        };
                        result.OriginalTask.Start();
                        result.TimedOut = true;

                        return result;
                    });
                    task.Start();
                    return task;
                });

            var conversation = new CommandConversation
            {
                Request =
                {
                    Command = CommandRequest.Types.CommandType.Run
                }
            };

            await m_controller.Command(conversation, response => successCalled = true, () => TimeoutAction.Cancel, 30);

            Assert.False(successCalled);
        }
    }
}
