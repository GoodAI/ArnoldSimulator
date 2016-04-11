using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Network;

namespace ArnoldUI.Network
{
    public class GetStateConversation : IConversation<GetStateRequest, StateResponse>
    {
        public string Handler => "GetStateHandler";
        public GetStateRequest Request { get; }

        public GetStateConversation()
        {
            Request = new GetStateRequest();
        }
    }
}
