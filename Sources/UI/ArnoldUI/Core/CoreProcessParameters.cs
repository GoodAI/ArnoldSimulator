﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Core
{
    public class CoreProcessParameters
    {
        public bool IsValid => (MaybeSubstituteArguments() != null);

        public string WorkingDirectory { get; }

        public string SubstitutedArguments =>
            MaybeSubstituteArguments() ?? "Invalid arguments, please check Port setting.";

        public int? MaybePort { get; }

        private readonly string m_rawArguments;

        public CoreProcessParameters(string workingDirectory, string rawArguments, int? maybePort)
        {
            if (maybePort.HasValue &&
                ((maybePort.Value > IPEndPoint.MaxPort) || (maybePort.Value < IPEndPoint.MinPort)))
            {
                throw new ArgumentOutOfRangeException(nameof(maybePort),
                    $"{nameof(maybePort)} must be valid port number or null.");
            }

            WorkingDirectory = workingDirectory;
            m_rawArguments = rawArguments;
            MaybePort = maybePort;
        }

        private string MaybeSubstituteArguments()
        {
            if (m_rawArguments.Contains("{Port}"))
            {
                if (!MaybePort.HasValue)
                    return null;

                return m_rawArguments.Replace("{Port}", MaybePort.Value.ToString());
            }

            return m_rawArguments;  // Nothing to substitute.
        }
    }
}