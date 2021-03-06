﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Communication;
using Moq;
using Xunit;

namespace GoodAI.Arnold.UI.Tests
{
    public class CoreProxyTests
    {
        private readonly IModelUpdater m_modelUpdater;

        public CoreProxyTests()
        {
            var modelUpdaterMock = new Mock<IModelUpdater>();
            m_modelUpdater = modelUpdaterMock.Object;
        }

        public class DummyCoreLink : ICoreLink
        {
            public bool Fail { get; set; }

            private StateType m_lastState = StateType.Empty;

            private static readonly string m_errorMessage = "Foo bar";

            Task<TResponse> ICoreLink.Request<TRequest, TResponse>(IConversation<TRequest, TResponse> conversation, int timeoutMs)
            {
                return Task<TResponse>.Factory.StartNew(() =>
                {
                    TRequest request = conversation.RequestData;

                    if (Fail)
                        throw new RemoteCoreException(m_errorMessage);

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

                    return responseMessage.GetResponse(new TResponse());
                });
            }
        }

        [Fact]
        public async void StateMachineTransitionsCorrectly()
        {
            // TODO(HonzaS): Rewrite this test to take advantage of the control methods being async.
            ICoreLink coreLink = new DummyCoreLink();

            var coreController = new CoreController(coreLink, keepaliveIntervalMs: 20);

            var coreProxy = new CoreProxy(coreController, m_modelUpdater);
            Assert.Equal(CoreState.Disconnected, coreProxy.State);

            // Simulate the core sending first state information.
            coreProxy.State = CoreState.Empty;

            await coreProxy.LoadBlueprintAsync("{}");
            Assert.Equal(CoreState.Paused, coreProxy.State);

            await coreProxy.RunAsync();
            Assert.Equal(CoreState.Running, coreProxy.State);

            await coreProxy.PauseAsync();
            Assert.Equal(CoreState.Paused, coreProxy.State);

            await coreProxy.ClearAsync();
            Assert.Equal(CoreState.Empty, coreProxy.State);

            // Test direct Clear from a Running state.
            await coreProxy.LoadBlueprintAsync("{}");
            await coreProxy.RunAsync();
            await coreProxy.ClearAsync();
            await coreProxy.ShutdownAsync();
            Assert.Equal(CoreState.ShuttingDown, coreProxy.State);
        }

        [Fact]
        public async void SimulationHandlesErrors()
        {
            var coreLink = new DummyCoreLink {Fail = true};

            var coreController = new CoreController(coreLink);

            var coreProxy = new CoreProxy(coreController, m_modelUpdater);
            Assert.Equal(CoreState.Disconnected, coreProxy.State);

            coreProxy.State = CoreState.Empty;

            await Assert.ThrowsAsync<RemoteCoreException>(() => coreProxy.LoadBlueprintAsync("{}"));
        }

        [Fact]
        public void RefreshesState()
        {
            const int timeoutMs = 150;

            var waitEvent = new AutoResetEvent(false);

            var coreControllerMock = new Mock<ICoreController>();
            coreControllerMock.Setup(controller => controller.StartStateChecking(It.IsAny<Action<KeepaliveResult>>()))
                .Callback(() => waitEvent.Set());
            var coreController = coreControllerMock.Object;

            var coreProxy = new CoreProxy(coreController, m_modelUpdater);

            Assert.True(waitEvent.WaitOne(timeoutMs));
        }
    }
}
