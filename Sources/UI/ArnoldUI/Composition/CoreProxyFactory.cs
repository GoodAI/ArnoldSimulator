using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;
using SimpleInjector;

namespace GoodAI.Arnold
{
    public interface ICoreProxyFactory
    {
        ICoreProxy Create(ICoreController controller, IModelUpdater modelUpdater);
    }

    public class CoreProxyFactory : PropertyInjectingFactory, ICoreProxyFactory
    {
        public CoreProxyFactory(Container container) : base(container)
        { }

        public ICoreProxy Create(ICoreController controller, IModelUpdater modelUpdater)
        {
            return InjectProperties(new CoreProxy(controller, modelUpdater));
        }
    }
}
