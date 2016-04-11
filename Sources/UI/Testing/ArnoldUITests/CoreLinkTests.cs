using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GoodAI.Arnold.Network;
using GoodAI.Net.ConverseSharp;
using Moq;
using Xunit;
using Xunit.Sdk;

namespace GoodAI.Arnold.UI.Tests
{
    public class CoreLinkTests
    {
        const int WaitMs = 20;

        class TestConversation : IConversation<CommandRequest, StateResponse>
        {
            public string Handler => "testHandler";
            public CommandRequest Request { get; }

            public TestConversation()
            {
                Request = new CommandRequest();
            }
        }

        [Fact]
        public void GetsAsyncConversationResult()
        {
            var conv = new TestConversation
            {
                Request =
                {
                    Command = CommandRequest.Types.CommandType.Start
                }
            };

            var response = new StateResponse
            {
                Data = new StateData
                {
                    State = StateData.Types.StateType.Running
                }
            };

            ICoreLink coreLink = GenerateCoreLink(conv, response);


            Task<StateResponse> futureResponse = coreLink.Request(conv);

            StateResponse receivedResponse = ReadResponse(futureResponse);
            Assert.Equal(StateResponse.ResponseOneofOneofCase.Data, receivedResponse.ResponseOneofCase);
            Assert.Equal(StateData.Types.StateType.Running, receivedResponse.Data.State);
        }

        private static StateResponse ReadResponse(Task<StateResponse> futureResponse)
        {
            Assert.True(futureResponse.Wait(WaitMs));
            StateResponse receivedResponse = futureResponse.Result;
            Assert.NotNull(receivedResponse);
            return receivedResponse;
        }

        [Fact]
        public void GetsAsyncConversationError()
        {
            const string errorMessage = "Foo bar";

            var conv = new TestConversation
            {
                Request =
                {
                    Command = CommandRequest.Types.CommandType.Start
                }
            };

            var response = new StateResponse
            {
                Error = new Error
                {
                    Message = errorMessage
                }
            };

            ICoreLink coreLink = GenerateCoreLink(conv, response);


            Task<StateResponse> futureResponse = coreLink.Request(conv);

            StateResponse receivedResponse = ReadResponse(futureResponse);
            Assert.Equal(StateResponse.ResponseOneofOneofCase.Error, receivedResponse.ResponseOneofCase);
            Assert.Equal(errorMessage, receivedResponse.Error.Message);
        }

        [Fact]
        public void TimesOut()
        {
            var conv = new TestConversation
            {
                Request =
                {
                    Command = CommandRequest.Types.CommandType.Start
                }
            };

            var response = new StateResponse();

            var converseClientMock = new Mock<IConverseProtoBufClient>();
            converseClientMock.Setup(client => client.SendQuery<CommandRequest, StateResponse>(conv.Handler, conv.Request))
                .Callback(() => Thread.Sleep(WaitMs+1))
                .Returns(response);
            IConverseProtoBufClient converseClient = converseClientMock.Object;

            var coreLink = new CoreLink(converseClient);

            Task<StateResponse> futureResponse = coreLink.Request(conv);

            Assert.False(futureResponse.Wait(WaitMs));
        }

        private static CoreLink GenerateCoreLink(TestConversation conv, StateResponse response)
        {
            IConverseProtoBufClient converseClient = GenerateConverseClient(conv, response);

            var coreLink = new CoreLink(converseClient);
            return coreLink;
        }

        private static IConverseProtoBufClient GenerateConverseClient(TestConversation conv, StateResponse response)
        {
            var converseClientMock = new Mock<IConverseProtoBufClient>();
            converseClientMock.Setup(client => client.SendQuery<CommandRequest, StateResponse>(conv.Handler, conv.Request))
                .Returns(response);
            IConverseProtoBufClient converseClient = converseClientMock.Object;
            return converseClient;
        }
    }
}
