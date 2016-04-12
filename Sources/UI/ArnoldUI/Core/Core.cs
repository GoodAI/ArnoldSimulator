using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ArnoldUI.Core
{
    public interface ICore : IDisposable
    {
        IPEndPoint Start();
    }
    
    public class LocalCore : ICore
    {
        private const string CoreProcessDirectory = "../Core/ArnoldCore.exe";

        private Process m_process;

        public IPEndPoint Start()
        {
            var processStartInfo = new ProcessStartInfo
            {
                CreateNoWindow = !Debugger.IsAttached,
                FileName = CoreProcessDirectory
            };
            m_process = new Process {StartInfo = processStartInfo};
            m_process.Start();

            IPAddress address;
            int port;
            ReadEndpoint(m_process.StandardOutput, out address, out port);

            return new IPEndPoint(address, port);
        }

        private void ReadEndpoint(StreamReader standardOutput, out IPAddress address, out int port)
        {
            // TODO(HonzaS): Parse the address and port from standardOutput.
            // Throw exception when the parsing fails or detects core not being able to start.
            address = new IPAddress(new byte[] {127, 0, 0, 1});
            port = 1337;
        }

        public void Dispose()
        {
            m_process.Kill();
        }
    }

    public class RemoteCore : ICore
    {
        private readonly IPEndPoint m_endPoint;

        public RemoteCore(IPEndPoint endPoint)
        {
            m_endPoint = endPoint;
        }

        public void Dispose()
        {
        }

        public IPEndPoint Start()
        {
            return m_endPoint;
        }
    }
}
