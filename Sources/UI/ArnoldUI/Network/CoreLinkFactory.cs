using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Network;
using GoodAI.Net.ConverseSharp;

namespace ArnoldUI.Network
{
    public interface ICoreLinkFactory
    {
        ICoreLink Create(string hostname, int port);
    }

    // TODO(HonzaS): This class still does some composition.
    public class CoreLinkFactory : ICoreLinkFactory
    {
        private const int TcpTimeoutMs = 5000;

        public ICoreLink Create(string hostname, int port)
        {
            var connector = new TcpConnector(hostname, port, TcpTimeoutMs);
            return Create(connector);
        }

        private ICoreLink Create(ITcpConnector connector)
        {
            return new CoreLink(new ConverseProtoBufClient(connector));
        }
    }
}
