using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Project
{
    public class Node
    {
        public string Name { get; set; }
        public PointF Location { get; set; }

        public HashSet<Connection>[] OutputPorts { get; private set; }
        public Connection[] InputPorts { get; private set; }

        public event EventHandler Updated;

        public int InputPortCount
        {
            get { return InputPorts?.Length ?? 0; }
            set
            {
                Connection[] oldInputs = InputPorts;

                InputPorts = new Connection[value];

                if (oldInputs == null)
                    return;

                CopyConnections(InputPorts, oldInputs);
            }
        }

        public int OutputPortCount
        {
            get { return OutputPorts?.Length ?? 0; }
            set
            {
                HashSet<Connection>[] oldOutputs = OutputPorts;

                OutputPorts = new HashSet<Connection>[value];
                for (int i = 0; i < value; i++)
                    OutputPorts[i] = new HashSet<Connection>();

                if (oldOutputs == null)
                    return;

                CopyConnections(OutputPorts, oldOutputs);
            }
        }

        private void CopyConnections<TValue>(IList<TValue> newCollection, IList<TValue> oldCollection)
        {
            for (int i = 0; i < Math.Min(oldCollection.Count, newCollection.Count); i++)
                newCollection[i] = oldCollection[i];

            OnUpdated();
        }

        public Node()
        {
            InputPortCount = 0;
            OutputPortCount = 0;
        }

        public void OnUpdated()
        {
            Updated?.Invoke(this, EventArgs.Empty);
        }
    }
}
