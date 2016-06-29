using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Communication
{
    public class CoreConfiguration
    {
        public string SystemConfiguration { get; set; }

        public CoreConfiguration(string systemConfiguration)
        {
            SystemConfiguration = systemConfiguration;
        }
    }
}
