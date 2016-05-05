using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace GoodAI.Arnold.Runtime
{
    public interface IUiLogSink : ILogEventSink
    { }

    public class RichTextBoxLogSink : IUiLogSink
    {
        private readonly RichTextBox m_textBox;
        private readonly ITextFormatter m_textFormatter;
        private readonly int m_maxTextLength;
        private readonly int m_dropLength;

        public RichTextBoxLogSink(RichTextBox textBox, ITextFormatter textFormatter, int maxTextLength, float dropPortion)
        {
            m_textBox = textBox;
            m_textFormatter = textFormatter;
            m_maxTextLength = maxTextLength;
            m_dropLength = (int) (m_maxTextLength*dropPortion);
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null)
                throw new ArgumentNullException(nameof(logEvent));

            int selectionStart = m_textBox.SelectionStart;
            int selectionLength = m_textBox.SelectionLength;
            bool wasCaretAtEnd = selectionStart == m_textBox.TextLength;

            var output = new StringWriter();
            m_textFormatter.Format(logEvent, output);

            Color eventColor;
            switch (logEvent.Level)
            {
                case LogEventLevel.Verbose:
                    eventColor = Color.DarkGray;
                    break;
                case LogEventLevel.Debug:
                    eventColor = Color.Black;
                    break;
                case LogEventLevel.Information:
                    eventColor = Color.DodgerBlue;
                    break;
                case LogEventLevel.Warning:
                    eventColor = Color.Chocolate;
                    break;
                case LogEventLevel.Error:
                    eventColor = Color.DarkRed;
                    break;
                case LogEventLevel.Fatal:
                    eventColor = Color.Red;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (m_textBox.Text.Length > m_maxTextLength)
            {
                // this method preserves the text colouring
                // find the first end-of-line past the endmarker

                Int32 endmarker = m_textBox.Text.IndexOf('\n', m_dropLength) + 1;
                if (endmarker < m_dropLength)
                    endmarker = m_dropLength;

                m_textBox.Select(0, endmarker);
                m_textBox.Cut();

                selectionStart -= endmarker;
            }

            m_textBox.SelectionStart = m_textBox.Text.Length;
            m_textBox.SelectionLength = 0;
            m_textBox.SelectionColor = eventColor;
            m_textBox.AppendText(output.ToString());

            if (wasCaretAtEnd)
                selectionStart = m_textBox.TextLength;  // Move the caret to the end again.
            else if (selectionStart < 0)
            {
                selectionStart = 0;  // The position was deleted, move to start.
                selectionLength = 0;
            }

            m_textBox.SelectionStart = selectionStart;
            m_textBox.SelectionLength = selectionLength;

            if (wasCaretAtEnd)
                m_textBox.ScrollToCaret();
        }
    }

    class RichTextBoxWriter : TextWriter
    {
        public override Encoding Encoding { get; } = Encoding.UTF8;
    }
}
