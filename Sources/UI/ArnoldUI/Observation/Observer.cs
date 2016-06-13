using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GoodAI.Arnold.Forms;

namespace GoodAI.Arnold.Observation
{
    public interface IObserver
    {
        byte[] Data { set; get; }
    }

    public class BitmapObserver : IObserver
    {
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

        public byte[] Data
        {
            get { throw new NotImplementedException(); }
            set
            {
                using (var stream = new MemoryStream(value))
                {
                    Image = new Bitmap(stream);
                }
            }
        }
    }
}
