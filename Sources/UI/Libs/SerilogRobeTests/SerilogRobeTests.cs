using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using Xunit;
using Xunit.Sdk;

namespace GoodAI.Logging.Tests
{
    internal class TestSink : ILogEventSink
    {
        public List<LogEvent> Events { get; private set; }

        public TestSink()
        {
            Events = new List<LogEvent>();
        }

        public void Emit(LogEvent logEvent)
        {
            Events.Add(logEvent);
        }
    }

    public class SerilogRobeTests
    {
        [Fact]
        public void BasicLogLevelTest()
        {
            var infoSink = new TestSink();
            var debugSink = new TestSink();

            ILog log = SerilogRobe.CreateLogger(configuration =>
            {
                configuration
                    .WriteTo.Sink(debugSink)
                    .WriteTo.Sink(infoSink, restrictedToMinimumLevel: LogEventLevel.Information);

                return configuration;
            });

            log.Debug("debug 1");
            log.Debug("debug 2: {Message}", "foo");
            log.Info("info 1");
            log.Info("info 2: {AnotherMessage:l}", "bar");

            // TODO(Premek)
            //var logger = log.ForContext<SerilogRobeTests>();
            //logger.Debug("debug 3");
            //logger.Information("info 3");

            Assert.Equal(4, debugSink.Events.Count);
            Assert.Equal(2, infoSink.Events.Count);

            Assert.Contains("debug 2: \"foo\"", debugSink.Events.Select(e => e.RenderMessage()));
            Assert.Contains("info 2: bar", debugSink.Events.Select(e => e.RenderMessage()));
        }
    }
}
