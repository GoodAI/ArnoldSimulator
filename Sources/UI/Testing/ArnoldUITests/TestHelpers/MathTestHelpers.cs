using System;
using System.Text;
using OpenTK;

namespace GoodAI.Arnold.UI.Tests
{
    public static class MathTestHelpers
    {
        private const float DefaultEpsilon = 1e-6f;

        public static Matrix4 BuildMatrix(float[,] values)
        {
            return new Matrix4(
                new Vector4(values[0, 0], values[0, 1], values[0, 2], values[0, 3]),
                new Vector4(values[1, 0], values[1, 1], values[1, 2], values[1, 3]),
                new Vector4(values[2, 0], values[2, 1], values[2, 2], values[2, 3]),
                new Vector4(values[3, 0], values[3, 1], values[3, 2], values[3, 3])
                );
        }

        public static CompareResult MatrixCompare(Matrix4 m1, Matrix4 m2, float epsilon = DefaultEpsilon)
        {
            var result = new CompareResult();

            var errorPositions = new StringBuilder();
            var errorValues = new StringBuilder();

            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    if (Math.Abs(m1[x, y] - m2[x, y]) > epsilon)
                    {
                        errorPositions.Append("X");
                        errorValues.AppendLine($"{m1[x, y]} vs {m2[x, y]}");
                    }
                    else
                    {
                        errorPositions.Append(".");
                    }
                }
                errorPositions.AppendLine("");
            }

            errorPositions.AppendLine("Errors, left to right, top to bottom:");

            result.AreEqual = errorValues.Length == 0;
            if (!result.AreEqual)
                result.DifferenceString = errorPositions.ToString() + errorValues.ToString();

            return result;
        }

        public static CompareResult VectorCompare(Vector3 v1, Vector3 v2, float epsilon = DefaultEpsilon)
        {
            var result = new CompareResult();

            var errorPositions = new StringBuilder();
            var errorValues = new StringBuilder();

            for (int x = 0; x < 3; x++)
            {
                if (Math.Abs(v1[x] - v2[x]) > epsilon)
                {
                    errorPositions.Append("X");
                    errorValues.AppendLine($"{v1[x]} vs {v2[x]}");
                }
                else
                {
                    errorPositions.Append(".");
                }
            }

            errorPositions.AppendLine("");
            errorPositions.AppendLine("Errors, left to right");

            result.AreEqual = errorValues.Length == 0;
            if (!result.AreEqual)
                result.DifferenceString = errorPositions.ToString() + errorValues.ToString();

            return result;
        }
    }

    public class CompareResult
    {
        public bool AreEqual;
        public string DifferenceString;
    }
}