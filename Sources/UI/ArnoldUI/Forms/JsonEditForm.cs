using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GoodAI.Arnold.Project;
using GoodAI.Arnold.Properties;
using WeifenLuo.WinFormsUI.Docking;

namespace GoodAI.Arnold.Forms
{
    public partial class JsonEditForm : DockContent
    {
        private static readonly string m_defaultBlueprint = Resources.DefaultBlueprint;

        private readonly IDesigner m_designer;

        public JsonEditForm(IDesigner designer)
        {
            m_designer = designer;

            InitializeComponent();

            content.TextChanged += OnTextChanged;
            content.Text = m_defaultBlueprint;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            content.TextChanged -= OnTextChanged;
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            m_designer.Blueprint = content.Text;
        }

        private void content_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Tab)
            {
                e.Handled = true;
                content.SelectedText = new string(' ', 4);
            }
        }
    }
}
