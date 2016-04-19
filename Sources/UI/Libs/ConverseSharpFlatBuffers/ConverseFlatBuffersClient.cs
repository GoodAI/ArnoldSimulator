using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Net.ConverseSharp;

namespace GoodAI.Net.ConverseSharpFlatBuffers
{
    public interface IConverseFlatBuffersClient
    {
    }

    public class ConverseFlatBuffersClient : ConverseClient, IConverseFlatBuffersClient
    {
        public ConverseFlatBuffersClient(ITcpConnector connector) : base(connector)
        { }
    }
}
