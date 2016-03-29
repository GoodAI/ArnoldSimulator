using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Net.ConverseSharp
{
    internal class ConverseReader
    {
        private const int HeaderSize = 4;
        private readonly byte[] lengthBuffer = new byte[HeaderSize];

        /// <summary>
        /// Reads the header and leaves the rest of the stream for reading in other ways (such as ProtoBufs).
        /// NOTE: Not thread-safe.
        /// </summary>
        /// <param name="stream">A connected stream.</param>
        public uint StartReadingReply(Stream stream)
        {
            ReadBytesToFixedBuffer(stream, lengthBuffer);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthBuffer);

            return BitConverter.ToUInt32(lengthBuffer, 0);
        }

        /// <summary>
        /// 
        /// NOTE: Not thread-safe.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="replyBuffer"></param>
        /// <returns></returns>
        public uint ReadReply(Stream stream, ref byte[] replyBuffer)
        {
            uint messageLength = StartReadingReply(stream);

            // TODO(Premek): warn if the length is suspiciously large, such as > 100 MB

            if (replyBuffer.Length < messageLength)
                replyBuffer = new byte[messageLength];

            ReadBytesToFixedBuffer(stream, replyBuffer, Convert.ToInt32(messageLength));

            return messageLength;
        }

        /// <summary>
        /// 
        /// NOTE: Not thread-safe.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="replyMemStream">Must be resizable MemoryStream.</param>
        /// <returns></returns>
        public uint ReadReply(Stream stream, MemoryStream replyMemStream)
        {
            uint messageLength = StartReadingReply(stream);

            if (replyMemStream.Capacity < messageLength)
                replyMemStream.Capacity = Convert.ToInt32(messageLength);

            ReadBytesToFixedBuffer(stream, replyMemStream.GetBuffer(), Convert.ToInt32(messageLength));

            return messageLength;
        }

        private static void ReadBytesToFixedBuffer(Stream stream, byte[] buffer, int lengthLimit = 0)
        {
            if (lengthLimit > buffer.Length)
                throw new ArgumentException($"{nameof(lengthLimit)} must be greater than buffer.Length", nameof(lengthLimit));

            int read = 0;
            int chunkRead;
            int lenghtToRead = (lengthLimit > 0) ? lengthLimit : buffer.Length;

            while ((chunkRead = stream.Read(buffer, read, lenghtToRead - read)) > 0)
            {
                read += chunkRead;

                if (read == lenghtToRead)
                    break;
            }
        }
    }
}
