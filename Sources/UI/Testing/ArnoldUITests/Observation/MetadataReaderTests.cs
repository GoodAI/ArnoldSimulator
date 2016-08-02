using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Observation;
using Xunit;

namespace GoodAI.Arnold.UI.Tests.Observation
{
    public class MetadataReaderTests
    {
        private readonly MetadataReader m_metadataReader = new MetadataReader();

        [Fact]
        public void GetsDefaultImageDimensionsFromLength()
        {
            var dims = m_metadataReader.GetImageDimensions(null, 28);

            Assert.True(dims.Equals(new Dimensions(28, 1)));
        }

        [Fact]
        public void ConvertsTwoItemsToImageDimensions()
        {
            var metadata = new int[] {32, 20};

            var dims = m_metadataReader.GetImageDimensions(metadata, 32*20);

            Assert.True(dims.Equals(new Dimensions(32, 20)));
        }

        [Fact]
        public void ConvertsTwoItemsEvenIfDataLenghtIsSmaller()
        {
            var dims = m_metadataReader.GetImageDimensions(new[] {5, 5}, 20);

            Assert.True(dims.Equals(new Dimensions(5, 5)));
        }
    }
}
