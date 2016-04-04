using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graph;
using Graph.Items;
using Node = GoodAI.Arnold.Project.Node;

namespace GoodAI.Arnold.GraphViews
{
    public class NodeView : Graph.Node, IDisposable
    {
        private Project.Node m_node;

        public Project.Node Node
        {
            get { return m_node; }
            set
            {
                m_node = value;
                UpdateView();
            }
        }

        public GraphControl Owner { get; set; }

        private List<NodeLabelItem> InputLabels { get; }
        private List<NodeLabelItem> OutputLabels { get; }

        public NodeView(Project.Node node, GraphControl owner) : base(node.Name)
        {
            Owner = owner;

            InputLabels = new List<NodeLabelItem>();
            OutputLabels = new List<NodeLabelItem>();

            Node = node;
            node.Updated += NodeOnUpdated;
        }

        private void NodeOnUpdated(object sender, EventArgs e)
        {
            UpdateView();
        }

        public void UpdateView()
        {
            Title = Node.Name;
            Location = Node.Location;

            UpdateIO();

            Owner.Invalidate();
        }

        private void UpdateIO()
        {
            int inputChange = InputLabels.Count - Node.InputPortCount;

            for (int i = 0; i < inputChange; i++)
                RemoveInputLabel();

            for (int i = 0; i < -inputChange; i++)
                AddInputLabel();
            

            int outputChange = OutputLabels.Count - Node.OutputPortCount;

            for (int i = 0; i < outputChange; i++)
                RemoveOutputLabel();

            for (int i = 0; i < -outputChange; i++)
                AddOutputLabel();
        }

        private void AddInputLabel()
        {
            int i = InputLabels.Count;
            var inputLabel = new NodeLabelItem($"Input {i + 1}", true, false)
            {
                Tag = i
            };

            AddItem(inputLabel);
            InputLabels.Add(inputLabel);
        }

        private void RemoveInputLabel()
        {
            NodeLabelItem lastInputLabel = InputLabels.Last();
            if (lastInputLabel.Input.HasConnection)
                foreach (var connectionView in lastInputLabel.Input.Connectors.ToList())
                    Owner.Disconnect(connectionView);

            RemoveItem(lastInputLabel);
            InputLabels.RemoveAt(InputLabels.Count-1);
        }

        private void AddOutputLabel()
        {
            int i = OutputLabels.Count;
            var outputLabel = new NodeLabelItem($"Output {i + 1}", false, true)
            {
                Tag = i
            };

            AddItem(outputLabel);
            OutputLabels.Add(outputLabel);
        }

        private void RemoveOutputLabel()
        {
            NodeLabelItem lastOutputLabel = OutputLabels.Last();
            if (lastOutputLabel.Output.HasConnection)
                foreach (var connectionView in lastOutputLabel.Output.Connectors.ToList())
                    Owner.Disconnect(connectionView);

            RemoveItem(lastOutputLabel);
            OutputLabels.RemoveAt(OutputLabels.Count-1);
        }

        public void Dispose()
        {
            Node.Updated -= NodeOnUpdated;
        }

        public override void OnEndDrag()
        {
            base.OnEndDrag();
            Node.Location = Location;
        }
    }
}
