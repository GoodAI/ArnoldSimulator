using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Communication;
using OpenTK;

namespace GoodAI.Arnold.Extensions
{
    internal static class MessagesExtensions
    {
        public static Vector3 ToVector3(this Position position)
        {
            return new Vector3(position.X, position.Y, position.Z);
        }

        public static Vector3 Position(this Box3D box)
        {
            return new Vector3(box.X, box.Y, box.Z);
        }

        public static Vector3 Size(this Box3D box)
        {
            return new Vector3(box.SizeX, box.SizeY, box.SizeZ);
        }
    }
}
