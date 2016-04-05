using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GoodAI.Arnold.Graphics
{
    public abstract class ModelBase
    {
        public ModelBase Owner { get; set; }

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

        /// <summary>
        /// Renders the model relative to model space.
        /// </summary>
        /// <param name="elapsedMs">Milliseconds elapsed since the last frame. Useful for animations.</param>
        protected abstract void RenderModel(float elapsedMs);
    }

    public interface ICompositeModel
    {
        IEnumerable<ModelBase> Models { get; }
    }

    public abstract class CompositeModelBase<T> : ModelBase, ICompositeModel, IEnumerable<T> where T : ModelBase
    {
        public IEnumerable<ModelBase> Models => m_children;

        private readonly IList<T> m_children = new List<T>();

        public void AddChild(T child)
        {
            child.Owner = this;
            m_children.Add(child);
        }

        public void Clear()
        {
            m_children.Clear();
        }

        internal override void Update(float elapsedMs)
        {
            base.Update(elapsedMs);

            foreach (var child in m_children)
                child.Update(elapsedMs);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class CompositeModel<T> : CompositeModelBase<T> where T : ModelBase
    {
        protected override void UpdateModel(float elapsedMs)
        {
        }

        protected override void RenderModel(float elapsedMs)
        {
        }
    }
}
