using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Logging
{
    public class NullLogger : ILog
    {
        private static ILog m_instance;

        public static ILog Instance => m_instance ?? (m_instance = new NullLogger());

        public void Add(Severity severity, string template, params object[] objects)
        {}

        public void Add(Severity severity, Exception ex, string template, params object[] objects)
        {}
    }
}
