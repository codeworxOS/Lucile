using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Dynamic.Convention
{

    public interface IProxyConvention
    {
        DynamicProperty ProxyTarget { get; }
    }
}
