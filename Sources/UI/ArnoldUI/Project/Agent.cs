using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Project
{
    public class AgentBlueprint
    {
        private int MaxId { get; set; }

        public Body Body { get; }
        public Brain Brain { get; }

        public AgentBlueprint()
        {
            // TODO: DI
            Body = Body.DefaultBody;
            Brain = new Brain(this);
        }
    }
}
