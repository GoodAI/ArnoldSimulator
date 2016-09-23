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
using GoodAI.Arnold.UI;
using WeifenLuo.WinFormsUI.Docking;

namespace GoodAI.Arnold.Forms
{
    public partial class SettingsForm : DockContent
    {
        public CoreProcessParameters CoreProcessParameters => new CoreProcessParameters(
            workingDirectory: coreProcessDirectoryTextBox.Text,
            rawArguments: coreProcessArgumentsTextBox.Text,
            maybePort: MaybeParseCorePort());

        private readonly UIMain m_uiMain;

        private readonly ColorTextControlValidator m_textControlValidator = new ColorTextControlValidator();
        
        public SettingsForm(UIMain uiMain)
        {
            m_uiMain = uiMain;

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

        private async Task UpdateLoadBalancingSettings()
        {
            try
            {
                var loadBalancingIntervalSeconds = (float?)Validator.MaybeParseUInt(loadBalancingIntervalTextBox.Text);

                await m_uiMain.UpdateCoreConfig(coreConfig =>
                {
                    coreConfig.System.LoadBalancingEnabled = loadBalancingEnabledCheckBox.Checked;

                    if (loadBalancingIntervalSeconds.HasValue)
                        coreConfig.System.LoadBalancingIntervalSeconds = loadBalancingIntervalSeconds.Value;
                });
            }
            catch (Exception)
            {
                loadBalancingEnabledCheckBox.Checked = m_uiMain.Conductor.CoreConfig.System.RegularCheckpointingEnabled;
                // (Already logged.)
            }
        }

        private async void loadBalancingEnabledCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            await UpdateLoadBalancingSettings();
        }

        private void loadBalancingIntervalTextBox_TextChanged(object sender, EventArgs e)
        {
            m_textControlValidator.ValidateAndColorControl(loadBalancingIntervalTextBox, Validator.TryParseUInt);
        }

        private async void loadBalancingIntervalTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                await UpdateLoadBalancingSettings();
            }
        }
    }
}
