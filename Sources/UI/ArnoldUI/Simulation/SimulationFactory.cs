using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Simulation;

namespace ArnoldUI.Simulation
{
    public interface ISimulationFactory
    {
        ISimulation Create(ICoreLink coreLink);
    }

    public class SimulationFactory : ISimulationFactory
    {
        public ISimulation Create(ICoreLink coreLink)
        {
            return new SimulationProxy(coreLink);
        }
    }
}
