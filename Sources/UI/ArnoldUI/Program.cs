using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GoodAI.Logging;
using Serilog;
using Serilog.Events;
using SimpleInjector;

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

            Container container = ContainerConfig.Create();

            UnhandledExceptionCatcher.Log = container.GetInstance<ILog>();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }


    }
}
