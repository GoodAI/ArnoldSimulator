using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Observation;
using Xunit;

namespace GoodAI.Arnold.UI.Tests.Observation
{
    public class ValueScalerTests
    {
        private const int Precision = 5;

        private readonly IValueScaler m_autoScaler = new AutoValueScaler();

        [Fact]
        public void UpdatesUpperBound()
        {
            var scaledValue = m_autoScaler.ScaleValue(5.0f);

            Assert.Equal(5.0f, m_autoScaler.UpperBound, Precision);
            Assert.Equal(1.0f, scaledValue, Precision);
        }

        [Fact]
        public void UpdatesLowerBound()
        {
            var scaledValue = m_autoScaler.ScaleValue(-3.1f);

            Assert.Equal(-3.1f, m_autoScaler.LowerBound, Precision);
            Assert.Equal(-1.0f, scaledValue, Precision);
        }

        [Theory]
        [InlineData(3.0f)]
        [InlineData(-3.0f)]
        [InlineData(0.0f)]
        public void ScalesNumber(float value)
        {
            const float bound = 5.0f;
            m_autoScaler.ScaleValue(bound);  // Set the upper bound.
            m_autoScaler.ScaleValue(-bound);  // Set the lower bound.

            var scaledValue = m_autoScaler.ScaleValue(value);
            Assert.Equal(value/bound, scaledValue, Precision);
        }

        [Fact]
        public void ScalesZeroWithDefaultBounds()
        {
            var scaledValue = m_autoScaler.ScaleValue(0.0f);
            Assert.Equal(0.0f, scaledValue, Precision);
        }

        [Fact]
        public void ScalesPositiveAndNegativeIndependently()
        {
            m_autoScaler.ScaleValue(2.0f);
            m_autoScaler.ScaleValue(-5.0f);

            Assert.Equal(0.5f, m_autoScaler.ScaleValue(1.0f), Precision);
            Assert.Equal(-0.2f, m_autoScaler.ScaleValue(-1.0f), Precision);

            // Zero still scales to zero.
            Assert.Equal(0.0f, m_autoScaler.ScaleValue(0.0f), Precision);
        }
    }
}
