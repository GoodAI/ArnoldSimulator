using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Net.ConverseSharp;
using Google.Protobuf;

namespace ArnoldUI.Network
{
    public interface ICoreLink
    {
        Task<TResponse> Request<TRequest, TResponse>(IConversation<TRequest, TResponse> conversation)
            where TRequest : IMessage
            where TResponse : IMessage<TResponse>, new();
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
            where TResponse : IMessage<TResponse>, new()
            where TRequest : IMessage
        {
            return Task<TResponse>.Factory.StartNew(() => m_converseClient.SendQuery<TRequest, TResponse>(conversation.Handler, conversation.Request));
        }
    }
}
