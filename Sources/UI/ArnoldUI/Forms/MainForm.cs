using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Forms;
using GoodAI.Arnold.Project;
using GoodAI.Logging;
using WeifenLuo.WinFormsUI.Docking;

namespace GoodAI.Arnold
{
    public sealed partial class MainForm : Form
    {
        // Injected.
        public ILog Log { get; set; } = NullLogger.Instance;

        private readonly UIMain m_uiMain;
        private string m_originalTitle;
        public LogForm LogForm { get; }
        public GraphForm GraphForm { get; }
        public VisualizationForm VisualizationForm { get; set; }
        public JsonEditForm JsonEditForm { get; set; }

        public MainForm(UIMain uiMain, LogForm logForm, GraphForm graphForm, JsonEditForm jsonEditForm)
        {
            InitializeComponent();

            m_originalTitle = Text;

            m_uiMain = uiMain;

            LogForm = logForm;
            LogForm.Show(dockPanel, DockState.DockBottom);

            //GraphForm = graphForm;
            //GraphForm.Show(dockPanel, DockState.Document);
            // TODO(HonzaS): The blueprint should be in the Designer later.
            //GraphForm.AgentBlueprint = m_uiMain.AgentBlueprint;

            JsonEditForm = jsonEditForm;
            JsonEditForm.Show(dockPanel, DockState.Document);

            SubscribeToUiMain();

            UpdateButtons();
        }

        private void SubscribeToUiMain()
        {
            m_uiMain.SimulationStateChanged += SimulationOnStateChanged;
            m_uiMain.SimulationStateChangeFailed += SimulationOnStateChangeFailed;
            m_uiMain.FileStatusChanged += UiMainOnFileStatusChanged;
        }

        private void UiMainOnFileStatusChanged(object sender, FileStatusChangedArgs fileStatusChangedArgs)
        {
            var projectName = Path.GetFileName(fileStatusChangedArgs.FileStatus.FileName);

            var projectNamePart = string.IsNullOrEmpty(projectName) ? "" : $" - {projectName}";
            var contentChangedPart = fileStatusChangedArgs.FileStatus.IsSaveNeeded ? " *" : "";
            Text = m_originalTitle + projectNamePart + contentChangedPart;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            m_uiMain.Initialize();
        }

        private void UpdateButtons()
        {
            if (m_uiMain.Conductor.CoreState == CoreState.CommandInProgress)
            {
                DisableCommandButtons();
                return;
            }

            connectButton.Enabled = !m_uiMain.Conductor.IsConnected;
            disconnectButton.Enabled = !connectButton.Enabled;

            loadBlueprintButton.Enabled = m_uiMain.Conductor.CoreState == CoreState.Empty;
            clearBlueprintButton.Enabled = m_uiMain.Conductor.IsConnected && (m_uiMain.Conductor.CoreState != CoreState.Empty);

            runButton.Enabled = (m_uiMain.Conductor.CoreState == CoreState.Paused) || (m_uiMain.Conductor.CoreState == CoreState.Empty);
            pauseButton.Enabled = m_uiMain.Conductor.CoreState == CoreState.Running;

            brainStepButton.Enabled = runButton.Enabled;
            bodyStepButton.Enabled = runButton.Enabled;

            showVisualizationButton.Enabled = m_uiMain.Conductor.CoreState != CoreState.Disconnected;
            showVisualizationButton.Checked = (VisualizationForm != null) && !VisualizationForm.IsDisposed;

            regularCheckpointingButton.Checked = m_uiMain.Conductor.CoreConfig.System.RegularCheckpointingEnabled;
        }

        private void DisableCommandButtons()
        {
            connectButton.Enabled = false;
            disconnectButton.Enabled = false;

            loadBlueprintButton.Enabled = false;
            clearBlueprintButton.Enabled = false;

            runButton.Enabled = false;
            pauseButton.Enabled = false;

            brainStepButton.Enabled = false;
            bodyStepButton.Enabled = false;

            showVisualizationButton.Enabled = false;
        }

