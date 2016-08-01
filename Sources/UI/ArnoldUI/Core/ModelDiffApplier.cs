using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Communication;
using GoodAI.Arnold.Extensions;
using GoodAI.Arnold.Observation;
using GoodAI.Arnold.Visualization.Models;
using GoodAI.Logging;
using GoodAI.Net.ConverseSharpFlatBuffers;
using OpenTK;

namespace GoodAI.Arnold.Core
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
            ApplyRepositionedRegions(model, diff);
            ApplyRemovedRegions(model, diff);

            ApplyAddedConnectors(model, diff);
            ApplyRemovedConnectors(model, diff);

            ApplyAddedConnections(model, diff);
            ApplyRemovedConnections(model, diff);

            ApplyAddedNeurons(model, diff);
            ApplyRepositionedNeurons(model, diff);
            ApplyRemovedNeurons(model, diff);

            ApplyAddedSynapses(model, diff);
            ApplySpikedSynapses(model, diff);
            ApplyRemovedSynapses(model, diff);

            ApplyObservers(model, diff);
        }

        private static void ApplyAddedRegions(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.AddedRegionsLength; i++)
            {
                Region addedRegion = diff.GetAddedRegions(i);

                Vector3 lower = addedRegion.Position.ToVector3();
                Vector3 size = addedRegion.Size.ToVector3();

                Vector3 position = lower + size/2;

                model.Regions[addedRegion.Index] = new RegionModel(addedRegion.Index, addedRegion.Name, addedRegion.Type,
                    position, size);
            }
        }

        private void ApplyRepositionedRegions(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.RepositionedRegionsLength; i++)
            {
                Region repositionedRegion = diff.GetRepositionedRegions(i);

                RegionModel regionModel;
                if (!model.Regions.TryGetModel(repositionedRegion.Index, out regionModel))
                {
                    Log.Warn("Could not reposition region {region}, not found", repositionedRegion.Index);
                    continue;
                }

                regionModel.Position = repositionedRegion.Position.ToVector3();
                regionModel.Size = repositionedRegion.Size.ToVector3();
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

        private void ApplyAddedConnectors(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.AddedConnectorsLength; i++)
            {
                Connector addedConnector = diff.GetAddedConnectors(i);

                ProcessConnector(model, addedConnector.RegionIndex, addedConnector.Name, "add", regionModel =>
                {
                    // TODO(HonzaS): A Shortcut for the creation of models?
                    // Replace them with factory + inject logger.
                    if (addedConnector.Direction == Direction.Forward)
                        regionModel.OutputConnectors.AddChild(new OutputConnectorModel(regionModel, addedConnector.Name, addedConnector.Size));
                    else
                        regionModel.InputConnectors.AddChild(new InputConnectorModel(regionModel, addedConnector.Name, addedConnector.Size));
                });
            }
        }

        private void ApplyRemovedConnectors(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.RemovedConnectorsLength; i++)
            {
                ConnectorRemoval removedConnector = diff.GetRemovedConnectors(i);

                ProcessConnector(model, removedConnector.RegionIndex, removedConnector.Name, "remove", regionModel =>
                {
                    var deleted = removedConnector.Direction == Direction.Backward
                        ? regionModel.InputConnectors.Remove(removedConnector.Name)
                        : regionModel.OutputConnectors.Remove(removedConnector.Name);

                    if (!deleted)
                        Log.Warn("Cannot remove connector {connector} from region {region}, connector not found",
                            removedConnector.Name, regionModel.Index);
                });
            }
        }

        private void ProcessConnector(SimulationModel model, uint regionIndex, string connectorName, string actionName, Action<RegionModel> action)
        {
            RegionModel targetRegionModel = model.Regions[regionIndex];
            if (targetRegionModel == null)
            {
                Log.Warn(
                    "Cannot " + actionName + " connector {connectorName}, region with index {regionIndex} was not found",
                    connectorName,
                    regionIndex);
                return;
            }

            action(targetRegionModel);
        }

        private void ApplyAddedConnections(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.AddedConnectionsLength; i++)
            {
                Connection addedConnection = diff.GetAddedConnections(i);

                ProcessConnection(model, addedConnection, "add", (fromConnector, toConnector) =>
                {
                    var connectionModel = new ConnectionModel(fromConnector, toConnector);

                    model.Connections.AddChild(connectionModel);
                });
            }
        }

        private void ApplyRemovedConnections(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.RemovedConnectionsLength; i++)
            {
                Connection removedConnection = diff.GetRemovedConnections(i);

                ProcessConnection(model, removedConnection, "remove", (fromConnector, toConnector) =>
                {
                    // TODO(HonzaS): Optimize lookup.
                    var connectionModel =
                        model.Connections.FirstOrDefault(
                            connection => connection.From == fromConnector && connection.To == toConnector);

                    if (connectionModel == null)
                    {
                        LogConnectionNotProcessed(removedConnection, "remove", "Connection not found");
                        return;
                    }

                    connectionModel.Disconnect();
                    model.Connections.Remove(connectionModel);
                });
            }
        }

        private void ProcessConnection(SimulationModel model, Connection connection, string actionName, Action<OutputConnectorModel, InputConnectorModel> action)
        {
            // TODO(HonzaS): These two mean connections to the sensors/actuators, add support for those
            if (connection.FromRegion == 0 || connection.ToRegion == 0)
                return;

            RegionModel fromRegion = model.Regions[connection.FromRegion];
            RegionModel toRegion = model.Regions[connection.ToRegion];

            if (fromRegion == null || toRegion == null)
            {
                string missingRegion = fromRegion == null ? "Source" : "Target";
                LogConnectionNotProcessed(connection, actionName, $"{missingRegion} region not found");
                return;
            }

            OutputConnectorModel fromConnector =
                fromRegion.OutputConnectors.FirstOrDefault(connector => connector.Name == connection.FromConnector);

            InputConnectorModel toConnector =
                toRegion.InputConnectors.FirstOrDefault(connector => connector.Name == connection.ToConnector);

            if (fromConnector == null || toConnector == null)
            {
                string missingConnector = fromConnector == null ? "Source" : "Target";
                LogConnectionNotProcessed(connection, actionName, $"{missingConnector} connector not found");
                return;
            }

            action(fromConnector, toConnector);
        }

        private void LogConnectionNotProcessed(Connection connection, string action, string reason)
        {
            Log.Warn(
                "Could not " + action +
                " connection from region {fromRegion}, connector {fromConnector} to region {toRegion}, connector {toConnector}: {reason}",
                connection.FromRegion,
                connection.FromConnector,
                connection.ToRegion,
                connection.ToConnector,
                reason);
        }

        private void ApplyAddedNeurons(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.AddedNeuronsLength; i++)
            {
                Neuron neuron = diff.GetAddedNeurons(i);
                var neuronId = neuron.Id;

                RegionModel region;
                if (!model.Regions.TryGetModel(neuronId.Region, out region))
                {
                    LogNeuronNotProcessed(neuronId, "add", "Region not found");
                    continue;
                }

                if (region.Neurons.ContainsKey(neuronId.Neuron))
                {
                    LogNeuronNotProcessed(neuronId, "add", "Neuron with this id is already present");
                    continue;
                }

                region.AddNeuron(new NeuronModel(neuron.Id.Neuron, neuron.Type, region, neuron.Position.ToVector3()));
            }
        }

        private void ApplyRepositionedNeurons(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.RepositionedNeuronsLength; i++)
            {
                Neuron neuron = diff.GetRepositionedNeurons(i);
                var neuronId = neuron.Id;

                RegionModel region;
                if (!model.Regions.TryGetModel(neuronId.Region, out region))
                {
                    LogNeuronNotProcessed(neuronId, "reposition", "Region not found");
                    continue;
                }

                NeuronModel neuronModel;
                if (!region.Neurons.TryGetModel(neuronId.Neuron, out neuronModel))
                {
                    LogNeuronNotProcessed(neuronId, "reposition", "Neuron not found");
                    continue;
                }

                neuronModel.Position = neuron.Position.ToVector3();
                foreach (SynapseModel synapse in neuronModel.Outputs.Values)
                    synapse.UpdatePosition();
                foreach (SynapseModel synapse in neuronModel.Inputs.Values)
                    synapse.UpdatePosition();
            }
        }

        private void ApplyRemovedNeurons(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.RemovedNeuronsLength; i++)
            {
                NeuronId neuronId = diff.GetRemovedNeurons(i);

                RegionModel region;
                if (!model.Regions.TryGetModel(neuronId.Region, out region))
                {
                    LogNeuronNotProcessed(neuronId, "remove", "Region not found");
                    continue;
                }

                if (!region.Neurons.ContainsKey(neuronId.Neuron))
                {
                    LogNeuronNotProcessed(neuronId, "remove", "Neuron not found");
                    continue;
                }

                region.Neurons.Remove(neuronId.Neuron);
            }
        }

        private void LogNeuronNotProcessed(NeuronId neuronId, string action, string reason)
        {
            Log.Warn(
                "Cannot " + action + " neuron with id {neuronIndex} in region {regionIndex}: {reason}",
                neuronId.Neuron, neuronId.Region, reason);
        }

        private void ApplyAddedSynapses(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.AddedSynapsesLength; i++)
            {
                Synapse addedSynapse = diff.GetAddedSynapses(i);
                ProcessSynapse(model, addedSynapse, "add", (fromRegion, fromNeuron, toRegion, toNeuron) =>
                {
                    var synapseModel = new SynapseModel(fromRegion, fromNeuron, toRegion, toNeuron);
                    fromNeuron.Outputs[synapseModel.ToNeuron.Index] = synapseModel;
                    fromRegion.AddSynapse(synapseModel);
                    toNeuron.Inputs[synapseModel.FromNeuron.Index] = synapseModel;
                });
            }
        }

        private void ApplyRemovedSynapses(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.RemovedSynapsesLength; i++)
            {
                var synapse = diff.GetRemovedSynapses(i);

                ProcessSynapse(model, synapse, "remove", (fromRegion, fromNeuron, toRegion, toNeuron) =>
                {
                    var synapseModel = fromNeuron.Outputs[toNeuron.Index];
                    fromNeuron.Outputs.Remove(toNeuron.Index);
                    fromRegion.Synapses.Remove(synapseModel);
                    toNeuron.Inputs.Remove(fromNeuron.Index);
                });
            }
        }

        private void ApplySpikedSynapses(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.SpikedSynapsesLength; i++)
            {
                Synapse spikedSynapse = diff.GetSpikedSynapses(i);
                ProcessSynapse(model, spikedSynapse, "spike", (fromRegion, fromNeuron, toRegion, toNeuron) =>
                {
                    SynapseModel synapseModel;
                    if (!fromNeuron.Outputs.TryGetValue(toNeuron.Index, out synapseModel))
                    {
                        // Do not log this, it happens a lot and is expected.
                        // The core sends a lot of synapses that lead to neurons which are not displayed.
                        //LogSynapseNotProcessed(spikedSynapse, "spike", "Synapse not found");
                        return;
                    }

                    synapseModel.Spike();
                });
            }
        }

        private void ProcessSynapse(SimulationModel model, Synapse synapse, string actionName, Action<RegionModel, NeuronModel, RegionModel, NeuronModel> action)
        {
            if (!CheckSameRegion(synapse))
                return;

            RegionModel fromRegion;
            RegionModel toRegion;
            if (!model.Regions.TryGetModel(synapse.From.Region, out fromRegion) ||
                !model.Regions.TryGetModel(synapse.To.Region, out toRegion))
            {
                string missingRegion = fromRegion == null ? "Source" : "Target";
                LogSynapseNotProcessed(synapse, actionName, $"{missingRegion} region not found");
                return;
            }

            NeuronModel fromNeuron;
            NeuronModel toNeuron;
            // These are synapses that outside of the region of interest, they are skipped.
            if (!fromRegion.Neurons.TryGetModel(synapse.From.Neuron, out fromNeuron) ||
                !toRegion.Neurons.TryGetModel(synapse.To.Neuron, out toNeuron))
                return;

            action(fromRegion, fromNeuron, toRegion, toNeuron);
        }

        private bool CheckSameRegion(Synapse synapse)
        {
            if (synapse.From.Region == synapse.To.Region)
                return true;

            //Log.Debug("Synapses crossing regions are not supported by visualization yet");
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

        private void ApplyObservers(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.ObserverResultsLength; i++)
            {
                ObserverResult observerData = diff.GetObserverResults(i);

                NeuronId neuronId = observerData.Observer.NeuronId;
                var definition = new ObserverDefinition(neuronId.Neuron, neuronId.Region, observerData.Observer.Type);

                model.Observers[definition] = new ObserverData(
                    CopyObserverArrayData(observerData.MetadataLength, (n => observerData.GetMetadata(n))),
                    observerData.GetPlainDataBytes()?.ToArray(),
                    CopyObserverArrayData(observerData.FloatDataLength, (n => observerData.GetFloatData(n))));
            }
        }

        private static TNumber[] CopyObserverArrayData<TNumber>(int count, Func<int, TNumber> getItem)
            where TNumber : struct
        {
            if (count <= 0)
                return null;

            var numberArray = new TNumber[count];

            for (var n = 0; n < count; n++)
            {
                numberArray[n] = getItem(n);
            }

            return numberArray;
        }
    }
}
