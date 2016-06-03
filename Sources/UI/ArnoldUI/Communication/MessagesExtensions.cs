using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace GoodAI.Arnold.Communication
{
    internal static class MessagesExtensions
    {
        public static Vector3 ToVector3(this Position position)
        {
            return new Vector3(position.X, position.Y, position.Z);
        }
    }
}
