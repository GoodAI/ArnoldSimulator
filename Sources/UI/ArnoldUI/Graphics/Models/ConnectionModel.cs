using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Graphics.Models;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace GoodAI.Arnold.Graphics.Models
{
    public class ConnectionModel : ModelBase
    {
        public static readonly Color4 ConnectionColor = new Color4(1f, 1f, 1f, 0.7f);

        public InputConnectorModel To { get; set; }
        public OutputConnectorModel From { get; set; }

        public ConnectionModel(OutputConnectorModel from, InputConnectorModel to)
        {
            From = from;
            To = to;

            Connect();
        }

        private void Connect()
        {
            From.Connection = this;
            To.Connection = this;
        }

        public void Disconnect()
        {
            From.Connection = null;
            To.Connection = null;
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
                GL.LineWidth(2f);

                GL.Begin(PrimitiveType.Lines);

                GL.Vertex3(fromPosition);
                GL.Vertex3(toPosition);

                GL.End();
            }
        }
    }
}
