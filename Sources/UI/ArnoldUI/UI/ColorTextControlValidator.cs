using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GoodAI.Arnold.UI
{
    public class ColorTextControlValidator
    {
        public Color TextColor { get; set; } = SystemColors.WindowText;
        public Color ErrorColor { get; set; } = Color.DarkRed;

        /// <summary>
        /// Runs given validator on a text control and sets ForeColor of the control. Suppresses any exceptions.
        /// </summary>
        /// <param name="textControl">The type is dynamic because TextBox and ToolstripTextBox don't have a common ancestor</param>
        /// <param name="isValid">Validator function</param>
        public void ValidateAndColorControl(dynamic textControl, Func<string, bool> isValid)
        {
            try
            {
                textControl.ForeColor = isValid(textControl.Text)
                    ? TextColor
                    : ErrorColor;
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
