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

        private class DummyModelDiffApplier : IModelDiffApplier
        {
            public Dictionary<SimulationModel, int> DiffsApplied { get; } = new Dictionary<SimulationModel, int>();

            public void ApplyModelDiff(SimulationModel model, ModelResponse diff)
            {
                if (!DiffsApplied.ContainsKey(model))
                    DiffsApplied[model] = 0;

                DiffsApplied[model]++;
            }
        }

        private readonly ModelUpdater m_modelUpdater;
        private DummyModelDiffApplier m_modelDiffApplier;

        public ModelUpdaterTests()
        {
            var coreControllerMock = new Mock<ICoreController>();
            ICoreController coreController = coreControllerMock.Object;

            var coreLink = new DummyModelResponseCoreLink();
            
            m_modelDiffApplier = new DummyModelDiffApplier();

            m_modelUpdater = new ModelUpdater(coreLink, coreController, m_modelDiffApplier);
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

            SimulationModel model = m_modelUpdater.GetNewModel();  // First, empty model.

            // Apply 100 diffs on the model.
            for (int i = 0; i < 100; i++)
                model = WaitAndGetNewModel();

            // The model we currently have must have been updated exactly 100 times.
            Assert.Equal(100, m_modelDiffApplier.DiffsApplied[model]);

            // Note: The hidden (buffered) model might have been updated 99, 100 or 101 times in this moment.

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
