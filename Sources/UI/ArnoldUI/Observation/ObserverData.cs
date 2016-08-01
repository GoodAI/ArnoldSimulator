using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Observation
{
    public class ObserverData
    {
        public int[] Metadata { get; }
        public byte[] PlainData { get; }
        public float[] FloatData { get; }

        public ObserverData(int[] metadata, byte[] plainData, float[] floatData)
        {
            Metadata = metadata;
            PlainData = plainData;
            FloatData = floatData;
        }
    }
}
