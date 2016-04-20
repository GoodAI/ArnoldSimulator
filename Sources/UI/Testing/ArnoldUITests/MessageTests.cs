using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatBuffers;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Network.Messages;
using Xunit;

namespace GoodAI.Arnold.UI.Tests
{
    public class MessageTests
    {
        [Fact]
        public void WritesReadsCommand()
        {
            var message = CommandRequestBuilder.Build(CommandType.Run);
            var receivedMessage = RequestMessage.GetRootAsRequestMessage(message.ByteBuffer);

            Assert.Equal(message, receivedMessage);
        }

        [Fact]
        public void WritesReadsGetState()
        {
            var message = GetStateRequestBuilder.Build();
            var receivedMessage = RequestMessage.GetRootAsRequestMessage(message.ByteBuffer);

            Assert.Equal(message, receivedMessage);
        }

        [Fact]
        public void WritesReadsStateResponseError()
        {
            var message = ErrorResponseBuilder.Build("foo");
            var receivedMessage = ResponseMessage.GetRootAsResponseMessage(message.ByteBuffer);

            Assert.Equal(message, receivedMessage);
        }
    }
}
