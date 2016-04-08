using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Graphics;
using OpenTK;
using Xunit;

namespace GoodAI.Arnold.UI.Tests
{
    public class CameraTests
    {
        [Fact]
        public void GetsCorrectViewMatrix()
        {
            var camera = new Camera
            {
                Position = new Vector3(10, 100, 1000),
                Orientation = new Vector3((float) (Math.PI/2), (float) -(Math.PI/3), 0)
            };

            camera.UpdateCurrentFrameMatrix();

            // The values were measured when it worked correctly. No way I'm calculating these.
            Matrix4 expected = MathTestHelpers.BuildMatrix(new float[,]
            {
                {0, 0.866f, -0.5f, 0},
                {0, 0.5f, 0.866f, 0},
                {1, 0, 0, 0},
                {-1000, -58.66f, -81.602f, 1},
            });

            CompareResult result = MathTestHelpers.MatrixCompare(expected, camera.CurrentFrameViewMatrix, epsilon: 1e-3f);

            Assert.True(result.AreEqual, result.DifferenceString);
        }

        [Fact]
        public void MovesCorrectly()
        {
            var camera = new Camera
            {
                Orientation = new Vector3((float) (Math.PI/2), (float) -(Math.PI/3), 0),
                MouseSpeedPerMs = 1f/10
            };

            camera.Move(10, 20, 30, 1);

            camera.UpdateCurrentFrameMatrix();

            // The values were measured when it worked correctly. No way I'm calculating these.
            Matrix4 expected = MathTestHelpers.BuildMatrix(new float[,]
            {
                {0, 0.866f, -0.5f, 0},
                {0, 0.5f, 0.866f, 0},
                {1, 0, 0, 0},
                {-0.0267f, -0.0534f, 0.0801f, 1},
            });

            CompareResult result = MathTestHelpers.MatrixCompare(expected, camera.CurrentFrameViewMatrix, epsilon: 1e-3f);

            Assert.True(result.AreEqual, result.DifferenceString);
        }

        [Fact]
        public void RotatesCorrectly()
        {
            var camera = new Camera
            {
                MouseSpeedPerMs = 1f/5000
            };

            camera.Rotate(10, 20, 1);

            camera.UpdateCurrentFrameMatrix();

            // The values were measured when it worked correctly. No way I'm calculating these.
            Matrix4 expected = MathTestHelpers.BuildMatrix(new float[,]
            {
                {-1, 0, -0.002f, 0},
                {0, 1, -0.004f, 0},
                {0.002f, -0.004f, -1, 0},
                {0, 0, 0, 1},
            });

            CompareResult result = MathTestHelpers.MatrixCompare(expected, camera.CurrentFrameViewMatrix, epsilon: 1e-3f);

            Assert.True(result.AreEqual, result.DifferenceString);
        }
    }
}
