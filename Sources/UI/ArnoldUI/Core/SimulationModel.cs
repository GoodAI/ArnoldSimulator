using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using GoodAI.Arnold.Graphics;
using GoodAI.Arnold.Graphics.Models;
using GoodAI.Arnold.Project;
using OpenTK;
using OpenTK.Platform.Windows;

namespace GoodAI.Arnold.Core
{
    public class ExpertAddedEventArgs : EventArgs
    {
        public ExpertModel Expert { get; }

        public ExpertAddedEventArgs(ExpertModel expert)
        {
            Expert = expert;
        }
    }

    public interface ISimulationModel
    {
        event EventHandler<ExpertAddedEventArgs> ExpertAdded;
        IList<RegionModel> Regions { get; }
    }

    public class SimulationModel : ISimulationModel
    {
        private const int LayerCount = 6;

        private const int LayerWidth = 12;
        private const int LayerHeight = 12;

        private const int LayerSpacing = 5;

        private const float SynapseProbability = 0.1f;

        private const float ResizeFactor = 1f;

        // Register to events like this in the visualization so that we can do stuff like inject camera.
        public event EventHandler<ExpertAddedEventArgs> ExpertAdded;

        private AgentBlueprint AgentBlueprint { get; }

        public IList<RegionModel> Regions { get; }

        public SimulationModel()
        {
            Regions = new List<RegionModel>();
        }

        public SimulationModel(AgentBlueprint agentBlueprint) : this()
        {
            AgentBlueprint = agentBlueprint;

            GenerateRegions();
        }

        private void GenerateRegions()
        {
            // First pass to get the maximum boundaries
            var size = new Vector2();
            foreach (Region region in AgentBlueprint.Brain.Regions)
            {
                size.X = Math.Max(region.Location.X, size.X);
                size.Y = Math.Max(region.Location.Y, size.Y);
            }

            Vector2 halfSize = size/2;

            foreach (Region region in AgentBlueprint.Brain.Regions)
            {
                var newSimulationRegion =
                    new RegionModel(new Vector3((region.Location.X - halfSize.X)*ResizeFactor, 0,
                        (region.Location.Y - halfSize.Y)*ResizeFactor));
                GenerateExperts(newSimulationRegion);

                Regions.Add(newSimulationRegion);
            }
        }

        private void GenerateExperts(RegionModel simulationRegionModel)
        {
            const float lowerLimit = ExpertModel.CellSize/2;// + RegionModel.RegionMargin;

            var layers = new List<List<ExpertModel>>();

            for (int l = 0; l < LayerCount; l++)
            {
                var layer = new List<ExpertModel>();
                layers.Add(layer);

                var x = l*LayerSpacing;

                for (int z = 0; z < LayerWidth; z++)
                {
                    for (int y = 0; y < LayerHeight; y++)
                    {
                        var expert = new ExpertModel(
                            simulationRegionModel,
                            new Vector3(
                                lowerLimit + x*ExpertModel.CellSize,
                                lowerLimit + y*ExpertModel.CellSize,
                                lowerLimit + z*ExpertModel.CellSize));
                        layer.Add(expert);
                        simulationRegionModel.AddExpert(expert);
                    }
                }
            }

            var rand = new Random();

            for (int l = 0; l < LayerCount - 1; l++)
            {
                List<ExpertModel> sourceLayer = layers[l];
                List<ExpertModel> targetLayer = layers[l + 1];

                foreach (ExpertModel sourceExpert in sourceLayer)
                    foreach (ExpertModel targetExpert in targetLayer)
                        if (rand.NextDouble() < SynapseProbability)
                        {
                            var synapse = new SynapseModel(simulationRegionModel, sourceExpert, targetExpert);
                            sourceExpert.Outputs.Add(synapse);
                            simulationRegionModel.AddSynapse(synapse);
                        }
            }

            simulationRegionModel.AdjustSize();
        }
    }
}