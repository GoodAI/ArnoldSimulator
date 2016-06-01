﻿using System;
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
        public static ResponseMessage Build(
            IList<RegionModel> addedRegions = null,
            IList<RegionModel> repositionedRegions = null,
            IList<RegionModel> removedRegions = null,
            IList<ConnectorModel> addedConnectors = null,
            IList<ConnectorModel> removedConnectors = null,
            IList<ConnectionModel> addedConnections = null,
            IList<ConnectionModel> removedConnections = null,
            IList<ExpertModel> addedNeurons = null,
            IList<ExpertModel> repositionedNeurons = null,
            IList<ExpertModel> removedNeurons = null,
            IList<SynapseModel> addedSynapses = null,
            IList<SynapseModel> spikedSynapses = null,
            IList<SynapseModel> removedSynapses = null)
        {
            var builder = new FlatBufferBuilder(ResponseMessageBuilder.BufferInitialSize);

            VectorOffset? addedRegionsVectorOffset = BuildAddedRegions(addedRegions, builder);
            VectorOffset? repositionedRegionsVectorOffset = BuildRepositionedRegions(repositionedRegions, builder);
            VectorOffset? removedRegionsVectorOffset = BuildRemovedRegions(removedRegions, builder);

            VectorOffset? addedConnectorsVectorOffset = BuildAddedConnectors(addedConnectors, builder);
            VectorOffset? removedConnectorsVectorOffset = BuildRemovedConnectors(removedConnectors, builder);

            VectorOffset? addedConnectionsVectorOffset = BuildAddedConnections(addedConnections, builder);
            VectorOffset? removedConnectionsVectorOffset = BuildRemovedConnections(removedConnections, builder);

            VectorOffset? addedNeuronsVectorOffset = BuildAddedNeurons(addedNeurons, builder);
            VectorOffset? repositionedNeuronsVectorOffset = BuildRepositionedNeurons(repositionedNeurons, builder);
            VectorOffset? removedNeuronsVectorOffset = BuildRemovedNeurons(removedNeurons, builder);

            VectorOffset? addedSynapsesVectorOffset = BuildAddedSynapses(addedSynapses, builder);
            VectorOffset? spikedSynapsesVectorOffset = BuildSpikedSynapses(spikedSynapses, builder);
            VectorOffset? removedSynapsesVectorOffset = BuildRemovedSynapses(removedSynapses, builder);

            ModelResponse.StartModelResponse(builder);

            if (addedRegionsVectorOffset.HasValue)
                ModelResponse.AddAddedRegions(builder, addedRegionsVectorOffset.Value);
            if (repositionedRegionsVectorOffset.HasValue)
                ModelResponse.AddRepositionedRegions(builder, repositionedRegionsVectorOffset.Value);
            if (removedRegionsVectorOffset.HasValue)
                ModelResponse.AddRemovedRegions(builder, removedRegionsVectorOffset.Value);

            if (addedConnectorsVectorOffset.HasValue)
                ModelResponse.AddAddedConnectors(builder, addedConnectorsVectorOffset.Value);
            if (removedConnectorsVectorOffset.HasValue)
                ModelResponse.AddRemovedConnectors(builder, removedConnectorsVectorOffset.Value);

            if (addedConnectionsVectorOffset.HasValue)
                ModelResponse.AddAddedConnections(builder, addedConnectionsVectorOffset.Value);
            if (removedConnectionsVectorOffset.HasValue)
                ModelResponse.AddRemovedConnections(builder, removedConnectionsVectorOffset.Value);

            if (addedNeuronsVectorOffset.HasValue)
                ModelResponse.AddAddedNeurons(builder, addedNeuronsVectorOffset.Value);
            if (repositionedNeuronsVectorOffset.HasValue)
                ModelResponse.AddRepositionedNeurons(builder, repositionedNeuronsVectorOffset.Value);
            if (removedNeuronsVectorOffset.HasValue)
                ModelResponse.AddRemovedNeurons(builder, removedNeuronsVectorOffset.Value);

            if (addedSynapsesVectorOffset.HasValue)
                ModelResponse.AddAddedSynapses(builder, addedSynapsesVectorOffset.Value);
            if (spikedSynapsesVectorOffset.HasValue)
                ModelResponse.AddSpikedSynapses(builder, spikedSynapsesVectorOffset.Value);
            if (removedSynapsesVectorOffset.HasValue)
                ModelResponse.AddRemovedSynapses(builder, removedSynapsesVectorOffset.Value);

            return ResponseMessageBuilder.Build(builder, Response.ModelResponse, ModelResponse.EndModelResponse(builder));
        }

        private static VectorOffset? BuildAddedRegions(IList<RegionModel> addedRegions, FlatBufferBuilder builder)
        {
            var addedRegionsOffsets = BuildRegionsOffsets(addedRegions, builder);

            if (addedRegionsOffsets == null)
                return null;

            return ModelResponse.CreateAddedRegionsVector(builder, addedRegionsOffsets);
        }

        private static VectorOffset? BuildRepositionedRegions(IList<RegionModel> repositionedRegions,
            FlatBufferBuilder builder)
        {
            var repositionedRegionsOffsets = BuildRegionsOffsets(repositionedRegions, builder);

            if (repositionedRegionsOffsets == null)
                return null;

            return ModelResponse.CreateRepositionedRegionsVector(builder, repositionedRegionsOffsets);
        }

        private static VectorOffset? BuildRemovedRegions(IList<RegionModel> removedRegions, FlatBufferBuilder builder)
        {
            if (removedRegions == null)
                return null;

            return ModelResponse.CreateRemovedRegionsVector(builder, removedRegions.Select(region => region.Index).ToArray());
        }

        private static Offset<Region>[] BuildRegionsOffsets(IList<RegionModel> regions, FlatBufferBuilder builder)
        {
            Offset<Region>[] regionsOffsets = BuildOffsets(regions, builder, region =>
            {
                StringOffset regionName = builder.CreateString(region.Name);
                StringOffset regionType = builder.CreateString(region.Type);

                Vector3 lowerBound = region.Position - region.Size/2;
                Offset<Position> lowerBounds = Position.CreatePosition(builder, lowerBound.X, lowerBound.Y, lowerBound.Z);
                Vector3 size = region.Size;
                Offset<Position> upperBounds = Position.CreatePosition(builder, lowerBound.X + size.X, lowerBound.Y + size.Y,
                    lowerBound.Z + size.Z);

                return Region.CreateRegion(builder, region.Index, regionName, regionType, lowerBounds, upperBounds);
            });
            return regionsOffsets;
        }

        private static VectorOffset? BuildAddedConnectors(IList<ConnectorModel> addedConnectors,
            FlatBufferBuilder builder)
        {
            Offset<Connector>[] addedConnectorsOffsets = BuildOffsets(addedConnectors, builder, connector =>
            {
                StringOffset connectorName = builder.CreateString(connector.Name);

                return Connector.CreateConnector(builder, connector.Region.Index, connectorName,
                    connector.Direction, connector.SlotCount);
            });

            if (addedConnectorsOffsets == null)
                return null;

            return ModelResponse.CreateAddedConnectorsVector(builder, addedConnectorsOffsets);
        }

        private static VectorOffset? BuildRemovedConnectors(IList<ConnectorModel> removedConnectors, FlatBufferBuilder builder)
        {
            if (removedConnectors == null)
                return null;

            var removedConnectorsOffsets = BuildOffsets(removedConnectors, builder,
                connector =>
                {
                    var name = builder.CreateString(connector.Name);
                    return Connector.CreateConnector(builder, connector.Region.Index, name, connector.Direction,
                        connector.SlotCount);
                });

            return ModelResponse.CreateRemovedConnectorsVector(builder, removedConnectorsOffsets);
        }

        private static VectorOffset? BuildAddedConnections(IList<ConnectionModel> addedConnections,
            FlatBufferBuilder builder)
        {
            Offset<Connection>[] addedConnectionsOffsets = BuildOffsets(addedConnections, builder, connection =>
            {
                StringOffset fromConnectorName = builder.CreateString(connection.From.Name);
                StringOffset toConnectorName = builder.CreateString(connection.To.Name);

                return Connection.CreateConnection(builder, connection.From.Region.Index,
                    fromConnectorName, connection.To.Region.Index, toConnectorName,
                    connection.From.Direction);
            });

            if (addedConnectionsOffsets == null)
                return null;

            return ModelResponse.CreateAddedConnectionsVector(builder,
                addedConnectionsOffsets);
        }

        private static VectorOffset? BuildRemovedConnections(IList<ConnectionModel> removedConnections,
            FlatBufferBuilder builder)
        {
            Offset<Connection>[] removedConnectionsOffsets = BuildOffsets(removedConnections, builder, connection =>
            {
                StringOffset fromConnectorName = builder.CreateString(connection.From.Name);
                StringOffset toConnectorName = builder.CreateString(connection.To.Name);

                return Connection.CreateConnection(builder, connection.From.Region.Index,
                    fromConnectorName, connection.To.Region.Index, toConnectorName,
                    connection.From.Direction);
            });

            if (removedConnectionsOffsets == null)
                return null;

            return ModelResponse.CreateRemovedConnectionsVector(builder,
                removedConnectionsOffsets);
        }

        private static VectorOffset? BuildAddedNeurons(IList<ExpertModel> addedNeurons,
            FlatBufferBuilder builder)
        {
            Offset<Neuron>[] addedNeuronsOffsets = BuildOffsets(addedNeurons, builder, neuron =>
            {
                StringOffset type = builder.CreateString(neuron.Type);
                Offset<Position> position = Position.CreatePosition(builder, 1, 2, 3);

                var neuronId = NeuronId.CreateNeuronId(builder, neuron.Index, neuron.RegionModel.Index);

                return Neuron.CreateNeuron(builder, neuronId, type, position);
            });

            if (addedNeuronsOffsets == null)
                return null;

            return ModelResponse.CreateAddedNeuronsVector(builder, addedNeuronsOffsets);
        }

        private static VectorOffset? BuildRepositionedNeurons(IList<ExpertModel> repositionedNeurons,
            FlatBufferBuilder builder)
        {
            return null;
        }

        private static VectorOffset? BuildRemovedNeurons(IList<ExpertModel> removedNeurons, FlatBufferBuilder builder)
        {
            if (removedNeurons == null)
                return null;

            Offset<NeuronId>[] removedNeuronsOffsets = BuildOffsets(removedNeurons, builder,
                neuron => NeuronId.CreateNeuronId(builder, neuron.Index, neuron.RegionModel.Index));

            return ModelResponse.CreateRemovedNeuronsVector(builder, removedNeuronsOffsets);
        }

        private static VectorOffset? BuildAddedSynapses(IList<SynapseModel> addedSynapses,
            FlatBufferBuilder builder)
        {
            Offset<Synapse>[] addedSynapsesOffsets = BuildOffsets(addedSynapses, builder,
                synapse =>
                {
                    var fromNeuronId = NeuronId.CreateNeuronId(builder, synapse.FromNeuron.Index, synapse.FromRegion.Index);
                    var toNeuronId = NeuronId.CreateNeuronId(builder, synapse.ToNeuron.Index, synapse.ToRegion.Index);

                    return Synapse.CreateSynapse(builder, fromNeuronId, toNeuronId);
                });

            if (addedSynapsesOffsets == null)
                return null;

            return ModelResponse.CreateAddedSynapsesVector(builder, addedSynapsesOffsets);
        }

        private static VectorOffset? BuildSpikedSynapses(IList<SynapseModel> spikedSynapses,
            FlatBufferBuilder builder)
        {
            return null;
        }

        private static VectorOffset? BuildRemovedSynapses(IList<SynapseModel> removedSynapses, FlatBufferBuilder builder)
        {
            if (removedSynapses == null)
                return null;

            Offset<Synapse>[] removedSynapsesOffsets = BuildOffsets(removedSynapses, builder,
                synapse =>
                {
                    var fromNeuronId = NeuronId.CreateNeuronId(builder, synapse.FromNeuron.Index, synapse.FromRegion.Index);
                    var toNeuronId = NeuronId.CreateNeuronId(builder, synapse.ToNeuron.Index, synapse.ToRegion.Index);

                    return Synapse.CreateSynapse(builder, fromNeuronId, toNeuronId);
                });

            return ModelResponse.CreateRemovedSynapsesVector(builder, removedSynapsesOffsets);
        }

        private static Offset<TMessageEntity>[] BuildOffsets<TModel, TMessageEntity>(IList<TModel> addedModels,
            FlatBufferBuilder builder, Func<TModel, Offset<TMessageEntity>> createEntity)
            where TMessageEntity : Table
        {
            if (addedModels == null)
                return null;

            var addedOffsets = new Offset<TMessageEntity>[addedModels.Count];

            for (int i = 0; i < addedModels.Count; i++)
            {
                TModel model = addedModels[i];
                Offset<TMessageEntity> entity = createEntity(model);

                addedOffsets[i] = entity;
            }

            return addedOffsets;
        }
    }
}