using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GoodAI.Net.ConverseSharp
{
    public class ConverseClientTests
    {
        private readonly MemoryStream m_memStream;
        private readonly DummyConnector m_connector;
        private readonly ConverseClient m_client;

        public ConverseClientTests()
        {
            m_memStream = new MemoryStream();
            m_connector = new DummyConnector(m_memStream);
            m_client = new ConverseClient(m_connector);
        }

        [Fact]
        public void SendMessageClosesConnection()
        {
            var connector = new DummyConnector(new MemoryStream());
            var client = new ConverseClient(connector);

            client.SendMessage("foo", Array.Empty<byte>());

            Assert.False(connector.IsConnected);
        }

        [Fact]
        public void ReadsQueryReply()
        {
            var message = new byte[] { 1, 2, 3 };

            m_connector.ImplantMessage = (stream =>
            {
                // Write the reply after the message (with zero length body).
                stream.Position = ConverseClient.MessageHeaderLength;  

                var writer = new ConverseWriter();
                writer.WriteReply(stream, message);

                stream.Position = 0;
            });

            var replyMemStream = new MemoryStream();

            m_client.SendQuery("foo", Array.Empty<byte>(), replyMemStream);

            Assert.ArraySegmentEqual(message, replyMemStream.GetBuffer());
            Assert.Equal(0, replyMemStream.Position);  // Check that the reply memory stream is rewound to the start.
        }
    }
}
