using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Logging
{
    public enum Sev
    {
        Error,
        Warn,
        Info,
        Debug,
        Verbose
    }

    public interface ILog
    {
        void Add(Sev severity, string template, params object[] objects);
        void Add(Sev severity, Exception ex, string template, params object[] objects);
    }
}
