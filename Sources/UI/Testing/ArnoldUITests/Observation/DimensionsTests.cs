using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Observation;
using Xunit;

namespace GoodAI.Arnold.UI.Tests.Observation
{
    public class DimensionsTests
    {
        [Fact]
        public void ElementCountIsCachedProperly()
        {
            var dims = new Dimensions(3, 5, 7);

            Assert.Equal(3 * 5 * 7, dims.ElementCount);
            Assert.Equal(3 * 5 * 7, dims.ElementCount);
        }

        [Fact]
        public void HashCodeIsCachedProperly()
        {
            var dims = new Dimensions(3, 5, 7);

            int hashCode = dims.GetHashCode();
            Assert.Equal(hashCode, dims.GetHashCode());
        }

        [Fact]
        public void EqualsTests()
        {
            Assert.True((new Dimensions(2, 3, 5)).Equals(new Dimensions(2, 3, 5)));
            Assert.True((new Dimensions()).Equals(new Dimensions()));

            Assert.False((new Dimensions(2, 3, 5)).Equals(new Dimensions(2, 3)));
            Assert.False((new Dimensions(2, 3, 5)).Equals(new Dimensions(2, 3, 11)));

            // ReSharper disable once SuspiciousTypeConversion.Global
            Assert.False((new Dimensions(2, 3, 5)).Equals(0));  // Compare with some other value type.
        }

        [Fact]
        public void RankReturnsNumberOfDims()
        {
            Assert.Equal(3, new Dimensions(5, 3, 2).Rank);
        }

        [Fact]
        public void DefaultDimIsRankOneOfSizeZero()
        {
            Dimensions emptyDims = Dimensions.Empty;

            Assert.Equal(1, emptyDims.Rank);
            Assert.Equal(0, emptyDims[0]);
            Assert.Equal(0, emptyDims.ElementCount);
        }

        [Fact]
        public void AnyDimensionCanBeZero()
        {
            var rank1Dims = new Dimensions(0);
            Assert.Equal(0, rank1Dims.ElementCount);

            var rankNDims = new Dimensions(3, 0, 5);
            Assert.Equal(3, rankNDims[0]);
            Assert.Equal(0, rankNDims.ElementCount);
        }

        [Fact]
        public void DefaultConstructorReturnsEmptyDims()
        {
            Assert.True(Dimensions.Empty.Equals(new Dimensions()));
        }
    }
}
