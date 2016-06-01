using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Graphics.Models;
using GoodAI.Logging;
using OpenTK;

namespace GoodAI.Arnold.Network
{
    public interface IModelDiffApplier
    {
        void ApplyModelDiff(SimulationModel model, ModelResponse diff);
    }

    public class ModelDiffApplier : IModelDiffApplier
    {
        // Injected.
        public ILog Log { get; set; } = NullLogger.Instance;

        public void ApplyModelDiff(SimulationModel model, ModelResponse diff)
        {
            ApplyAddedRegions(model, diff);
            ApplyAddedConnectors(model, diff);
            ApplyAddedConnections(model, diff);
            ApplyAddedNeurons(model, diff);
            ApplyAddedSynapses(model, diff);

            ApplyRemovedRegions(model, diff);
            ApplyRemovedConnectors(model, diff);
            ApplyRemovedConnections(model, diff);
            ApplyRemovedNeurons(model, diff);
            ApplyRemovedSynapses(model, diff);

            ApplySpikedSynapses(model, diff);
        }

        private static void ApplyAddedRegions(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.AddedRegionsLength; i++)
            {
                Region addedRegion = diff.GetAddedRegions(i);

                Position lowerBound = addedRegion.LowerBound;
                Position upperBound = addedRegion.UpperBound;

                var size = new Vector3(upperBound.X - lowerBound.X, upperBound.Y - lowerBound.Y, upperBound.Z - lowerBound.Z);

                Vector3 position = lowerBound.ToVector3() + size/2;

                model.Regions[addedRegion.Index] = new RegionModel(addedRegion.Index, addedRegion.Name, addedRegion.Type, position, size);
            }
        }

