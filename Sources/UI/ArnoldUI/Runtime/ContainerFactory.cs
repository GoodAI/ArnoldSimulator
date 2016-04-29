using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleInjector;

namespace GoodAI.Arnold
{
    internal static class ContainerFactory
    {
        public static Container Create()
        {
            var container = new Container();

            new ArnoldContainerConfig().Configure(container);

            return container;
        }
    }
}
