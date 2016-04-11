using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArnoldUI.Network;
using GoodAI.Arnold.Net;
using GoodAI.Net.ConverseSharp;
using Moq;
using Xunit;

namespace GoodAI.Arnold.UI.Tests
{
    public class CoreLinkTests
    {
        const int WaitMs = 100;

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

            var converseClientMock = new Mock<IConverseProtoBufClient>();
            converseClientMock.Setup(client => client.SendQuery<CommandRequest, StateResponse>(conv.Handler, conv.Request))
                .Returns(response);
            IConverseProtoBufClient converseClient = converseClientMock.Object;

            var coreLink = new CoreLink(converseClient);


            Task<StateResponse> futureResponse = coreLink.Request(conv);

            Assert.True(futureResponse.Wait(WaitMs));
            StateResponse receivedResponse = futureResponse.Result;
            Assert.NotNull(receivedResponse);
            Assert.Equal(StateResponse.ResponseOneofOneofCase.Data, receivedResponse.ResponseOneofCase);
            Assert.Equal(StateData.Types.StateType.Running, receivedResponse.Data.State);
        }
    }
}
