using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Synchronization
{
    public enum ChangeState
    {
        Inserted = 0x00,
        Modified = 0x01,
        Deleted = 0x02,
        Initial = 0x04
    }
}
