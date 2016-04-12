using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoodAI.Arnold.Network;
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

            public Task<TResponse> Request<TRequest, TResponse>(IConversation<TRequest, TResponse> conversation)
                where TRequest : class, IMessage
                where TResponse : class, IMessage<TResponse>, new()
            {
                return Task<TResponse>.Factory.StartNew(() =>
                {
                    TRequest request = conversation.Request;

                    var commandRequest = request as CommandRequest;
                    if (commandRequest != null)
                    {
                        if (Fail)
                            return new StateResponse {Error = m_error} as TResponse;

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
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        return new StateResponse
                        {
                            Data = new StateData {State = resultState}
                        } as TResponse;
                    }

                    var getStateRequest = request as GetStateRequest;
                    if (getStateRequest != null)
                    {
                        return new StateResponse
                        {
                            Data = new StateData {State = StateType.Empty}
                        } as TResponse;
                    }

                    // Unexpected test case.
                    throw new InvalidOperationException("Unexpected test case");
                });
            }
        }

        [Fact]
        public void StateMachineTransitionsCorrectly()
        {
            const int timeoutMs = 100;

            ICoreLink coreLink = new DummyCoreLink();

            var waitEvent = new AutoResetEvent(false);

            var simulation = new SimulationProxy(coreLink);
            Assert.Equal(SimulationState.Empty, simulation.State);

            simulation.StateUpdated += (sender, args) => waitEvent.Set();

            simulation.LoadBlueprint(new AgentBlueprint());
            waitEvent.WaitOne(timeoutMs);
            Assert.Equal(SimulationState.Paused, simulation.State);

            simulation.Run();
            waitEvent.WaitOne(timeoutMs);
            Assert.Equal(SimulationState.Running, simulation.State);

            simulation.Pause();
            waitEvent.WaitOne(timeoutMs);
            Assert.Equal(SimulationState.Paused, simulation.State);

            simulation.Clear();
            waitEvent.WaitOne(timeoutMs);
            Assert.Equal(SimulationState.Empty, simulation.State);
        }

        [Fact]
        public void SimulationHandlesErrors()
        {
            const int timeoutMs = 100;

            var coreLink = new DummyCoreLink();
            coreLink.Fail = true;

            var waitEvent = new AutoResetEvent(false);

            var simulation = new SimulationProxy(coreLink);
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

            var simulation = new SimulationProxy(coreLink);
            Assert.Equal(SimulationState.Empty, simulation.State);

            simulation.StateUpdated += (sender, args) => waitEvent.Set();

            simulation.RefreshState();
            waitEvent.WaitOne(timeoutMs);
            Assert.Equal(SimulationState.Empty, simulation.State);
        }
    }
}
