using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace GoodAI.Net.ConverseSharp
{
    /// <summary>
    /// Not thread safe! Do not share one instance among multiple threads.
    /// </summary>
    public class ConverseProtoBufClient : ConverseClient
    {
        internal const ProtoBuf.PrefixStyle ProtoBufPrefixStyle = PrefixStyle.Fixed32;

        private const int InitialBufferSize = 32 * 1024;

        private readonly MemoryStream m_sendingMemStream = new MemoryStream(InitialBufferSize);  // Resizable memory stream.
        private readonly MemoryStream m_replyMemStream = new MemoryStream(InitialBufferSize);    // Resizable memory stream.

        public ConverseProtoBufClient(ITcpConnector connector) : base(connector)
        {}

        /// <summary>
        /// Serialize messageBody using ProtoBuf and send it via the ConversationClient. NOTE: Not thread safe!
        /// </summary>
        public void SendMessage<TRequest>(string handlerName, TRequest messageBody)
        {
            SerializeMessage(messageBody);

            SendMessage(handlerName, m_sendingMemStream.GetBuffer(), Convert.ToInt32(m_sendingMemStream.Length));
        }

        public TResponse SendQuery<TRequest, TResponse>(string handlerName, TRequest messageBody)
        {
            SerializeMessage(messageBody);

            SendQuery(handlerName, m_sendingMemStream.GetBuffer(), m_replyMemStream,
                Convert.ToInt32(m_sendingMemStream.Length));

            m_replyMemStream.Position = 0;  // Read the stream from the beginning.

            return Serializer.Deserialize<TResponse>(m_replyMemStream);
        }

        private void SerializeMessage<TRequest>(TRequest messageBody)
        {
            ResetSendingBuffer();

            Serializer.SerializeWithLengthPrefix(m_sendingMemStream, messageBody, ProtoBufPrefixStyle);
        }

        private void ResetSendingBuffer()
        {
            m_sendingMemStream.Position = 0;
            m_sendingMemStream.SetLength(0);
        }
    }
}
