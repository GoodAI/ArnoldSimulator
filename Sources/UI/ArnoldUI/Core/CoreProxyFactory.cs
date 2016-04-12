using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArnoldUI.Core
{
    public interface ICoreProxyFactory
    {
        ICoreProxy Create(EndPoint endPoint);
    }

    public class CoreProxyFactory : ICoreProxyFactory
    {
        public ICoreProxy Create(EndPoint endPoint)
        {
            if (endPoint == null)
                return new LocalCoreProxy();
            else
                return new RemoteCoreProxy(endPoint);
        }
    }
}
