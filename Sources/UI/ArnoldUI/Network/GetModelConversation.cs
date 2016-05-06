using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Network.Messages;

namespace GoodAI.Arnold.Network
{
    public sealed class GetModelConversation : Conversation<GetModelRequest, ModelResponse>
    {
        public GetModelConversation()
        {
            RequestMessage = GetModelRequestBuilder.Build();
        }
    }
}
