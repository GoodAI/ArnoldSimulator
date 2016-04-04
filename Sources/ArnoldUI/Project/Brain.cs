using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Project
{
    public class Brain
    {
        public ISet<Region> Regions { get; private set; }

        public AgentBlueprint AgentBlueprint { get; }

        public ISet<Node> Sensors => AgentBlueprint.Body.Sensors;
        public ISet<Node> Actuators => AgentBlueprint.Body.Actuators;

        public Brain(AgentBlueprint agentBlueprint)
        {
            Regions = new HashSet<Region>();
            AgentBlueprint = agentBlueprint;
        }
    }
}
