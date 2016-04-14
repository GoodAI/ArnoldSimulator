using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GoodAI.Net.ConverseSharp
{
    public class ConverseReaderTests
    {
        private readonly ConverseReader m_reader = new ConverseReader();

        [Fact]
        public void ReadsReplyHeader()
        {
            MemoryStream memStream = GetMessageStream(new byte[] {0, 0, 1, 2, 5});

            uint lenght = m_reader.ReadInteger(memStream);
            Assert.Equal(256u + 2, lenght);

            int next = memStream.ReadByte();
            Assert.Equal(5, next);
        }

        [Fact]
        public void ReadsReply()
        {
            MemoryStream memStream = GetMessageStream(new byte[] {0, 0, 0, 3, 1, 2, 3});

            var buffer = new byte[10];
            int originalLength = buffer.Length;

            m_reader.ReadReply(memStream, ref buffer);

            Assert.ArraySegmentEqual(new byte[] { 1, 2, 3 }, buffer);
        }

        [Fact]
        public void ReadsReplyToSmallBuffer()
        {
            var message = new byte[] {0, 0, 0, 3, 1, 2, 3};
            MemoryStream memStream = GetMessageStream(message);

            var buffer = new byte[2];

            m_reader.ReadReply(memStream, ref buffer);

            Assert.Equal(message.Length - 4, buffer.Length);  // 4 is header length.

            Assert.ArraySegmentEqual(new byte [] { 1, 2, 3 }, buffer);
        }

        [Fact]
        public void ReadsRequest()
        {
            var header = new byte[]
            {
                0, 0, 0, 3,
                0, 0, 0, 1,
            };

            string handlerName = "someHandler";
            var handlerBuffer = new byte[32];
            var handlerNameAscii = Encoding.ASCII.GetBytes(handlerName);
            Buffer.BlockCopy(handlerNameAscii, 0, handlerBuffer, 0, handlerNameAscii.Length);

            var message = new byte[] {1, 2, 3};

            var memStream = new MemoryStream();
            memStream.Write(header, 0, 8);
            memStream.Write(handlerBuffer, 0, 32);
            memStream.Write(message, 0, 3);
            memStream.Position = 0;

            var outputStream = new MemoryStream();

            var details = m_reader.ReadRequest(memStream, outputStream);

            Assert.Equal((uint)3, details.MessageLength);
            Assert.Equal(handlerName, details.HandlerName);
            Assert.Equal((uint)1, details.ProcessorNumber);
            Assert.Equal(message, outputStream.GetBuffer());
        }

        private static MemoryStream GetMessageStream(byte[] message)
        {
            var memStream = new MemoryStream();
            memStream.Write(message, 0, message.Length);
            memStream.Position = 0;

            return memStream;
        }

        [Fact]
        public void ReadsLongNullTerminatedAsciiIntoString()
        {
            var length = 32;
            var input = "0123456789012345678901234567890";

            // String with length = 32.
            var terminatedInput = input + '\0';
            var buffer = Encoding.ASCII.GetBytes(terminatedInput);

            var memStream = new MemoryStream();
            memStream.Write(buffer, 0, length);
            memStream.Position = 0;

            var result = ConverseReader.ReadHandlerName(memStream);

            Assert.Equal(input, result);
        }

        [Fact]
        public void ReadsShortNullTerminatedAsciiIntoString()
        {
            var length = 32;
            var input = "0123456789";

            // String with length = 32.
            var terminatedInput = input + '\0' + new String('f', length - input.Length - 1);

            var buffer = Encoding.ASCII.GetBytes(terminatedInput);

            var memStream = new MemoryStream();
            memStream.Write(buffer, 0, length);
            memStream.Position = 0;

            var result = ConverseReader.ReadHandlerName(memStream);

            Assert.Equal(input, result);
        }
    }
}
