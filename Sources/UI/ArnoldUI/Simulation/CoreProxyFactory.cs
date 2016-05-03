using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Network;
using GoodAI.Logging;
using SimpleInjector;

namespace GoodAI.Arnold.Simulation
{
    public interface ICoreProxyFactory
    {
        ICoreProxy Create(ICoreLink coreLink, ICoreController controller);
    }

    public class CoreProxyFactory : ICoreProxyFactory
    {
        private Registration m_logRegistration;

        public CoreProxyFactory(Container container)
        {
            InstanceProducer instanceProducer = container.GetRegistration(typeof(ILog), throwOnFailure: true);
            m_logRegistration = instanceProducer.Registration;
        }

        public ICoreProxy Create(ICoreLink coreLink, ICoreController controller)
        {
            var coreProxy = new CoreProxy(coreLink, controller);
            m_logRegistration.InitializeInstance(coreProxy);

            return coreProxy;
        }
    }
}
