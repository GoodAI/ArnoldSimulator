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

        public bool Translucent { get; set; }

        public bool Visible { get; set; } = true;

        // The matrices' calculations are virtual because some objects might want to 
        // do freaky stuff - e.g. sprite banners have camera rotation based on camera.

        public virtual Matrix4 RotationMatrix =>
                Matrix4.CreateRotationX(Rotation.X)*Matrix4.CreateRotationY(Rotation.Y)*
                Matrix4.CreateRotationZ(Rotation.Z);

        public virtual Matrix4 ScaleMatrix => Matrix4.CreateScale(Scale);

        public virtual Matrix4 TranslationMatrix => Matrix4.CreateTranslation(Position);

        public virtual Matrix4 OwnerCurrentFrameMatrix
        {
            get
            {
                if (Owner != null)
                    return Owner.CurrentFrameMatrix;

                return Matrix4.Identity;
            }
        }

        protected virtual Matrix4 ModelViewMatrix
            => ScaleMatrix*RotationMatrix*TranslationMatrix*OwnerCurrentFrameMatrix;

        internal Matrix4 CurrentFrameMatrix { get; private set; }
        internal void UpdateCurrentFrameMatrix() => CurrentFrameMatrix = ModelViewMatrix;

        internal virtual void Update(float elapsedMs)
        {
            UpdateModel(elapsedMs);
        }

        protected abstract void UpdateModel(float elapsedMs);

        internal virtual void Render(Camera camera, float elapsedMs)
        {
            GL.PushMatrix();

            var modelViewMatrix = CurrentFrameMatrix;
            GL.MultMatrix(ref modelViewMatrix);

            RenderModel(camera, elapsedMs);

            GL.PopMatrix();
        }

        protected abstract void RenderModel(Camera camera, float elapsedMs);
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
