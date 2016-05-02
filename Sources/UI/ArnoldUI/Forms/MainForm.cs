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
using GoodAI.Arnold.Simulation;
using GoodAI.Arnold.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace GoodAI.Arnold
{
    public partial class MainForm : Form
    {
        private readonly UIMain m_uiMain;
        public LogForm LogForm { get; }
        public GraphForm GraphForm { get; }
        public VisualizationForm VisualizationForm { get; set; }

        public MainForm(UIMain uiMain)
        {
            InitializeComponent();

            m_uiMain = uiMain;

            LogForm = new LogForm();
            LogForm.Show(dockPanel, DockState.DockBottom);

            GraphForm = new GraphForm();
            GraphForm.Show(dockPanel, DockState.Document);

            // TODO(HonzaS): The blueprint should be in the Designer later.
            GraphForm.AgentBlueprint = m_uiMain.AgentBlueprint;

            m_uiMain.SimulationStateUpdated += SimulationOnStateUpdated;
            m_uiMain.SimulationStateChangeFailed += SimulationOnStateChangeFailed;

            UpdateButtons();
            //Simulation = new RemoteSimulation(new CoreLink(new ConverseProtoBufClient(new DummyConnector())));
        }

        private void UpdateButtons()
        {
            connectButton.Enabled = !m_uiMain.Conductor.IsConnected;
            disconnectButton.Enabled = !connectButton.Enabled;

            runButton.Enabled = m_uiMain.Conductor.CoreState == CoreState.Paused || m_uiMain.Conductor.CoreState == CoreState.Empty;
            pauseButton.Enabled = m_uiMain.Conductor.CoreState == CoreState.Running;
        }

        private void SimulationOnStateUpdated(object sender, StateUpdatedEventArgs stateUpdatedEventArgs)
        {
            Invoke((MethodInvoker)UpdateButtons);
        }

        private void SimulationOnStateChangeFailed(object sender, StateChangeFailedEventArgs e)
        {
        }

        private void VisualizationFormOnClosed(object sender, FormClosedEventArgs e)
        {
            VisualizationForm.FormClosed -= VisualizationFormOnClosed;
            m_uiMain.VisualizationClosed();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void StartSimulation()
        {
            m_uiMain.StartSimulation();

            if (VisualizationForm == null || VisualizationForm.IsDisposed)
                VisualizationForm = new VisualizationForm(m_uiMain.Conductor);

            VisualizationForm.Show();
            VisualizationForm.FormClosed += VisualizationFormOnClosed;
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            // TODO(HonzaS): Handle the core type (local/remote).
            m_uiMain.ConnectToCore();
        }

        private void disconnectButton_Click(object sender, EventArgs e)
        {
            m_uiMain.Disconnect();
        }

        private void runButton_Click(object sender, EventArgs e)
        {
            m_uiMain.StartSimulation();
        }

        private void pauseButton_Click(object sender, EventArgs e)
        {
        }
    }
}
