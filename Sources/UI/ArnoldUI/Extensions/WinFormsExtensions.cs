using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GoodAI.Arnold.Extensions
{
    public static class WinFormsExtensions
    {
        public static void Invoke(this Control control, Action action)
        {
            control.Invoke(action);
        }

        public static void BeginInvoke(this Control control, Action action)
        {
            control.BeginInvoke(action);
        }
    }
}
