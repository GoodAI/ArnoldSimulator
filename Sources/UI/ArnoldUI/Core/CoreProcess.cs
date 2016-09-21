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
        public string Hostname { get; private set; }
        public int Port { get; private set; }

        public EndPoint(string hostname, int port)
        {
            Hostname = hostname;
            Port = port;
        }
    }

    public interface ICoreProcess : IDisposable
    {
        EndPoint EndPoint { get; }
    }

    public class CoreProcess : ICoreProcess
    {
        public const int ShutdownTimeoutMs = 5000;

        private static readonly string CoreProcessDirectory = Path.GetFullPath("../../../../core/core/debug");
        //private static readonly string CoreProcessDirectory = Path.GetFullPath("C:/arnold");

        private const string CoreProcessExecutable = "charmrun.exe";
        private const string CoreHostname = "localhost";
        private const int CorePort = 46324;

        private static readonly string CoreProcessParameters =
            $"core +p4 ++ppn 4 +noisomalloc +LBCommOff +balancer DistributedLB +cs +ss ++verbose ++server ++server-port {CorePort}";
            //$"core +p4 ++ppn 4 +noisomalloc +LBCommOff +balancer DistributedLB +restart checkpoint +cs +ss ++verbose ++server ++server-port {CorePort}";
            //$"core +p8 ++ppn 4 +noisomalloc +LBCommOff +balancer DistributedLB ++nodelist nodelist.txt +cs +ss ++verbose ++server ++server-port {CorePort}";
            //$"core +p8 ++ppn 4 +noisomalloc +LBCommOff +balancer DistributedLB ++nodelist nodelist.txt +restart checkpoint +cs +ss ++verbose ++server ++server-port {CorePort}";

        private readonly Process m_process;

        public CoreProcess()
        {
            m_process = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = !Debugger.IsAttached,
                    WorkingDirectory = CoreProcessDirectory,
                    FileName = CoreProcessExecutable,
                    Arguments = CoreProcessParameters
                }
            };
            m_process.Start();
        }

        public void Dispose()
        {
            if(!m_process.WaitForExit(ShutdownTimeoutMs))
                m_process.Kill();

            m_process.Dispose();
        }

        public EndPoint EndPoint => new EndPoint(CoreHostname, CorePort);
    }
}