using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using Xunit;

namespace GoodAI.Net.ConverseSharp
{
    [ProtoContract]
    internal struct Command
    {
        [ProtoMember(1)]
        public int Code;
        [ProtoMember(2)]
        public string Method;
    }

    public class ConverseProtoBufClientTests
    {
        private readonly Command m_command;
        private readonly MemoryStream m_stream = new MemoryStream();
        private readonly ConverseProtoBufClient m_protoBufClient;

        public ConverseProtoBufClientTests()
        {
            m_command = new Command {Code = 5, Method = "Go!"};

            m_protoBufClient = new ConverseProtoBufClient(new DummyConnector(m_stream));
        }

        [Fact]
        public void SentObjectCanBeDeserialized()
        {
            m_protoBufClient.SendMessage("command", m_command);

            m_stream.Position = ConverseClient.MessageHeaderLength;
            var receivedCommand = Serializer.DeserializeWithLengthPrefix<Command>(m_stream,
                ConverseProtoBufClient.ProtoBufPrefixStyle);

            Assert.Equal(m_command, receivedCommand);
        }

        [Fact]
        public void MessageHasReasonableSize()
        {
            m_protoBufClient.SendMessage("command", m_command);

            const int maxMessageLength = ConverseClient.MessageHeaderLength + 40;
            Assert.True(m_stream.Length < maxMessageLength, $"Received message suspiciously long: {m_stream.Length} >= {maxMessageLength}");
        }
    }
}
