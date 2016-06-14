using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Forms;

namespace GoodAI.Arnold.Observation
{
    public class ObserverHandle : IDisposable
    {
        public IObserver Observer { get; }
        public ObserverForm Form { get; }
        public ObserverDefinition Definition => Observer.Definition;

        public ObserverHandle(IObserver observer, ObserverForm form)
        {
            Observer = observer;
            Form = form;
        }

        public void Dispose()
        {
            Form.Close();
            Observer.Dispose();
        }
    }
}
