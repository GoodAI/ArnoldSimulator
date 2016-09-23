using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.UI
{
    public static class Validator
    {
        public static uint? MaybeParseUInt(string text)
        {
            uint number;

            return UInt32.TryParse(text, out number) ? number : (uint?) null;
        }

        public static bool TryParseUInt(string text)
        {
            return MaybeParseUInt(text).HasValue;
        }

        public static int? MaybeParsePortNumber(string text)
        {
            uint? port = MaybeParseUInt(text);

            if (port.HasValue && (port.Value > IPEndPoint.MaxPort))
                return null;

            return (int?)port;
        }

        public static bool TryParsePortNumber(string text)
        {
            return MaybeParsePortNumber(text).HasValue;
        }
    }
}
