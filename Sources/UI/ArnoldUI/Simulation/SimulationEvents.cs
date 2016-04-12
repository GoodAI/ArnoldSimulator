using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Simulation;

namespace ArnoldUI.Simulation
{
    public class StateUpdatedEventArgs : EventArgs
    {
        public SimulationState PreviousState { get; set; }
        public SimulationState CurrentState { get; set; }

        public StateUpdatedEventArgs(SimulationState previousState, SimulationState currentState)
        {
            PreviousState = previousState;
            CurrentState = currentState;
        }
    }

    public class StateChangeFailedEventArgs : EventArgs
    {
        public StateChangeFailedEventArgs(Error error)
        {
            Error = error;
        }

        public Error Error { get; set; }
    }
}
