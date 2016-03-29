using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunit
{ 
    public partial class Assert
    {
        public static void ArraySegmentEqual<T>(T[] expectedSequence, T[] buffer, int offset = 0)
        {
            if (buffer.Length < (expectedSequence.Length + offset))
                throw new ArgumentException($"{nameof(buffer)} too short (Length = {buffer.Length}) for given " +
                    $"{nameof(expectedSequence)} (Length = {expectedSequence.Length}) " + 
                    $"and {nameof(offset)} ({offset})", nameof(buffer));

            for (int i = 0; i < expectedSequence.Length; i++)
            {
                int b = i + offset;

                True(buffer[b].Equals(expectedSequence[i]),
                    $"Byte #{b} differs: {buffer[b]} != {expectedSequence[i]}");
            }
        }
    }
}
