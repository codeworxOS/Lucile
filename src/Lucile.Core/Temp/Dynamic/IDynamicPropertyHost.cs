using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Dynamic
{
    public interface IDynamicPropertyHost
    {
        object GetValue(string propertyName);

        void SetValue(string propertyName, object value);
    }
}
