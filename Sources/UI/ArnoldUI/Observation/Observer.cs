using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Extensions;
using GoodAI.Arnold.Forms;
using GoodAI.Logging;

namespace GoodAI.Arnold.Observation
{
    public interface IObserver : IDisposable
    {
        ObserverDefinition Definition { get; }
    }

    // TODO(HonzaS): Add a base class requiring IModelProvider in constructor as a hint at how to get the data.
    public class CanvasObserver : IObserver
    {
        // Injected.
        public ILog Log { get; set; } = NullLogger.Instance;

        public ObserverDefinition Definition { get; }

        private readonly IModelProvider m_modelProvider;

        private IPainter m_painter = new RedGreenPainter(new AutoValueScaler());

        public CanvasObserver(ObserverDefinition observerDefinition, IModelProvider modelProvider)
        {
            Definition = observerDefinition;
            m_modelProvider = modelProvider;
            m_modelProvider.ModelUpdated += OnModelUpdated;
        }

        private void OnModelUpdated(object sender, NewModelEventArgs e)
        {
            if (e.Model == null)
                return;

            ObserverData data;
            if (!e.Model.Observers.TryGetValue(Definition, out data))
            {
                // This is only a debug message - sometimes it happens that the observer gets a new model
                // before the request propagates to Core.
                Log.Debug("Observer with {@observerDefinition} is missing data from Core", Definition);
                return;
            }

            SetData(data);
        }

        public event EventHandler Updated;

        private Image m_image;

        public Image Image
        {
            get { return m_image; }
            set
            {
                m_image = value;
                Updated?.Invoke(this, EventArgs.Empty);
            }
        }

        private void SetData(ObserverData data)
        {
            try
            {
                Image = m_painter.Paint(data);
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Observer with {@observerDefinition} received invalid data from Core", Definition);
            }
        }

        public void Dispose()
        {
            m_modelProvider.ModelUpdated -= OnModelUpdated;
        }
    }
}
