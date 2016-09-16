using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Service
{
    public enum ConnectionState
    {
        None = 0x00,
        Connected = 0x01,
        Disconnected = 0x02,
        Pending = 0x04,
        Checking = 0x08
    }
}
