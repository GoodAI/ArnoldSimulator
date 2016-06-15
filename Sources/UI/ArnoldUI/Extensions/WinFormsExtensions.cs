using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GoodAI.Arnold.Extensions
{
    /// <summary>
    /// System.Windows.Forms.Control has methods Invoke and BeginInvoke that both take a delegate as a parameter.
    /// If one wants to use a lambda expression, it must first be cast to MethodInvoker or Action, like so:
    /// control.Invoke((Action)(() => expr));
    /// But lambda can be automatically converted to an Action parameter, and Action is accepted as delegate.
    /// These can be then used directly with a lambda expression:
    /// control.Invoke(() => expr);
    /// </summary>
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