        private void ApplyAddedNeurons(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.AddedNeuronsLength; i++)
            {
                Neuron neuron = diff.GetAddedNeurons(i);

                RegionModel targetRegionModel = model.Regions[neuron.Id.Region];
                if (targetRegionModel == null)
                {
                    Log.Warn("Cannot add neuron {neuronId}, region with index {regionIndex} was not found", neuron.Id.Neuron,
                        neuron.Id.Region);
                    continue;
                }

                targetRegionModel.AddExpert(new ExpertModel(neuron.Id.Neuron, neuron.Type, targetRegionModel, neuron.Position.ToVector3()));
            }
        }

        private void ApplyAddedConnectors(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.AddedConnectorsLength; i++)
            {
                Connector addedConnector = diff.GetAddedConnectors(i);

                RegionModel targetRegionModel = model.Regions[addedConnector.RegionIndex];
                if (targetRegionModel == null)
                {
                    Log.Warn("Cannot add connector {connectorName}, region with index {regionIndex} was not found",
                        addedConnector.Name,
                        addedConnector.RegionIndex);
                    continue;
                }

                // TODO(HonzaS): A Shortcut for the creation of models?
                // Replace them with factory + inject logger.
                if (addedConnector.Direction == Direction.Forward)
                    targetRegionModel.OutputConnectors.AddChild(new OutputConnectorModel(targetRegionModel, addedConnector.Name, addedConnector.Size));
                else
                    targetRegionModel.InputConnectors.AddChild(new InputConnectorModel(targetRegionModel, addedConnector.Name, addedConnector.Size));
            }
        }

        private void ApplyAddedConnections(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.AddedConnectionsLength; i++)
            {
                Connection addedConnection = diff.GetAddedConnections(i);

                RegionModel fromRegion = model.Regions[addedConnection.FromRegion];
                RegionModel toRegion = model.Regions[addedConnection.ToRegion];

                if (fromRegion == null || toRegion == null)
                {
                    string missingRegion = fromRegion == null ? "Source" : "Target";
                    LogConnectionNotProcessed(addedConnection, "add", $"{missingRegion} region not found");
                    continue;
                }

                OutputConnectorModel fromConnector =
                    fromRegion.OutputConnectors.FirstOrDefault(connector => connector.Name == addedConnection.FromConnector);

                InputConnectorModel toConnector =
                    toRegion.InputConnectors.FirstOrDefault(connector => connector.Name == addedConnection.ToConnector);


                if (fromConnector == null || toConnector == null)
                {
                    string missingConnector = fromConnector == null ? "Source" : "Target";
                    LogConnectionNotProcessed(addedConnection, "add", $"{missingConnector} connector not found");
                    continue;
                }

                var connectionModel = new ConnectionModel(fromConnector, toConnector);

                model.Connections.AddChild(connectionModel);
            }
        }

        private void LogConnectionNotProcessed(Connection addedConnection, string action, string reason)
        {
            Log.Warn(
                "Could not " + action +
                " add connection from region {fromRegion}, connector {fromConnector} to region {toRegion}, connector {toConnector}: {reason}",
                addedConnection.FromRegion,
                addedConnection.FromConnector,
                addedConnection.ToRegion,
                addedConnection.ToConnector,
                reason);
        }

        private void ApplyAddedSynapses(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.AddedSynapsesLength; i++)
            {
                Synapse addedSynapse = diff.GetAddedSynapses(i);
                ProcessSynapse(model, addedSynapse, (fromRegion, fromNeuron, toRegion, toNeuron) =>
                {
                    var synapseModel = new SynapseModel(fromRegion, fromNeuron, toRegion, toNeuron);
                    fromNeuron.Outputs[synapseModel.ToNeuron.Index] = synapseModel;
                    fromRegion.AddSynapse(synapseModel);
                });
            }
        }

        private void ApplySpikedSynapses(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.SpikedSynapsesLength; i++)
            {
                Synapse spikedSynapse = diff.GetSpikedSynapses(i);
                ProcessSynapse(model, spikedSynapse, (fromRegion, fromNeuron, toRegion, toNeuron) =>
                {
                    SynapseModel synapseModel = fromNeuron.Outputs[toNeuron.Index];

                    if (synapseModel == null)
                    {
                        LogSynapseNotProcessed(spikedSynapse, "spike", "Synapse not found");
                        return;
                    }

                    synapseModel.Spike();
                });
            }
        }

        private void ApplyRemovedRegions(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.RemovedRegionsLength; i++)
            {
                uint regionIndex = diff.GetRemovedRegions(i);


                if (!model.Regions.ContainsKey(regionIndex))
                {
                    Log.Warn("Cannot remove region with index {regionIndex}, region not found", regionIndex);
                    continue;
                }

                model.Regions.Remove(regionIndex);
            }
        }

        private void ApplyRemovedConnectors(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.RemovedConnectorsLength; i++)
            {
                Connector removedConnector = diff.GetRemovedConnectors(i);

                RegionModel targetRegionModel = model.Regions[removedConnector.RegionIndex];
                if (targetRegionModel == null)
                {
                    Log.Warn("Cannot add connector {connectorName}, region with index {regionIndex} was not found",
                        removedConnector.Name,
                        removedConnector.RegionIndex);
                    continue;
                }

                var deleted = removedConnector.Direction == Direction.Backward
                    ? targetRegionModel.InputConnectors.Remove(removedConnector.Name)
                    : targetRegionModel.OutputConnectors.Remove(removedConnector.Name);

                if (!deleted)
                    Log.Warn("Cannot remove connector {connector} from region {region}, connector not found",
                        removedConnector.Name, targetRegionModel.Index);
            }
        }

        private void ApplyRemovedConnections(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.RemovedConnectionsLength; i++)
            {
                Connection removedConnection = diff.GetRemovedConnections(i);

                RegionModel fromRegion = model.Regions[removedConnection.FromRegion];
                RegionModel toRegion = model.Regions[removedConnection.ToRegion];

                if (fromRegion == null || toRegion == null)
                {
                    string missingRegion = fromRegion == null ? "Source" : "Target";
                    LogConnectionNotProcessed(removedConnection, "remove", $"{missingRegion} region not found");
                    continue;
                }

                OutputConnectorModel fromConnector =
                    fromRegion.OutputConnectors.FirstOrDefault(connector => connector.Name == removedConnection.FromConnector);

                InputConnectorModel toConnector =
                    toRegion.InputConnectors.FirstOrDefault(connector => connector.Name == removedConnection.ToConnector);


                if (fromConnector == null || toConnector == null)
                {
                    string missingConnector = fromConnector == null ? "Source" : "Target";
                    LogConnectionNotProcessed(removedConnection, "remove", $"{missingConnector} connector not found");
                    continue;
                }

                // TODO(HonzaS): Optimize lookup.
                var connectionModel =
                    model.Connections.FirstOrDefault(
                        connection => connection.From == fromConnector && connection.To == toConnector);

                model.Connections.Remove(connectionModel);
                fromConnector.Connections.Remove(connectionModel);
                toConnector.Connections.Remove(connectionModel);
            }
        }

        private void ApplyRemovedNeurons(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.RemovedNeuronsLength; i++)
            {
                NeuronId id = diff.GetRemovedNeurons(i);

                RegionModel region;
                if (!model.Regions.TryGetModel(id.Region, out region))
                {
                    LogRegionNeuronNotFound("region", id);
                    continue;
                }

                if (!region.Experts.ContainsKey(id.Neuron))
                {
                    LogRegionNeuronNotFound("neuron", id);
                    continue;
                }

                region.Experts.Remove(id.Neuron);
            }
        }

        private void ApplyRemovedSynapses(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.RemovedSynapsesLength; i++)
            {
                var synapse = diff.GetRemovedSynapses(i);

                ProcessSynapse(model, synapse, (fromRegion, fromNeuron, toRegion, toNeuron) =>
                {
                    var synapseModel = fromNeuron.Outputs[toNeuron.Index];
                    fromNeuron.Outputs.Remove(toNeuron.Index);
                    fromRegion.Synapses.Remove(synapseModel);
                });
            }
        }

        private void LogRegionNeuronNotFound(string itemNotFound, NeuronId id)
        {
            Log.Warn(
                "Cannot remove neuron with id {neuronIndex} in region {regionIndex}, " + itemNotFound + " not found",
                id.Region);
        }

        private void ProcessSynapse(SimulationModel model, Synapse synapse, Action<RegionModel, ExpertModel, RegionModel, ExpertModel> action)
        {
            if (!CheckSameRegion(synapse))
                return;

            RegionModel fromRegion;
            RegionModel toRegion;
            if (!TryGetRegions(model, synapse, out fromRegion, out toRegion))
                return;

            ExpertModel fromNeuron;
            ExpertModel toNeuron;
            if (!TryGetNeurons(fromRegion, synapse, toRegion, out fromNeuron, out toNeuron))
                return;

            action(fromRegion, fromNeuron, toRegion, toNeuron);
        }

        private bool TryGetNeurons(RegionModel fromRegion, Synapse synapse, RegionModel toRegion,
            out ExpertModel fromNeuron, out ExpertModel toNeuron)
        {
            fromNeuron = fromRegion.Experts[synapse.From.Neuron];
            toNeuron = toRegion.Experts[synapse.To.Neuron];

            if (fromNeuron != null && toNeuron != null)
                return true;

            string missingNeuron = fromNeuron == null ? "Source" : "Target";
            LogSynapseNotProcessed(synapse, "add", $"{missingNeuron} neuron not found");
            return false;
        }

        private bool TryGetRegions(SimulationModel model, Synapse synapse, out RegionModel fromRegion,
            out RegionModel toRegion)
        {
            fromRegion = model.Regions[synapse.From.Region];
            toRegion = model.Regions[synapse.From.Region];

            if (fromRegion != null && toRegion != null)
                return true;

            string missingRegion = fromRegion == null ? "Source" : "Target";
            LogSynapseNotProcessed(synapse, "add", $"{missingRegion} region not found");
            return false;
        }

        private bool CheckSameRegion(Synapse synapse)
        {
            if (synapse.From.Region == synapse.To.Region)
                return true;

            Log.Debug("Synapses crossing regions are not supported by visualization yet");
            return false;
        }

        private void LogSynapseNotProcessed(Synapse addedSynapse, string synapseAction, string reason)
        {
            Log.Warn(
                "Could not {synapseAction:l} synapse from source region {fromRegion}, neuron {fromNeuron} to target region {toRegion}, neuron {toNeuron}: {reason}",
                synapseAction,
                addedSynapse.From.Region,
                addedSynapse.From.Neuron,
                addedSynapse.To.Region,
                addedSynapse.To.Neuron,
                reason);
        }
    }
}
