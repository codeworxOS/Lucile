using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.ComponentModel
{
    public enum AssemblyValidationMode
    {
        None = 0x00,
        HasStrongName = 0x01,
        ValidateStrongName = 0x02
    }
}