        private void SimulationOnStateChanged(object sender, StateChangedEventArgs stateChangedEventArgs)
        {
            if (!IsDisposed)
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
                VisualizationForm = new VisualizationForm(m_uiMain);

            VisualizationForm.Show();
            VisualizationForm.FormClosed += VisualizationFormOnClosed;
        }

        private async Task RunButtonActionAsync(Func<Task> asyncAction)
        {
            DisableCommandButtons();

            try
            {
                await asyncAction();
            }
            finally
            {
                UpdateButtons();
            }
        }

        private async void connectButton_Click(object sender, EventArgs e)
        {
            // TODO(HonzaS): Handle the core type (local/remote).
            await RunButtonActionAsync(() => m_uiMain.ConnectToCoreAsync());
        }

        private void disconnectButton_Click(object sender, EventArgs e)
        {
            DisableCommandButtons();
            VisualizationForm?.Close();
            m_uiMain.Disconnect();
        }

        private async void runButton_Click(object sender, EventArgs e)
        {
            await RunButtonActionAsync(() => m_uiMain.StartSimulationAsync());
        }

        private async void pauseButton_Click(object sender, EventArgs e)
        {
            await RunButtonActionAsync(() => m_uiMain.PauseSimulationAsync());
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_uiMain.Dispose();
        }

        private async void brainStepButton_Click(object sender, EventArgs e)
        {
            await m_uiMain.PerformBrainStepAsync();
        }

        private void showVisualizationButton_CheckedChanged(object sender, EventArgs e)
        {
            if (showVisualizationButton.Checked)
                StartVisualization();
            else
                VisualizationForm?.Close();
        }

        private async void loadBlueprintButton_Click(object sender, EventArgs e)
        {
            await RunButtonActionAsync(() => m_uiMain.LoadBlueprintAsync());
        }

        private async void clearBlueprintButton_Click(object sender, EventArgs e)
        {
            await RunButtonActionAsync(() => m_uiMain.ClearBlueprintAsync());
        }

        private async void bodyStepButton_Click(object sender, EventArgs e)
        {
            await RunButtonActionAsync(() => m_uiMain.RunToBodyStepAsync());
        }

        private void newBlueprintButton_Click(object sender, EventArgs e)
        {
            m_uiMain.NewBlueprint();
        }

        private void openBlueprintButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                m_uiMain.OpenBlueprint(openFileDialog.FileName);
        }

        private void saveBlueprintButton_Click(object sender, EventArgs e)
        {
            if (m_uiMain.FileStatus.IsFileOpen)
            {
                m_uiMain.SaveBlueprint();
            }
            else
            {
                SaveAs();
            }
        }

        private void saveAsBlueprintButton_Click(object sender, EventArgs e)
        {
            SaveAs();
        }

        private void SaveAs()
        {
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
                m_uiMain.SaveBlueprint(saveFileDialog.FileName);
        }

        private async void regularCheckpointingButton_Click(object sender, EventArgs e)
        {
            try
            {
                float? checkpointingIntervalSeconds = ParseCheckpointingIntervalInSeconds();

                await m_uiMain.UpdateCoreConfig(coreConfig =>
                {
                    coreConfig.System.RegularCheckpointingEnabled = regularCheckpointingButton.Checked;

                    if (checkpointingIntervalSeconds.HasValue)
                        coreConfig.System.CheckpointingIntervalSeconds = checkpointingIntervalSeconds.Value;
                });

            }
            catch (Exception)
            {
                regularCheckpointingButton.Checked = m_uiMain.Conductor.CoreConfig.System.RegularCheckpointingEnabled;
                // (Already logged.)
            }
        }

        private float? ParseCheckpointingIntervalInSeconds()
        {
            uint intervalMs;

            if (!UInt32.TryParse(checkpointingIntervalTextBox.Text, out intervalMs))
                return null;

            return intervalMs/1000.0f;
        }
    }
}
