using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;

namespace GoodAI.Arnold.Communication
{
    public sealed class GetModelConversation : Conversation<GetModelRequest, ModelResponse>
    {
        public GetModelConversation(bool full, ModelFilter filter = null)
        {
            RequestMessage = GetModelRequestBuilder.Build(full, filter);
        }
    }
}
