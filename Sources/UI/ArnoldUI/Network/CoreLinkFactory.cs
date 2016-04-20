using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Network;
using GoodAI.Net.ConverseSharp;
using GoodAI.Net.ConverseSharpFlatBuffers;

namespace GoodAI.Arnold.Network
{
    public interface ICoreLinkFactory
    {
        ICoreLink Create(EndPoint endPoint, IResponseParser responseParser);
    }

    // TODO(HonzaS): This class still does some composition.
    public class CoreLinkFactory : ICoreLinkFactory
    {
        private const int TcpTimeoutMs = 5000;

        public ICoreLink Create(EndPoint endPoint, IResponseParser responseParser)
        {
            var connector = new TcpConnector(endPoint.Hostname, endPoint.Port, TcpTimeoutMs);
            return Create(connector, responseParser);
        }

        private ICoreLink Create(ITcpConnector connector, IResponseParser responseParser)
        {
            return new CoreLink(new ConverseFlatBuffersClient(connector, responseParser));
        }
    }
}
