using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatBuffers;
using GoodAI.Arnold.Core;

namespace GoodAI.Arnold.Communication
{
    public sealed class CommandConversation : Conversation<CommandRequest, StateResponse>
    {
        // TODO(HonzaS): Unify the parameters - either configuration -> json string, or blueprint -> AgentBlueprint (or similar).
        public CommandConversation(CommandType commandType, uint stepsToRun = 0, bool runToBodyStep = false,string blueprint = null, CoreConfiguration configuration = null)
        {
            RequestMessage = CommandRequestBuilder.Build(commandType, stepsToRun, runToBodyStep, blueprint, configuration);
        }
    }
}
