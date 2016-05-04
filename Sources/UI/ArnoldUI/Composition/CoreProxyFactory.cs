using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Network;
using SimpleInjector;

namespace GoodAI.Arnold
{
    public interface ICoreProxyFactory
    {
        ICoreProxy Create(ICoreLink coreLink, ICoreController controller);
    }

    public class CoreProxyFactory : PropertyInjectingFactory, ICoreProxyFactory
    {
        public CoreProxyFactory(Container container) : base(container)
        { }

        public ICoreProxy Create(ICoreLink coreLink, ICoreController controller)
        {
            return InjectProperties(new CoreProxy(coreLink, controller));
        }
    }
}
