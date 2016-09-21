using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GoodAI.Arnold.Core;
using WeifenLuo.WinFormsUI.Docking;

namespace GoodAI.Arnold.Forms
{
    public partial class SettingsForm : DockContent
    {
        public CoreProcessParameters CoreProcessParameters => new CoreProcessParameters(
            workingDirectory: coreProcessDirectoryTextBox.Text,
            arguments: CoreProcessArgumentsTextBox.Text);

        public SettingsForm()
        {
            InitializeComponent();
        }
    }
}
