using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Extensions;

namespace GoodAI.Arnold.Observation
{
    public interface IPainter
    {
        Image Paint(ObserverData data);
    }

    public class GreyscalePainter : IPainter
    {
        public Image Paint(ObserverData data)
        {
            var image = new Bitmap(data.FloatData.Length, 1);
            data.FloatData.EachWithIndex((index, value) =>
            {
                byte byteValue = (byte)(value * 255);  // TODO(Premek): More robust conversion.

                image.SetPixel(index, 0, Color.FromArgb(255, byteValue, byteValue, byteValue));
            });

            return image;
        }
    }

    public class RedGreenPainter : IPainter
    {
        private readonly IValueScaler m_valueScaler;

        public RedGreenPainter(IValueScaler valueScaler)
        {
            m_valueScaler = valueScaler;
        }

        public Image Paint(ObserverData data)
        {
            var image = new Bitmap(data.FloatData.Length, 1);

            data.FloatData.EachWithIndex((index, value) =>
            {
                float scaledValue = m_valueScaler.ScaleValue(value);

                image.SetPixel(index, 0, GetPixelColor(scaledValue));
            });

            return image;
        }

        internal Color GetPixelColor(float scaledValue)
        {
            return (scaledValue >= 0.0f)
                ? Color.FromArgb(255, 0, (int) (scaledValue*255), 0)  // Green scale.
                : Color.FromArgb(255, (int) ((-scaledValue)*255), 0, 0);  // Red scale.
        }
    }
}
