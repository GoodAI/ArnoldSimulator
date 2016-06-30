using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GoodAI.Arnold.Visualization.Models
{
    public sealed class RegionModel : CompositeModelBase<IModel>
    {
        private Vector3 m_size;
        public const float RegionMargin = 2f;

        public uint Index { get; }
        public string Name { get; }
        public string Type { get; }

        public Vector3 Size
        {
            get { return m_size; }
            set
            {
                m_size = value;
                HalfSize = Size/2;
                InnerHalfSize = HalfSize - new Vector3(RegionMargin);
                InnerSize = 2*InnerHalfSize;
                foreach (NeuronModel neuron in Neurons)
                    neuron.UpdatePosition();
            }
        }

        public ConnectorStripModel<InputConnectorModel> InputConnectors { get; }
        public ConnectorStripModel<OutputConnectorModel> OutputConnectors { get; }
        public CompositeLookupModel<uint, NeuronModel> Neurons { get; } = new CompositeLookupModel<uint, NeuronModel>();
        public CompositeModel<SynapseModel> Synapses { get; } = new CompositeModel<SynapseModel>();

        public Vector3 HalfSize { get; private set; }

        public Vector3 InnerHalfSize { get; private set; }
        public Vector3 InnerSize { get; private set; }

        public RegionModel(uint index, string name, string type, Vector3 position, Vector3 size)
        {
            Index = index;
            Name = name;
            Type = type;

            Position = position;
            Size = size;

            Translucent = true;

            InputConnectors = new InputConnectorStripModel(this);
            OutputConnectors = new OutputConnectorStripModel(this);

            AddChild(InputConnectors);
            AddChild(OutputConnectors);
            AddChild(Neurons);
            AddChild(Synapses);
        }

        public void AddNeuron(NeuronModel neuron) => Neurons[neuron.Index] = neuron;

        public void AddSynapse(SynapseModel synapse) => Synapses.AddChild(synapse);

        protected override void UpdateModel(float elapsedMs)
        {
        }

        protected override void RenderModel(float elapsedMs)
        {
            using (Blender.MultiplicativeBlender())
            {
                GL.Color4(0, 0.2, 0.4, 0.6);

                GL.LineWidth(3f);

                GL.Begin(PrimitiveType.Lines);

                // Face one.

                GL.Vertex3(-HalfSize.X, -HalfSize.Y, -HalfSize.Z);
                GL.Vertex3(-HalfSize.X, HalfSize.Y, -HalfSize.Z);

                GL.Vertex3(-HalfSize.X, HalfSize.Y, -HalfSize.Z);
                GL.Vertex3(HalfSize.X, HalfSize.Y, -HalfSize.Z);

                GL.Vertex3(HalfSize.X, HalfSize.Y, -HalfSize.Z);
                GL.Vertex3(HalfSize.X, -HalfSize.Y, -HalfSize.Z);

                GL.Vertex3(HalfSize.X, -HalfSize.Y, -HalfSize.Z);
                GL.Vertex3(-HalfSize.X, -HalfSize.Y, -HalfSize.Z);

                // Face two.

                GL.Vertex3(-HalfSize.X, -HalfSize.Y, HalfSize.Z);
                GL.Vertex3(-HalfSize.X, HalfSize.Y, HalfSize.Z);

                GL.Vertex3(-HalfSize.X, HalfSize.Y, HalfSize.Z);
                GL.Vertex3(HalfSize.X, HalfSize.Y, HalfSize.Z);

                GL.Vertex3(HalfSize.X, HalfSize.Y, HalfSize.Z);
                GL.Vertex3(HalfSize.X, -HalfSize.Y, HalfSize.Z);

                GL.Vertex3(HalfSize.X, -HalfSize.Y, HalfSize.Z);
                GL.Vertex3(-HalfSize.X, -HalfSize.Y, HalfSize.Z);

                // Face connectors.

                GL.Vertex3(HalfSize.X, -HalfSize.Y, -HalfSize.Z);
                GL.Vertex3(HalfSize.X, -HalfSize.Y, HalfSize.Z);

                GL.Vertex3(-HalfSize.X, -HalfSize.Y, -HalfSize.Z);
                GL.Vertex3(-HalfSize.X, -HalfSize.Y, HalfSize.Z);

                GL.Vertex3(-HalfSize.X, HalfSize.Y, -HalfSize.Z);
                GL.Vertex3(-HalfSize.X, HalfSize.Y, HalfSize.Z);

                GL.Vertex3(HalfSize.X, HalfSize.Y, -HalfSize.Z);
                GL.Vertex3(HalfSize.X, HalfSize.Y, HalfSize.Z);

                GL.End();
            }
        }
    }
}
