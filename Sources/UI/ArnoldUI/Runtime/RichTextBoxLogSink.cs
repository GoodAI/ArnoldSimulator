using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog.Core;
using Serilog.Events;

namespace GoodAI.Arnold.Runtime
{
    public interface IWinFormsLogSink : ILogEventSink
    { }

    internal class RichTextBoxLogSink : IWinFormsLogSink
    {
        public RichTextBox TextBox { get; set; }

        public void Emit(LogEvent logEvent)
        {
            if (TextBox == null)
                return;
        }
    }
}
