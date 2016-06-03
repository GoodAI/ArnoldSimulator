using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Communication;

namespace GoodAI.Arnold.Core
{
    public class StateChangedEventArgs : EventArgs
    {
        public CoreState PreviousState { get; set; }
        public CoreState CurrentState { get; set; }

        public StateChangedEventArgs(CoreState previousState, CoreState currentState)
        {
            PreviousState = previousState;
            CurrentState = currentState;
        }
    }

    public class StateChangeFailedEventArgs : EventArgs
    {
        public StateChangeFailedEventArgs(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public string ErrorMessage { get; }
    }
}
