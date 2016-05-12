using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Extensions;
using GoodAI.Logging;

namespace GoodAI.Arnold.Network
{
    public interface IModelUpdater : IDisposable
    {
        void AllowModelRequest();
        SimulationModel GetNewModel();
        void Start();
        void Stop();
    }

    public class ModelUpdater : IModelUpdater
    {
        // Injected.
        public ILog Log { get; set; } = NullLogger.Instance;

        private const int TimeoutMs = 1000;

        private readonly ICoreLink m_coreLink;
        private readonly ICoreController m_coreController;

        private AutoResetEvent m_requestModelEvent;
        private AutoResetEvent m_modelReadEvent;

        private bool m_isNewModelReady;
        private SimulationModel m_newModel;
        private CancellationTokenSource m_cancellationTokenSource;

        public ModelUpdater(ICoreLink coreLink, ICoreController coreController)
        {
            m_coreLink = coreLink;
            m_coreController = coreController;
        }

        public void Start()
        {
            Stop();

            m_requestModelEvent = new AutoResetEvent(false);
            m_modelReadEvent = new AutoResetEvent(true);

            m_cancellationTokenSource = new CancellationTokenSource();
            Task task = RepeatGetModelAsync(m_cancellationTokenSource);
        }

        public void Stop()
        {
            if (m_cancellationTokenSource != null && !m_cancellationTokenSource.IsCancellationRequested)
                m_cancellationTokenSource?.Cancel();

            // Disposal releases all the waiting threads. That will make the repeating task stop due to the cancellation.
            m_requestModelEvent?.Dispose();
            m_modelReadEvent?.Dispose();

            m_requestModelEvent = null;
            m_modelReadEvent = null;
        }

        /// <summary>
        /// Called from Visualization. This limits the speed of requests.
        /// </summary>
        public void AllowModelRequest()
        {
            m_requestModelEvent.Set();
        }

        /// <summary>
        /// Gets new model if there was one, otherwise null. This allows the paused network thread to swap the current
        /// model for a new one.
        /// </summary>
        /// <returns></returns>
        public SimulationModel GetNewModel()
        {
            SimulationModel result = null;

            // If a new model is not ready, return null.
            if (m_isNewModelReady)
            {
                m_isNewModelReady = false;
                result = m_newModel;
            }

            // Allow the network thread to replace the model with whatever it has buffered.
            m_modelReadEvent.Set();

            // A new model is ready - retrieve it.
            return result;
        }

        enum WaitEventResult
        {
            EventSet,
            Cancelled
        }

        private static Task<WaitEventResult> WaitForEvent(AutoResetEvent resetEvent, CancellationTokenSource cancellationTokenSource)
        {
            return Task<WaitEventResult>.Factory.StartNew(() =>
            {
                while (!resetEvent.WaitOne(TimeoutMs))
                    if (cancellationTokenSource.IsCancellationRequested)
                        return WaitEventResult.Cancelled;

                return WaitEventResult.EventSet;
            });
        }

        // TODO(HonzaS): Add filtering.
        private async Task RepeatGetModelAsync(CancellationTokenSource cancellationTokenSource)
        {
            // TODO(HonzaS): If a command is in progress and visualization is fast enough, this actively waits (loops).
            // Can we replace this with another reset event?
            while (true)
            {
                if (await WaitForEvent(m_requestModelEvent, cancellationTokenSource) == WaitEventResult.Cancelled)
                    return;

                if (cancellationTokenSource.IsCancellationRequested)
                    return;

                if (!m_coreController.IsCommandInProgress)
                {
                    // TODO(HonzaS): If this is a first model request, request a full model.

                    try
                    {
                        ModelResponse modelResponse =
                            await m_coreLink.Request(new GetModelConversation(), TimeoutMs).ConfigureAwait(false);

                        // Process the model message.
                        SimulationModel newModel = ProcessModel(modelResponse);

                        // Wait for the visualization to read the previous model.
                        if (await WaitForEvent(m_modelReadEvent, cancellationTokenSource) == WaitEventResult.Cancelled)
                            return;

                        if (cancellationTokenSource.IsCancellationRequested)
                            return;

                        m_newModel = newModel;
                        m_isNewModelReady = true;
                    }
                    catch (Exception ex)
                    {
                        // TODO(HonzaS): Handle TaskTimeoutException better - wait, exponential backoff, ...
                        Log.Error(ex, "Model retrieval failed");
                    }
                }
            }
        }

        private static SimulationModel ProcessModel(ModelResponse data)
        {
            var model = new SimulationModel();
            return model;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
