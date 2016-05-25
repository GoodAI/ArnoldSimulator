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

        public static ResponseMessage Build<TRequest>(FlatBufferBuilder builder, Response responseType,
            Offset<TRequest> responseOffset)
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
        public static ResponseMessage Build(IList<RegionModel> addedRegions = null,
            IList<ConnectorModel> addedConnectors = null, IList<ConnectionModel> addedConnections = null,
            IList<ExpertModel> addedNeurons = null)
        {
            var builder = new FlatBufferBuilder(ResponseMessageBuilder.BufferInitialSize);

            VectorOffset? addedRegionsVectorOffset = BuildAddedRegions(addedRegions, builder);
            VectorOffset? addedConnectorsVectorOffset = BuildAddedConnectors(addedConnectors, builder);
            VectorOffset? addedConnectionsVectorOffset = BuildAddedConnections(addedConnections, builder);
            VectorOffset? addedNeuronsVectorOffset = BuildAddedNeurons(addedNeurons, builder);

            ModelResponse.StartModelResponse(builder);
            if (addedRegionsVectorOffset.HasValue)
                ModelResponse.AddAddedRegions(builder, addedRegionsVectorOffset.Value);
            if (addedConnectorsVectorOffset.HasValue)
                ModelResponse.AddAddedConnectors(builder, addedConnectorsVectorOffset.Value);
            if (addedConnectionsVectorOffset.HasValue)
                ModelResponse.AddAddedConnections(builder, addedConnectionsVectorOffset.Value);
            if (addedNeuronsVectorOffset.HasValue)
                ModelResponse.AddAddedNeurons(builder, addedNeuronsVectorOffset.Value);

            Offset<ModelResponse> responseOffset = ModelResponse.EndModelResponse(builder);

            return ResponseMessageBuilder.Build(builder, Response.ModelResponse, responseOffset);
        }

        private static VectorOffset? BuildAddedRegions(IList<RegionModel> addedRegions, FlatBufferBuilder builder)
        {
            if (addedRegions == null)
                return null;

            var addedRegionsOffsets = new Offset<Region>[addedRegions.Count];

            for (int i = 0; i < addedRegions.Count; i++)
            {
                var region = addedRegions[i];
                var regionName = builder.CreateString(region.Name);
                var regionType = builder.CreateString(region.Type);

                Vector3 lowerBound = region.Position - region.Size/2;
                var lowerBounds = Position.CreatePosition(builder, lowerBound.X, lowerBound.Y, lowerBound.Z);
                Vector3 size = region.Size;
                var upperBounds = Position.CreatePosition(builder, lowerBound.X + size.X, lowerBound.Y + size.Y,
                    lowerBound.Z + size.Z);

                addedRegionsOffsets[i] = Region.CreateRegion(builder, region.Index, regionName, regionType, lowerBounds,
                    upperBounds);
            }

            return ModelResponse.CreateAddedRegionsVector(builder, addedRegionsOffsets);
        }

        private static VectorOffset? BuildAddedConnectors(IList<ConnectorModel> addedConnectors,
            FlatBufferBuilder builder)
        {
            if (addedConnectors == null)
                return null;

            var addedConnectorsOffsets = new Offset<Connector>[addedConnectors.Count];

            for (int i = 0; i < addedConnectors.Count; i++)
            {
                var connector = addedConnectors[i];

                var connectorName = builder.CreateString(connector.Name);

                Direction direction = connector.Direction == ConnectorDirection.Forward
                    ? Direction.Forward
                    : Direction.Backward;
                addedConnectorsOffsets[i] = Connector.CreateConnector(builder, connector.Region.Index, connectorName,
                    direction, connector.SlotCount);
            }

            return ModelResponse.CreateAddedConnectorsVector(builder, addedConnectorsOffsets);
        }

        private static VectorOffset? BuildAddedConnections(IList<ConnectionModel> addedConnections,
            FlatBufferBuilder builder)
        {
            if (addedConnections == null)
                return null;

            var addedConnectionsOffsets = new Offset<Connection>[addedConnections.Count];

            for (int i = 0; i < addedConnections.Count; i++)
            {
                var connection = addedConnections[i];

                var fromConnectorName = builder.CreateString(connection.From.Name);
                var toConnectorName = builder.CreateString(connection.To.Name);

                Direction direction = connection.From.Direction == ConnectorDirection.Forward
                    ? Direction.Forward
                    : Direction.Backward;

                addedConnectionsOffsets[i] = Connection.CreateConnection(builder, connection.From.Region.Index,
                    fromConnectorName, connection.To.Region.Index, toConnectorName,
                    direction);
            }

            return ModelResponse.CreateAddedConnectionsVector(builder,
                addedConnectionsOffsets);
        }

        private static VectorOffset? BuildAddedNeurons(IList<ExpertModel> addedNeurons,
            FlatBufferBuilder builder)
        {
            if (addedNeurons == null)
                return null;

            var addedNeuronsOffsets = new Offset<Neuron>[addedNeurons.Count];

            for (int i = 0; i < addedNeurons.Count; i++)
            {
                var neuron = addedNeurons[i];

                var type = builder.CreateString(neuron.Type);
                var position = Position.CreatePosition(builder, 1, 2, 3);

                addedNeuronsOffsets[i] = Neuron.CreateNeuron(builder, neuron.Id, neuron.RegionModel.Index, type,
                    position);
            }

            return ModelResponse.CreateAddedNeuronsVector(builder, addedNeuronsOffsets);
        }
    }
}