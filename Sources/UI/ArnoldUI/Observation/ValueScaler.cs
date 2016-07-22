using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Observation
{
    public interface IValueScaler
    {
        float LowerBound { get; }
        float UpperBound { get; }

        /// <summary>
        /// Get relative value on the interval [LowerBound, 0) or [0 UpperBound].
        /// </summary>
        float ScaleValue(float inputValue);
    }

    public class AutoValueScaler : IValueScaler
    {
        public float LowerBound { get; private set; } = -0.01f;
        public float UpperBound { get; private set; } = +0.01f;

        /// <summary>
        /// Get relative value on the interval [LowerBound, 0) or [0 UpperBound].
        /// And strech the bounds before doing that if necessary.
        /// </summary>
        public float ScaleValue(float inputValue)
        {
            if (inputValue >= 0.0f)
            {
                if (inputValue > UpperBound)
                    UpperBound = inputValue;

                return inputValue/UpperBound;
            }
            else
            {
                if (inputValue < LowerBound)
                    LowerBound = inputValue;

                return -inputValue/LowerBound;  // Keep the negative sign.
            }
        }
    }
}
