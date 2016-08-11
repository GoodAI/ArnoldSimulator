using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Properties;

namespace GoodAI.Arnold.UserSettings
{
    internal static class AppSettings
    {
        internal static void SaveSettings(Action<Settings> action)
        {
            Settings settings = Settings.Default;

            action(settings);

            // The settings are only saved if no handlers threw anything.
            // This is because Settings is INotifyPropertyChanged and the app might crash as a result of a setting, 
            // and if it gets saved, it might cause the app to crash at startup next time.
            settings.Save();
        }
    }
}
