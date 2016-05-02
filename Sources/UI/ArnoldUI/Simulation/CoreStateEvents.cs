using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Simulation;

namespace GoodAI.Arnold.Simulation
{
    public class StateUpdatedEventArgs : EventArgs
    {
        public CoreState PreviousState { get; set; }
        public CoreState CurrentState { get; set; }

        public StateUpdatedEventArgs(CoreState previousState, CoreState currentState)
        {
            PreviousState = previousState;
            CurrentState = currentState;
        }
    }

    public class StateChangeFailedEventArgs : EventArgs
    {
        public StateChangeFailedEventArgs(ErrorResponse error)
        {
            Error = error;
        }

        public ErrorResponse Error { get; set; }
    }
}
