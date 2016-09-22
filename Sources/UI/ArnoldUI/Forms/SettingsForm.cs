using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GoodAI.Arnold.Core;
using WeifenLuo.WinFormsUI.Docking;

namespace GoodAI.Arnold.Forms
{
    public partial class SettingsForm : DockContent
    {
        public CoreProcessParameters CoreProcessParameters => new CoreProcessParameters(
            workingDirectory: coreProcessDirectoryTextBox.Text,
            rawArguments: coreProcessArgumentsTextBox.Text,
            maybePort: MaybeParseCorePort());

        public SettingsForm()
        {
            InitializeComponent();

            UpdateSubstitutedArguments();
        }

        private int? MaybeParseCorePort()
        {
            uint port;
            if (!UInt32.TryParse(portTextBox.Text, out port))
                return null;

            if (port > IPEndPoint.MaxPort)
                return null;

            return (int)port;
        }

        private void UpdateSubstitutedArguments()
        {
            substitutedArgumentsTextBox.Text = CoreProcessParameters.SubstitutedArguments;
        }

        private void portTextBox_TextChanged(object sender, EventArgs e)
        {
            portTextBox.ForeColor = MaybeParseCorePort().HasValue
                ? DefaultForeColor
                : Color.DarkRed;
              
            UpdateSubstitutedArguments();
        }

        private void coreProcessArgumentsTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateSubstitutedArguments();
        }
    }
}
