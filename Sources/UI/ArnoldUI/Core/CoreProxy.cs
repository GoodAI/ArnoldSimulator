using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Core
{
    public class EndPoint
    {
        public string Hostname { get; set; }
        public int Port { get; set; }

        public EndPoint(string hostname, int port)
        {
            Hostname = hostname;
            Port = port;
        }
    }

    public interface ICoreProxy : IDisposable
    {
        EndPoint Start();
    }
    
    public class LocalCoreProxy : ICoreProxy
    {
        private const string CoreProcessDirectory = "../Core/ArnoldCore.exe";

        private Process m_process;

        public EndPoint Start()
        {
            var processStartInfo = new ProcessStartInfo
            {
                CreateNoWindow = !Debugger.IsAttached,
                FileName = CoreProcessDirectory
            };
            m_process = new Process {StartInfo = processStartInfo};
            m_process.Start();

            string hostname;
            int port;
            ReadEndpoint(m_process.StandardOutput, out hostname, out port);

            return new EndPoint(hostname, port);
        }

        private void ReadEndpoint(StreamReader standardOutput, out string hostname, out int port)
        {
            // TODO(HonzaS): Parse the address and port from standardOutput.
            // Throw exception when the parsing fails or detects core not being able to start.
            hostname = "localhost";
            port = 1337;
        }

        public void Dispose()
        {
            m_process.Kill();
        }
    }

    public class RemoteCoreProxy : ICoreProxy
    {
        private readonly EndPoint m_endPoint;

        public RemoteCoreProxy(EndPoint endPoint)
        {
            m_endPoint = endPoint;
        }

        public void Dispose()
        {
        }

        public EndPoint Start()
        {
            return m_endPoint;
        }
    }
}
