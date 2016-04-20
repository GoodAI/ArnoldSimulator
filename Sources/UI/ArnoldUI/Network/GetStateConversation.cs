using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatBuffers;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Network.Messages;

namespace GoodAI.Arnold.Network
{
    public sealed class GetStateConversation : Conversation<GetStateRequest, StateResponse>
    {
        public GetStateConversation()
        {
            RequestMessage = GetStateRequestBuilder.Build();
        }
    }
}
