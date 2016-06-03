using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Communication
{
    public sealed class GetModelConversation : Conversation<GetModelRequest, ModelResponse>
    {
        public GetModelConversation(bool full)
        {
            RequestMessage = GetModelRequestBuilder.Build(full);
        }
    }
}
