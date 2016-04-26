using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace GoodAI.Logging
{
    public static class SerilogRobeConfig
    {
#if DEBUG
        public const string DefaultOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} <{SourceContext:l}> [{Level}] ({ThreadId}): {Message}{NewLine}{Exception}";
        public const LogEventLevel DefaultLevel = LogEventLevel.Debug;
#else
        public const string DefaultOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}]: {Message}{NewLine}{Exception}";
        public const LogEventLevel DefaultLevel = LogEventLevel.Information;
#endif

        private const int DefaultSizeLimit = 100*1024*1024;
        private const int DefaultFileCountLimit = 10;

        private static LoggerConfiguration m_config;

        public static LoggerConfiguration CurrentConfig => m_config ?? (m_config = DefaultConfig);

        public static LoggerConfiguration DefaultConfig => new LoggerConfiguration()
            .MinimumLevel.Is(DefaultLevel)
            .Enrich.WithThreadId()
            .Enrich.With(new ExceptionEnricher(new ExceptionDestructurer(), new AggregateExceptionDestructurer()))
            .WriteTo.ColoredConsole(outputTemplate: DefaultOutputTemplate);

        public static void Setup(Func<LoggerConfiguration, LoggerConfiguration> configAction)
        {
            m_config = configAction(DefaultConfig);
        }

        public static void SetupLoggingToFile(string logPath, string logName, int sizeLimit = DefaultSizeLimit)
        {
            SerilogRobeConfig.Setup(configuration => configuration.WriteTo.RollingFile(
                Path.Combine(logPath, logName + "-{Date}.log"),
                LogEventLevel.Information,
                outputTemplate: SerilogRobeConfig.DefaultOutputTemplate,
                fileSizeLimitBytes: sizeLimit / DefaultFileCountLimit,
                retainedFileCountLimit: DefaultFileCountLimit));
        }
    }
}
