using System;
using System.IO;
using System.Net.Sockets;

namespace GoodAI.Net.ConverseSharp
{
    public interface ITcpConnector
    {
        IConnectedStream GetConnectedStream();
    }

    public interface IConnectedStream : IDisposable
    {
        Stream Stream { get; }
    }

    public class NetworkException : Exception
    {
        public NetworkException(string message) : base(message)
        {}
    }

    public class TcpConnectedStream : IConnectedStream
    {
        private readonly TcpClient m_tcpClient;

        public TcpConnectedStream(string hostName, int port, int timeoutMs)
        {
            m_tcpClient = new TcpClient();

            bool connected = m_tcpClient.ConnectAsync(hostName, port).Wait(timeoutMs);
            if (!connected)
                throw new NetworkException("Unable to connect within timeout");
        }

        public void Dispose()
        {
            m_tcpClient.Close();
            m_tcpClient.Dispose();
        }

        public Stream Stream => m_tcpClient.GetStream();
    }

    public class TcpConnector : ITcpConnector
    {
        private readonly string m_hostName;
        private readonly int m_port;
        private readonly int m_timeoutMs;

        public TcpConnector(string hostName, int port, int timeoutMs)
        {
            m_hostName = hostName;
            m_port = port;
            m_timeoutMs = timeoutMs;
        }

        public IConnectedStream GetConnectedStream()
        {
            return new TcpConnectedStream(m_hostName, m_port, m_timeoutMs);
        }
    }

    public class DummyConnectedStream : IConnectedStream
    {
        public bool IsDisposed { get; set; }

        public DummyConnectedStream(Stream stream)
        {
            Stream = stream;
        }

        public Stream Stream { get; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    public class DummyConnector : ITcpConnector
    {
        private readonly Stream m_stream;
        public DummyConnectedStream ConnectedStream { get; private set; }

        public Action<Stream> ImplantMessage { get; set; }

        public DummyConnector(Stream stream)
        {
            m_stream = stream;
        }

        public IConnectedStream GetConnectedStream()
        {
            ImplantMessage?.Invoke(m_stream);

            ConnectedStream = new DummyConnectedStream(m_stream);
            return ConnectedStream;
        }
    }
}