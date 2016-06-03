using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatBuffers;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Extensions;
using GoodAI.Net.ConverseSharpFlatBuffers;

namespace GoodAI.Arnold.Communication
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

                    // ReSharper disable once SwitchStatementMissingSomeCases
                    switch (result.ResponseType)
                    {
                        case Response.NONE:
                            throw new RemoteCoreException("Response type NONE, server rejected the message(?)");
                        case Response.ErrorResponse:
                            throw new RemoteCoreException(result.GetResponse(new ErrorResponse()).Message);
                    }

                    TResponse response = result.GetResponse(new TResponse());
                    if (response == null)
                        throw new RemoteCoreException("Null response.");

                    return response;
                })
                .TimeoutAfter(timeoutMs);
        }
    }
}
