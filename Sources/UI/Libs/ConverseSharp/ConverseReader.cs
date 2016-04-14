using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;

namespace GoodAI.Net.ConverseSharp
{
    public class ConverseReader
    {
        private const int HeaderSize = 4;

        /// <summary>
        /// Reads the header and leaves the rest of the stream for reading in other ways (such as ProtoBufs).
        /// NOTE: Not thread-safe.
        /// </summary>
        /// <param name="stream">A connected stream.</param>
        public uint ReadInteger(Stream stream)
        {
            var buffer = new byte[4];

            ReadBytesToFixedBuffer(stream, ref buffer);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer);

            return BitConverter.ToUInt32(buffer, 0);
        }

        /// <summary>
        /// 
        /// NOTE: Not thread-safe.
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="replyBuffer"></param>
        /// <returns></returns>
        public uint ReadReply(Stream inputStream, ref byte[] replyBuffer)
        {
            uint messageLength = ReadInteger(inputStream);

            // TODO(Premek): warn if the length is suspiciously large, such as > 100 MB

            if (replyBuffer.Length < messageLength)
                replyBuffer = new byte[messageLength];

            ReadBytesToFixedBuffer(inputStream, ref replyBuffer, Convert.ToInt32(messageLength));

            return messageLength;
        }

        /// <summary>
        /// 
        /// NOTE: Not thread-safe.
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="replyMemoryStream">Must be resizable MemoryStream.</param>
        /// <returns></returns>
        public uint ReadReply(Stream inputStream, MemoryStream replyMemoryStream)
        {
            uint messageLength = ReadInteger(inputStream);

            if (replyMemoryStream.Capacity < messageLength)
                replyMemoryStream.Capacity = Convert.ToInt32(messageLength);

            byte[] buffer = replyMemoryStream.GetBuffer();
            ReadBytesToFixedBuffer(inputStream, ref buffer, Convert.ToInt32(messageLength));

            return messageLength;
        }

        public RequestDetails ReadRequest(Stream inputStream, MemoryStream requestMemoryStream)
        {
            var details = new RequestDetails();
            details.MessageLength = ReadInteger(inputStream);

            if (requestMemoryStream.Capacity < details.MessageLength)
                requestMemoryStream.Capacity = Convert.ToInt32(details.MessageLength);

            details.ProcessorNumber = ReadInteger(inputStream);

            details.HandlerName = ReadHandlerName(inputStream);

            byte[] buffer = requestMemoryStream.GetBuffer();
            ReadBytesToFixedBuffer(inputStream, ref buffer, Convert.ToInt32(details.MessageLength));

            return details;
        }

        internal static string ReadHandlerName(Stream inputStream)
        {
            const int handlerNameLength = 32;

            var buffer = new byte[handlerNameLength];
            inputStream.Read(buffer, 0, handlerNameLength);

            string fullName = Encoding.ASCII.GetString(buffer);
            string trimmedName = fullName.Substring(0, fullName.IndexOf('\0'));

            return trimmedName;
        }

        private static void ReadBytesToFixedBuffer(Stream stream, ref byte[] buffer, int lengthLimit = 0)
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

        public class RequestDetails
        {
            public uint MessageLength { get; set; }
            public uint ProcessorNumber { get; set; }
            public string HandlerName { get; set; }
        }
    }
}
