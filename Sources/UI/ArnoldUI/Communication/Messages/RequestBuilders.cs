using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlatBuffers;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Extensions;
using GoodAI.Arnold.Observation;

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
        public static RequestMessage Build(CommandType commandType, uint stepsToRun = 0, string blueprint = null)
        {
            var builder = new FlatBufferBuilder(RequestMessageBuilder.BufferInitialSize);

            StringOffset? blueprintOffset = null;
            if (blueprint != null)
                blueprintOffset = builder.CreateString(blueprint);

            CommandRequest.StartCommandRequest(builder);

            CommandRequest.AddCommand(builder, commandType);
            CommandRequest.AddStepsToRun(builder, stepsToRun);

            if (blueprintOffset.HasValue)
                CommandRequest.AddBlueprint(builder, blueprintOffset.Value);

            Offset<CommandRequest> requestOffset = CommandRequest.EndCommandRequest(builder);

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

    public static class ObserverSetupRequestBuilder
    {
        public static RequestMessage Build(IList<ObserverDefinition> definitions)
        {
            var builder = new FlatBufferBuilder(RequestMessageBuilder.BufferInitialSize);

            var observerOffsets = new Offset<ObserverSetup>[definitions.Count()];
            definitions.EachWithIndex((i, definition) =>
            {
                Offset<NeuronId> neuronId = NeuronId.CreateNeuronId(builder, definition.NeuronIndex,
                    definition.RegionIndex);
                StringOffset observerType = builder.CreateString(definition.Type);

                observerOffsets[i] = ObserverSetup.CreateObserverSetup(builder, neuronId, observerType);
            });

            VectorOffset observersVectorOffset = ObserverSetupRequest.CreateObserversVector(builder, observerOffsets);
            Offset<ObserverSetupRequest> requestOffset = ObserverSetupRequest.CreateObserverSetupRequest(builder, observersVectorOffset);

            return RequestMessageBuilder.Build(builder, Request.ObserverSetupRequest, requestOffset);
        }
    }
}
