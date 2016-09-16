using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Dynamic.Convention
{
    public abstract class DynamicTypeConvention
    {
        public abstract void Apply(DynamicTypeBuilder typeBuilder);   
    }
}
