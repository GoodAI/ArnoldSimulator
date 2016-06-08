using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Project
{
    public interface IDesigner
    {
        void OnTextChanged(object sender, EventArgs e);
        string Blueprint { get; set; }
    }

    // TODO(HonzaS): This will manage everything around the creation and editing of a blueprint.
    public class Designer : IDesigner
    {
        public string Blueprint { get; set; }

        public void OnTextChanged(object sender, EventArgs e)
        { }
    }
}
