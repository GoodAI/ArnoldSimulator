using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoodAI.Arnold.Communication;
using GoodAI.Arnold.Extensions;
using GoodAI.Arnold.Observation;
using GoodAI.Arnold.Visualization.Models;
using GoodAI.Logging;

namespace GoodAI.Arnold.Core
{
    public class LockingModelUpdater : IModelUpdater
    {
        // Injected.
        public ILog Log { get; set; } = NullLogger.Instance;

        private const int TimeoutMs = 5000;

        private readonly ICoreLink m_coreLink;
        private readonly ICoreController m_coreController;
        private readonly IModelDiffApplier m_modelDiffApplier;

        private AutoResetEvent m_requestModelEvent;
        private AutoResetEvent m_modelReadEvent;

        private bool m_isNewModelReady;

        // Double buffering.
        private SimulationModel m_model;

        private CancellationTokenSource m_cancellation;
        private bool m_getFullModel;

        private bool m_filterChanged;
        private ModelFilter m_filter;
        private ModelResponse m_modelResponse;
        private IList<ObserverDefinition> m_observerRequests;

        public ModelFilter Filter
        {
            set
            {
                m_filter = value;
                m_filterChanged = true;
            }
        }

        public IList<ObserverDefinition> ObserverRequests
        {
            set { m_observerRequests = value; }
        }

        public LockingModelUpdater(ICoreLink coreLink, ICoreController coreController, IModelDiffApplier modelDiffApplier)
        {
            m_coreLink = coreLink;
            m_coreController = coreController;
            m_modelDiffApplier = modelDiffApplier;
        }

        public void Start()
        {
            Stop();

            m_requestModelEvent = new AutoResetEvent(false);
            m_modelReadEvent = new AutoResetEvent(false);

            m_cancellation = new CancellationTokenSource();

            m_getFullModel = true;

            m_model = new SimulationModel();

            // The empty model is what we have at the beginning.
            m_isNewModelReady = true;

            Task task = RepeatGetModelAsync(m_cancellation);
        }

        public void Stop()
        {
            if (m_cancellation != null && !m_cancellation.IsCancellationRequested)
                m_cancellation?.Cancel();

            // Disposal releases all the waiting threads. That will make the repeating task stop due to the cancellation.
            m_requestModelEvent?.Dispose();
            m_modelReadEvent?.Dispose();

            m_requestModelEvent = null;
            m_modelReadEvent = null;
        }

        /// <summary>
        /// Gets new model if there was one, otherwise null. This allows the paused network thread to swap the current
        /// model for a new one.
        /// </summary>
        /// <returns></returns>
        public SimulationModel GetNewModel()
        {
            if (m_requestModelEvent == null)
                throw new InvalidOperationException("Start() was not called");

            SimulationModel result = null;

            // If a new model is not ready, return null.
            if (m_isNewModelReady)
            {
                m_isNewModelReady = false;

                // This is null the first time a model is requested.
                if (m_modelResponse != null)
                {
                    ApplyModelDiff(m_modelResponse);
                    m_modelResponse = null;
                }

                result = m_model;

                // Let the other thread know the model has been updated so it can replace m_modelResponse.
                m_modelReadEvent.Set();
            }

            m_requestModelEvent.Set();

            // A new model is ready - retrieve it.
            return result;
        }

        enum WaitEventResult
        {
            EventSet,
            Cancelled
        }

        private static Task<WaitEventResult> WaitForEvent(AutoResetEvent resetEvent, CancellationTokenSource cancellation)
        {
            return Task<WaitEventResult>.Factory.StartNew(() =>
            {
                while (!resetEvent.WaitOne(TimeoutMs))
                    if (cancellation.IsCancellationRequested)
                        return WaitEventResult.Cancelled;

                // Even if the event fired, check if cancellation was done.
                return cancellation.IsCancellationRequested ? WaitEventResult.Cancelled : WaitEventResult.EventSet;
            });
        }

        // TODO(HonzaS): Add filtering.
        private async Task RepeatGetModelAsync(CancellationTokenSource cancellation)
        {
            // TODO(HonzaS): If a command is in progress and visualization is fast enough, this actively waits (loops).
            // Can we replace this with another reset event?
            m_modelResponse = null;
            while (true)
            {
                if (await WaitForEvent(m_requestModelEvent, cancellation) == WaitEventResult.Cancelled)
                    return;

                if (m_coreController.IsCommandInProgress)
                    continue;

                try
                {
                    // If there is no change to the filter, send null.
                    ModelFilter filterToSend = m_filterChanged ? m_filter : null;
                    m_filterChanged = false;

                    // Request a model diff from the core.
                    Log.Debug("Sending model request.");
                    var modelResponseTask =
                        m_coreLink.Request(new GetModelConversation(m_getFullModel, filterToSend, m_observerRequests),
                            TimeoutMs).ConfigureAwait(false);
                    m_getFullModel = false;

                    // Wait until the model has been read. This happens before the first request as well.
                    if (await WaitForEvent(m_modelReadEvent, cancellation) == WaitEventResult.Cancelled)
                        return;

                    // Wait for a new diff from the core.
                    m_modelResponse = await modelResponseTask;

                    // Allow visualization to read current (updated) model.
                    m_isNewModelReady = true;
                }
                catch (Exception exception)
                {
                    var timeoutException = exception as TaskTimeoutException<ModelResponse>;
                    if (timeoutException != null)
                    {
                        // TODO(HonzaS): handle this. Wait for a while and then request a new full model state.
                        Log.Error(timeoutException, "Model request timed out");
                    }
                    else
                    {
                        // Keep trying for now. TODO(Premek): Do something smarter...
                        Log.Error(exception, "Model retrieval failed");
                    }

                    m_getFullModel = true;
                }
            }
        }

        private void ApplyModelDiff(ModelResponse diff)
        {
            if (diff.IsFull)
                m_model = new SimulationModel();

            m_modelDiffApplier.ApplyModelDiff(m_model, diff);
        }


        public void Dispose()
        {
            Stop();
        }
    }
}
