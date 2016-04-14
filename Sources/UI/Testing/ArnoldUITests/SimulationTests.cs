using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Extensions;
using GoodAI.Arnold.Project;
using GoodAI.Arnold.Simulation;
using Google.Protobuf;
using Moq;
using Xunit;

using static GoodAI.Arnold.Network.StateData.Types;

namespace GoodAI.Arnold.UI.Tests
{
    public class SimulationTests
    {
        public class DummyCoreLink : ICoreLink
        {
            public bool Fail { get; set; }

            private static readonly Error m_error = new Error {Message = "Foo bar"};

            public Task<TimeoutResult<TResponse>> Request<TRequest, TResponse>(IConversation<TRequest, TResponse> conversation, int timeoutMs = 0)
                where TRequest : class, IMessage
                where TResponse : class, IMessage<TResponse>, new()
            {
                return Task<TimeoutResult<TResponse>>.Factory.StartNew(() =>
                {
                    TRequest request = conversation.Request;

                    var result = new TimeoutResult<TResponse>();

                    var commandRequest = request as CommandRequest;
                    if (commandRequest != null)
                    {
                        if (Fail)
                        {
                            result.Result = new StateResponse {Error = m_error} as TResponse;
                        }
                        else
                        {
                            StateType resultState;
                            switch (commandRequest.Command)
                            {
                                case CommandRequest.Types.CommandType.Load:
                                    resultState = StateType.Paused;
                                    break;
                                case CommandRequest.Types.CommandType.Run:
                                    resultState = StateType.Running;
                                    break;
                                case CommandRequest.Types.CommandType.Pause:
                                    resultState = StateType.Paused;
                                    break;
                                case CommandRequest.Types.CommandType.Clear:
                                    resultState = StateType.Empty;
                                    break;
                                case CommandRequest.Types.CommandType.Shutdown:
                                    resultState = StateType.ShuttingDown;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            result.Result = new StateResponse
                            {
                                Data = new StateData {State = resultState}
                            } as TResponse;
                        }
                    }

                    var getStateRequest = request as GetStateRequest;
                    if (getStateRequest != null)
                    {
                        result.Result = new StateResponse
                        {
                            Data = new StateData {State = StateType.Empty}
                        } as TResponse;
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
            const int timeoutMs = 1000;

            ICoreLink coreLink = new DummyCoreLink();

            var coreController = new CoreController(coreLink);

            var waitEvent = new AutoResetEvent(false);

            var simulation = new SimulationProxy(coreLink, coreController);
            Assert.Equal(SimulationState.Empty, simulation.State);

            simulation.StateUpdated += (sender, args) => waitEvent.Set();

            simulation.LoadBlueprint(new AgentBlueprint());
            await WaitFor(waitEvent);
            Assert.Equal(SimulationState.Paused, simulation.State);

            simulation.Run();
            await WaitFor(waitEvent);
            Assert.Equal(SimulationState.Running, simulation.State);

            simulation.Pause();
            await WaitFor(waitEvent);
            Assert.Equal(SimulationState.Paused, simulation.State);

            simulation.Clear();
            await WaitFor(waitEvent);
            Assert.Equal(SimulationState.Empty, simulation.State);

            // Test direct Clear from a Running state.
            simulation.LoadBlueprint(new AgentBlueprint());
            await WaitFor(waitEvent);
            simulation.Run();
            await WaitFor(waitEvent);
            simulation.Clear();
            await WaitFor(waitEvent);
            Assert.Equal(SimulationState.Empty, simulation.State);
        }

        [Fact]
        public void SimulationHandlesErrors()
        {
            const int timeoutMs = 100;

            var coreLink = new DummyCoreLink {Fail = true};

            var coreController = new CoreController(coreLink);

            var waitEvent = new AutoResetEvent(false);

            var simulation = new SimulationProxy(coreLink, coreController);
            Assert.Equal(SimulationState.Empty, simulation.State);

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

            var simulation = new SimulationProxy(coreLink, coreController);
            Assert.Equal(SimulationState.Empty, simulation.State);

            simulation.StateUpdated += (sender, args) => waitEvent.Set();

            simulation.RefreshState();
            waitEvent.WaitOne(timeoutMs);
            Assert.Equal(SimulationState.Empty, simulation.State);
        }
    }
}
