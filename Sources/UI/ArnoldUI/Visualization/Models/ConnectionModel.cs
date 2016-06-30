using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace GoodAI.Arnold.Visualization.Models
{
    public class ConnectionModel : SynapseModelBase
    {
        public static readonly Color4 ConnectionColor = new Color4(1f, 1f, 1f, 0.7f);

        public InputConnectorModel To { get; }
        public OutputConnectorModel From { get; }

        public ConnectionModel(OutputConnectorModel from, InputConnectorModel to)
        {
            From = from;
            To = to;

            Connect();

            Translucent = true;
        }

        private void Connect()
        {
            From.Connections.Add(this);
            To.Connections.Add(this);
        }

        public void Disconnect()
        {
            From.Connections.Remove(this);
            To.Connections.Remove(this);
        }

        protected override void UpdateModel(float elapsedMs)
        {
        }

        protected override void RenderModel(float elapsedMs)
        {
            Vector3 fromPosition = From.CurrentWorldMatrix.ExtractTranslation();
            Vector3 toPosition = To.CurrentWorldMatrix.ExtractTranslation();

            using (Blender.AveragingBlender())
            {
                GL.Color4(ConnectionColor);
                GL.Color4(Color.FromArgb(Math.Max((int) (255 * Alpha), 50), 255, 255, 255));
                GL.LineWidth(2f);

                GL.Begin(PrimitiveType.Lines);

                GL.Vertex3(fromPosition);
                GL.Vertex3(toPosition);

                GL.End();
            }
        }
    }
}
