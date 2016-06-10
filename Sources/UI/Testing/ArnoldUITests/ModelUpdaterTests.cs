using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FlatBuffers;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Visualization.Models;
using GoodAI.Arnold.Communication;
using Moq;
using OpenTK;
using Xunit;

namespace GoodAI.Arnold.UI.Tests
{
    public abstract class ModelUpdaterTestBase
    {
        protected class DummyModelResponseCoreLink : ICoreLink
        {
            public Filter Filter { get; set; }

            public Task<TResponse> Request<TRequest, TResponse>(IConversation<TRequest, TResponse> conversation, int timeoutMs) where TRequest : Table where TResponse : Table, new()
            {
                var request = conversation.RequestData as GetModelRequest;
                if (request == null)
                    throw new ArgumentException("Can only respond to a model request");

                Filter = Filter ?? request.Filter;

                return Task<TResponse>.Factory.StartNew(() =>
                {
                    var regionModel = new RegionModel(1, "foo", "bar", Vector3.One, Vector3.Zero);

                    return ModelResponseBuilder.Build(new List<RegionModel> {regionModel})
                        .GetResponse(new ModelResponse()) as TResponse;
                });
            }
        }

        protected class DummyModelDiffApplier : IModelDiffApplier
        {
            public Dictionary<SimulationModel, int> DiffsApplied { get; } = new Dictionary<SimulationModel, int>();

            public void ApplyModelDiff(SimulationModel model, ModelResponse diff)
            {
                if (!DiffsApplied.ContainsKey(model))
                    DiffsApplied[model] = 0;

                DiffsApplied[model]++;
            }
        }

        private readonly IModelUpdater m_modelUpdater;
        protected readonly DummyModelDiffApplier ModelDiffApplier;
        protected readonly DummyModelResponseCoreLink CoreLink;
        protected readonly ICoreController CoreController;

        public ModelUpdaterTestBase()
        {
            var coreControllerMock = new Mock<ICoreController>();
            CoreController = coreControllerMock.Object;

            CoreLink = new DummyModelResponseCoreLink();
            
            ModelDiffApplier = new DummyModelDiffApplier();

            m_modelUpdater = SetupModelUpdater();
        }

        protected abstract IModelUpdater SetupModelUpdater();

        private SimulationModel WaitAndGetNewModel()
        {
            const int timeoutMs = 10000;
            var stopwatch = new Stopwatch();

            SimulationModel model;
            while (true)
            {
                if ((model = m_modelUpdater.GetNewModel()) != null)
                    break;

                Assert.True(stopwatch.ElapsedMilliseconds < timeoutMs);
            }
            return model;
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
            Assert.Equal(100, ModelDiffApplier.DiffsApplied[model]);

            // Note: The hidden (buffered) model might have been updated 99, 100 or 101 times in this moment.

            m_modelUpdater.Stop();
        }

        [Fact]
        public void GetNewModelFailsWhenNotStarted()
        {
            Assert.Throws<InvalidOperationException>(() => m_modelUpdater.GetNewModel());
        }

        [Fact]
        public void SendsFilter()
        {
            m_modelUpdater.Start();

            m_modelUpdater.GetNewModel();
            WaitAndGetNewModel();
            Assert.Null(CoreLink.Filter);

            var modelFilter = new ModelFilter
            {
                Boxes = {new FilterBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6))}
            };

            m_modelUpdater.Filter = modelFilter;

            // This allows the updater to provide the already received modelResponse.
            WaitAndGetNewModel();

            // Now the updater is sending the filter, wait for the filtered model.
            WaitAndGetNewModel();

            // Only now can we check if it was sent (there is a race condition before the second wait).
            Assert.NotNull(CoreLink.Filter);

            m_modelUpdater.Stop();
        }
    }

    public class ModelUpdaterTests : ModelUpdaterTestBase
    {
        protected override IModelUpdater SetupModelUpdater()
        {
            return new ModelUpdater(CoreLink, CoreController, ModelDiffApplier);
        }
    }

    public class LockingModelUpdaterTests : ModelUpdaterTestBase
    {
        protected override IModelUpdater SetupModelUpdater()
        {
            return new LockingModelUpdater(CoreLink, CoreController, ModelDiffApplier);
        }
    }
}
