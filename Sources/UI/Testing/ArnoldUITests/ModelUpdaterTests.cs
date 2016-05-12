using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatBuffers;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Graphics.Models;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Network.Messages;
using Moq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Xunit;

namespace GoodAI.Arnold.UI.Tests
{
    public class ModelUpdaterTests
    {
        class DummyModelResponseCoreLink : ICoreLink
        {
            public Task<TResponse> Request<TRequest, TResponse>(IConversation<TRequest, TResponse> conversation, int timeoutMs) where TRequest : Table where TResponse : Table, new()
            {
                var request = conversation.RequestData as GetModelRequest;
                if (request == null)
                    throw new ArgumentException("Can only respond to a model request");

                return Task<TResponse>.Factory.StartNew(() =>
                {
                    var regionModel = new RegionModel("foo", "bar", Vector3.One, Vector3.Zero);

                    return ModelResponseBuilder.Build(new List<RegionModel> {regionModel})
                        .GetResponse(new ModelResponse()) as TResponse;
                });
            }
        }

        private readonly ModelUpdater m_modelUpdater;

        public ModelUpdaterTests()
        {
            var coreControllerMock = new Mock<ICoreController>();
            ICoreController coreController = coreControllerMock.Object;

            var coreLink = new DummyModelResponseCoreLink();

            m_modelUpdater = new ModelUpdater(coreLink, coreController);
        }

        [Fact]
        public void GetsModelSeveralTimes()
        {
            m_modelUpdater.Start();

            // There is an "initial" empty model.
            Assert.NotNull(m_modelUpdater.GetNewModel());

            for (int i = 0; i < 5; i++)
            {
                // Now that the first model was taken, there is no other to be had.
                Assert.Null(m_modelUpdater.GetNewModel());

                WaitAndGetNewModel();

                // Repeat several times to see that the dynamics work repeatedly.
            }

            // A new model was not allowed to be requested, so there is no update.
            Assert.Null(m_modelUpdater.GetNewModel());

            m_modelUpdater.Stop();
        }

        [Fact]
        public void IncrementallyUpdatesModels()
        {
            m_modelUpdater.Start();

            // Get model three times (apply model diff twice).
            m_modelUpdater.GetNewModel();  // First.

            WaitAndGetNewModel();  // Second model, apply first diff.

            SimulationModel model = WaitAndGetNewModel();  // Third model, apply second diff.

            Assert.Equal(2, model.Regions.Count);

            m_modelUpdater.Stop();
        }

        private SimulationModel WaitAndGetNewModel()
        {
            const int timeoutMs = 100;
            var stopwatch = new Stopwatch();

            SimulationModel third;
            while (true)
            {
                if ((third = m_modelUpdater.GetNewModel()) != null)
                    break;

                Assert.True(stopwatch.ElapsedMilliseconds < timeoutMs);
            }
            return third;
        }
    }
}
