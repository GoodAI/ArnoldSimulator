using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GoodAI.Arnold.Graphics.Models
{
    public class SynapseModel : ModelBase
    {
        public const float AlphaReductionPerMs = 1/1000f;
        public const float SpikeAlpha = 0.4f;
        public const float MinAlpha = 0.0f;

        private static readonly Random Random = new Random();

        public RegionModel RegionModel { get; }
        public Vector3 Target { get; }

        private float m_alpha;

        public SynapseModel(RegionModel regionModel, ExpertModel from, ExpertModel to)
        {
            RegionModel = regionModel;
            From = from;
            To = to;
            Position = from.Position;
            Target = to.Position;

            Translucent = true;
        }

        public ExpertModel From { get; private set; }
        public ExpertModel To { get; private set; }

        public void Spike()
        {
            m_alpha = SpikeAlpha;
        }

        public override Matrix4 TranslationMatrix
        {
            // The experts are indexed from 0 now (not centered within the region).
            // Therefore we need to translate them to the region's corner of origin.
            get
            {
                var baseMatrix = base.TranslationMatrix;
                return baseMatrix*
                       Matrix4.CreateTranslation(
                           -RegionModel.HalfSize.X + RegionModel.RegionMargin,
                           -RegionModel.HalfSize.Y + RegionModel.RegionMargin,
                           -RegionModel.HalfSize.Z + RegionModel.RegionMargin);
            }
        }

        protected override void UpdateModel(float elapsedMs)
        {
            if (m_alpha > MinAlpha)
                m_alpha -= AlphaReductionPerMs*elapsedMs;

            if (m_alpha < MinAlpha)
                m_alpha = MinAlpha;

            // TODO: threshold when the synapse is barely visible.
            Visible = m_alpha > 0;
        }

        protected override void RenderModel(Camera camera, float elapsedMs)
        {
            using (Blender.MultiplicativeBlender())
            {
                GL.LineWidth(1f);
                GL.Begin(PrimitiveType.Lines);

                GL.Color4(Color.FromArgb((int)(255 * m_alpha), 150, 220, 255));
                GL.Vertex3(Vector3.Zero);
                GL.Color4(Color.FromArgb((int)(255 * m_alpha), 255, 255, 255));
                GL.Vertex3(Target - Position);

                GL.End();

                GL.LineWidth(3f);

                GL.Begin(PrimitiveType.Lines);

                GL.Color4(Color.FromArgb((int)(255 * m_alpha/3), 150, 220, 255));
                GL.Vertex3(Vector3.Zero);
                GL.Color4(Color.FromArgb((int)(255 * m_alpha/3), 255, 255, 255));
                GL.Vertex3(Target - Position);

                GL.End();
            }
        }
    }
}
