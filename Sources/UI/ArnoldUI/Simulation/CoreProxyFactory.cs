using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Network;

namespace GoodAI.Arnold.Simulation
{
    public interface ICoreProxyFactory
    {
        ICoreProxy Create(ICoreLink coreLink, ICoreController controller);
    }

    public class CoreProxyFactory : ICoreProxyFactory
    {
        public ICoreProxy Create(ICoreLink coreLink, ICoreController controller)
        {
            return new CoreProxy(coreLink, controller);
        }
    }
}
