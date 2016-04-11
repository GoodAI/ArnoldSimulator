using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Network;

namespace ArnoldUI.Network
{
    public class CommandConversation : IConversation<CommandRequest, StateResponse>
    {
        public string Handler => "CommandHandler";
        public CommandRequest Request { get; }

        public CommandConversation()
        {
            Request = new CommandRequest();
        }
    }
}
