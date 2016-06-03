using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatBuffers;

namespace GoodAI.Arnold.Communication
{
    public abstract class Conversation
    {
        public const string Handler = "request";
    }

    public abstract class Conversation<TRequest, TResponse> : Conversation, IConversation<TRequest, TResponse> where TRequest : Table, new()
    {
        public TRequest RequestData => RequestMessage.GetRequest(new TRequest());
        protected RequestMessage RequestMessage { private get; set; }
    }
}
