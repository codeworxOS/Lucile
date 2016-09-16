using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Codeworx.Dynamic
{
    public class DynamicAssemblyBuilderFactory : AssemblyBuilderFactory
    {
        private Guid assemblyGuid;

        public override System.Reflection.Emit.AssemblyBuilder GetAssemblyBuilder()
        {
            this.assemblyGuid = Guid.NewGuid();
            var an = new AssemblyName(string.Format("Codeworx.Dynamic.Assembly_{0}", this.assemblyGuid));
#if (!SILVERLIGHT)
#if(DEBUGDYNAMIC)
            AssemblyBuilder builder = null;
            if (this.CanPersist) {
                builder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave);
            } else {
                builder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndCollect);
            }
#else
            AssemblyBuilder builder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
#endif
#else
            AssemblyBuilder builder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
#endif
            return builder;
        }

        public override string GetUniqueTypeName(Type baseType)
        {
            return string.Format("Codeworx.Dynamic_{0}.{1}_dynamic", this.assemblyGuid, baseType.Name);
        }
    }
}
