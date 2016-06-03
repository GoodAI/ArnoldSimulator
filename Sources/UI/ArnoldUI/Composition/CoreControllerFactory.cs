using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Communication;
using GoodAI.Arnold.Core;
using SimpleInjector;

namespace GoodAI.Arnold
{
    public interface ICoreControllerFactory
    {
        ICoreController Create(ICoreLink coreLink);
    }

    public class CoreControllerFactory : PropertyInjectingFactory, ICoreControllerFactory
    {
        public CoreControllerFactory(Container container) : base(container)
        {
        }

        public ICoreController Create(ICoreLink coreLink)
        {
            return InjectProperties(new CoreController(coreLink));
        }
    }
}
