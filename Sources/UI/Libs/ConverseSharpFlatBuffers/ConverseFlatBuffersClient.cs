using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatBuffers;
using GoodAI.Net.ConverseSharp;

namespace GoodAI.Net.ConverseSharpFlatBuffers
{
    public interface IConverseFlatBuffersClient
    {
        void SendMessage<TRequest>(string handlerName, TRequest messageBody)
            where TRequest : Table;
        TResponse SendQuery<TRequest, TResponse>(string handlerName, TRequest messageBody)
            where TRequest : Table
            where TResponse : Table;
    }

    public interface IResponseParser
    {
        T Parse<T>(byte[] buffer) where T : class;
    }

    public class ConverseFlatBuffersClient : ConverseClient, IConverseFlatBuffersClient
    {
        private readonly IResponseParser m_responseParser;
        private const int InitialBufferSize = 512;

        public ConverseFlatBuffersClient(ITcpConnector connector, IResponseParser responseParser) : base(connector)
        {
            m_responseParser = responseParser;
        }

        /// <summary>
        /// Warning: There's one unnecessary copying of the data. You should fix it before sending large data too often.
        /// </summary>
        public void SendMessage<TRequest>(string handlerName, TRequest messageBody)
            where TRequest : Table
        {
            byte[] buffer = BufferConverter.Convert(messageBody.ByteBuffer);

            SendMessage(handlerName, buffer, buffer.Length);
        }

        /// <summary>
        /// Warning: There's one unnecessary copying of the data. You should fix it before sending large data too often.
        /// </summary>
        public TResponse SendQuery<TRequest, TResponse>(string handlerName, TRequest messageBody)
            where TRequest : Table
            where TResponse : Table
        {
            var receiveStream = new MemoryStream(InitialBufferSize);

            ByteBuffer byteBuffer = messageBody.ByteBuffer;

            byte[] buffer = BufferConverter.Convert(byteBuffer);

            SendQuery(handlerName, buffer, receiveStream, buffer.Length);

            receiveStream.Position = 0;

            return m_responseParser.Parse<TResponse>(receiveStream.GetBuffer());
        }
    }

    public static class BufferConverter
    {
        // TODO(Premek): Eliminate buffer copy. Add support for sending from the middle of a buffer to the lower layers.
        public static byte[] Convert(ByteBuffer source)
        {
            int dataLength = source.Length - source.Position;
            var destination = new byte[dataLength];
            
            Buffer.BlockCopy(source.Data, source.Position, destination, 0, destination.Length);

            return destination;
        }
    }
}
