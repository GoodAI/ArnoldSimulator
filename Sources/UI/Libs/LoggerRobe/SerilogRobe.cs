using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace GoodAI.LoggerRobe
{
    public class SerilogRobe : ILog
    {

#if DEBUG
        public const string DefaultOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} <{SourceContext:l}> [{Level}] ({ThreadId}): {Message}{NewLine}{Exception}";
        public const LogEventLevel DefaultDebugLevel = LogEventLevel.Debug;
#else
        public const string DefaultOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}]: {Message}{NewLine}{Exception}";
        public const LogEventLevel DefaultDebugLevel = LogEventLevel.Information;
#endif

        static SerilogRobe()
        {
            // Serilog diagnostic output. Serilog won't write its errors into the user-space sinks.
            Serilog.Debugging.SelfLog.Out = Console.Out;
        }

        public SerilogRobe()
        {
            m_logger = DefaultConfig().CreateLogger();
        }

        private static LoggerConfiguration DefaultConfig()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Is(DefaultDebugLevel)
                .Enrich.WithThreadId()
                .Enrich.With(new ExceptionEnricher(new ExceptionDestructurer(), new AggregateExceptionDestructurer()))
                .WriteTo.ColoredConsole(outputTemplate: DefaultOutputTemplate);
        }

        private ILogger m_logger;
    }
}
