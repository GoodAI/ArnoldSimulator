using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleInjector;

namespace GoodAI.Arnold.Network
{
    public interface ICoreControllerFactory
    {
        ICoreController Create(ICoreLink coreLink);
    }

    public class CoreControllerFactory : ICoreControllerFactory
    {
        private readonly Registration m_logRegistration;

        public CoreControllerFactory(Container container)
        {
            m_logRegistration = Lifestyle.Transient.CreateRegistration<CoreController>(container);
        }

        public ICoreController Create(ICoreLink coreLink)
        {
            var controller = new CoreController(coreLink);
            m_logRegistration.InitializeInstance(controller);

            return controller;
        }
    }
}
