using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        public Image Paint(ObserverData data)  // TODO(Premek): Support dimensions.
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

        private readonly MetadataReader m_metadataReader = new MetadataReader();

        public RedGreenPainter(IValueScaler valueScaler)
        {
            m_valueScaler = valueScaler;
        }

        public Image Paint(ObserverData data)
        {
            var dimensions = m_metadataReader.GetImageDimensions(data.Metadata, data.FloatData.Length);

            var image = new Bitmap(dimensions.Width, dimensions.Height);

            data.FloatData.EachWithIndex((index, value) =>
            {
                float scaledValue = m_valueScaler.ScaleValue(value);

                image.SetPixel(index % dimensions.Width, index / dimensions.Width, GetPixelColor(scaledValue));
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
