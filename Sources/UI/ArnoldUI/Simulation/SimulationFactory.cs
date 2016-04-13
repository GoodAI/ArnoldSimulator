using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArnoldUI.Network;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Simulation;

namespace ArnoldUI.Simulation
{
    public interface ISimulationFactory
    {
        ISimulation Create(ICoreLink coreLink, ICoreController controller);
    }

    public class SimulationFactory : ISimulationFactory
    {
        public ISimulation Create(ICoreLink coreLink, ICoreController controller)
        {
            return new SimulationProxy(coreLink, controller);
        }
    }
}
