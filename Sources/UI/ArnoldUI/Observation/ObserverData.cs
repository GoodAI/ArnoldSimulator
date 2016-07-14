using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Observation
{
    public class ObserverData
    {
        public byte[] PlainData { get; }
        public float[] FloatData { get; }

        public ObserverData(byte[] plainData, float[] floatData)
        {
            PlainData = plainData;
            FloatData = floatData;
        }
    }
}
