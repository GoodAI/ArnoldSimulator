using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GoodAI.Logging.Tests
{
    public class NullLoggerTests
    {
        [Fact]
        public void IgnoresLogEvents()
        {
            var nullLogger = new NullLogger {FailOnUse = false};

            nullLogger.Add(Severity.Error, "foo {one}", 1);
            nullLogger.Add(Severity.Error, new Exception("bar"), "foo {something}", "baz");
        }

        [Fact]
        public void ThrowsWhenForbiddenAndUsed()
        {
            var nullLogger = new NullLogger {FailOnUse = true};
            Assert.Throws<InvalidOperationException>(() => nullLogger.Add(Severity.Error, "foo {one}", 1));
        }
    }
}
