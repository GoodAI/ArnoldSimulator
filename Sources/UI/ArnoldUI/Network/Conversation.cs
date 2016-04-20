using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatBuffers;
using GoodAI.Arnold.Network;

namespace GoodAI.Arnold.Network
{
    public abstract class Conversation
    {
        public const string Handler = "RequestHandler";
    }

    public abstract class Conversation<TRequest, TResponse> : Conversation, IConversation<TRequest, TResponse> where TRequest : Table, new()
    {
        public TRequest RequestData => RequestMessage.GetRequest(new TRequest());
        public RequestMessage RequestMessage { get; protected set; }
    }
}
