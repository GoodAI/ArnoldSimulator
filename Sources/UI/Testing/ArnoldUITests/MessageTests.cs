using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Net;
using Google.Protobuf;
using Xunit;

namespace GoodAI.Arnold.UI.Tests
{
    public class MessageTests
    {
        private static void AssertWriteReadEquality<T>(T message)
            where T : IMessage<T>, new()
        {
            var stream = new MemoryStream();

            message.WriteTo(stream);
            stream.Position = 0;

            var parser = new MessageParser<T>(() => new T());
            T receivedCommand = parser.ParseFrom(stream);

            Assert.Equal(message, receivedCommand);
        }

        [Fact]
        public void WritesReadsCommand()
        {
            var message = new CommandRequest
            {
                Command = CommandRequest.Types.CommandType.Start
            };
            AssertWriteReadEquality(message);
        }

        [Fact]
        public void WritesReadsGetState()
        {
            var message = new GetStateRequest();
            AssertWriteReadEquality(message);
        }

        [Fact]
        public void WritesReadsStateResponseError()
        {
            var message = new StateResponse
            {
                Error = new Error {ErrorMessage = "Foo bar"}
            };
            Assert.Equal(StateResponse.ResponseOneofOneofCase.Error, message.ResponseOneofCase);
            AssertWriteReadEquality(message);
        }

        [Fact]
        public void WritesReadsStateResponseData()
        {
            var message = new StateResponse
            {
                Data = new StateData {State = StateData.Types.StateType.Running}
            };
            Assert.Equal(StateResponse.ResponseOneofOneofCase.Data, message.ResponseOneofCase);
            AssertWriteReadEquality(message);
        }
    }
}
