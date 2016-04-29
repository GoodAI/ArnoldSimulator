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
            UnhandledExceptionCatcher.RegisterHandlers();

            // This injects logger to UnhandledExceptionCatcher (maybe do it less statically?)
            LoggingConfig.Setup();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }


    }
}
