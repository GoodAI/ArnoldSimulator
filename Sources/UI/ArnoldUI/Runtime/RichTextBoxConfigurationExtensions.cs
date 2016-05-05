using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace GoodAI.Arnold.Runtime
{
    public static class RichTextBoxConfigurationExtensions
    {
        // TODO(HonzaS): {Level,4} is only supported in v2, it should become active when (if) we update.
        private const string DefaultTemplate = "{Timestamp:HH:mm:ss.fff} [{Level,4}] {Message}{NewLine}{Exception}";
        private const int DefaultMaxTextLength = 1024000;
        private const float DefaultDropPortion = 1/4f;

        public static LoggerConfiguration RichTextBox(
            this LoggerSinkConfiguration sinkConfiguration,
            RichTextBox textBox,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            string outputTemplate = DefaultTemplate,
            IFormatProvider formatProvider = null,
            int maxTextLength = DefaultMaxTextLength,
            float dropPortion = DefaultDropPortion)
        {
            var textFormatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
            var sink = new RichTextBoxLogSink(textBox, textFormatter, maxTextLength, dropPortion);
            return sinkConfiguration.Sink(sink, restrictedToMinimumLevel);
        }
    }
}
