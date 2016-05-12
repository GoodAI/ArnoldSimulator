using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Extensions;
using GoodAI.Arnold.Graphics.Models;
using GoodAI.Logging;

namespace GoodAI.Arnold.Network
{
    public interface IModelUpdater : IDisposable
    {
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
        private readonly IModelDiffApplier m_modelDiffApplier;

        private AutoResetEvent m_requestModelEvent;
        private AutoResetEvent m_modelReadEvent;

        private bool m_isNewModelReady;

        // Double buffering.
        private SimulationModel m_currentModel;
        private SimulationModel m_previousModel;

        private CancellationTokenSource m_cancellation;


        public ModelUpdater(ICoreLink coreLink, ICoreController coreController, IModelDiffApplier modelDiffApplier)
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
            Task task = RepeatGetModelAsync(m_cancellation);

            m_currentModel = new SimulationModel();
            m_previousModel = new SimulationModel();

            // The empty model is what we have at the beginning.
            m_isNewModelReady = true;
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
            SimulationModel result = null;

            // If a new model is not ready, return null.
            if (m_isNewModelReady)
            {
                m_isNewModelReady = false;
                result = m_currentModel;
                m_currentModel = m_previousModel;
                m_previousModel = result;

                // Allow the network thread to replace the model with whatever it has buffered.
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
            ModelResponse modelResponse = null;
            while (true)
            {
                if (await WaitForEvent(m_requestModelEvent, cancellation) == WaitEventResult.Cancelled)
                    return;

                if (m_coreController.IsCommandInProgress)
                    continue;

                try
                {
                    // Request a model diff from the core.
                    // TODO(HonzaS): If this is a first model request in this "session", request a full model.
                    var modelResponseTask = m_coreLink.Request(new GetModelConversation(), TimeoutMs).ConfigureAwait(false);

                    // Wait until the model has been read. This happens before the first request as well.
                    if (await WaitForEvent(m_modelReadEvent, cancellation) == WaitEventResult.Cancelled)
                        return;

                    // Wait for the previous diff to be applied to the new model (skip if this is the first request).
                    if (modelResponse != null)
                        await ApplyModelDiffAsync(modelResponse);

                    // Wait for a new diff from the core.
                    modelResponse = await modelResponseTask;

                    // Apply current diff to the new model.
                    await ApplyModelDiffAsync(modelResponse);

                    // Allow visualization to read current (updated) model.
                    m_isNewModelReady = true;
                }
                catch (TaskTimeoutException<ModelResponse> timeoutException)
                {
                    // TODO(HonzaS): handle this. Wait for a while and then request a new full model state.
                    Log.Error(timeoutException, "Model request timed out");
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "Model retrieval failed");
                    Stop();
                    return;
                }
            }
        }

        private Task ApplyModelDiffAsync(ModelResponse diff)
        {
            return Task.Factory.StartNew(() =>
            {
                m_modelDiffApplier.ApplyModelDiff(m_currentModel, diff);
            });
        }


        public void Dispose()
        {
            Stop();
        }
    }
}
