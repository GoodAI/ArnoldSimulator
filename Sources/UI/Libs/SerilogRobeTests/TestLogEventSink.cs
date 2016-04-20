using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog.Core;
using Serilog.Events;

namespace GoodAI.Logging.Tests
{
    internal class TestLogEventSink : ILogEventSink
    {
        public List<LogEvent> Events { get; }

        public TestLogEventSink()
        {
            Events = new List<LogEvent>();
        }

        public void Emit(LogEvent logEvent)
        {
            Events.Add(logEvent);
        }

        public void Clear()
        {
            Events.Clear();
        }
    }
}
