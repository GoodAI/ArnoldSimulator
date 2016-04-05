using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Graphics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace GoodAI.Arnold.Graphics.Models
{
    public class ExpertModel : ModelBase, IPickable
    {
        private static readonly Random m_random = new Random();

        public static int NeuronTexture;

        public const float MinAlpha = 0.4f;
        public const float SpikeAlpha = 1f;
        public const float AlphaReductionPerMs = 1/1000f;

        public const float SpikesPerMs = 0.1f;
        public const float CellSize = 2f;
        public const float SpriteSize = 1f;

        private float m_alpha = MinAlpha;

        public Camera Camera { get; set; }

        public RegionModel RegionModel { get; }

        public List<SynapseModel> Outputs { get; } = new List<SynapseModel>();

        public ExpertModel(RegionModel regionModel, Vector3 position)
        {
            RegionModel = regionModel;
            Position = position;

            Translucent = true;
        }

        // The experts are indexed from 0 (not centered within the region).
        // Therefore we need to translate them to the region's corner of origin.
        protected override Matrix4 TranslationMatrix
        {
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

        // Experts are rendered as billboards - they turn towards the camera.
        // Their world space rotation is equal to the camera's inverse rotation.
        protected override Matrix4 RotationMatrix
            => Camera.CurrentFrameViewMatrix.ClearScale().ClearTranslation().Inverted();

        public bool Picked { get; set; }

        protected override void UpdateModel(float elapsedMs)
        {
            if (m_alpha > MinAlpha)
                m_alpha -= AlphaReductionPerMs*elapsedMs;

            if (m_alpha < MinAlpha)
                m_alpha = MinAlpha;

            if (m_random.NextDouble() < SpikesPerMs*elapsedMs/1000f)
                Spike();
        }

        private void Spike()
        {
            m_alpha = SpikeAlpha;

            foreach (var synapse in Outputs)
                synapse.Spike();
        }

        protected override void RenderModel(float elapsedMs)
        {
            GL.BindTexture(TextureTarget.Texture2D, NeuronTexture);

            const float halfSize = SpriteSize/2;

            Color4 color = new Color4(255, 255, 255, (byte) (255 * m_alpha));

            GL.Enable(EnableCap.Texture2D);

            using (Blender.AveragingBlender())
            {
                GL.Color4(color);

                GL.Begin(PrimitiveType.Quads);
                GL.TexCoord2(0, 0);
                GL.Vertex2(-halfSize, -halfSize);
                GL.TexCoord2(1, 0);
                GL.Vertex2(halfSize, -halfSize);
                GL.TexCoord2(1, 1);
                GL.Vertex2(halfSize, halfSize);
                GL.TexCoord2(0, 1);
                GL.Vertex2(-halfSize, halfSize);
                GL.End();
            }

            GL.Disable(EnableCap.Texture2D);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public float DistanceToRayOrigin(PickRay pickRay)
        {
            var r = SpriteSize/2;

            Vector3 m = pickRay.Position - CurrentWorldMatrix.ExtractTranslation();
            float b = Vector3.Dot(m, pickRay.Direction);
            float r2 = r*r;
            float c = Vector3.Dot(m, m) - r2;

            // Ray starting outside of the sphere and pointing away.
            if (c > 0 && b > 0)
                return float.MaxValue;

            float discriminant = b * b - c;

            // Ray missing the sphere.
            if (discriminant < 0)
                return float.MaxValue;

            float t = -b - (float)Math.Sqrt(discriminant);

            // Ray starting inside sphere.
            if (t < 0)
                t = 0;

            return t;
        }
    }
}
