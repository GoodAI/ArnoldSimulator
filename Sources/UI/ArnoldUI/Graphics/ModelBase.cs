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
    public interface IModel
    {
        /// <summary>
        /// The owner of this model. If it's null, the world is the owner.
        /// </summary>
        IModel Owner { get; set; }

        /// <summary>
        /// Use these to orient the model inside it's owner's space.
        /// </summary>
        Vector3 Position { get; set; }

        Vector3 Rotation { get; set; }
        Vector3 Scale { get; set; }

        /// <summary>
        /// Translucent models get rendered in the second pass, when all opaque models have already been done.
        /// </summary>
        bool Translucent { get; set; }

        /// <summary>
        /// If a model's not visible, it's not rendered at all. This serves as an important optimization.
        /// A good idea is to set this to false if the model is translucent and it's visibility is (close to) zero.
        /// </summary>
        bool Visible { get; set; }

        /// <summary>
        /// After all models are Updated, this will hold the world matrix for the current frame.
        /// Use this inside your Render methods if needed.
        /// </summary>
        Matrix4 CurrentWorldMatrix { get; }

        /// <summary>
        /// Saves the current world matrix into the cache.
        /// </summary>
        void UpdateCurrentWorldMatrix();

        /// <summary>
        /// Updates the model. An example of an override is in CompositeModelBase.
        /// </summary>
        /// <param name="elapsedMs"></param>
        void Update(float elapsedMs);

        /// <summary>
        /// This is used for basic rendering logic. It applies the modelView matrix so that RenderModel can
        /// operate in model space.
        /// 
        /// Override this if you need to do some additional operations with the modelView transformation.
        /// </summary>
        /// <param name="elapsedMs"></param>
        void Render(float elapsedMs);
    }

    public abstract class ModelBase : IModel
    {
        public IModel Owner { get; set; }

        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = Vector3.One;

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

        /// <summary>
        /// Provides the world matrix of the owner for hierarchical models.
        /// </summary>
        protected virtual Matrix4 OwnerWorldMatrix => Owner?.CurrentWorldMatrix ?? Matrix4.Identity;

        /// <summary>
        /// Used for calculation of the world matrix.
        /// Models can override this, but should not use it inside Render. For rendering purposes, the
        /// current world matrix is cached in CurrentWorldMatrix.
        /// </summary>
        protected virtual Matrix4 WorldMatrix => ScaleMatrix*RotationMatrix*TranslationMatrix*OwnerWorldMatrix;

        public Matrix4 CurrentWorldMatrix { get; private set; }
        public void UpdateCurrentWorldMatrix() => CurrentWorldMatrix = WorldMatrix;

        public virtual void Update(float elapsedMs)
        {
            UpdateCurrentWorldMatrix();
            UpdateModel(elapsedMs);
        }

        protected abstract void UpdateModel(float elapsedMs);

        public virtual void Render(float elapsedMs)
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

    /// <summary>
    /// All composite models must implement this, or they will not be collected for rendering.
    /// </summary>
    public interface ICompositeModel : IModel
    {
        IEnumerable<IModel> Models { get; }
    }

    /// <summary>
    /// A base for a typical composite model. Subclass this in composite models that have
    /// their own update or render logic.
    /// </summary>
    /// <typeparam name="T">Type of the contained models.</typeparam>
    public abstract class CompositeModelBase<T> : ModelBase, ICompositeModel, IEnumerable<T> where T : IModel
    {
        public IEnumerable<IModel> Models => m_children as IEnumerable<IModel>;

        private readonly IList<T> m_children = new List<T>();

        public void AddChild(T child)
        {
            child.Owner = this;
            m_children.Add(child);
        }

        public void Clear() => m_children.Clear();

        public override void Update(float elapsedMs)
        {
            base.Update(elapsedMs);

            foreach (var child in m_children)
                child.Update(elapsedMs);
        }

        public IEnumerator<T> GetEnumerator() => m_children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// A composite model without any update or render logic. Use this for hierarchical composition.
    /// </summary>
    /// <typeparam name="T">Type of the contained models.</typeparam>
    public class CompositeModel<T> : CompositeModelBase<T> where T : IModel
    {
        protected override void UpdateModel(float elapsedMs)
        {
        }

        protected override void RenderModel(float elapsedMs)
        {
        }
    }
}
