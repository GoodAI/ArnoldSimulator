using OpenTK;
using System;

namespace GoodAI.Arnold.Graphics
{
    /// <summary>
    /// A basic camera using Euler angles
    /// </summary>
    public class Camera
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Orientation = Vector3.Zero;

        public float MoveSpeedPerMs = 1f/10;
        public float MoveSpeedSlowFactor = 4;
        public float MouseSpeedPerMs = 1f/5000;

        public Matrix4 CurrentFrameViewMatrix { get; private set; }
        
        /// <summary>
        /// Calculate a view matrix for this camera
        /// </summary>
        /// <returns>A view matrix from this camera</returns>
        public void UpdateCurrentFrameMatrix()
        {
            /**This code uses some trigonometry to create a vector in the direction that the camera is looking,
             * and then uses the LookAt static function of the Matrix4 class to use that vector and
             * the position to create a view matrix we can use to change where our scene is viewed from. 
             * The Vector3.UnitY is being assigned to the "up" parameter,
             * which will keep our camera angle so that the right side is up.*/
            CurrentFrameViewMatrix = Matrix4.LookAt(Position, Position + LookAtVector, Vector3.UnitY);
        }

        //public Matrix4 GetLookAtMatrix(Vector3 position)
        //{
        //    Vector3 lookAt = GetLookAtVector();

        //    return Matrix4.LookAt(position, position + lookAt, Vector3.UnitY);
        //}

        private Vector3 LookAtVector => new Vector3
        {
            X = (float) (Math.Cos(Orientation.Y)*Math.Sin(Orientation.X)),
            Y = (float) Math.Sin(Orientation.Y),
            Z = (float) (Math.Cos(Orientation.Y)*Math.Cos(Orientation.X))
        };

        private Vector3 RightVector => new Vector3
        {
            X = (float) Math.Sin(Orientation.X - Math.PI/2.0f),
            Y = 0,
            Z = (float) Math.Cos(Orientation.X - Math.PI/2.0f)
        };

        /// <summary>
        /// Moves the camera in local space
        /// </summary>
        /// <param name="x">Distance to move along the right direction of the camera</param>
        /// <param name="y">Distance to move along the forward direction of the camera</param>
        /// <param name="z">Distance to move along the up direction of the camera</param>
        /// <param name="elapsedMs">Milliseconds elapsed since last frame</param>
        /// <param name="slow">If true, move slower</param>
        public void Move(float x, float y, float z, float elapsedMs, bool slow=false)
        {
            var offset = new Vector3();

            Vector3 forward = LookAtVector;
            Vector3 right = RightVector;
            Vector3 up = Vector3.Cross(right, forward);

            offset += x * right;
            offset += z * forward;
            offset += y * up;

            if (offset == Vector3.Zero)
                return;

            offset.Normalize();

            float speed = MoveSpeedPerMs*elapsedMs;
            if (slow)
                speed /= MoveSpeedSlowFactor;

            Vector3 adjustedOffset;
            Vector3.Multiply(ref offset, speed, out adjustedOffset);

            Position += adjustedOffset;
        }

        /// <summary>
        /// Changes the rotation of the camera based on mouse input
        /// </summary>
        /// <param name="x">The x distance the mouse moved</param>
        /// <param name="y">The y distance the mouse moved</param>
        /// <param name="elapsedMs">Milliseconds elapsed since last frame</param>
        public void Rotate(float x, float y, float elapsedMs)
        { 
            /** In this case, our rotation is due to mouse input, so it's based on the distances the mouse moved along each axis.*/
            float speed = MouseSpeedPerMs*elapsedMs;
            x = x*speed;
            y = y*speed;

            Orientation.X = (Orientation.X + x) % ((float)Math.PI * 2.0f);
            Orientation.Y = Math.Max(Math.Min(Orientation.Y + y, (float)Math.PI / 2.0f - 0.001f), (float)-Math.PI / 2.0f + 0.001f);
        }
    }
}
