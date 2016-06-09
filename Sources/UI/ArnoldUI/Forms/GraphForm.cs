using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GoodAI.Arnold.Project;
using Graph;
using Graph.Compatibility;
using WeifenLuo.WinFormsUI.Docking;

namespace GoodAI.Arnold.Forms
{
    public partial class GraphForm : DockContent
    {
        private AgentBlueprint m_agentBlueprint;

        public AgentBlueprint AgentBlueprint
        {
            get { return m_agentBlueprint; }
            set
            {
                m_agentBlueprint = value;
                Reload();
            }
        }

        public GraphControl Desktop => graphControl;

        public GraphForm()
        {
            InitializeComponent();

            Desktop.CompatibilityStrategy = new AlwaysCompatible();
        }

        public void Reload()
        {
            UnregisterEvents();

            Desktop.RemoveNodes(Desktop.Nodes.ToList());

            LoadAgent();

            RegisterEvents();
        }

        private void LoadAgent()
        {
            foreach (Project.Node sensor in AgentBlueprint.Brain.Sensors)
                AddNodeView(sensor);

            foreach (Project.Node actuator in AgentBlueprint.Brain.Actuators)
                AddNodeView(actuator);

            foreach (Region region in AgentBlueprint.Brain.Regions)
                AddNodeView(region);
        }

        private void AddNodeView(Project.Node node)
        {
            Desktop.AddNode(new NodeView(node, Desktop));
        }

        private void UnregisterEvents()
        {
            Desktop.ConnectionAdded -= OnConnectionAdded;

            Desktop.ConnectionRemoving -= OnConnectionRemoving;
            Desktop.ConnectionRemoved -= OnConnectionRemoved;

            Desktop.NodeAdded -= OnNodeAdded;

            Desktop.NodeRemoving -= OnNodeRemoving;
            Desktop.NodeRemoved -= OnNodeRemoved;
        }

        private void RegisterEvents()
        {
            Desktop.ConnectionAdded += OnConnectionAdded;

            Desktop.ConnectionRemoving += OnConnectionRemoving;
            Desktop.ConnectionRemoved += OnConnectionRemoved;

            Desktop.NodeAdded += OnNodeAdded;

            Desktop.NodeRemoving += OnNodeRemoving;
            Desktop.NodeRemoved += OnNodeRemoved;
        }

        #region GraphControl events

        private void OnNodeRemoved(object sender, NodeEventArgs e)
        {
            var nodeView = (e.Node as NodeView);
            Project.Node node = nodeView?.Node;
            AgentBlueprint.Brain.Sensors.Remove(node);
            AgentBlueprint.Brain.Actuators.Remove(node);
            AgentBlueprint.Brain.Regions.Remove(node as Region);

            nodeView?.Dispose();
        }

        private void OnNodeRemoving(object sender, AcceptNodeEventArgs e)
        {
        }

        private void OnNodeAdded(object sender, AcceptNodeEventArgs e)
        {
            var nodeView = e.Node as NodeView;
            var node = nodeView?.Node as Region;
            if (node != null && !AgentBlueprint.Brain.Regions.Contains(node))
                AgentBlueprint.Brain.Regions.Add(node);
        }

        private void OnConnectionRemoved(object sender, NodeConnectionEventArgs e)
        {
            var connection = e.Connection.Tag as Connection;
            connection?.Disconnect();
        }

        private void OnConnectionRemoving(object sender, AcceptNodeConnectionEventArgs e)
        {
        }

        private void OnConnectionAdded(object sender, AcceptNodeConnectionEventArgs e)
        {
            NodeConnection graphConnection = e.Connection;
            Project.Node nodeFrom = (graphConnection.From.Node as NodeView)?.Node;
            Project.Node nodeTo = (graphConnection.To.Node as NodeView)?.Node;

            var indexFrom = (int) graphConnection.From.Item.Tag;
            var indexTo = (int) graphConnection.To.Item.Tag;

            if (nodeTo.InputPorts[indexTo] != null)
            {
                e.Cancel = true;
                return;
            }

            var connection = new Connection(nodeFrom, indexFrom, nodeTo, indexTo);
            connection.Connect();

            graphConnection.Tag = connection;
        }

        private void graphControl_MouseDown(object sender, MouseEventArgs e)
        {
            Desktop.Focus();
        }

        private void graphControl_MouseLeave(object sender, EventArgs e)
        {
            if (Desktop.Focused)
                Desktop.Parent.Focus();
        }

        #endregion

        #region Buttons

        private void addRegionButton_Click(object sender, EventArgs e)
        {
            AddNodeView(new Region());
        }

        private void addInputButton_Click(object sender, EventArgs e)
        {
            var region = GetSelectedRegion();
            if (region != null)
                region.InputPortCount++;
        }

        private void removeInputButton_Click(object sender, EventArgs e)
        {
            var region = GetSelectedRegion();
            if (region != null && region.InputPortCount > 0)
                region.InputPortCount--;
        }

        private void addOutputButton_Click(object sender, EventArgs e)
        {
            var region = GetSelectedRegion();
            if (region != null)
                region.OutputPortCount++;
        }

        private void removeOutputButton_Click(object sender, EventArgs e)
        {
            var region = GetSelectedRegion();
            if (region != null && region.OutputPortCount > 0)
                region.OutputPortCount--;
        }

        #endregion

        private Region GetSelectedRegion()
        {
            return (Desktop.FocusElement as NodeView)?.Node as Region;
        }
    }
}
