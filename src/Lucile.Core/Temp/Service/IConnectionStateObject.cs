using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Service
{
    public interface IConnectionStateObject
    {
        ConnectionState State { get; }

        event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;
    }
}
