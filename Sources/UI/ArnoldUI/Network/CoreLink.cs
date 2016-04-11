using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Net.ConverseSharp;
using Google.Protobuf;

namespace GoodAI.Arnold.Network
{
    public interface ICoreLink
    {
        Task<TResponse> Request<TRequest, TResponse>(IConversation<TRequest, TResponse> conversation)
            where TRequest : class, IMessage
            where TResponse : class, IMessage<TResponse>, new();
    }

    public interface IConversation<out TRequest, TResponse>
    {
        string Handler { get; }
        TRequest Request { get; }
    }

    public class CoreLink : ICoreLink
    {
        private readonly IConverseProtoBufClient m_converseClient;

        public CoreLink(IConverseProtoBufClient converseClient)
        {
            m_converseClient = converseClient;
        }

        public Task<TResponse> Request<TRequest, TResponse>(IConversation<TRequest, TResponse> conversation)
            where TResponse : class, IMessage<TResponse>, new()
            where TRequest : class, IMessage
        {
            return
                Task<TResponse>.Factory.StartNew(
                    () => m_converseClient.SendQuery<TRequest, TResponse>(conversation.Handler, conversation.Request));
        }
    }
}
