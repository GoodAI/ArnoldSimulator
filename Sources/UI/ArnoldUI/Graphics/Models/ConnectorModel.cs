using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace GoodAI.Arnold.Graphics.Models
{
    public enum ConnectorDirection
    {
        Forward,
        Backward
    }

    public abstract class ConnectorModel : ModelBase
    {
        public const float SizeX = 1f;
        public const float SizeY = 3f;
        public const float MarginZ = 0.5f;

        public RegionModel Region { get; }

        public string Name { get; private set; }

        public ConnectionModel Connection { get; set; }

        public ConnectorDirection Direction { get; }

        public Vector3 Size
        {
            get { return m_size; }
            private set
            {
                m_size = value;
                HalfSize = m_size/2;
            }
        }
        private Vector3 m_size;

        public Vector3 HalfSize { get; private set; }

        public uint SlotCount { get; set; }

        public ConnectorModel(RegionModel region, ConnectorDirection direction, string name, uint slotCount)
        {
            Region = region;
            Direction = direction;
            SlotCount = slotCount;
            Name = name;
        }

        protected override void UpdateModel(float elapsedMs)
        {
        }

        protected override void RenderModel(float elapsedMs)
        {
            using (Blender.AveragingBlender())
            {
                // Draw the body.

                GL.Color4(Color);

                GL.Begin(PrimitiveType.Quads);

                GL.Vertex3(-HalfSize.X, -HalfSize.Y, -HalfSize.Z);
                GL.Vertex3(HalfSize.X, -HalfSize.Y, -HalfSize.Z);
                GL.Vertex3(HalfSize.X, HalfSize.Y, -HalfSize.Z);
                GL.Vertex3(-HalfSize.X, HalfSize.Y, -HalfSize.Z);

                GL.Vertex3(-HalfSize.X, -HalfSize.Y, HalfSize.Z);
                GL.Vertex3(HalfSize.X, -HalfSize.Y, HalfSize.Z);
                GL.Vertex3(HalfSize.X, HalfSize.Y, HalfSize.Z);
                GL.Vertex3(-HalfSize.X, HalfSize.Y, HalfSize.Z);

                GL.Vertex3(-HalfSize.X, -HalfSize.Y, -HalfSize.Z);
                GL.Vertex3(-HalfSize.X, HalfSize.Y, -HalfSize.Z);
                GL.Vertex3(-HalfSize.X, HalfSize.Y, HalfSize.Z);
                GL.Vertex3(-HalfSize.X, -HalfSize.Y, HalfSize.Z);

                GL.Vertex3(HalfSize.X, -HalfSize.Y, -HalfSize.Z);
                GL.Vertex3(HalfSize.X, HalfSize.Y, -HalfSize.Z);
                GL.Vertex3(HalfSize.X, HalfSize.Y, HalfSize.Z);
                GL.Vertex3(HalfSize.X, -HalfSize.Y, HalfSize.Z);

                GL.Vertex3(-HalfSize.X, -HalfSize.Y, -HalfSize.Z);
                GL.Vertex3(HalfSize.X, -HalfSize.Y, -HalfSize.Z);
                GL.Vertex3(HalfSize.X, -HalfSize.Y, HalfSize.Z);
                GL.Vertex3(-HalfSize.X, -HalfSize.Y, HalfSize.Z);

                GL.Vertex3(-HalfSize.X, HalfSize.Y, -HalfSize.Z);
                GL.Vertex3(HalfSize.X, HalfSize.Y, -HalfSize.Z);
                GL.Vertex3(HalfSize.X, HalfSize.Y, HalfSize.Z);
                GL.Vertex3(-HalfSize.X, HalfSize.Y, HalfSize.Z);

                GL.End();

                // Draw the borders.

                GL.Color4(0.7f, 0.7f, 0.7f, 0.1f);
                GL.LineWidth(2f);

                GL.Begin(PrimitiveType.LineLoop);

                GL.Vertex3(-HalfSize.X, -HalfSize.Y, -HalfSize.Z);
                GL.Vertex3(-HalfSize.X, -HalfSize.Y, HalfSize.Z);
                GL.Vertex3(-HalfSize.X, HalfSize.Y, HalfSize.Z);
                GL.Vertex3(-HalfSize.X, HalfSize.Y, -HalfSize.Z);

                GL.End();

                GL.Begin(PrimitiveType.LineLoop);

                GL.Vertex3(HalfSize.X, -HalfSize.Y, -HalfSize.Z);
                GL.Vertex3(HalfSize.X, -HalfSize.Y, HalfSize.Z);
                GL.Vertex3(HalfSize.X, HalfSize.Y, HalfSize.Z);
                GL.Vertex3(HalfSize.X, HalfSize.Y, -HalfSize.Z);

                GL.End();

                GL.Begin(PrimitiveType.Lines);

                GL.Vertex3(-HalfSize.X, -HalfSize.Y, -HalfSize.Z);
                GL.Vertex3(HalfSize.X, -HalfSize.Y, -HalfSize.Z);

                GL.Vertex3(-HalfSize.X, HalfSize.Y, HalfSize.Z);
                GL.Vertex3(HalfSize.X, HalfSize.Y, HalfSize.Z);

                GL.Vertex3(-HalfSize.X, -HalfSize.Y, HalfSize.Z);
                GL.Vertex3(HalfSize.X, -HalfSize.Y, HalfSize.Z);

                GL.Vertex3(-HalfSize.X, HalfSize.Y, -HalfSize.Z);
                GL.Vertex3(HalfSize.X, HalfSize.Y, -HalfSize.Z);

                GL.End();
            }
        }

        internal void Reposition(float position, float sizeZ)
        {
            Position = new Vector3(0, 0, position + sizeZ/2);
            Size = new Vector3(ConnectorModel.SizeX, ConnectorModel.SizeY, sizeZ - MarginZ*2);
        }

        protected abstract Color4 Color { get; }
    }

    public sealed class InputConnectorModel : ConnectorModel
    {

        protected override Color4 Color { get; } = new Color4(255, 100, 255, 30);

        public InputConnectorModel(RegionModel region, string name, uint slotCount) : base(region, ConnectorDirection.Backward, name, slotCount)
        {
        }
    }

    public sealed class OutputConnectorModel : ConnectorModel
    {
        protected override Color4 Color { get; } = new Color4(0, 255, 50, 30);

        public OutputConnectorModel(RegionModel region, string name, uint slotCount) : base(region, ConnectorDirection.Forward, name, slotCount)
        {
        }
    }
}
