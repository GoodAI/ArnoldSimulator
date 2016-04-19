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

        private SerilogRobe(ILogger serilogLogger)
        {
            m_logger = serilogLogger;
        }

        public static ILog CreateLogger(Func<LoggerConfiguration, LoggerConfiguration> configAction)
        {
            return new SerilogRobe(configAction(DefaultConfig()).CreateLogger());
        }

        private static LoggerConfiguration DefaultConfig()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Is(DefaultDebugLevel)
                .Enrich.WithThreadId()
                .Enrich.With(new ExceptionEnricher(new ExceptionDestructurer(), new AggregateExceptionDestructurer()))
                .WriteTo.ColoredConsole(outputTemplate: DefaultOutputTemplate);
        }

        private readonly ILogger m_logger;

        #region ILog Implementation

        public void Add(Severity severity, string template, params object[] objects)
        {
            m_logger.Write(ConvertSeverity(severity), template, objects);
        }

        public void Add(Severity severity, Exception ex, string template, params object[] objects)
        {
            m_logger.Write(ConvertSeverity(severity), ex, template, objects);
        }

        #endregion

        private static LogEventLevel ConvertSeverity(Severity severity)
        {
            switch (severity)
            {
                case Severity.Error:   return LogEventLevel.Error;
                case Severity.Warn:    return LogEventLevel.Warning;
                case Severity.Info:    return LogEventLevel.Information;
                case Severity.Debug:   return LogEventLevel.Debug;
                case Severity.Verbose: return LogEventLevel.Verbose;
                default: return LogEventLevel.Error;
            }
        }
    }
}
