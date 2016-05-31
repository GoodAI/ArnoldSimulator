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

                RegionModel targetRegionModel = model.Regions[neuron.RegionIndex];
                if (targetRegionModel == null)
                {
                    Log.Warn("Cannot add neuron {neuronId}, region with index {regionIndex} was not found", neuron.Id,
                        neuron.RegionIndex);
                    continue;
                }

                targetRegionModel.AddExpert(new ExpertModel(neuron.Id, neuron.Type, targetRegionModel, neuron.Position.ToVector3()));
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
                    LogConnectionNotAdded(addedConnection, $"{missingRegion} region not found");
                    continue;
                }

                OutputConnectorModel fromConnector =
                    fromRegion.OutputConnectors.FirstOrDefault(connector => connector.Name == addedConnection.FromConnector);

                InputConnectorModel toConnector =
                    toRegion.InputConnectors.FirstOrDefault(connector => connector.Name == addedConnection.ToConnector);


                if (fromConnector == null || toConnector == null)
                {
                    string missingConnector = fromConnector == null ? "Source" : "Target";
                    LogConnectionNotAdded(addedConnection, $"{missingConnector} connector not found");
                    continue;
                }

                var connectionModel = new ConnectionModel(fromConnector, toConnector);

                model.Connections.AddChild(connectionModel);
            }
        }

        private void LogConnectionNotAdded(Connection addedConnection, string reason)
        {
            Log.Warn(
                "Could not add connection from region {fromRegion}, connector {fromConnector} to region {toRegion}, connector {toConnector}: {reason}",
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
                    fromNeuron.Outputs[synapseModel.ToNeuron.Id] = synapseModel;
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
                    SynapseModel synapseModel = fromNeuron.Outputs[toNeuron.Id];

                    if (synapseModel == null)
                    {
                        LogSynapseNotProcessed(spikedSynapse, "spike", "Synapse not found");
                        return;
                    }

                    synapseModel.Spike();
                });
            }
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
            fromNeuron = fromRegion.Experts[synapse.FromNeuron];
            toNeuron = toRegion.Experts[synapse.ToNeuron];

            if (fromNeuron != null && toNeuron != null)
                return true;

            string missingNeuron = fromNeuron == null ? "Source" : "Target";
            LogSynapseNotProcessed(synapse, "add", $"{missingNeuron} neuron not found");
            return false;
        }

        private bool TryGetRegions(SimulationModel model, Synapse synapse, out RegionModel fromRegion,
            out RegionModel toRegion)
        {
            fromRegion = model.Regions[synapse.FromRegion];
            toRegion = model.Regions[synapse.FromRegion];
            if (fromRegion != null && toRegion != null)
                return true;

            string missingRegion = fromRegion == null ? "Source" : "Target";
            LogSynapseNotProcessed(synapse, "add", $"{missingRegion} region not found");
            return false;
        }

        private bool CheckSameRegion(Synapse synapse)
        {
            if (synapse.FromRegion == synapse.ToRegion)
                return true;

            Log.Debug("Synapses crossing regions are not supported by visualization yet");
            return false;
        }

        private void LogSynapseNotProcessed(Synapse addedSynapse, string synapseAction, string reason)
        {
            Log.Warn(
                "Could not {synapseAction:l} synapse from source region {fromRegion}, neuron {fromNeuron} to target region {toRegion}, neuron {toNeuron}: {reason}",
                synapseAction,
                addedSynapse.FromRegion,
                addedSynapse.FromNeuron,
                addedSynapse.ToRegion,
                addedSynapse.ToNeuron,
                reason);
        }
    }
}
