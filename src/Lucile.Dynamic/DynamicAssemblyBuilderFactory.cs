using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Lucile.Dynamic
{
    public class DynamicAssemblyBuilderFactory : AssemblyBuilderFactory
    {
        private Guid _assemblyGuid;

        public override System.Reflection.Emit.AssemblyBuilder GetAssemblyBuilder()
        {
            this._assemblyGuid = Guid.NewGuid();
            var an = new AssemblyName(string.Format("Lucile.Dynamic.Assembly_{0}", this._assemblyGuid));

#if DEBUGDYNAMIC
            AssemblyBuilder builder = null;
            if (this.CanPersist) {
                builder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave);
            } else {
                builder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndCollect);
            }
#else
            AssemblyBuilder builder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
#endif

            return builder;
        }

        public override string GetUniqueTypeName(Type baseType)
        {
            return string.Format("Lucile.Dynamic_{0}.{1}_dynamic", this._assemblyGuid, baseType.Name);
        }
    }
}