using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatBuffers;
using GoodAI.Arnold.Network.Messages;

namespace GoodAI.Arnold.Network
{
    public sealed class CommandConversation : Conversation<CommandRequest, StateResponse>
    {
        public CommandConversation(CommandType commandType, uint stepsToRun = 0)
        {
            RequestMessage = CommandRequestBuilder.Build(commandType, stepsToRun);
        }
    }
}
