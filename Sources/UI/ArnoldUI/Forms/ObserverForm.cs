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
        public GreyscaleObserver Observer { get; }

        public ObserverForm(GreyscaleObserver observer)
        {
            Observer = observer;
            InitializeComponent();

            Observer.Updated += OnObserverUpdated;
        }

        private void OnObserverUpdated(object sender, EventArgs e)
        {
            this.Invoke(() => pictureBox.Image = Observer.Image);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Observer.Updated -= OnObserverUpdated;
            Observer.Dispose();
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
