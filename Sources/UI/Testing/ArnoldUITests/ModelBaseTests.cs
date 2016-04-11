using System;
using GoodAI.Arnold.Graphics;
using Moq;
using OpenTK;
using Xunit;

namespace GoodAI.Arnold.UI.Tests
{
    public class ModelBaseTests
    {
        class TestModel : ModelBase
        {
            protected override void UpdateModel(float elapsedMs)
            {
            }

            protected override void RenderModel(float elapsedMs)
            {
            }
        }

        private static void AssertCurrentMatrixEqual(TestModel model, Matrix4 expected)
        {
            CompareResult result = MathTestHelpers.MatrixCompare(expected, model.CurrentWorldMatrix);

            Assert.True(result.AreEqual, result.DifferenceString);
        }

        [Fact]
        public void TestRotationMatrix()
        {
            var model = new TestModel
            {
                Rotation = new Vector3((float) Math.PI, (float) (2*Math.PI), (float) (3*Math.PI))
            };
            model.UpdateCurrentWorldMatrix();

            Matrix4 expected = MathTestHelpers.BuildMatrix(new float[,]
            {
                {-1, 0, 0, 0},
                {0, 1, 0, 0},
                {0, 0, -1, 0},
                {0, 0, 0, 1},
            });

            AssertCurrentMatrixEqual(model, expected);
        }

        [Fact]
        public void TestTranslationMatrix()
        {
            var model = new TestModel
            {
                Position = new Vector3(2, 3, 4)
            };
            model.UpdateCurrentWorldMatrix();

            Matrix4 expected = MathTestHelpers.BuildMatrix(new float[,]
            {
                {1, 0, 0, 0},
                {0, 1, 0, 0},
                {0, 0, 1, 0},
                {2, 3, 4, 1},
            });

            AssertCurrentMatrixEqual(model, expected);
        }

        [Fact]
        public void TestScaleMatrix()
        {
            var model = new TestModel
            {
                Scale = new Vector3(2, 3, 4)
            };
            model.UpdateCurrentWorldMatrix();

            Matrix4 expected = MathTestHelpers.BuildMatrix(new float[,]
            {
                {2, 0, 0, 0},
                {0, 3, 0, 0},
                {0, 0, 4, 0},
                {0, 0, 0, 1},
            });

            AssertCurrentMatrixEqual(model, expected);
        }

        [Fact]
        public void TestCombinedMatrix()
        {
            var model = new TestModel
            {
                Rotation = new Vector3((float) Math.PI, (float) (2*Math.PI), (float) (3*Math.PI)),
                Position = new Vector3(2, 3, 4),
                Scale = new Vector3(2, 3, 4)
            };
            model.UpdateCurrentWorldMatrix();

            Matrix4 expected = MathTestHelpers.BuildMatrix(new float[,]
            {
                {-2, 0, 0, 0},
                {0, 3, 0, 0},
                {0, 0, -4, 0},
                {2, 3, 4, 1},
            });

            AssertCurrentMatrixEqual(model, expected);
        }

        [Fact]
        public void TestExistingOwnerMatrix()
        {
            var ownerModel = new TestModel
            {
                Position = new Vector3(2, 3, 4)
            };
            var model = new TestModel
            {
                Owner = ownerModel,
                Scale = new Vector3(4, 3, 2)
            };


            // The order is important, the matrix is cached and needs to get propagated.
            ownerModel.UpdateCurrentWorldMatrix();
            model.UpdateCurrentWorldMatrix();

            Matrix4 expected = MathTestHelpers.BuildMatrix(new float[,]
            {
                {4, 0, 0, 0},
                {0, 3, 0, 0},
                {0, 0, 2, 0},
                {2, 3, 4, 1},
            });

            AssertCurrentMatrixEqual(model, expected);
        }

        [Fact]
        public void UpdateRefreshesWorldMatrix()
        {
            var model = new TestModel
            {
                Position = new Vector3(2, 3, 4)
            };
            model.Update(1);

            Matrix4 expected = MathTestHelpers.BuildMatrix(new float[,]
            {
                {1, 0, 0, 0},
                {0, 1, 0, 0},
                {0, 0, 1, 0},
                {2, 3, 4, 1},
            });

            AssertCurrentMatrixEqual(model, expected);
        }

        [Fact]
        public void CompositeModelUpdatesChildren()
        {
            var modelMock = new Mock<IModel>();
            IModel model = modelMock.Object;

            var compositeModel = new CompositeModel<IModel>();
            compositeModel.AddChild(model);

            compositeModel.Update(0);

            modelMock.Verify(m => m.Update(0));
        }
    }
}