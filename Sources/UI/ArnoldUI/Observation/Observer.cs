using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GoodAI.Arnold.Forms;

namespace GoodAI.Arnold.Observation
{
    public class BitmapObserver
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
    }
}
