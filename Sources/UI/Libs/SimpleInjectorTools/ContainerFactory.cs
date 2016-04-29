using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleInjector;

namespace GoodAI.TypeMapping
{
    public static class ContainerFactory
    {
        public static Container Create<TContainerConfiguration>()
            where TContainerConfiguration : IContainerConfiguration, new()
        {
            var container = new Container();

            new TContainerConfiguration().Configure(container);

            container.Verify();
            
            return container;
        }
    }
}
