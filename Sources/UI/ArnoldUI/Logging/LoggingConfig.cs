using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GoodAI.Logging;
using Serilog;
using Serilog.Events;

namespace GoodAI.Arnold.Logging
{
    internal static class LoggingConfig
    {
        public static LoggerConfiguration Setup(RichTextBox textBox, string logPath = null)
        {
            try
            {
                string appName = typeof(Program).Namespace ?? "(default)";

                if (string.IsNullOrEmpty(logPath))
                {
                    // Log to 'GoodAI\Arnold\Logs'.
                    logPath = InitLogDirectory(appName.Replace('.', '\\'), "Logs");
                }

                return SerilogRobeConfig.SetupLoggingToFile(logPath, appName)
                    .WriteTo.RichTextBox(textBox, maxTextLength: 10000, restrictedToMinimumLevel: LogEventLevel.Debug);
            }
            catch (Exception ex)
            {
                // TODO: Print inner exceptions and stack traces
                MessageBox.Show(
                    "Logging setup failed. The application will not log.\n\n" +
                    $"Attempted log path: {logPath}\n\nException message: {ex.Message}",
                    "Logging initialization error.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            return null;
        }

        private static string InitLogDirectory(string subfolderPath, string logFolder)
        {
            string logPath = Path.Combine(GetDefaultLogPath(), subfolderPath, logFolder);

            Directory.CreateDirectory(logPath);

            return logPath;
        }

        private static string GetDefaultLogPath()
        {
            try
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }
            catch (Exception)
            {
                string path = Path.GetTempPath();

                MessageBox.Show($"Error getting application data path. Logging to alternative path: {path}");

                return path;
            }
        }
    }
}
