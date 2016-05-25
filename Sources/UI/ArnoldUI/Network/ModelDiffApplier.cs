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

                model.Regions.AddChild(new RegionModel(addedRegion.Index, addedRegion.Name, addedRegion.Type, position, size));
            }
        }

        private void ApplyAddedNeurons(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.AddedNeuronsLength; i++)
            {
                Neuron neuron = diff.GetAddedNeurons(i);

                // TODO(HonzaS): Add a lookup table instead of this.
                var targetRegionModel = FindRegion(model, neuron.RegionIndex);
                if (targetRegionModel == null)
                {
                    Log.Warn("Cannot add neuron {neuronId}, region with index {regionIndex} was not found", neuron.Id,
                        neuron.RegionIndex);
                    continue;
                }

                targetRegionModel.AddExpert(new ExpertModel(neuron.Id, targetRegionModel, neuron.Position.ToVector3()));
            }
        }

        private void ApplyAddedConnectors(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.AddedConnectorsLength; i++)
            {
                Connector addedConnector = diff.GetAddedConnectors(i);

                var targetRegionModel = FindRegion(model, addedConnector.RegionIndex);
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
                    targetRegionModel.OutputConnectors.AddChild(new OutputConnectorModel(addedConnector.Name, (int) addedConnector.Size));
                else
                    targetRegionModel.InputConnectors.AddChild(new InputConnectorModel(addedConnector.Name, (int) addedConnector.Size));
            }
        }

        private static void ApplyAddedConnections(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.AddedConnectionsLength; i++)
            {
                Connection addedConnection = diff.GetAddedConnections(i);

                OutputConnectorModel fromConnector = FindRegion(model, addedConnection.FromRegion)?
                    .OutputConnectors.FirstOrDefault(connector => connector.Name == addedConnection.FromConnector);

                InputConnectorModel toConnector = FindRegion(model, addedConnection.ToRegion)?
                    .InputConnectors.FirstOrDefault(connector => connector.Name == addedConnection.ToConnector);

                var connectionModel = new ConnectionModel(fromConnector, toConnector);

                model.Connections.AddChild(connectionModel);
            }
        }

        private void ApplyAddedSynapses(SimulationModel model, ModelResponse diff)
        {
            for (int i = 0; i < diff.AddedSynapsesLength; i++)
            {
                Synapse addedSynapse = diff.GetAddedSynapses(i);

                RegionModel region = FindRegion(model, addedSynapse.RegionIndex);
                if (region == null)
                {
                    LogSynapseNotAdded(addedSynapse, "Region not found");
                    continue;
                }

                ExpertModel fromNeuron = FindNeuron(region, addedSynapse.From);
                ExpertModel toNeuron = FindNeuron(region, addedSynapse.To);

                if (fromNeuron == null || toNeuron == null)
                {
                    LogSynapseNotAdded(addedSynapse, "Source or target neuron not found");
                    continue;
                }

                var synapse = new SynapseModel(region, fromNeuron, toNeuron);
                fromNeuron.Outputs.Add(synapse);
                region.AddSynapse(synapse);
            }
        }

        private static ExpertModel FindNeuron(RegionModel region, uint id)
        {
            // TODO(HonzaS): Optimize.
            return region.Experts.FirstOrDefault(expert => expert.Id == id);
        }

        private void LogSynapseNotAdded(Synapse addedSynapse, string reason)
        {
            Log.Warn(
                "Could not add synapse in region {regionIndex} from neuron {fromNeuron} to neuron {toNeuron}: {reason}",
                addedSynapse.RegionIndex,
                addedSynapse.From,
                addedSynapse.To,
                reason);
        }

        private static RegionModel FindRegion(SimulationModel model, uint regionIndex)
        {
            // TODO(HonzaS): Faster lookup.
            return model.Regions.FirstOrDefault(region => region.Index == regionIndex);
        }
    }
}
