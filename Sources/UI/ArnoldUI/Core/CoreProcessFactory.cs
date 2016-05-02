using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Core
{
    public interface ICoreProcessFactory
    {
        ICoreProcess Create();
    }

    public class CoreProcessFactory : ICoreProcessFactory
    {
        public ICoreProcess Create()
        {
            return new CoreProcess();
        }
    }
}
