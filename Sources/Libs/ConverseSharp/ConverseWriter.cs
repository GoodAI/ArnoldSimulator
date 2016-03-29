using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

/*
(Copied from  http://charm.cs.illinois.edu/gerrit/charm/src/conv-ccs/ccs-client.c)

Converse Client-Server Protocol spec.

 A CCS request message asks a running Converse program to
execute a pre-registered "handler" routine.  You send the
request directly to conv-host's CCS server port.

The request, with header, has the following format on the
network: 
Ccs Message----------------------------------------------
 /--CcsMessageHeader---------------------------       ^
 | 4 bytes  |   Message data length d         ^       |
 | 4 bytes  |   Dest. processor number        |       |
 |          |   (big-endian binary integers)  |   40+d bytes
 +-----------------------------------      40 bytes   |
 |32 bytes  |   CCS Handler name              |       |
 |          |   (ASCII, Null-terminated)      v       |
 \---------------------------------------------       |
    d bytes |   User data (passed to handler)         v
-------------------------------------------------------

 A CCS reply message (if any) comes back on the request socket,
and has only a length header:
CCS Reply ----------------------------------
 | 4 bytes  |   Message data length d        
 |          |   (big-endian binary integer)  
 +----------------------------------------- 
 | d bytes  |   User data                   
--------------------------------------------
 */

namespace GoodAI.Net.ConverseSharp
{
    internal class ConverseWriter
    {
        private const int HandlerNameMaxLength = 32;

        public const int HeaderLength = 4 + 4 + HandlerNameMaxLength;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="processorNumber"></param>
        /// <param name="handlerName"></param>
        /// <param name="messageBody"></param>
        /// <param name="realBodyLength">Use this param to truncate the message without allocating and copying to a smaller byte array.</param>
        public void WriteMessage(Stream stream, uint processorNumber, string handlerName, byte[] messageBody, int realBodyLength = 0)
        {
            if (handlerName.Length >= HandlerNameMaxLength)
                throw new ArgumentException(
                    $"handlerName is too long (>={HandlerNameMaxLength})", nameof(handlerName));

            CheckArgumentRealBodyLength(messageBody, realBodyLength);

            int messageBodyLength = (realBodyLength > 0) ? realBodyLength : messageBody.Length;

            WriteBigEndianUnsignedInt(stream, (uint)messageBodyLength);
            WriteBigEndianUnsignedInt(stream, processorNumber);

            // Write handler name.
            var handlerNameBytes = new byte[HandlerNameMaxLength];

            Encoding.ASCII.GetBytes(handlerName, 0, handlerName.Length, handlerNameBytes, 0);
            stream.Write(handlerNameBytes, 0, handlerNameBytes.Length);

            // Write message body.
            stream.Write(messageBody, 0, messageBodyLength);
        }

        public void WriteReply(Stream stream, byte[] messageBody, int realBodyLength = 0)
        {
            CheckArgumentRealBodyLength(messageBody, realBodyLength);

            int messageBodyLength = (realBodyLength > 0) ? realBodyLength : messageBody.Length;

            WriteBigEndianUnsignedInt(stream, (uint)messageBodyLength);

            stream.Write(messageBody, 0, messageBodyLength);
        }

        private static void CheckArgumentRealBodyLength(byte[] messageBody, int realBodyLength)
        {
            if (realBodyLength < 0 || realBodyLength > messageBody.Length)
                throw new ArgumentException(
                    $"invalid value ({realBodyLength}), it must not be negative nor larger than {messageBody.Length}", nameof(realBodyLength));
        }

        private static void WriteBigEndianUnsignedInt(Stream stream, uint number)
        {
            byte[] numberBytes = GetBigEndianBytes(number);

            stream.Write(numberBytes, 0, numberBytes.Length); 
        }

        private static byte[] GetBigEndianBytes(uint number)
        {
            byte[] bytes = BitConverter.GetBytes(number);

            return (BitConverter.IsLittleEndian) ? bytes.Reverse().ToArray() : bytes;
        }
    }
}
