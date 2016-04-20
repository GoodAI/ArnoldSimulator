using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatBuffers;
using GoodAI.Arnold.Extensions;
using GoodAI.Net.ConverseSharpFlatBuffers;

namespace GoodAI.Arnold.Network
{
    public interface ICoreLink
    {
        Task<TimeoutResult<Response<TResponse>>> Request<TRequest, TResponse>(IConversation<TRequest, TResponse> conversation,
            int timeoutMs = 0)
            where TRequest : Table
            where TResponse : Table, new();
    }

    public interface IConversation<out TRequest, TResponse>
    {
        TRequest RequestData { get; }
    }

    public class Response<TResponse>
    {
        public Response(ErrorResponse error)
        {
            Error = error;
        }

        public Response(TResponse data)
        {
            Data = data;
        }

        public ErrorResponse Error { get; private set; }
        public TResponse Data { get; private set; }
    }

    public class CoreLink : ICoreLink
    {
        private readonly IConverseFlatBuffersClient m_converseClient;

        public CoreLink(IConverseFlatBuffersClient converseClient)
        {
            m_converseClient = converseClient;
        }

        public Task<TimeoutResult<Response<TResponse>>> Request<TRequest, TResponse>(IConversation<TRequest, TResponse> conversation, int timeoutMs = 0)
            where TRequest : Table
            where TResponse : Table, new()
        {
            return Task<ResponseMessage>.Factory.StartNew(
                () => m_converseClient.SendQuery<TRequest, ResponseMessage>(Conversation.Handler, conversation.RequestData)).ContinueWith(task
                    =>
                {
                    ResponseMessage result = task.Result;
                    if (result.ResponseType == Response.ErrorResponse)
                        return new Response<TResponse>(result.GetResponse(new ErrorResponse()));
                    else
                        return new Response<TResponse>(result.GetResponse(new TResponse()));
                }).TimeoutAfter(timeoutMs);
        }
    }
}
