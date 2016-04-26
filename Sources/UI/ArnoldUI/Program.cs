using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GoodAI.Logging;
using Serilog;
using Serilog.Events;

namespace GoodAI.Arnold
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            UnhandledExceptionCatcher.RegisterHandlers();  // TODO(P): prop. inject logger after it is initialized

            SetupLogging();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private static void SetupLogging(string logPath = null)
        {
            try
            {
                string appName = AppDomain.CurrentDomain.FriendlyName;

                if (string.IsNullOrEmpty(logPath))
                {
                    logPath = InitLogDirectory(appName);
                }

                SerilogRobeConfig.SetupLoggingToFile(logPath, appName);

                UnhandledExceptionCatcher.Log = SerilogRobe.CreateLogger();
            }
            catch (Exception ex)
            {
                // TODO: Print inner exceptions and stack traces
                MessageBox.Show(
                    "Logging setup failed. The application will not log.\n\n" +
                    $"Attempted log path:{logPath}\n\n{ex.Message}",
                    "Logging initialization error.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private static string InitLogDirectory(string appName)
        {
            string logPath = Path.Combine(GetDefaultLogPath(), appName);

            Directory.CreateDirectory(logPath);

            return logPath;
        }

        private static string GetDefaultLogPath()
        {
            try
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
            catch (Exception)
            {
                return Path.GetTempPath();
            }
        }
    }
}
