using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoodAI.Arnold.Extensions;
using GoodAI.Arnold.Communication;
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

            ResponseMessage responseMessage = StateResponseBuilder.Build(StateType.Running);

            ICoreLink coreLink = GenerateCoreLink(responseMessage);

            Task<StateResponse> futureResponse = coreLink.Request(conversation, WaitMs);

            StateResponse receivedResponse = ReadResponse(futureResponse);
            Assert.Equal(StateType.Running, receivedResponse.State);
        }

        private static StateResponse ReadResponse(Task<StateResponse> futureResponse)
        {
            StateResponse response = futureResponse.Result;
            Assert.NotNull(response);
            return response;
        }

        [Fact]
        public async Task GetsAsyncConversationError()
        {
            const string errorMessage = "Foo bar";

            var conv = new CommandConversation(CommandType.Run);
            var responseMessage = ErrorResponseBuilder.Build(errorMessage);
            ICoreLink coreLink = GenerateCoreLink(responseMessage);

            var ex = await Assert.ThrowsAsync<RemoteCoreException>(() => coreLink.Request(conv, WaitMs));

            Assert.Equal(errorMessage, ex.Message);
        }

        [Fact]
        public async Task TimesOut()
        {
            var conv = new CommandConversation(CommandType.Run);

            var response = StateResponseBuilder.Build(StateType.Paused);

            var converseClientMock = new Mock<IConverseFlatBuffersClient>();
            converseClientMock.Setup(client =>
                    client.SendQuery<CommandRequest, ResponseMessage>(Conversation.Handler, It.IsAny<CommandRequest>()))
                .Callback(() => Thread.Sleep(WaitMs*2))
                .Returns(response);
            IConverseFlatBuffersClient converseClient = converseClientMock.Object;

            var coreLink = new CoreLink(converseClient);

            await Assert.ThrowsAsync<TaskTimeoutException<StateResponse>>(() => coreLink.Request(conv, WaitMs));
        }

        private static CoreLink GenerateCoreLink(ResponseMessage response)
        {
            IConverseFlatBuffersClient converseClient = GenerateConverseClient(response);

            var coreLink = new CoreLink(converseClient);
            return coreLink;
        }

        private static IConverseFlatBuffersClient GenerateConverseClient(ResponseMessage response)
        {
            var converseClientMock = new Mock<IConverseFlatBuffersClient>();
            converseClientMock.Setup(
                client =>
                    client.SendQuery<CommandRequest, ResponseMessage>(Conversation.Handler, It.IsAny<CommandRequest>()))
                .Returns(response);
            IConverseFlatBuffersClient converseClient = converseClientMock.Object;
            return converseClient;
        }
    }
}