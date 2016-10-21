using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Logging;

namespace GoodAI.Arnold.Core
{
    public interface ICharmdRunner : IDisposable
    {
        Task StartIfNotRunningAndWaitAsync(string charmdPath, int waitTimeMs);
    }

    public class CharmdRunner : ICharmdRunner
    {
        // Injected.
        public ILog Log { get; set; } = NullLogger.Instance;

        private Process m_charmdProcess;

        /// '.exe' not necessary and breaks detection of already running process, @see IsAnyCharmProcessRunning()
        private readonly string CharmdExecutable = "charmd";

        public async Task StartIfNotRunningAndWaitAsync(string charmdPath, int waitTimeMs)
        {
            if (m_charmdProcess != null)
            {
                if (!m_charmdProcess.HasExited)
                    return;  // Already running.

                m_charmdProcess.Dispose();
            }

            if (IsAnyCharmdProcessRunning())
            {
                Log.Debug($"Some {CharmdExecutable} detected. We will not run a new one.");
                return;
            }

            m_charmdProcess = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = !Debugger.IsAttached,
                    WorkingDirectory = Path.GetFullPath(charmdPath),
                    FileName = CharmdExecutable
                }
            };

            Log.Info($"Starting {CharmdExecutable}");

            try
            {
                m_charmdProcess.Start();

                await Task.Delay(waitTimeMs);
            }
            catch (Exception ex)
            {
                Log.Error($"Error running {CharmdExecutable} ({ex.Message}). You can try to run it manually.");
            }
        }

        public void Dispose()
        {
            if (m_charmdProcess == null)
                return;

            if (!m_charmdProcess.HasExited)
                m_charmdProcess.Kill();

            m_charmdProcess.Dispose();
        }

        private bool IsAnyCharmdProcessRunning()
        {
            // Note: ProcessName does not contain the '.exe' extension
            return Process.GetProcesses().Any(p => p.ProcessName.Contains(CharmdExecutable));
        }
    }
}
