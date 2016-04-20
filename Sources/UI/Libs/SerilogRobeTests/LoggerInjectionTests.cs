using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.TypeMapping;
using Serilog;
using Serilog.Events;
using SimpleInjector;
using Xunit;

namespace GoodAI.Logging.Tests
{
    public class TestContainerConfig : IContainerConfiguration
    {
        public void Configure(Container container)
        {
            container.RegisterSingleton(SerilogRobeConfig.CurrentConfig);
            container.RegisterConditional(
                typeof(ILog),
                c => typeof(SerilogRobe<>).MakeGenericType(c.Consumer.ImplementationType),
                Lifestyle.Transient,
                c => true);

            container.Register<Fool, Fool>(Lifestyle.Transient);
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class Fool
    {
        private readonly ILog m_log;

        public Fool(ILog log)
        {
            m_log = log;
        }

        public void LogJoke()
        {
            m_log.Info("10 words: binary code");
        }
    }

    public class LoggerInjectionTests
    {
        private static readonly TestSink TestSink = new TestSink();

        static LoggerInjectionTests()
        {
            SerilogRobeConfig.Setup(config =>
                config.WriteTo.Sink(TestSink));

            TypeMap.InitializeConfiguration<TestContainerConfig>();
            TypeMap.SimpleInjectorContainer.Verify();
        }

        [Fact]
        public void LoggerIsInjected()
        {
            var fool = TypeMap.GetInstance<Fool>();

            Assert.NotNull(fool);
        }

        [Fact]
        public void LoggerInjectedWithProperContext()
        {
            var fool = TypeMap.GetInstance<Fool>();

            fool.LogJoke();

            LogEvent logEvent = TestSink.Events.FirstOrDefault();
            Assert.NotNull(logEvent);
            Assert.Equal("\"Fool\"", logEvent.Properties["SourceContext"].ToString());
        }
    }
}
