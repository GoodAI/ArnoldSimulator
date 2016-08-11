using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Project
{
    public class BlueprintChangedArgs : EventArgs
    {
        public string Blueprint { get; }

        public BlueprintChangedArgs(string blueprint)
        {
            Blueprint = blueprint;
        }
    }

    public interface IDesigner
    {
        string Blueprint { get; set; }
        event EventHandler<BlueprintChangedArgs> BlueprintChanged;
    }
    
    // TODO(HonzaS): This will manage everything around the creation and editing of a blueprint.
    public class Designer : IDesigner
    {
        public event EventHandler<BlueprintChangedArgs> BlueprintChanged;

        private string m_blueprint;

        public string Blueprint
        {
            get { return m_blueprint; }
            set
            {
                m_blueprint = value;
                BlueprintChanged?.Invoke(this, new BlueprintChangedArgs(m_blueprint));
            }
        }
    }
}
