using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleInjector;
using Xunit;

namespace GoodAI.Arnold.UI.Tests
{
    public class ConfigurationTests
    {
        [Fact]
        public void VerifiesContainerConfiguration()
        {
            var container = new Container();
            new ArnoldContainerConfig().Configure(container);
            container.Verify();
        }
    }
}
