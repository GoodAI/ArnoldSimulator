using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GoodAI.Arnold.Observation;
using GoodAI.Arnold.Extensions;
using WeifenLuo.WinFormsUI.Docking;

namespace GoodAI.Arnold.Forms
{
    public partial class ObserverForm : DockContent
    {
        private readonly GreyscaleObserver m_observer;

        public ObserverForm(GreyscaleObserver observer)
        {
            m_observer = observer;
            InitializeComponent();

            m_observer.Updated += OnObserverUpdated;
        }

        private void OnObserverUpdated(object sender, EventArgs e)
        {
            this.Invoke(() => pictureBox.Image = m_observer.Image);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            m_observer.Updated -= OnObserverUpdated;
        }
    }

    public class InterpolationPictureBox : PictureBox
    {
        protected override void OnPaint(PaintEventArgs pe)
        {
            pe.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            pe.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            base.OnPaint(pe);
        }
    }
}
