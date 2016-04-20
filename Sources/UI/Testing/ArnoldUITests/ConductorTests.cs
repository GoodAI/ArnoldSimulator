using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Simulation;
using GoodAI.Arnold.Extensions;
using GoodAI.Arnold.Network.Messages;
using GoodAI.Arnold.Project;
using GoodAI.Net.ConverseSharpFlatBuffers;
using Moq;
using Xunit;

namespace GoodAI.Arnold.UI.Tests
{
    public class ConductorTests
    {
        private Mock<ICoreProxy> m_coreProxyMock;
        private Mock<ICoreLink> m_coreLinkMock;
        private Mock<ISimulation> m_simulationMock;
        private Mock<ICoreProxyFactory> m_coreProxyFactoryMock;
        private Mock<ICoreLinkFactory> m_coreLinkFactoryMock;
        private Mock<ICoreControllerFactory> m_coreControllerFactoryMock;
        private Mock<ISimulationFactory> m_simulationFactoryMock;
        private readonly Conductor m_conductor;
        private ICoreController m_coreController;
        private const int TimeoutMs = 100;

        public ConductorTests()
        {
            m_coreProxyMock = new Mock<ICoreProxy>();
            m_coreLinkMock = new Mock<ICoreLink>();
            m_simulationMock = new Mock<ISimulation>();

            m_coreProxyFactoryMock = new Mock<ICoreProxyFactory>();
            m_coreProxyFactoryMock.Setup(factory => factory.Create(It.IsAny<EndPoint>()))
                .Returns(m_coreProxyMock.Object);

            m_coreLinkFactoryMock = new Mock<ICoreLinkFactory>();
            m_coreLinkFactoryMock.Setup(factory => factory.Create(It.IsAny<EndPoint>(), It.IsAny<IResponseParser>()))
                .Returns(m_coreLinkMock.Object);

            m_coreController = new CoreController(m_coreLinkMock.Object);
            m_coreControllerFactoryMock = new Mock<ICoreControllerFactory>();
            m_coreControllerFactoryMock.Setup(factory => factory.Create(It.IsAny<ICoreLink>())).Returns(m_coreController);

            var response = StateResponseBuilder.Build(StateType.ShuttingDown);
            var stateResponse = response.GetResponse(new StateResponse());

            m_coreLinkMock.Setup(link => link.Request(It.IsAny<CommandConversation>(), It.IsAny<int>())).Returns(() =>
            {
                return Task<TimeoutResult<Response<StateResponse>>>.Factory.StartNew(
                    () => new TimeoutResult<Response<StateResponse>> {Result = new Response<StateResponse>(stateResponse)});
            });


            m_simulationFactoryMock = new Mock<ISimulationFactory>();
            m_simulationFactoryMock.Setup(factory => factory.Create(It.IsAny<ICoreLink>(), It.IsAny<ICoreController>()))
                .Returns(m_simulationMock.Object);

            m_conductor = new Conductor(m_coreProxyFactoryMock.Object, m_coreLinkFactoryMock.Object,
                m_coreControllerFactoryMock.Object,
                m_simulationFactoryMock.Object);
        }

        [Fact]
        public void ConductorSetsUpAndTearsDown()
        {
            m_conductor.Setup();

            m_coreProxyMock.Verify(proxy => proxy.Start());
            Assert.Equal(m_coreLinkMock.Object, m_conductor.CoreLink);
            Assert.Equal(m_simulationMock.Object, m_conductor.Simulation);

            m_simulationMock.Verify(simulation => simulation.RefreshState());

            var waitEvent = new AutoResetEvent(false);
            m_conductor.SimulationStateUpdated += (sender, args) => waitEvent.Set();
            m_conductor.Teardown();

            waitEvent.WaitOne();
            m_coreProxyMock.Verify(proxy => proxy.Dispose());
        }

        [Fact]
        public void DoubleSetupFails()
        {
            m_conductor.Setup();

            Assert.Throws<InvalidOperationException>(() => m_conductor.Setup());
        }

        [Fact]
        public void DoubleTeardownFails()
        {
            m_conductor.Setup();
            var waitEvent = new AutoResetEvent(false);
            m_conductor.SimulationStateUpdated += (sender, args) => waitEvent.Set();
            m_conductor.Teardown();

            waitEvent.WaitOne();
            Assert.Throws<InvalidOperationException>(() => m_conductor.Teardown());
        }

        [Fact]
        public void StartsAndStopsSimulation()
        {
            m_conductor.Setup();

            m_conductor.LoadBlueprint(new AgentBlueprint());
            m_simulationMock.Verify(simulation => simulation.LoadBlueprint(It.IsAny<AgentBlueprint>()));

            m_conductor.StartSimulation();
            m_simulationMock.Verify(simulation => simulation.Run(It.IsAny<uint>()));

            m_conductor.PauseSimulation();
            m_simulationMock.Verify(simulation => simulation.Pause());

            m_simulationMock.Setup(simulation => simulation.Clear())
                .Callback(() =>
                {
                    m_simulationMock.Raise(simulation => simulation.StateUpdated += null,
                        new StateUpdatedEventArgs(SimulationState.Paused, SimulationState.Empty));
                });


            var waitEvent = new AutoResetEvent(false);
            m_conductor.SimulationStateUpdated += (sender, args) => waitEvent.Set();
            m_conductor.KillSimulation();
            m_simulationMock.Verify(simulation => simulation.Clear());

            // This checks that the CleanupSimulation method completed. The conductor must emit the Null state.
            Assert.True(waitEvent.WaitOne());

            Assert.Equal(SimulationState.Null, m_conductor.SimulationState);
        }
    }
}