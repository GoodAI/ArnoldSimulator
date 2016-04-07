using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GoodAI.Net.ConverseSharp
{
    public class ConverseWriterTests
    {
        
        private readonly MemoryStream m_stream = new MemoryStream();
        private readonly ConverseWriter m_converseWriter = new ConverseWriter();

        private void CheckWriteMessage(uint processorNumber, string handlerName, byte[] messageBody,
            byte[] expectedMessage, int offset = 0)
        {
            m_stream.Position = 0;

            // Call the "SUT".
            m_converseWriter.WriteMessage(m_stream, processorNumber, handlerName, messageBody);

            Assert.ArraySegmentEqual(expectedMessage, m_stream.GetBuffer(), offset);
        }

        private readonly byte[] EmptyBytes = Array.Empty<byte>();

        [Fact]
        public void WritesHeaderLengthInBigEndian()
        {
            CheckWriteMessage(0, "Foo", EmptyBytes, new byte[] { 0, 0, 0, 0 });
            CheckWriteMessage(0, "Foo", new byte[] {5, 5}, new byte[] {0, 0, 0, 2});
        }

        [Fact]
        public void WritesRealMessageBodyLenght()
        {
            m_stream.Capacity = 100;
            m_converseWriter.WriteMessage(m_stream, 0, "Foo", new byte[] { 1, 2, 3, 4, 5, 6 }, realBodyLength: 4);

            Assert.ArraySegmentEqual(new byte[] { 0, 0, 0, 4 }, m_stream.GetBuffer());
            Assert.ArraySegmentEqual(new byte[] { 1, 2, 3, 4, 0 }, m_stream.GetBuffer(), offset: 40);
        }
        
        [Fact]
        public void WritesAlsoProcessorNoInBigEndian()
        {
            CheckWriteMessage(256 + 2, "Foo", EmptyBytes, new byte[] { 0, 0, 0, 0, 0, 0, 1, 2 });
        }

        [Fact]
        public void WritesHandlerName()
        {
            CheckWriteMessage(0, "ab", EmptyBytes, new byte[]{(byte)'a', (byte)'b', 0}, offset: 8);
        }

        [Fact]
        public void WritesMesageBody()
        {
            CheckWriteMessage(0, "a", new byte[] {5, 5}, new byte[]{5, 5}, offset: 40);
        }

        [Fact]
        public void CheckAllPartsTogether()
        {
            CheckWriteMessage(3, "abcd", new byte[] { 5, 6, 7 },
                new byte[] { 0, 0, 0, 3, 0, 0, 0, 3,  // Length and processor number.
                    (byte)'a', (byte)'b', (byte)'c', (byte)'d', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // 32 bytes handler name.
                    5, 6, 7}); // Message body. 
        }

        [Fact]
        public void WritesReply()
        {
            m_converseWriter.WriteReply(m_stream, new byte[] { 1, 2, 3 });

            Assert.ArraySegmentEqual(new byte[] { 0, 0, 0, 3, 1, 2, 3 }, m_stream.GetBuffer());
        }
    }
}
