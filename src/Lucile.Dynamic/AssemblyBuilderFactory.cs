using System;
using System.Reflection.Emit;

namespace Lucile.Dynamic
{
    public abstract class AssemblyBuilderFactory
    {
        public virtual bool CanPersist => true;

        public abstract AssemblyBuilder GetAssemblyBuilder();

        public abstract string GetUniqueTypeName(Type baseType);
    }
}