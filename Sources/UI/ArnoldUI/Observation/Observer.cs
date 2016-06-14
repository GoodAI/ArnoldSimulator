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
    public class GreyscaleObserver : IDisposable
    {
        // Injected.
        public ILog Log { get; set; } = NullLogger.Instance;

        private readonly ObserverDefinition m_observerDefinition;
        private readonly IModelProvider m_modelProvider;

        public GreyscaleObserver(ObserverDefinition observerDefinition, IModelProvider modelProvider)
        {
            m_observerDefinition = observerDefinition;
            m_modelProvider = modelProvider;
            m_modelProvider.ModelUpdated += OnModelUpdated;
        }

        private void OnModelUpdated(object sender, NewModelEventArgs e)
        {
            if (e.Model == null)
                return;

            byte[] data;
            if (!e.Model.Observers.TryGetValue(m_observerDefinition, out data))
            {
                Log.Warn("Observer with {@observerDefinition} is missing data from Core", m_observerDefinition);
                return;
            }

            Data(data);
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

        private void Data(byte[] data)
        {
            try
            {
                var image = new Bitmap(data.Length, 1);
                data.EachWithIndex((index, value) =>
                {
                    image.SetPixel(index, 0, Color.FromArgb(255, value, value, value));
                });
                Image = image;
                //using (var stream = new MemoryStream(data))
                //{
                //    Image = new Bitmap(stream);
                //}
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Observer with {@observerDefinition} received invalid data from Core", m_observerDefinition);
            }
        }

        public void Dispose()
        {
            m_modelProvider.ModelUpdated -= OnModelUpdated;
        }
    }
}
