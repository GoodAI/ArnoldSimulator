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
    public interface IModelProviderFactory
    {
        IModelProvider Create(IConductor conductor);
    }

    public class ModelProviderFactory : PropertyInjectingFactory, IModelProviderFactory
    {
        public ModelProviderFactory(Container container) : base(container)
        {
        }

        public IModelProvider Create(IConductor conductor)
        {
            return InjectProperties(new ModelProvider(conductor));
        }
    }
}
