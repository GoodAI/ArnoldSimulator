using System;
using System.Collections.Generic;
using System.Data;
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
        private readonly UIMain m_uiMain;
        public bool IsClosing { get; private set; }
        public CanvasObserver Observer { get; }

        public ObserverForm(UIMain uiMain, CanvasObserver observer)
        {
            m_uiMain = uiMain;
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

            IsClosing = true;

            Observer.Updated -= OnObserverUpdated;
            m_uiMain.CloseObserver(Observer.Definition);
        }

        public void CloseOnce()
        {
            if (IsClosing)
                return;

            IsClosing = true;
            this.Invoke(Close);
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
