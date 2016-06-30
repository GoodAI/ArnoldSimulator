using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Extensions;
using GoodAI.Arnold.Forms;
using GoodAI.Arnold.Visualization;

namespace GoodAI.Arnold.Observation
{
    public class ObserverHandle : IDisposable
    {
        private readonly Scene m_scene;
        public IObserver Observer { get; }
        public ObserverForm Form { get; }
        public ObserverDefinition Definition => Observer.Definition;

        public bool IsDisposing { get; private set; }

        public ObserverHandle(IObserver observer, ObserverForm form, Scene scene)
        {
            m_scene = scene;
            Observer = observer;
            Form = form;
        }

        public void Dispose()
        {
            if (IsDisposing)
                return;

            IsDisposing = true;

            Form.CloseOnce();
            Observer.Dispose();
            m_scene.DeselectNeuron(Definition.RegionIndex, Definition.NeuronIndex);
        }
    }
}
