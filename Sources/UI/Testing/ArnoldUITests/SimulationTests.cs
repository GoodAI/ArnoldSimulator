using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FlatBuffers;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Extensions;
using GoodAI.Arnold.Network.Messages;
using GoodAI.Arnold.Project;
using Moq;
using Xunit;

namespace GoodAI.Arnold.UI.Tests
{
    public class SimulationTests
    {
        public class DummyCoreLink : ICoreLink
        {
            public bool Fail { get; set; }

            private StateType m_lastState = StateType.Empty;

            private static readonly string m_errorMessage = "Foo bar";

            Task<TimeoutResult<Response<TResponse>>> ICoreLink.Request<TRequest, TResponse>(IConversation<TRequest, TResponse> conversation, int timeoutMs)
            {
                return Task<TimeoutResult<Response<TResponse>>>.Factory.StartNew(() =>
                {
                    TRequest request = conversation.RequestData;

                    var result = new TimeoutResult<Response<TResponse>>();

                    if (Fail)
                    {
                        ResponseMessage responseMessage = ErrorResponseBuilder.Build(m_errorMessage);

                        result.Result = new Response<TResponse>(responseMessage.GetResponse(new ErrorResponse()));
                    }
                    else
                    {

                        StateType resultState = StateType.Empty;

                        var commandRequest = request as CommandRequest;
                        if (commandRequest != null)
                        {
                            switch (commandRequest.Command)
                            {
                                case CommandType.Load:
                                    resultState = StateType.Paused;
                                    break;
                                case CommandType.Run:
                                    resultState = StateType.Running;
                                    break;
                                case CommandType.Pause:
                                    resultState = StateType.Paused;
                                    break;
                                case CommandType.Clear:
                                    resultState = StateType.Empty;
                                    break;
                                case CommandType.Shutdown:
                                    resultState = StateType.ShuttingDown;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            m_lastState = resultState;
                        }

                        var getStateRequest = request as GetStateRequest;
                        if (getStateRequest != null)
                            resultState = m_lastState;

                        ResponseMessage responseMessage = StateResponseBuilder.Build(resultState);

                        result.Result = new Response<TResponse>(responseMessage.GetResponse(new TResponse()));
                    }
                    
                    return result;
                });
            }
        }

        private async Task WaitFor(AutoResetEvent waitEvent)
        {
            await Task.Factory.StartNew(waitEvent.WaitOne).ConfigureAwait(false);
        }

        [Fact]
        public async void StateMachineTransitionsCorrectly()
        {
            ICoreLink coreLink = new DummyCoreLink();

            var coreController = new CoreController(coreLink);

            var waitEvent = new AutoResetEvent(false);

            var simulation = new CoreProxy(coreLink, coreController);
            Assert.Equal(CoreState.Empty, simulation.State);

            simulation.StateChanged += (sender, args) => waitEvent.Set();

            simulation.LoadBlueprint(new AgentBlueprint());
            await WaitFor(waitEvent);
            Assert.Equal(CoreState.Paused, simulation.State);

            simulation.Run();
            await WaitFor(waitEvent);
            Assert.Equal(CoreState.Running, simulation.State);

            simulation.Pause();
            await WaitFor(waitEvent);
            Assert.Equal(CoreState.Paused, simulation.State);

            simulation.Clear();
            await WaitFor(waitEvent);
            Assert.Equal(CoreState.Empty, simulation.State);

            // Test direct Clear from a Running state.
            simulation.LoadBlueprint(new AgentBlueprint());
            await WaitFor(waitEvent);
            simulation.Run();
            await WaitFor(waitEvent);
            simulation.Clear();
            await WaitFor(waitEvent);
            Assert.Equal(CoreState.Empty, simulation.State);
        }

        [Fact]
        public void SimulationHandlesErrors()
        {
            const int timeoutMs = 100;

            var coreLink = new DummyCoreLink {Fail = true};

            var coreController = new CoreController(coreLink);

            var waitEvent = new AutoResetEvent(false);

            var simulation = new CoreProxy(coreLink, coreController);
            Assert.Equal(CoreState.Empty, simulation.State);

            simulation.StateChangeFailed += (sender, args) => waitEvent.Set();

            simulation.LoadBlueprint(new AgentBlueprint());
            // The event should be fired via StateChangeFailed.
            Assert.True(waitEvent.WaitOne(timeoutMs), "Timed out");
        }

        [Fact]
        public void RefreshesState()
        {
            const int timeoutMs = 100;

            ICoreLink coreLink = new DummyCoreLink();

            var waitEvent = new AutoResetEvent(false);

            var coreControllerMock = new Mock<ICoreController>();
            var coreController = coreControllerMock.Object;

            var simulation = new CoreProxy(coreLink, coreController);
            Assert.Equal(CoreState.Empty, simulation.State);

            simulation.StateChanged += (sender, args) => waitEvent.Set();

            waitEvent.WaitOne(timeoutMs);
            Assert.Equal(CoreState.Empty, simulation.State);
        }
    }
}
