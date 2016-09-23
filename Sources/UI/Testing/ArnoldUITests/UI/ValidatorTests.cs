using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GoodAI.Arnold.UI.Tests.UI
{
    public class ValidatorTests
    {
        [Fact]
        public void ParsesUInt()
        {
            Assert.Equal((uint?)150, Validator.MaybeParseUInt("150"));

            Assert.False(Validator.MaybeParseUInt("-5").HasValue);
            Assert.False(Validator.MaybeParseUInt("foobar").HasValue);
        }

        [Fact]
        public void ParsesPortNuber()
        {
            Assert.Equal(43200, Validator.MaybeParsePortNumber("43200"));

            Assert.False(Validator.MaybeParsePortNumber("66000").HasValue);
            Assert.False(Validator.MaybeParsePortNumber("-43200").HasValue);
            Assert.False(Validator.MaybeParsePortNumber("foobar").HasValue);
        }
    }
}
