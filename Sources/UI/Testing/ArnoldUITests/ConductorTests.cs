using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Communication;
using Moq;
using Xunit;

namespace GoodAI.Arnold.UI.Tests
{
    public class ConductorTests
    {
        private readonly Mock<ICoreProcess> m_coreProcessMock;
        private readonly Mock<ICoreProxy> m_coreProxyMock;
        private readonly Conductor m_conductor;

        public ConductorTests()
        {
            m_coreProcessMock = new Mock<ICoreProcess>();
            m_coreProcessMock.Setup(process => process.EndPoint).Returns(new EndPoint("localhost", 42));

            var coreLinkMock = new Mock<ICoreLink>();

            m_coreProxyMock = new Mock<ICoreProxy>();
            m_coreProxyMock.Setup(coreProxy => coreProxy.ShutdownAsync())
                .Returns(Task.CompletedTask)
                .Raises(coreProxy => coreProxy.StateChanged += null,
                    new StateChangedEventArgs(CoreState.Empty, CoreState.ShuttingDown));

            var coreProcessFactoryMock = new Mock<ICoreProcessFactory>();
            coreProcessFactoryMock.Setup(factory => factory.Create())
                .Returns(m_coreProcessMock.Object);

            var coreLinkFactoryMock = new Mock<ICoreLinkFactory>();
            coreLinkFactoryMock.Setup(factory => factory.Create(It.IsAny<EndPoint>()))
                .Returns(coreLinkMock.Object);

            var coreControllerFactoryMock = new Mock<ICoreControllerFactory>();
            coreControllerFactoryMock.Setup(factory => factory.Create(It.IsAny<ICoreLink>())).Returns(new CoreController(coreLinkMock.Object));


            var response = StateResponseBuilder.Build(StateType.ShuttingDown);
            var stateResponse = response.GetResponse(new StateResponse());

            coreLinkMock.Setup(link => link.Request(It.IsAny<CommandConversation>(), It.IsAny<int>())).Returns(() =>
            {
                return Task<StateResponse>.Factory.StartNew(
                    () => stateResponse);
            });


            var coreProxyFactoryMock = new Mock<ICoreProxyFactory>();
            coreProxyFactoryMock.Setup(factory => factory.Create(It.IsAny<ICoreController>(), It.IsAny<IModelUpdater>()))
                .Returns(m_coreProxyMock.Object);

            m_conductor = new Conductor(coreProcessFactoryMock.Object, coreLinkFactoryMock.Object,
                coreControllerFactoryMock.Object, coreProxyFactoryMock.Object, new Mock<IModelUpdaterFactory>().Object,
                new Mock<IModelProviderFactory>().Object);
        }

        [Fact]
        public async void ConductorSetsUpAndTearsDown()
        {
            await m_conductor.ConnectToCoreAsync();

            Assert.Equal(m_coreProxyMock.Object, m_conductor.CoreProxy);

            await m_conductor.ShutdownAsync();
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

            await m_conductor.StartSimulationAsync();
            m_coreProxyMock.Verify(coreProxy => coreProxy.RunAsync(It.IsAny<uint>()));

            await m_conductor.PauseSimulationAsync();
            m_coreProxyMock.Verify(coreProxy => coreProxy.PauseAsync());

            m_coreProxyMock.Setup(coreProxy => coreProxy.ClearAsync())
                .Callback(() =>
                {
                    m_coreProxyMock.Raise(coreProxy => coreProxy.StateChanged += null,
                        new StateChangedEventArgs(CoreState.Paused, CoreState.Empty));
                });


            await m_conductor.ClearSimulationAsync();
            m_coreProxyMock.Verify(coreProxy => coreProxy.ClearAsync());

            // This checks that the CleanupSimulation method completed. The conductor must emit the Null state.
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