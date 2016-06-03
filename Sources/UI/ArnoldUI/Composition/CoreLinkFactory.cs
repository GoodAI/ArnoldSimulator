using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Communication;
using GoodAI.Net.ConverseSharp;
using GoodAI.Net.ConverseSharpFlatBuffers;
using SimpleInjector;

namespace GoodAI.Arnold
{
    public interface ICoreLinkFactory
    {
        ICoreLink Create(EndPoint endPoint);
    }

    public class CoreLinkFactory : PropertyInjectingFactory, ICoreLinkFactory
    {
        private readonly IResponseParser m_responseParser;
        private const int TcpTimeoutMs = 5000;

        public CoreLinkFactory(Container container, IResponseParser responseParser) : base(container)
        {
            m_responseParser = responseParser;
        }

        public ICoreLink Create(EndPoint endPoint)
        {
            var connector = new TcpConnector(endPoint.Hostname, endPoint.Port, TcpTimeoutMs);
            return InjectProperties(new CoreLink(new ConverseFlatBuffersClient(connector, m_responseParser)));
        }
    }
}
