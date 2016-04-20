using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Net.ConverseSharpFlatBuffers;

namespace GoodAI.Arnold.Network
{
    public class CoreResponseParser : IResponseParser
    {
        public T Parse<T>(byte[] buffer) where T : class
        {
            throw new NotImplementedException();
        }
    }
}
