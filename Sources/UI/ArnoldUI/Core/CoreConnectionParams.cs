using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Core
{
    public interface ICoreConnectionParams
    {
        bool IsCoreLocal { get; }

        EndPoint EndPoint { get; }

        CoreProcessParams CoreProcessParams { get; }

        bool IsValid { get; }
    }

    public abstract class CoreConnectionParamsBase : ICoreConnectionParams
    {
        public bool IsCoreLocal { get; protected set; }

        public EndPoint EndPoint { get; private set; }

        public CoreProcessParams CoreProcessParams { get; protected set; }

        public bool IsValid => (IsEndPointValid() && (!IsCoreLocal || CoreProcessParams.IsValid));

        private bool m_portIsValid;

        protected void SetEndPoint(string hostname, int? maybePort)
        {
            if (maybePort.HasValue &&
                ((maybePort.Value > IPEndPoint.MaxPort) || (maybePort.Value < IPEndPoint.MinPort)))
            {
                throw new ArgumentOutOfRangeException(nameof(maybePort),
                    $"{nameof(maybePort)} must be valid port number or null.");
            }

            m_portIsValid = maybePort.HasValue;

            EndPoint = new EndPoint(hostname, maybePort ?? -1);
        }

        private bool IsEndPointValid()
        {
            // TODO(Premek)
            return m_portIsValid && EndPoint.Hostname.Length > 0;
        }

    }

    /// <summary>
    /// Local core variant.
    /// </summary>
    public class LocalCoreConnectionParams : CoreConnectionParamsBase
    {
        public LocalCoreConnectionParams(string workingDirectory, string rawArguments, int? maybePort)
        {
            IsCoreLocal = true;

            SetEndPoint("localhost", maybePort);

            CoreProcessParams = new CoreProcessParams(workingDirectory, rawArguments, maybePort);
        }
    }

    /// <summary>
    /// Remote core variant.
    /// </summary>
    public class RemoteCoreConnectionParams : CoreConnectionParamsBase
    {
        public RemoteCoreConnectionParams(string hostname, int? maybePort)
        {
            IsCoreLocal = false;

            SetEndPoint(hostname, maybePort);
        }
    }
}
