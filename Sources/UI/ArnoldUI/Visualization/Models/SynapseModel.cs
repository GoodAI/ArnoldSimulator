﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GoodAI.Arnold.Visualization.Models
{
    public abstract class SynapseModelBase : ModelBase
    {
        public const float AlphaReductionPerMs = 1/1000f;
        public const float SpikeAlpha = 0.4f;
        public const float MinAlpha = 0.0f;
        
        protected float Alpha;

        public void Spike()
        {
            Alpha = SpikeAlpha;
        }

        // TODO(HonzaS): Remove this, it's only used in a test.
        // Once there is an interface and this is decoupled from diff applier, use mocks instead .
        internal bool IsSpiked => Math.Abs(Alpha - SpikeAlpha) < 1e-10f;

        protected override void UpdateModel(float elapsedMs)
        {
            if (Alpha > MinAlpha)
                Alpha -= AlphaReductionPerMs*elapsedMs;

            if (Alpha < MinAlpha)
                Alpha = MinAlpha;

            // TODO: threshold when the synapse is barely visible.
            Visible = Alpha > 0;
        }
    }

    public class SynapseModel : SynapseModelBase
    {
        public RegionModel FromRegion { get; }
        public RegionModel ToRegion { get; }

        public Vector3 TargetPosition { get; private set; }

        public SynapseModel(RegionModel fromRegion, NeuronModel fromNeuron, RegionModel toRegion, NeuronModel toNeuron)
        {
            FromRegion = fromRegion;
            ToRegion = toRegion;
            FromNeuron = fromNeuron;
            ToNeuron = toNeuron;

            UpdatePosition();

            Translucent = true;
        }

        public NeuronModel FromNeuron { get; }
        public NeuronModel ToNeuron { get; }

        protected override void RenderModel(float elapsedMs)
        {
            using (Blender.MultiplicativeBlender())
            {
                GL.LineWidth(1f);
                GL.Begin(PrimitiveType.Lines);

                GL.Color4(Color.FromArgb((int)(255 * Alpha), 150, 220, 255));
                GL.Vertex3(Vector3.Zero);
                GL.Color4(Color.FromArgb((int)(255 * Alpha), 255, 255, 255));
                GL.Vertex3(TargetPosition);

                GL.End();

                GL.LineWidth(3f);

                GL.Begin(PrimitiveType.Lines);

                GL.Color4(Color.FromArgb((int)(255 * Alpha/3), 150, 220, 255));
                GL.Vertex3(Vector3.Zero);
                GL.Color4(Color.FromArgb((int)(255 * Alpha/3), 255, 255, 255));
                GL.Vertex3(TargetPosition);

                GL.End();
            }
        }

        public void UpdatePosition()
        {
            Position = FromNeuron.Position;
            TargetPosition = ToNeuron.Position - Position;
        }
    }
}
