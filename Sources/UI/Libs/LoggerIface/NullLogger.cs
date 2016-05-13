using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Logging
{
    public class NullLogger : ILog
    {
        public bool FailOnUse { get; set; }

        private static NullLogger m_instance;

        public static NullLogger Instance => m_instance ?? (m_instance = new NullLogger());

        public void Add(Severity severity, string template, params object[] objects)
        {
            ThrowOrIgnore();
        }

        public void Add(Severity severity, Exception ex, string template, params object[] objects)
        {
            ThrowOrIgnore();
        }

        private void ThrowOrIgnore()
        {
            if (FailOnUse)
                throw new InvalidOperationException($"NullLogger has been forbidden and used");
        }
    }
}
