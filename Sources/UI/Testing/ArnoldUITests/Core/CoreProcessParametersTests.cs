using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Core;
using Xunit;

namespace GoodAI.Arnold.UI.Tests.Core
{
    public class CoreProcessParametersTests
    {
        [Fact]
        public void SubstituesPort()
        {
            var coreParams = new CoreProcessParameters("dir", "--port {Port}", 80);

            Assert.True(coreParams.IsValid);
            Assert.Equal("--port 80", coreParams.SubstitutedArguments);
        }

        [Fact]
        public void DoesNotRequirePortWhenTheMacroIsNotThere()
        {
            var coreParams = new CoreProcessParameters("dir", "--some --args", null);

            Assert.True(coreParams.IsValid);
        }

        [Fact]
        public void DoesNotAcceptInvalidPortNumber()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var p = new CoreProcessParameters("dir", "args", -1);
            });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var p = new CoreProcessParameters("dir", "args", 1000000);
            });
        }
    }
}
