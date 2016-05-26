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

                RegionModel region = model.Regions[addedSynapse.RegionIndex];
                if (region == null)
                {
                    LogSynapseNotProcessed(addedSynapse, "add", "Region not found");
                    continue;
                }

                ExpertModel fromNeuron = region.Experts[addedSynapse.From];
                ExpertModel toNeuron = region.Experts[addedSynapse.To];

                if (fromNeuron == null || toNeuron == null)
                {
                    LogSynapseNotProcessed(addedSynapse, "add", "Source or target neuron not found");
                    continue;
                }

                var synapse = new SynapseModel(region, fromNeuron, toNeuron);
                fromNeuron.Outputs.Add(synapse);
                region.AddSynapse(synapse);
            }
        }

        private void ApplySpikedSynapses(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.SpikedSynapsesLength; i++)
            {
                Synapse spikedSynapse = diff.GetSpikedSynapses(i);

                RegionModel region = model.Regions[spikedSynapse.RegionIndex];
                if (region == null)
                {
                    LogSynapseNotProcessed(spikedSynapse, "spike", "Region not found");
                    continue;
                }

                ExpertModel fromNeuron = region.Experts[spikedSynapse.From];
                ExpertModel toNeuron = region.Experts[spikedSynapse.To];

                if (fromNeuron == null || toNeuron == null)
                {
                    LogSynapseNotProcessed(spikedSynapse, "spike", "Source or target neuron not found");
                    continue;
                }

                SynapseModel synapseModel = fromNeuron.Outputs.FirstOrDefault(synapse => synapse.To == toNeuron);

                if (synapseModel == null)
                {
                    LogSynapseNotProcessed(spikedSynapse, "spike", "Synapse not found");
                    continue;
                }

                synapseModel.Spike();
            }
        }

        private void LogSynapseNotProcessed(Synapse addedSynapse, string synapseAction, string reason)
        {
            Log.Warn(
                "Could not {synapseAction:l} synapse in region {regionIndex} from neuron {fromNeuron} to neuron {toNeuron}: {reason}",
                synapseAction,
                addedSynapse.RegionIndex,
                addedSynapse.From,
                addedSynapse.To,
                reason);
        }
    }
}
