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
        ICoreProcess Create();
    }

    public class CoreProcessFactory : PropertyInjectingFactory, ICoreProcessFactory
    {
        public CoreProcessFactory(Container container) : base(container)
        {
        }

        public ICoreProcess Create()
        {
            return InjectProperties(new CoreProcess());
        }
    }
}
