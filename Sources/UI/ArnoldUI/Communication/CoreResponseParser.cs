using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatBuffers;
using GoodAI.Net.ConverseSharpFlatBuffers;

namespace GoodAI.Arnold.Communication
{
    public class CoreResponseParser : IResponseParser
    {
        public T Parse<T>(byte[] buffer) where T : class
        {
            if (typeof(T) == typeof(ResponseMessage))
            {
                return ResponseMessage.GetRootAsResponseMessage(new ByteBuffer(buffer)) as T;
            }

            throw new InvalidOperationException($"Unknown return type: {typeof(T).Name}");
        }
    }
}
