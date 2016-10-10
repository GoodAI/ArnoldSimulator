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
        ICoreProcess Create(CoreProcessParams parameters);
    }

    public class CoreProcessFactory : PropertyInjectingFactory, ICoreProcessFactory
    {
        public CoreProcessFactory(Container container) : base(container) { }

        public ICoreProcess Create(CoreProcessParams parameters)
        {
            return InjectProperties(new CoreProcess(parameters));
        }
    }
}
