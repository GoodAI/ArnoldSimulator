using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GoodAI.Arnold.Graphics
{
    public abstract class ModelBase
    {
        public CompositeModelBase Owner { get; set; }

        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = Vector3.One;

        /// <summary>
        /// Translucent models get rendered in the second pass, when all opaque models have already been done.
        /// </summary>
        public bool Translucent { get; set; }

        public bool Visible { get; set; } = true;

        // The matrices' calculations are virtual because some objects might want to 
        // do freaky stuff - e.g. sprite banners have rotation based on camera.

        protected virtual Matrix4 RotationMatrix =>
            Matrix4.CreateRotationX(Rotation.X)*
            Matrix4.CreateRotationY(Rotation.Y)*
            Matrix4.CreateRotationZ(Rotation.Z);

        protected virtual Matrix4 ScaleMatrix => Matrix4.CreateScale(Scale);

        protected virtual Matrix4 TranslationMatrix => Matrix4.CreateTranslation(Position);

        protected virtual Matrix4 OwnerWorldMatrix => Owner?.CurrentWorldMatrix ?? Matrix4.Identity;

        /// <summary>
        /// Used for calculation of the world matrix.
        /// Models can override this, but should not use it inside Render. For rendering purposes, the
        /// current world matrix is cached in CurrentWorldMatrix.
        /// </summary>
        protected virtual Matrix4 WorldMatrix => ScaleMatrix*RotationMatrix*TranslationMatrix*OwnerWorldMatrix;

        /// <summary>
        /// After all models are Updated, this will hold the world matrix for the current frame.
        /// Use this inside your Render methods if needed.
        /// </summary>
        internal Matrix4 CurrentWorldMatrix { get; private set; }
        internal void UpdateCurrentWorldMatrix() => CurrentWorldMatrix = WorldMatrix;

        internal virtual void Update(float elapsedMs)
        {
            UpdateModel(elapsedMs);
        }

        protected abstract void UpdateModel(float elapsedMs);

        internal virtual void Render(float elapsedMs)
        {
            GL.PushMatrix();

            Matrix4 modelViewMatrix = CurrentWorldMatrix;
            GL.MultMatrix(ref modelViewMatrix);

            RenderModel(elapsedMs);

            GL.PopMatrix();
        }

        protected abstract void RenderModel(float elapsedMs);
    }

    public abstract class CompositeModelBase : ModelBase
    {
        public IList<ModelBase> Children { get; }

        public CompositeModelBase()
        {
            Children = new List<ModelBase>();
        }

        public void AddChild(ModelBase child)
        {
            child.Owner = this;
            Children.Add(child);
        }

        internal override void Update(float elapsedMs)
        {
            base.Update(elapsedMs);

            foreach (var child in Children)
                child.Update(elapsedMs);
        }
    }
}
