using GoodAI.Net.ConverseSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConverseManualTest
{
    internal class ChatClient
    {
        private ConverseClient m_converseClient;

        public ChatClient(string hostName, int port)
        {
            m_converseClient = new ConverseClient(new TcpConnector(hostName, port, timeoutMs: 3000));
        }

        public string SendQuery(string query)
        {
            var replyMemStream = new MemoryStream();

            // Null-terminate the string to work with the converse server example.
            m_converseClient.SendQuery("ping", Encoding.ASCII.GetBytes(query + Char.MinValue), replyMemStream);

            return Encoding.ASCII.GetString(replyMemStream.GetBuffer());
        }
    }
}
