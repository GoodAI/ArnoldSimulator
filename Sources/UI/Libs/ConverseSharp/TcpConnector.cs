using System;
using System.IO;
using System.Net.Sockets;

namespace GoodAI.Net.ConverseSharp
{
    public interface ITcpConnector
    {
        Stream GetConnectedStream();
        void Close();
    }

    public class NetworkException : Exception
    {
        public NetworkException(string message) : base(message)
        {}
    }

    public class TcpConnector : ITcpConnector
    {
        private TcpClient m_tcpClient;

        private readonly string m_hostName;
        private readonly int m_port;
        private readonly int m_timeoutMs;

        public TcpConnector(string hostName, int port, int timeoutMs)
        {
            m_hostName = hostName;
            m_port = port;
            m_timeoutMs = timeoutMs;
        }

        public Stream GetConnectedStream()
        {
            // ReSharper disable once InvertIf
            if (m_tcpClient == null)
            {
                m_tcpClient = new TcpClient();  // TODO(Premek): Connect without allocations?

                bool connected = m_tcpClient.ConnectAsync(m_hostName, m_port).Wait(m_timeoutMs);
                if (!connected)
                    throw new NetworkException("Unable to connect within timeout");
            }

            return m_tcpClient.GetStream();
        }

        public void Close()
        {
            m_tcpClient.Close();  // Disposes the TcpClient.
            m_tcpClient = null;
        }
    }

    internal class DummyConnector : ITcpConnector
    {
        private readonly Stream m_stream;

        public bool IsConnected { get; private set; }

        public Action<Stream> ImplantMessage { get; set; }

        public DummyConnector(Stream stream)
        {
            m_stream = stream;
        }

        public Stream GetConnectedStream()
        {
            IsConnected = true;

            if (ImplantMessage != null)
                ImplantMessage(m_stream);

            return m_stream;
        }

        public void Close()
        {
            IsConnected = false;
        }
    }
}