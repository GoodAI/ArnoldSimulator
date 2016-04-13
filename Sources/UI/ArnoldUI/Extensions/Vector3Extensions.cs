using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace GoodAI.Arnold.Extensions
{
    public static class Vector3Extensions
    {
        public static float DistanceFrom(this Vector3 vector, Vector3 other)
        {
            return (vector - other).Length;
        }

        public static float DistanceTo(this Vector3 vector, Vector3 other)
        {
            return (other - vector).Length;
        }
    }
}
