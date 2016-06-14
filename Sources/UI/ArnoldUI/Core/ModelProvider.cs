using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Visualization.Models;
using GoodAI.Logging;

namespace GoodAI.Arnold.Core
{
    public class NewModelEventArgs : EventArgs
    {
        public SimulationModel Model { get; }

        public NewModelEventArgs(SimulationModel model)
        {
            Model = model;
        }
    }

    public interface IModelProvider
    {
        ModelFilter Filter { set; }
        void GetNewModel();
        event EventHandler<NewModelEventArgs> ModelUpdated;
        SimulationModel LastReceivedModel { get; }
    }

    public class ModelProvider : IModelProvider
    {
        private readonly IConductor m_conductor;
        
        // Injected.
        public ILog Log { get; set; } = NullLogger.Instance;

        public SimulationModel LastReceivedModel { get; private set; }

        public event EventHandler<NewModelEventArgs> ModelUpdated;

        public ModelFilter Filter
        {
            set
            {
                if (m_conductor.CoreState == CoreState.Disconnected || m_conductor.CoreState == CoreState.Empty)
                    return;

                m_conductor.CoreProxy.ModelUpdater.Filter = value;
            }
        }

        public ModelProvider(IConductor conductor)
        {
            m_conductor = conductor;
        }

        public void GetNewModel()
        {
            // NOTE: the idea was that we would get empty model when core state is Empty,
            // but in reality there is some time in the Empty state when the core is not really connected yet
            // You can comment out "m_conductor.CoreState == CoreState.Empty" to debug model retrieval error handling.
            //if (m_conductor.CoreState == CoreState.Disconnected || m_conductor.CoreState == CoreState.Empty)
            if (m_conductor.CoreState == CoreState.Disconnected)
                return;

            try
            {
                SimulationModel newModel = m_conductor.CoreProxy.ModelUpdater.GetNewModel();
                if (newModel != null)
                    LastReceivedModel = newModel;

                ModelUpdated?.Invoke(this, new NewModelEventArgs(newModel));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to get new model");
            }
        }
    }
}
