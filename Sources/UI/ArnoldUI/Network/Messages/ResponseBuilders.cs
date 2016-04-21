using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatBuffers;

namespace GoodAI.Arnold.Network.Messages
{
    public static class ResponseMessageBuilder
    {
        public const int BufferInitialSize = 64;

        public static ResponseMessage Build<TRequest>(FlatBufferBuilder builder, Response responseType, Offset<TRequest> responseOffset)
            where TRequest : Table
        {
            Offset<ResponseMessage> responseMessageOffset = ResponseMessage.CreateResponseMessage(builder, responseType,
                responseOffset.Value);
            builder.Finish(responseMessageOffset.Value);

            return ResponseMessage.GetRootAsResponseMessage(builder.DataBuffer);
        }
    }

    public static class ErrorResponseBuilder
    {
        public static ResponseMessage Build(string message)
        {
            var builder = new FlatBufferBuilder(ResponseMessageBuilder.BufferInitialSize);

            StringOffset messageOffset = builder.CreateString(message);

            Offset<ErrorResponse> responseOffset = ErrorResponse.CreateErrorResponse(builder, messageOffset);

            return ResponseMessageBuilder.Build(builder, Response.ErrorResponse, responseOffset);
        }
    }

    public static class StateResponseBuilder
    {
        public static ResponseMessage Build(StateType state)
        {
            var builder = new FlatBufferBuilder(ResponseMessageBuilder.BufferInitialSize);

            Offset<StateResponse> responseOffset = StateResponse.CreateStateResponse(builder, state);

            return ResponseMessageBuilder.Build(builder, Response.StateResponse, responseOffset);
        }
    }
}
