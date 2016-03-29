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

            uint lenght = m_reader.StartReadingReply(memStream);
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

        private static MemoryStream GetMessageStream(byte[] message)
        {
            var memStream = new MemoryStream();
            memStream.Write(message, 0, message.Length);
            memStream.Position = 0;

            return memStream;
        }
    }
}
