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
            container.Options.PropertySelectionBehavior = new PropertyInjectionForType<ILog>(container);

            container.RegisterSingleton(new TestLogEventSink());

            container.RegisterSingleton(() => SerilogRobeConfig.Setup(config =>
                config.WriteTo.Sink(container.GetInstance<TestLogEventSink>())));

            container.RegisterConditional(
                typeof(ILog),
                typeFactory => typeof(SerilogRobe<>).MakeGenericType(typeFactory.Consumer.ImplementationType),
                Lifestyle.Singleton,
                predicateContext => true);

            container.Register<Fool>();
            container.Register<PropertyJester>();
            container.Register<PropertyMute>();
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

    public class PropertyJester
    {
        public ILog Log { get; set; } = NullLogger.Instance;

        public Fool DontInjectToMe { get; set; }

        public SerilogRobe DontInjectToDerivedType { get; set; }

        public void LogJoke()
        {
            Log.Warn("I'm not a fool!");
        }
    }

    public class PropertyMute
    {
        public ILog Log { get; set; }
    }

    public class LoggerInjectionTests
    {
        private readonly Container m_container;

        private readonly TestLogEventSink m_testSink;

        public LoggerInjectionTests()
        {
            m_container = ContainerFactory.Create<TestContainerConfig>();

            m_testSink = m_container.GetInstance<TestLogEventSink>();
        }

        [Fact]
        public void LoggerIsInjected()
        {
            var fool = m_container.GetInstance<Fool>();

            Assert.NotNull(fool);
        }

        [Fact]
        public void LoggerInjectedWithProperContext()
        {
            var fool = m_container.GetInstance<Fool>();

            fool.LogJoke();

            LogEvent logEvent = m_testSink.Events.FirstOrDefault();
            Assert.NotNull(logEvent);
            Assert.Equal("\"Fool\"", logEvent.Properties["SourceContext"].ToString());
        }

        [Fact]
        public void LoggerIsInjectedToProperty()
        {
            var jester = m_container.GetInstance<PropertyJester>();

            Assert.NotNull(jester);

            jester.LogJoke();
            Assert.NotNull(m_testSink.Events.FirstOrDefault());
        }

        [Fact]
        public void DoesNotInjectOtherProperties()
        {
            var jester = m_container.GetInstance<PropertyJester>();

            Assert.Null(jester.DontInjectToMe);
            Assert.Null(jester.DontInjectToDerivedType);
        }

        [Fact]
        public void SingletonLogsDifferBetweenClients()
        {
            var jester = m_container.GetInstance<PropertyJester>();
            var jester2 = m_container.GetInstance<PropertyJester>();
            var mute = m_container.GetInstance<PropertyMute>();

            Assert.Equal(jester.Log, jester2.Log);
            Assert.NotEqual(jester.Log, mute.Log);
        }
    }
}
