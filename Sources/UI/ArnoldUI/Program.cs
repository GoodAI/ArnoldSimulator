using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ArnoldUI;
using GoodAI.Logging;
using GoodAI.TypeMapping;
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

            Container container = ContainerFactory.Create<ArnoldContainerConfig>();

            var log = container.GetInstance<ILog>();
            UnhandledExceptionCatcher.Log = log;
            log.Debug("Container configured");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // This should not fail as the container configuration verifies the setup.
            var uiMain = container.GetInstance<UIMain>();

            // NOTE(HonzaS): Consider this.
            //container.Dispose();

            log.Info("Application set up, starting");
            Application.Run(new MainForm(uiMain));
        }
    }
}
