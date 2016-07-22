using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Observation;
using Xunit;

namespace GoodAI.Arnold.UI.Tests.Observation
{
    public class PainterTests
    {
        private readonly RedGreenPainter m_painter = new RedGreenPainter(new AutoValueScaler());

        [Theory]
        [InlineData(0.0f, 0, 0, 0)]
        [InlineData(0.5f, 0, 127, 0)]
        [InlineData(-0.5f, 127, 0, 0)]
        [InlineData(1.0f, 0, 255, 0)]
        [InlineData(-1.0f, 255, 0, 0)]
        public void PaintsCorrectRedGreenColors(float scaledValue, byte r, byte g, byte b)
        {
            Color color = m_painter.GetPixelColor(scaledValue);

            Assert.Equal(r, color.R);
            Assert.Equal(g, color.G);
            Assert.Equal(b, color.B);
        }

    }
}
