using OpenTK;
using System;
using OpenTK.Input;

namespace GoodAI.Arnold.Graphics
{
    /// <summary>
    /// A basic camera using Euler angles
    /// </summary>
    public class Camera
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Orientation = new Vector3((float)Math.PI, 0f, 0f);
        public float MoveSpeed = 1f;
        public const float MoveSpeedSlowFactor = 4;
        public float MouseSensitivity = 0.01f;

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
            Vector3 lookAt = GetLookAtVector();
            CurrentFrameViewMatrix = Matrix4.LookAt(Position, Position + lookAt, Vector3.UnitY);
        }

        //public Matrix4 GetLookAtMatrix(Vector3 position)
        //{
        //    Vector3 lookAt = GetLookAtVector();

        //    return Matrix4.LookAt(position, position + lookAt, Vector3.UnitY);
        //}

        public Vector3 GetLookAtVector()
        {
            Vector3 lookAt = new Vector3();

            lookAt.X = (float)(Math.Sin(Orientation.X) * Math.Cos(Orientation.Y));
            lookAt.Y = (float)Math.Sin(Orientation.Y);
            lookAt.Z = (float)(Math.Cos(Orientation.X) * Math.Cos(Orientation.Y));

            return lookAt;
        }

        /// <summary>
        /// Moves the camera in local space
        /// </summary>
        /// <param name="x">Distance to move along the screen's x axis</param>
        /// <param name="y">Distance to move along the axis of the camera</param>
        /// <param name="z">Distance to move along the screen's y axis</param>
        public void Move(float x, float y, float z, bool slow=false)
        {
            /** When the camera moves, we don't want it to move relative to the world coordinates 
             * (like the XYZ space its position is in), but instead relative to the camera's view. 
             * Like the view angle, this requires a bit of trigonometry. */

            Vector3 offset = new Vector3();

            float sinX = (float) Math.Sin(Orientation.X);
            float cosX = (float) Math.Cos(Orientation.X);
            float sinY = (float) Math.Sin(Orientation.Y);
            float cosY = (float) Math.Cos(Orientation.Y);

            Vector3 forward = new Vector3(sinX, sinY, cosX);
            // TODO: This doesn't work correctly if you look forward.
            Vector3 up = new Vector3(sinX, cosY, cosX);
            Vector3 right = new Vector3(-cosX, 0, sinX);

            offset += x * right;
            offset += z * forward;
            offset += y * up;
            //offset.Y += y;

            offset.Normalize();

            float speed = MoveSpeed;
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
        public void AddRotation(float x, float y)
        { 
            /** In this case, our rotation is due to mouse input, so it's based on the distances the mouse moved along each axis.*/
            x = x * MouseSensitivity;
            y = y * MouseSensitivity;

            Orientation.X = (Orientation.X + x) % ((float)Math.PI * 2.0f);
            Orientation.Y = Math.Max(Math.Min(Orientation.Y + y, (float)Math.PI / 2.0f - 0.1f), (float)-Math.PI / 2.0f + 0.1f);
        }
    }
}
