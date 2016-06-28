using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace GoodAI.Arnold.Core
{
    public class FilterBox
    {
        public FilterBox(Vector3 lowerBound, Vector3 size)
        {
            LowerBound = lowerBound;
            Size = size;
        }

        public Vector3 LowerBound;
        public Vector3 Size;
    }

    public class ModelFilter
    {
        public IList<FilterBox> Boxes { get; } = new List<FilterBox>();
    }
}
