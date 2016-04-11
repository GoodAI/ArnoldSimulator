using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Graphics;
using Moq;
using OpenTK;
using Xunit;

namespace GoodAI.Arnold.UI.Tests
{
    public class PickRayTests
    {
        [Fact]
        public void CalculatesPickRay()
        {
            const int x = 100;
            const int y = 200;

            var cameraMock = new Mock<ICamera>();
            cameraMock.Setup(c => c.CurrentFrameViewMatrix).Returns(MathTestHelpers.BuildMatrix(new float[,]
            {
                { 1, 2, 3, 0},
                { 1, 2, 3, 0},
                { 1, 2, 3, 0},
                { 1, 2, 3, 1}
            }));
            cameraMock.Setup(c => c.Position).Returns(new Vector3(10, 20, 30));

            ICamera camera = cameraMock.Object;

            var viewSize = new Size(1000, 600);

            Matrix4 projectionMatrix = MathTestHelpers.BuildMatrix(new float[,]
            {
                { 4, 3, 2, 0},
                { 4, 3, 2, 0},
                { 4, 3, 2, 0},
                { 4, 3, 2, 1}
            });

            PickRay ray = PickRay.Pick(x, y, camera, viewSize, projectionMatrix);

            var expectedDirection = new Vector3(-0.26726f, -0.53452f, -0.80178f);

            Assert.Equal(camera.Position, ray.Position);

            CompareResult result = MathTestHelpers.VectorCompare(expectedDirection, ray.Direction, 1e-05f);

            Assert.True(result.AreEqual, result.DifferenceString);
        }
    }
}
