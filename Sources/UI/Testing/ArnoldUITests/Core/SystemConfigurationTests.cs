using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;
using Newtonsoft.Json.Linq;
using Xunit;

namespace GoodAI.Arnold.UI.Tests.Core
{
    public class SystemConfigurationTests
    {
        [Fact]
        public void ConvertsToJsonAsExpected()
        {
            var systemConfig = new SystemConfiguration()
            {
                BrainStepsPerBodyStep = 7,
                RegularCheckpointingEnabled = true
            };

            var configString = systemConfig.ToJsonString();

            var jObject = JObject.Parse(configString);

            Assert.Equal(systemConfig.BrainStepsPerBodyStep, (int)jObject["BrainStepsPerBodyStep"]);
            Assert.Equal(systemConfig.RegularCheckpointingEnabled, (bool)jObject["RegularCheckpointingEnabled"]);
        }
    }
}
