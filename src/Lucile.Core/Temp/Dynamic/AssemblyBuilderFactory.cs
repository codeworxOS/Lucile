using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;

namespace Codeworx.Dynamic
{
    public abstract class AssemblyBuilderFactory
    {
        public abstract AssemblyBuilder GetAssemblyBuilder();

        public abstract string GetUniqueTypeName(Type baseType);

        public virtual bool CanPersist { get { return true; } }
    }
}
