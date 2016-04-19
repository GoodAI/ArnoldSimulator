using System;
using System.IO;

namespace GoodAI.Net.ConverseSharp
{
    public class ConverseClient
    {
        public const int MessageHeaderLength = ConverseWriter.HeaderLength;

        private readonly ITcpConnector m_connector;
        private readonly ConverseWriter m_converseWriter = new ConverseWriter();
        private readonly ConverseReader m_converseReader = new ConverseReader();

        public ConverseClient(ITcpConnector connector)
        {
            m_connector = connector;
        }

        public void SendMessage(string handlerName, byte[] messageBody, int realBodyLength = 0)
        {
            WriteMessage(m_connector.GetConnectedStream(), handlerName, messageBody, realBodyLength);

            m_connector.Close();
        }

        public void SendQuery(string handlerName, byte[] messageBody, ref byte[] replyBuffer, int realBodyLength = 0)
        {
            Stream stream = m_connector.GetConnectedStream();
            WriteMessage(stream, handlerName, messageBody, realBodyLength);

            m_converseReader.ReadReply(stream, ref replyBuffer);

            m_connector.Close();
        }

        public void SendQuery(string handlerName, byte[] messageBody, MemoryStream replyMemStream, int realBodyLength = 0)
        {
            Stream stream = m_connector.GetConnectedStream();
            WriteMessage(stream, handlerName, messageBody, realBodyLength);

            m_converseReader.ReadReply(stream, replyMemStream);

            m_connector.Close();
        }

        private void WriteMessage(Stream stream, string handlerName, byte[] messageBody, int realBodyLength)
        {
            m_converseWriter.WriteMessage(stream, 0, handlerName, messageBody, realBodyLength);
        }
    }
}
