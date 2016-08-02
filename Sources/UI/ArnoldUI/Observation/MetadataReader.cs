using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Observation
{
    public class MetadataReader
    {
        public Dimensions GetImageDimensions(int[] metadata, int dataLength)
        {
            if (dataLength < 1)
                throw new ArgumentException("Wrong data lenght", nameof(dataLength));

            if (metadata == null)
                return new Dimensions(dataLength, 1);

            var dimensions = GetDimensions(metadata);

            if (dimensions.Rank != 2)
                throw new NotImplementedException("Only exactly 2 dimensions are supported.");  // TODO(Premek)

            for (var i = 0; i < dimensions.Rank; i++)
                if (dimensions[i] < 1)
                    throw new ArgumentException($"Dimension #{i}: {dimensions[i]} < 1 is not allowed in image dimensions.");

            if (dimensions.ElementCount < dataLength)
                throw new NotImplementedException("Dimensions smaller than the data are not supported.");  // TODO(Premek)

            return dimensions;
        }

        private static Dimensions GetDimensions(int[] metadata)
        {
            return new Dimensions(metadata);
        }
    }
}
