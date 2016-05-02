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
        ICoreLink Create(EndPoint endPoint);
    }

    // TODO(HonzaS): This class still does some composition.
    public class CoreLinkFactory : ICoreLinkFactory
    {
        private readonly IResponseParser m_responseParser;
        private const int TcpTimeoutMs = 5000;

        public CoreLinkFactory(IResponseParser responseParser)
        {
            m_responseParser = responseParser;
        }

        public ICoreLink Create(EndPoint endPoint)
        {
            var connector = new TcpConnector(endPoint.Hostname, endPoint.Port, TcpTimeoutMs);
            return new CoreLink(new ConverseFlatBuffersClient(connector, m_responseParser));
        }
    }
}
