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
        private IResponseParser m_responseParser;
        private const int InitialBufferSize = 32 * 1024;

        public ConverseFlatBuffersClient(ITcpConnector connector, IResponseParser responseParser) : base(connector)
        {
            m_responseParser = responseParser;
        }

        public void SendMessage<TRequest>(string handlerName, TRequest messageBody)
            where TRequest : Table
        {
            ByteBuffer byteBuffer = messageBody.ByteBuffer;
            int dataLength = byteBuffer.Length - byteBuffer.Position;
            var buffer = new byte[dataLength];

            Buffer.BlockCopy(byteBuffer.Data, byteBuffer.Position, buffer, 0, dataLength);

            SendMessage(handlerName, buffer, buffer.Length);
        }

        public TResponse SendQuery<TRequest, TResponse>(string handlerName, TRequest messageBody)
            where TRequest : Table
            where TResponse : Table
        {
            var receiveStream = new MemoryStream(InitialBufferSize);

            ByteBuffer buffer = messageBody.ByteBuffer;
            SendQuery(handlerName, buffer.Data, receiveStream, buffer.Length);

            receiveStream.Position = 0;

            return m_responseParser.Parse<TResponse>(receiveStream.GetBuffer());
        }
    }
}
