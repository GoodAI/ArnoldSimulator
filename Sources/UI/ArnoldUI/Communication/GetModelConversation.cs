using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Observation;

namespace GoodAI.Arnold.Communication
{
    public sealed class GetModelConversation : Conversation<GetModelRequest, ModelResponse>
    {
        public GetModelConversation(bool full, ModelFilter filter = null, IList<ObserverDefinition> observerRequests = null)
        {
            RequestMessage = GetModelRequestBuilder.Build(full, filter, observerRequests);
        }
    }
}
