using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Core
{
    public class CoreConfiguration
    {
        public string SystemConfigString => System.ToJsonString();

        public readonly SystemConfiguration System;

        public CoreConfiguration(SystemConfiguration systemConfig)
        {
            System = systemConfig;
        }

    }
}
