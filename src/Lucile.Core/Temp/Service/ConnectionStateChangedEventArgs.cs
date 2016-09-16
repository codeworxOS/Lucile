using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Service
{
    public class ConnectionStateChangedEventArgs : EventArgs
    {
        public ConnectionStateChangedEventArgs(ConnectionState oldState, ConnectionState newState)
        {
            this.OldState = oldState;
            this.NewState = newState;
        }

        public ConnectionState OldState { get; private set; }

        public ConnectionState NewState { get; private set; }
    }
}
