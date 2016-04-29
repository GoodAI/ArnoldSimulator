using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArnoldUI;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Simulation;
using GoodAI.Logging;
using GoodAI.TypeMapping;
using SimpleInjector;

namespace GoodAI.Arnold
{
    internal class ArnoldContainerConfig : IContainerConfiguration
    {
        public void Configure(Container container)
        {
            container.Options.PropertySelectionBehavior = new PropertyInjectionForType<ILog>(container);

            container.RegisterSingleton(() => LoggingConfig.Setup());
            container.RegisterConditional(
                typeof(ILog),
                typeFactory => typeof(SerilogRobe<>).MakeGenericType(typeFactory.Consumer.ImplementationType),
                Lifestyle.Transient,
                predicateContext => predicateContext.Consumer != null);
            container.RegisterConditional(
                typeof(ILog),
                typeFactory => typeof(SerilogRobe),
                Lifestyle.Transient,
                predicateContext => predicateContext.Consumer == null);

            container.RegisterSingleton<ICoreProxyFactory, CoreProxyFactory>();
            container.RegisterSingleton<ICoreLinkFactory, CoreLinkFactory>();
            container.RegisterSingleton<ICoreControllerFactory, CoreControllerFactory>();
            container.RegisterSingleton<ISimulationFactory, SimulationFactory>();
            container.RegisterSingleton<IConductor, Conductor>();
            container.RegisterSingleton<UIMain>();
        }
    }
}
