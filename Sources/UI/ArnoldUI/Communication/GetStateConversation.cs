using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Communication
{
    public sealed class GetStateConversation : Conversation<GetStateRequest, StateResponse>
    {
        public GetStateConversation()
        {
            RequestMessage = GetStateRequestBuilder.Build();
        }
    }
}
