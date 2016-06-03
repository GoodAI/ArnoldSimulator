using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Communication;
using GoodAI.Net.ConverseSharpFlatBuffers;
using Xunit;

namespace GoodAI.Arnold.UI.Tests
{
    public class CoreResponseParserTests
    {
        [Fact]
        public void ParsesResponseMessage()
        {
            ResponseMessage responseMessage = StateResponseBuilder.Build(StateType.Running);

            var parser = new CoreResponseParser();

            byte[] buffer = BufferConverter.Convert(responseMessage.ByteBuffer);

            var response = parser.Parse<ResponseMessage>(buffer);

            Assert.Equal(Response.StateResponse, response.ResponseType);

            StateType state = response.GetResponse(new StateResponse()).State;

            Assert.Equal(StateType.Running, state);
        }
    }
}
