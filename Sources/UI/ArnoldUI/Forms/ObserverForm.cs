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

namespace GoodAI.Arnold.Forms
{
    public partial class ObserverForm : Form
    {
        private readonly BitmapObserver m_observer;

        public ObserverForm(BitmapObserver observer)
        {
            m_observer = observer;
            InitializeComponent();
            pictureBox.InterpolationMode = InterpolationMode.NearestNeighbor;

            m_observer.Updated += OnObserverUpdated;
        }

        private void OnObserverUpdated(object sender, EventArgs e)
        {
            pictureBox.Image = m_observer.Image;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            m_observer.Updated -= OnObserverUpdated;
        }
    }

    public class InterpolationPictureBox : PictureBox
    {
        public InterpolationPictureBox()
        {
            InterpolationMode = InterpolationMode.Low;
        }

        public InterpolationMode InterpolationMode { get; set; }

        protected override void OnPaint(PaintEventArgs pe)
        {
            pe.Graphics.InterpolationMode = InterpolationMode;
            base.OnPaint(pe);
        }
    }
}
