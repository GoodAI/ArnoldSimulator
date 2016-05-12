using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatBuffers;
using GoodAI.Arnold.Graphics.Models;
using OpenTK;

namespace GoodAI.Arnold.Network.Messages
{
    // NOTE: All of these are mostly used for stubbing/testing.

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

    public static class ModelResponseBuilder
    {
        public static ResponseMessage Build(IList<RegionModel> addedRegions)
        {
            var builder = new FlatBufferBuilder(ResponseMessageBuilder.BufferInitialSize);

            var addedRegionsOffsets = new Offset<Region>[addedRegions.Count];

            for (int i = 0; i < addedRegions.Count; i++)
            {
                var region = addedRegions[i];
                var regionName = builder.CreateString(region.Name);
                var regionType = builder.CreateString(region.Type);

                Vector3 lowerBound = region.Position;
                var lowerBounds = Position.CreatePosition(builder, lowerBound.X, lowerBound.Y, lowerBound.Z);
                Vector3 size = region.Size;
                var upperBounds = Position.CreatePosition(builder, lowerBound.X + size.X, lowerBound.Y + size.Y, lowerBound.Z + size.Z);

                addedRegionsOffsets[0] = Region.CreateRegion(builder, 1, regionName, regionType, lowerBounds, upperBounds);
            }

            var addedRegionsVectorOffset = ModelResponse.CreateAddedRegionsVector(builder, addedRegionsOffsets);

            ModelResponse.StartModelResponse(builder);
            ModelResponse.AddAddedRegions(builder, addedRegionsVectorOffset);
            Offset<ModelResponse> responseOffset = ModelResponse.EndModelResponse(builder);

            return ResponseMessageBuilder.Build(builder, Response.ModelResponse, responseOffset);
        }
    }
}
