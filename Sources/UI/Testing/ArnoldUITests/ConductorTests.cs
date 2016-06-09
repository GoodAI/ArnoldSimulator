using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Communication;
using GoodAI.Arnold.Project;
using Moq;
using Xunit;

namespace GoodAI.Arnold.UI.Tests
{
    public class ConductorTests
    {
        private Mock<ICoreProcess> m_coreProcessMock;
        private Mock<ICoreLink> m_coreLinkMock;
        private Mock<ICoreProxy> m_coreProxyMock;
        private Mock<ICoreProcessFactory> m_coreProcessFactoryMock;
        private Mock<ICoreLinkFactory> m_coreLinkFactoryMock;
        private Mock<ICoreControllerFactory> m_coreControllerFactoryMock;
        private Mock<ICoreProxyFactory> m_coreProxyFactoryMock;
        private readonly Conductor m_conductor;
        private ICoreController m_coreController;
        private Mock<IModelUpdaterFactory> m_modelUpdaterFactoryMock;
        private Mock<IModelProviderFactory> m_modelProviderFactoryMock;
        private const int TimeoutMs = 100;

        public ConductorTests()
        {
            m_coreProcessMock = new Mock<ICoreProcess>();
            m_coreProcessMock.Setup(process => process.EndPoint).Returns(new EndPoint("localhost", 42));

            m_coreLinkMock = new Mock<ICoreLink>();

            m_coreProxyMock = new Mock<ICoreProxy>();
            m_coreProxyMock.Setup(coreProxy => coreProxy.ShutdownAsync())
                .Returns(Task.CompletedTask)
                .Raises(coreProxy => coreProxy.StateChanged += null,
                    new StateChangedEventArgs(CoreState.Empty, CoreState.ShuttingDown));

            m_coreProcessFactoryMock = new Mock<ICoreProcessFactory>();
            m_coreProcessFactoryMock.Setup(factory => factory.Create())
                .Returns(m_coreProcessMock.Object);

            m_coreLinkFactoryMock = new Mock<ICoreLinkFactory>();
            m_coreLinkFactoryMock.Setup(factory => factory.Create(It.IsAny<EndPoint>()))
                .Returns(m_coreLinkMock.Object);

            m_coreController = new CoreController(m_coreLinkMock.Object);
            m_coreControllerFactoryMock = new Mock<ICoreControllerFactory>();
            m_coreControllerFactoryMock.Setup(factory => factory.Create(It.IsAny<ICoreLink>())).Returns(m_coreController);

            m_modelUpdaterFactoryMock = new Mock<IModelUpdaterFactory>();
            m_modelProviderFactoryMock = new Mock<IModelProviderFactory>();


            var response = StateResponseBuilder.Build(StateType.ShuttingDown);
            var stateResponse = response.GetResponse(new StateResponse());

            m_coreLinkMock.Setup(link => link.Request(It.IsAny<CommandConversation>(), It.IsAny<int>())).Returns(() =>
            {
                return Task<StateResponse>.Factory.StartNew(
                    () => stateResponse);
            });


            m_coreProxyFactoryMock = new Mock<ICoreProxyFactory>();
            m_coreProxyFactoryMock.Setup(factory => factory.Create(It.IsAny<ICoreController>(), It.IsAny<IModelUpdater>()))
                .Returns(m_coreProxyMock.Object);

            m_conductor = new Conductor(m_coreProcessFactoryMock.Object, m_coreLinkFactoryMock.Object,
                m_coreControllerFactoryMock.Object, m_coreProxyFactoryMock.Object, m_modelUpdaterFactoryMock.Object,
                m_modelProviderFactoryMock.Object);
        }

        [Fact]
        public async void ConductorSetsUpAndTearsDown()
        {
            await m_conductor.ConnectToCoreAsync();

            Assert.Equal(m_coreProxyMock.Object, m_conductor.CoreProxy);

            var waitEvent = new AutoResetEvent(false);
            m_conductor.StateChanged += (sender, args) => waitEvent.Set();
            m_conductor.Shutdown();

            waitEvent.WaitOne();
            Assert.Null(m_conductor.CoreProxy);
        }

        [Fact]
        public async void DoubleSetupFails()
        {
            await m_conductor.ConnectToCoreAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(() => m_conductor.ConnectToCoreAsync());
        }

        [Fact(Skip = "When we have the logic in conductor done, enable and fix this")]
        public async void StartsAndStopsSimulation()
        {
            await m_conductor.ConnectToCoreAsync();

            await m_conductor.LoadBlueprintAsync("{}");
            m_coreProxyMock.Verify(coreProxy => coreProxy.LoadBlueprintAsync(It.IsAny<string>()));

            m_conductor.StartSimulation();
            m_coreProxyMock.Verify(coreProxy => coreProxy.RunAsync(It.IsAny<uint>()));

            await m_conductor.PauseSimulationAsync();
            m_coreProxyMock.Verify(coreProxy => coreProxy.PauseAsync());

            m_coreProxyMock.Setup(coreProxy => coreProxy.ClearAsync())
                .Callback(() =>
                {
                    m_coreProxyMock.Raise(coreProxy => coreProxy.StateChanged += null,
                        new StateChangedEventArgs(CoreState.Paused, CoreState.Empty));
                });


            var waitEvent = new AutoResetEvent(false);
            m_conductor.StateChanged += (sender, args) => waitEvent.Set();
            m_conductor.KillSimulation();
            m_coreProxyMock.Verify(coreProxy => coreProxy.ClearAsync());

            // This checks that the CleanupSimulation method completed. The conductor must emit the Null state.
            Assert.True(waitEvent.WaitOne());

            Assert.Equal(CoreState.Empty, m_conductor.CoreState);
        }

        [Fact]
        public async void NeedsEndpointToStartWithLocalCore()
        {
            m_coreProcessMock.Setup(process => process.EndPoint).Returns(null as EndPoint);

            await Assert.ThrowsAsync<InvalidOperationException>(() => m_conductor.ConnectToCoreAsync());
        }
    }
}