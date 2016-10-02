using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GoodAI.Arnold.Core
{
    public class SystemConfiguration
    {
        public int BrainStepsPerBodyStep { get; set; } = 10;

        public bool RegularCheckpointingEnabled { get; set; }

        public float CheckpointingIntervalSeconds { get; set; } = 10.0f;

        public bool LoadBalancingEnabled { get; set; } = true;

        public float LoadBalancingIntervalSeconds { get; set; } = 15.0f;

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
