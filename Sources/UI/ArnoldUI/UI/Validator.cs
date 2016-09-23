using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.UI
{
    public static class Validator
    {
        public static bool TryParseUInt(string text)
        {
            uint number;

            return UInt32.TryParse(text, out number);
        }

        public static uint? MaybeParseUInt(string text)
        {
            uint number;

            return UInt32.TryParse(text, out number) ? number : (uint?) null;

        }
    }
}
