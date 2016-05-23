using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace GoodAI.Arnold.Graphics.Models
{
    public abstract class ConnectorModel : ModelBase
    {
        private Vector3 m_size;
        public Vector3 HalfSize { get; private set; }
        public const float SizeX = 1f;
        public const float SizeY = 3f;
        public const float MarginZ = 0.5f;

        public Vector3 Size
        {
            get { return m_size; }
            private set
            {
                m_size = value;
                HalfSize = m_size/2;
            }
        }

        public int Slots { get; set; }

        public ConnectorModel(int slots)
        {
            Slots = slots;
        }

        protected override void UpdateModel(float elapsedMs)
        {
        }

        protected override void RenderModel(float elapsedMs)
        {
            using (Blender.AveragingBlender())
            {
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
        public InputConnectorModel(int slots) : base(slots)
        {
        }

        protected override Color4 Color { get; } = new Color4(255, 100, 255, 30);
    }

    public sealed class OutputConnectorModel : ConnectorModel
    {
        public OutputConnectorModel(int slots) : base(slots)
        {
        }

        protected override Color4 Color { get; } = new Color4(0, 255, 50, 30);
    }
}
