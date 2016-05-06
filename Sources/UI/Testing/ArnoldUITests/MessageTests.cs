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

            Assert.Equal(CommandType.Run, message.GetRequest(new CommandRequest()).Command);
        }

        [Fact]
        public void WritesReadsGetState()
        {
            var message = GetStateRequestBuilder.Build();

            Assert.Equal(Request.GetStateRequest, message.RequestType);
        }

        [Fact]
        public void WritesReadsGetModel()
        {
            var message = GetModelRequestBuilder.Build();

            Assert.Equal(Request.GetModelRequest, message.RequestType);
        }

        [Fact]
        public void WritesReadsStateResponse()
        {
            var message = StateResponseBuilder.Build(StateType.ShuttingDown);

            Assert.Equal(StateType.ShuttingDown, message.GetResponse(new StateResponse()).State);
        }

        [Fact]
        public void WritesReadsStateResponseError()
        {
            var message = ErrorResponseBuilder.Build("foo");

            Assert.Equal("foo", message.GetResponse(new ErrorResponse()).Message);
        }
    }
}
