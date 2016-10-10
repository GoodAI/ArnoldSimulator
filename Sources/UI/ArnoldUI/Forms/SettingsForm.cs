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
    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class SettingsForm : DockContent
    {
        public CoreProcessParameters CoreProcessParameters => new CoreProcessParameters(
            workingDirectory: coreProcessDirectoryTextBox.Text,
            rawArguments: coreProcessArgumentsTextBox.Text,
            maybePort: Validator.MaybeParsePortNumber(portTextBox.Text));

        private readonly UIMain m_uiMain;

        private readonly ColorTextControlValidator m_textControlValidator = new ColorTextControlValidator();
        
        public SettingsForm(UIMain uiMain)
        {
            m_uiMain = uiMain;

            m_uiMain.SimulationStateChanged += SimulationOnStateChanged;

            InitializeComponent();

            UpdateSubstitutedArguments();
        }

        private void UpdateSubstitutedArguments()
        {
            substitutedArgumentsTextBox.Text = CoreProcessParameters.SubstitutedArguments;
        }

        private void portTextBox_TextChanged(object sender, EventArgs e)
        {
            m_textControlValidator.ValidateAndColorControl(portTextBox, Validator.TryParsePortNumber);
              
            UpdateSubstitutedArguments();
        }

        private void coreProcessArgumentsTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateSubstitutedArguments();
        }

        private void UpdateControls()
        {
            loadBalancingEnabledCheckBox.Checked = m_uiMain.Conductor.CoreConfig.System.LoadBalancingEnabled;
        }

        private void SimulationOnStateChanged(object sender, StateChangedEventArgs stateChangedEventArgs)
        {
            if (!IsDisposed)
                Invoke((MethodInvoker)UpdateControls);
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
