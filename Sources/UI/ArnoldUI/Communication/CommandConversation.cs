using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatBuffers;

namespace GoodAI.Arnold.Communication
{
    public sealed class CommandConversation : Conversation<CommandRequest, StateResponse>
    {
        public CommandConversation(CommandType commandType, uint stepsToRun = 0)
        {
            RequestMessage = CommandRequestBuilder.Build(commandType, stepsToRun);
        }
    }
}
