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
using GoodAI.Arnold.Forms;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Project;
using GoodAI.Arnold.Simulation;
using GoodAI.Net.ConverseSharp;
using WeifenLuo.WinFormsUI.Docking;
using Region = GoodAI.Arnold.Project.Region;

namespace GoodAI.Arnold
{
    public partial class MainForm : Form
    {
        public LogForm LogForm { get; }
        public GraphForm GraphForm { get; }
        public VisualizationForm VisualizationForm { get; set; }

        public AgentBlueprint AgentBlueprint { get; }

        public RemoteSimulation Simulation { get; set; }

        public MainForm()
        {
            InitializeComponent();

            AgentBlueprint = new AgentBlueprint();
            AgentBlueprint.Brain.Regions.Add(new Project.Region
            {
                Location = new PointF(100, 100)
            });

            LogForm = new LogForm();
            LogForm.Show(dockPanel, DockState.DockBottom);

            GraphForm = new GraphForm();
            GraphForm.Show(dockPanel, DockState.Document);
            GraphForm.AgentBlueprint = AgentBlueprint;

            //Simulation = new RemoteSimulation(new CoreLink(new ConverseProtoBufClient(new DummyConnector())));
        }

        private void VisualizationFormOnClosed(object sender, FormClosedEventArgs e)
        {
            VisualizationForm.FormClosed -= VisualizationFormOnClosed;
            Simulation.Clear();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartSimulation();
        }

        private void StartSimulation()
        {
            Simulation.LoadBlueprint(AgentBlueprint);

            if (VisualizationForm == null || VisualizationForm.IsDisposed)
                VisualizationForm = new VisualizationForm(Simulation);

            VisualizationForm.Show();
            VisualizationForm.FormClosed += VisualizationFormOnClosed;
        }
    }
}
