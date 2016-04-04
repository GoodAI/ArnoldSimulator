using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GoodAI.Arnold.Graphics
{
    /// <summary>
    /// Models that are pickable by the PickRay must implement this interface.
    /// </summary>
    public interface IPickable
    {
        /// <summary>
        /// Returns the distance of the intersection of the model's bounds with the provided PickRay.
        /// </summary>
        /// <param name="ray">The PickRay for the bounds to be tested against.</param>
        /// <returns></returns>
        float DistanceToRayOrigin(PickRay ray);
    }

    /// <summary>
    /// PickRay represents a ray cast from the mouse into the world.
    /// Note that it is not a model - the position and direction are not in model space but directly in world space.
    /// 
    /// Position is the origin of the ray - the camera's position.
    /// Direction is the vector of the ray.
    /// Length is only used for debugging (set RenderRay to true to see the ray).
    /// </summary>
    public class PickRay
    {
        public bool RenderRay { get; set; }

        public Vector3 Position { get; set; }
        public Vector3 Direction { get; set; }
        public float Length { get; set; } = 1f;

        internal void Render(Camera camera, float elapsedMs)
        {
            if (!RenderRay)
                return;

            GL.PushMatrix();

            // ModelViewMatrix is not applied - the ray's position is calculated directly in world space.

            GL.LineWidth(1f);

            GL.Begin(PrimitiveType.Lines);

            GL.Color4(Color.Red);
            GL.Vertex3(Position);

            GL.Color4(Color.Yellow);
            GL.Vertex3(Position + Direction * Length);

            GL.End();

            GL.PopMatrix();
        }
    }
}
