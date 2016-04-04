using System;
using System.Collections.Generic;
using System.Linq;
using GoodAI.Arnold.Graphics;
using GoodAI.Arnold.Project;
using OpenTK;
using OpenTK.Platform.Windows;

namespace GoodAI.Arnold.Simulation
{
    public class BrainSimulation : IDisposable
    {
        private const int LayerCount = 6;

        private const int LayerWidth = 12;
        private const int LayerHeight = 12;

        private const int LayerSpacing = 5;

        private const float SynapseProbability = 0.1f;

        private const int ResizeFactor = 10;

        private AgentBlueprint AgentBlueprint { get; }

        public List<RegionModel> Regions { get; }

        public BrainSimulation(AgentBlueprint agentBlueprint)
        {
            Regions = new List<RegionModel>();
            AgentBlueprint = agentBlueprint;

            GenerateExperts();
        }

        private void GenerateExperts()
        {
            foreach (Region region in AgentBlueprint.Brain.Regions)
            {
                RegionModel newSimulationRegion;

                newSimulationRegion = new RegionModel(new Vector3((region.Location.X+100)/ResizeFactor, 0, (region.Location.Y+100)/ResizeFactor));
                GenerateExperts(newSimulationRegion);

                Regions.Add(newSimulationRegion);

                newSimulationRegion = new RegionModel(new Vector3((region.Location.X+300)/ResizeFactor, 0, (region.Location.Y+600)/ResizeFactor));
                GenerateExperts(newSimulationRegion);

                Regions.Add(newSimulationRegion);

                newSimulationRegion = new RegionModel(new Vector3((region.Location.X+500)/ResizeFactor, 0, (region.Location.Y-600)/ResizeFactor));
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

        public void Step()
        {
        }

        public void Dispose()
        {
        }
    }
}