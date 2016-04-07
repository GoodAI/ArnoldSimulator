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
            // The values were measured when it worked correctly - there's no apriori knowledge :-)
            var camera = new Camera
            {
                Position = new Vector3(10, 100, 1000),
                Orientation = new Vector3((float) (Math.PI/2), (float) -(Math.PI/3), 0)
            };

            camera.UpdateCurrentFrameMatrix();

            Matrix4 expected = MatrixTestHelpers.BuildMatrix(new float[,]
            {
                {0, 0.866f, -0.5f, 0},
                {0, 0.5f, 0.866f, 0},
                {1, 0, 0, 0},
                {-1000, -58.66f, -81.602f, 1},
            });

            CompareResult result = MatrixTestHelpers.MatrixCompare(expected, camera.CurrentFrameViewMatrix, epsilon: 1e-3f);

            Assert.True(result.AreEqual, result.DifferenceString);
        }
    }
}
