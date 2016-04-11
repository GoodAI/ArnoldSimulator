using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;

namespace GoodAI.Net.ConverseSharp
{
    public interface IConverseProtoBufClient
    {
        void SendMessage<TRequest>(string handlerName, TRequest messageBody) where TRequest : IMessage;
        TResponse SendQuery<TRequest, TResponse>(string handlerName, TRequest messageBody)
            where TRequest : IMessage
            where TResponse : IMessage<TResponse>, new();
    }

    public class ConverseProtoBufClient : ConverseClient, IConverseProtoBufClient
    {
        private const int InitialBufferSize = 32 * 1024;

        // TODO(HonzaS): This was not thread safe, but now we're allocating a lot - optimize later.
        //private readonly MemoryStream m_sendingMemStream = new MemoryStream(InitialBufferSize);  // Resizable memory stream.
        //private readonly MemoryStream m_replyMemStream = new MemoryStream(InitialBufferSize);    // Resizable memory stream.

        public ConverseProtoBufClient(ITcpConnector connector) : base(connector)
        {}

        /// <summary>
        /// Serialize messageBody using ProtoBuf and send it via the ConversationClient. NOTE: Not thread safe!
        /// </summary>
        public void SendMessage<TRequest>(string handlerName, TRequest messageBody)
            where TRequest : IMessage
        {
            var sendStream = new MemoryStream(InitialBufferSize);

            SerializeMessage(messageBody, sendStream);

            SendMessage(handlerName, sendStream.GetBuffer(), Convert.ToInt32(sendStream.Length));
        }

        public TResponse SendQuery<TRequest, TResponse>(string handlerName, TRequest messageBody)
            where TResponse : IMessage<TResponse>, new()
            where TRequest : IMessage
        {
            var sendStream = new MemoryStream(InitialBufferSize);
            var receiveStream = new MemoryStream(InitialBufferSize);
            SerializeMessage(messageBody, sendStream);

            SendQuery(handlerName, sendStream.GetBuffer(), receiveStream,
                Convert.ToInt32(sendStream.Length));

            receiveStream.Position = 0;  // Read the stream from the beginning.

            var parser = new MessageParser<TResponse>(() => new TResponse());
            return parser.ParseFrom(receiveStream);
        }

        private void SerializeMessage<TRequest>(TRequest messageBody, MemoryStream sendStream) where TRequest : IMessage
        {
            //ResetSendingBuffer();

            messageBody.WriteTo(sendStream);
        }

        //private void ResetSendingBuffer()
        //{
        //    m_sendingMemStream.Position = 0;
        //    m_sendingMemStream.SetLength(0);
        //}
    }
}
