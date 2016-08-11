using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Project
{
    public sealed class BlueprintChangedArgs : EventArgs
    {
        public string Blueprint { get; }
        public bool ChangesMade { get; }

        public BlueprintChangedArgs(string blueprint, bool changesMade)
        {
            Blueprint = blueprint;
            ChangesMade = changesMade;
        }
    }

    public interface IDesigner
    {
        string Blueprint { get; }
        void SetBlueprint(string blueprint, bool reset = false);
        event EventHandler<BlueprintChangedArgs> BlueprintChanged;
    }
    
    // TODO(HonzaS): This will manage everything around the creation and editing of a blueprint.
    public sealed class Designer : IDesigner
    {
        public event EventHandler<BlueprintChangedArgs> BlueprintChanged;

        private string m_blueprint;

        public string Blueprint => m_blueprint;

        /// <summary>
        /// Sets new blueprint contents. If reset is true, the contents are "clean" and need not be saved.
        /// Otherwise it is considered an editor change, contents are "dirty" and need to be saved.
        /// </summary>
        /// <param name="blueprint"></param>
        /// <param name="reset"></param>
        public void SetBlueprint(string blueprint, bool reset = false)
        {
            m_blueprint = blueprint;
            BlueprintChanged?.Invoke(this, new BlueprintChangedArgs(m_blueprint, changesMade: !reset));
        }
    }
}
