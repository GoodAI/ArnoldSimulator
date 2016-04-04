using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Project
{
    public class Connection
    {
        public Node From { get; }
        public Node To { get; }

        public int FromIndex { get; }
        public int ToIndex { get; }

        public Connection(Node from, int fromIndex, Node to, int toIndex)
        {
            From = from;
            To = to;
            FromIndex = fromIndex;
            ToIndex = toIndex;
        }

        public void Connect()
        {
            From.OutputPorts[FromIndex].Add(this);
            To.InputPorts[ToIndex] = this;
        }

        public void Disconnect()
        {
            if (FromIndex < From.OutputPortCount)
                From.OutputPorts[FromIndex].Remove(this);

            if (ToIndex < To.InputPortCount)
            {
                Connection toConnection = To.InputPorts[ToIndex];

                if (toConnection == this)
                    To.InputPorts[ToIndex] = null;
            }
        }
    }
}
