using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Dynamic
{
    public abstract class TypeBuilderConvention
    {
        public virtual void OnBuilderInitialized(DynamicTypeBuilder dynamicTypeBuilder) { }

        public virtual void OnMembersCreated(DynamicTypeBuilder dynamicTypeBuilder) { }
    }
}
