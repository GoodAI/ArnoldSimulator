using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ArnoldUI;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Extensions;
using GoodAI.Arnold.Forms;
using GoodAI.Arnold.Network;
using GoodAI.Logging;
using WeifenLuo.WinFormsUI.Docking;

namespace GoodAI.Arnold
{
    public partial class MainForm : Form
    {
        // Injected.
        public ILog Log { get; set; } = NullLogger.Instance;

        private readonly UIMain m_uiMain;
        public LogForm LogForm { get; }
        public GraphForm GraphForm { get; }
        public VisualizationForm VisualizationForm { get; set; }

        public MainForm(UIMain uiMain, LogForm logForm, GraphForm graphForm)
        {
            InitializeComponent();

            m_uiMain = uiMain;

            LogForm = logForm;
            LogForm.Show(dockPanel, DockState.DockBottom);

            GraphForm = graphForm;
            GraphForm.Show(dockPanel, DockState.Document);

            // TODO(HonzaS): The blueprint should be in the Designer later.
            GraphForm.AgentBlueprint = m_uiMain.AgentBlueprint;

            m_uiMain.SimulationStateChanged += SimulationOnStateChanged;
            m_uiMain.SimulationStateChangeFailed += SimulationOnStateChangeFailed;

            UpdateButtons();
        }

        private void UpdateButtons()
        {
            connectButton.Enabled = !m_uiMain.Conductor.IsConnected;
            disconnectButton.Enabled = !connectButton.Enabled;

            runButton.Enabled = m_uiMain.Conductor.CoreState == CoreState.Paused || m_uiMain.Conductor.CoreState == CoreState.Empty;
            brainStepButton.Enabled = runButton.Enabled;
            pauseButton.Enabled = m_uiMain.Conductor.CoreState == CoreState.Running;

            showVisualizationButton.Enabled = m_uiMain.Conductor.CoreState != CoreState.Disconnected;
            showVisualizationButton.Checked = VisualizationForm != null && !VisualizationForm.IsDisposed;
        }

        private void DisableCommandButtons()
        {
            connectButton.Enabled = false;
            disconnectButton.Enabled = false;

            runButton.Enabled = false;
            brainStepButton.Enabled = false;
            pauseButton.Enabled = false;

            showVisualizationButton.Enabled = false;
        }

        private void SimulationOnStateChanged(object sender, StateChangedEventArgs stateChangedEventArgs)
        {
            Invoke((MethodInvoker)UpdateButtons);
        }

        private void SimulationOnStateChangeFailed(object sender, StateChangeFailedEventArgs e)
        {
        }

        private void VisualizationFormOnClosed(object sender, FormClosedEventArgs e)
        {
            VisualizationForm.FormClosed -= VisualizationFormOnClosed;
            VisualizationForm.Dispose();
            VisualizationForm = null;

            m_uiMain.VisualizationClosed();

            UpdateButtons();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void StartVisualization()
        {
            if (VisualizationForm == null || VisualizationForm.IsDisposed)
                VisualizationForm = new VisualizationForm(m_uiMain.Conductor);

            VisualizationForm.Show();
            VisualizationForm.FormClosed += VisualizationFormOnClosed;
        }


        private void connectButton_Click(object sender, EventArgs e)
        {
            // TODO(HonzaS): Handle the core type (local/remote).
            DisableCommandButtons();
            m_uiMain.ConnectToCore();
        }

        private void disconnectButton_Click(object sender, EventArgs e)
        {
            DisableCommandButtons();
            m_uiMain.Disconnect();
        }

        private void runButton_Click(object sender, EventArgs e)
        {
            if (m_uiMain.Conductor.CoreState == CoreState.Empty)
            {
                Log.Warn("Cannot run empty simulation, loading blueprint not yet implemented");
                return;
            }

            DisableCommandButtons();
            m_uiMain.StartSimulation();
        }

        private void pauseButton_Click(object sender, EventArgs e)
        {
            DisableCommandButtons();
            m_uiMain.PauseSimulation();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_uiMain.Dispose();
        }

        private void brainStepButton_Click(object sender, EventArgs e)
        {
            m_uiMain.PerformBrainStep();
        }

        private async void testButton_Click(object sender, EventArgs e)
        {
            ModelResponse response =
                await m_uiMain.Conductor.CoreLink.Request(new GetModelConversation(full: true), 60000)
                    .ConfigureAwait(false);
        }

        private void showVisualizationButton_CheckedChanged(object sender, EventArgs e)
        {
            if (showVisualizationButton.Checked)
                StartVisualization();
            else
                VisualizationForm?.Close();
        }
    }
}
