using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatBuffers;

namespace GoodAI.Arnold.Network.Messages
{
    public static class RequestMessageBuilder
    {
        public static RequestMessage Build<TRequest>(FlatBufferBuilder builder, Request requestType, Offset<TRequest> requestOffset)
            where TRequest : Table
        {
            Offset<RequestMessage> requestMessageOffset = RequestMessage.CreateRequestMessage(builder, requestType,
                requestOffset.Value);
            builder.Finish(requestMessageOffset.Value);

            return RequestMessage.GetRootAsRequestMessage(builder.DataBuffer);
        }
    }

    public static class CommandRequestBuilder
    {
        public static RequestMessage Build(CommandType commandType, uint stepsToRun = 0)
        {
            var builder = new FlatBufferBuilder(1);

            Offset<CommandRequest> requestOffset = CommandRequest.CreateCommandRequest(builder, commandType, stepsToRun);

            return RequestMessageBuilder.Build(builder, Request.CommandRequest, requestOffset);
        }
    }

    public static class GetStateRequestBuilder
    {
        public static RequestMessage Build()
        {
            var builder = new FlatBufferBuilder(1);

            // Tables without fields don't have the Create...() static method, it needs to be done like this.
            GetStateRequest.StartGetStateRequest(builder);
            Offset<GetStateRequest> requestOffset = GetStateRequest.EndGetStateRequest(builder);

            return RequestMessageBuilder.Build(builder, Request.CommandRequest, requestOffset);
        }
    }
}
