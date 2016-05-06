using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatBuffers;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Extensions;
using GoodAI.Net.ConverseSharpFlatBuffers;

namespace GoodAI.Arnold.Network
{
    public interface ICoreLink
    {
        Task<TResponse> Request<TRequest, TResponse>(
            IConversation<TRequest, TResponse> conversation,
            int timeoutMs)
            where TRequest : Table
            where TResponse : Table, new();
    }

    public interface IConversation<out TRequest, TResponse>
    {
        TRequest RequestData { get; }
    }

    public class RemoteCoreException : Exception
    {
        public RemoteCoreException(string message) : base(message)
        { }
    }

    public class CoreLink : ICoreLink
    {
        private readonly IConverseFlatBuffersClient m_converseClient;

        public CoreLink(IConverseFlatBuffersClient converseClient)
        {
            m_converseClient = converseClient;
        }

        public Task<TResponse> Request<TRequest, TResponse>(
            IConversation<TRequest, TResponse> conversation, int timeoutMs)
            where TRequest : Table
            where TResponse : Table, new()
        {
            return Task<ResponseMessage>.Factory.StartNew(() =>
                    m_converseClient.SendQuery<TRequest, ResponseMessage>(Conversation.Handler, conversation.RequestData))
                .ContinueWith(task =>
                {
                    ResponseMessage result = task.Result;
                    if (result.ResponseType == Response.ErrorResponse)
                        throw new RemoteCoreException(result.GetResponse(new ErrorResponse()).Message);

                    return result.GetResponse(new TResponse());
                })
                .TimeoutAfter(timeoutMs);
        }
    }
}
