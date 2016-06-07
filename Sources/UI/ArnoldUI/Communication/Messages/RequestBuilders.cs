using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatBuffers;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Extensions;

namespace GoodAI.Arnold.Communication
{
    public static class RequestMessageBuilder
    {
        public const int BufferInitialSize = 64;

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
            var builder = new FlatBufferBuilder(RequestMessageBuilder.BufferInitialSize);

            Offset<CommandRequest> requestOffset = CommandRequest.CreateCommandRequest(builder, commandType, stepsToRun);

            return RequestMessageBuilder.Build(builder, Request.CommandRequest, requestOffset);
        }
    }

    public static class GetStateRequestBuilder
    {
        public static RequestMessage Build()
        {
            var builder = new FlatBufferBuilder(RequestMessageBuilder.BufferInitialSize);

            // Tables without fields don't have the Create...() static method, it needs to be done like this.
            GetStateRequest.StartGetStateRequest(builder);
            Offset<GetStateRequest> requestOffset = GetStateRequest.EndGetStateRequest(builder);

            return RequestMessageBuilder.Build(builder, Request.GetStateRequest, requestOffset);
        }
    }

    public static class GetModelRequestBuilder
    {
        public static RequestMessage Build(bool full, ModelFilter filter = null)
        {
            var builder = new FlatBufferBuilder(RequestMessageBuilder.BufferInitialSize);
            Offset<Filter>? filterOffset = null;
            if (filter != null)
            {
                var boxes = new Offset<Box3D>[filter.Boxes.Count];
                filter.Boxes.EachWithIndex((i, box) =>
                {
                    boxes[i] = Box3D.CreateBox3D(builder,
                        box.Position.X,
                        box.Position.Y,
                        box.Position.Z,
                        box.Size.X,
                        box.Size.Y,
                        box.Size.Z);
                });

                VectorOffset boxesOffset = Filter.CreateBoxesVector(builder, boxes);

                filterOffset = Filter.CreateFilter(builder, boxesOffset);
            }

            GetModelRequest.StartGetModelRequest(builder);
            GetModelRequest.AddFull(builder, full);
            if (filterOffset.HasValue)
                GetModelRequest.AddFilter(builder, filterOffset.Value);
            Offset<GetModelRequest> requestOffset = GetModelRequest.EndGetModelRequest(builder);

            return RequestMessageBuilder.Build(builder, Request.GetModelRequest, requestOffset);
        }
    }
}
