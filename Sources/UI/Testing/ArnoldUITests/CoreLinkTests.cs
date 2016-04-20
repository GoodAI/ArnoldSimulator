using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoodAI.Arnold.Extensions;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Network.Messages;
using GoodAI.Net.ConverseSharpFlatBuffers;
using Moq;
using Xunit;

namespace GoodAI.Arnold.UI.Tests
{
    public class CoreLinkTests
    {
        const int WaitMs = 50;

        [Fact]
        public void GetsAsyncConversationResult()
        {
            var conversation = new CommandConversation(CommandType.Run);

            var responseMessage = StateResponseBuilder.Build(StateType.Running);

            ICoreLink coreLink = GenerateCoreLink(conversation, responseMessage);

            var futureResponse = coreLink.Request(conversation);

            Response<StateResponse> receivedResponse = ReadResponse(futureResponse);
            Assert.NotNull(receivedResponse.Data);
            Assert.Equal(StateType.Running, receivedResponse.Data.State);
        }

        private static Response<StateResponse> ReadResponse(Task<TimeoutResult<Response<StateResponse>>> futureResponse)
        {
            TimeoutResult<Response<StateResponse>> timeoutResult = futureResponse.Result;
            Assert.False(timeoutResult.TimedOut);
            Response<StateResponse> receivedResponse = timeoutResult.Result;
            Assert.NotNull(receivedResponse);
            return receivedResponse;
        }

        [Fact]
        public void GetsAsyncConversationError()
        {
            const string errorMessage = "Foo bar";

            var conv = new CommandConversation(CommandType.Run);

            var responseMessage = ErrorResponseBuilder.Build(errorMessage);

            ICoreLink coreLink = GenerateCoreLink(conv, responseMessage);


            var futureResponse = coreLink.Request(conv);

            Response<StateResponse> receivedResponse = ReadResponse(futureResponse);
            Assert.Null(receivedResponse.Data);
            Assert.Equal(errorMessage, receivedResponse.Error.Message);
        }

        [Fact]
        public void TimesOut()
        {
            var conv = new CommandConversation(CommandType.Run);

            var response = StateResponseBuilder.Build(StateType.Invalid);

            var converseClientMock = new Mock<IConverseFlatBuffersClient>();
            converseClientMock.Setup(client => client.SendQuery<CommandRequest, ResponseMessage>(Conversation.Handler, It.IsAny<CommandRequest>()))
                .Callback(() => Thread.Sleep(WaitMs*2))
                .Returns(response);
            IConverseFlatBuffersClient converseClient = converseClientMock.Object;

            var coreLink = new CoreLink(converseClient);

            var futureResponse = coreLink.Request(conv, WaitMs);

            Assert.True(futureResponse.Result.TimedOut);
        }

        private static CoreLink GenerateCoreLink(CommandConversation conv, ResponseMessage response)
        {
            IConverseFlatBuffersClient converseClient = GenerateConverseClient(conv, response);

            var coreLink = new CoreLink(converseClient);
            return coreLink;
        }

        private static IConverseFlatBuffersClient GenerateConverseClient(CommandConversation conv, ResponseMessage response)
        {
            var converseClientMock = new Mock<IConverseFlatBuffersClient>();
            converseClientMock.Setup(
                    client => client.SendQuery<CommandRequest, ResponseMessage>(Conversation.Handler, It.IsAny<CommandRequest>()))
                .Returns(response);
            IConverseFlatBuffersClient converseClient = converseClientMock.Object;
            return converseClient;
        }
    }
}
