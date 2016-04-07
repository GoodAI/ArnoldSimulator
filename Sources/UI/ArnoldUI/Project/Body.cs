using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Project
{
    public class Body
    {
        public ISet<Node> Sensors { get; }
        public ISet<Node> Actuators { get; }

        public Body()
        {
            Sensors = new HashSet<Node>();
            Actuators = new HashSet<Node>();
        }

        public static Body DefaultBody
        {
            get
            {
                var body = new Body();
                body.Sensors.Add(new Node
                {
                    Name = "Visual",
                    OutputPortCount = 1,
                    Location = new PointF(10, 100)
                });
                body.Actuators.Add(new Node
                {
                    Name = "Actions",
                    InputPortCount = 1,
                    Location = new PointF(800, 100)
                });
                return body;
            }
        }
    }
}
