using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatBuffers;
using GoodAI.Net.ConverseSharp;
using GoodAI.Net.ConverseSharpFlatBuffers;
using Xunit;

namespace ConverseSharpFlatBuffersTests
{
    public class ConverseFlatBuffersClientTests
    {
        private readonly Command m_command;
        private readonly MemoryStream m_stream = new MemoryStream();
        private readonly IConverseFlatBuffersClient m_flatBuffersClient;
        private readonly IResponseParser m_responseParser;

        public ConverseFlatBuffersClientTests()
        {
            var builder = new FlatBufferBuilder(1);

            StringOffset methodOffset = builder.CreateString("Go!");

            Command.StartCommand(builder);
            Command.AddCode(builder, 5);
            Command.AddMethod(builder, methodOffset);

            Offset<Command> commandOffset = Command.EndCommand(builder);
            builder.Finish(commandOffset.Value);
            m_command = Command.GetRootAsCommand(builder.DataBuffer);

            m_responseParser = new ResponseParser();
            m_flatBuffersClient = new ConverseFlatBuffersClient(new DummyConnector(m_stream), m_responseParser);
        }

        [Fact]
        public void SentObjectCanBeDeserialized()
        {
            m_flatBuffersClient.SendMessage("command", m_command);

            var buffer = new byte[m_stream.Position - ConverseClient.MessageHeaderLength];

            m_stream.Position = ConverseClient.MessageHeaderLength;
            m_stream.Read(buffer, 0, buffer.Length);

            var receivedCommand = m_responseParser.Parse<Command>(buffer);

            Assert.Equal(m_command.Code, receivedCommand.Code);
            Assert.Equal(m_command.Method, receivedCommand.Method);
        }
    }

    class ResponseParser : IResponseParser
    {
        public T Parse<T>(byte[] buffer) where T : class
        {
            return Command.GetRootAsCommand(new ByteBuffer(buffer)) as T;
        }
    }
}
