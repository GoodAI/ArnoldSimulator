using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;

namespace GoodAI.Arnold.Network
{
    public interface IModelProviderFactory
    {
        IModelProvider Create(IConductor conductor);
    }

    public class ModelProviderFactory : IModelProviderFactory
    {
        public IModelProvider Create(IConductor conductor)
        {
            return new ModelProvider(conductor);
        }
    }
}
