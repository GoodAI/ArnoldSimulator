using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleInjector;

namespace GoodAI.Arnold.Core
{
    public interface ICoreProcessFactory
    {
        ICoreProcess Create(CoreProcessParameters parameters);
    }

    public class CoreProcessFactory : PropertyInjectingFactory, ICoreProcessFactory
    {
        public CoreProcessFactory(Container container) : base(container) { }

        public ICoreProcess Create(CoreProcessParameters parameters)
        {
            return InjectProperties(new CoreProcess(parameters));
        }
    }
}
