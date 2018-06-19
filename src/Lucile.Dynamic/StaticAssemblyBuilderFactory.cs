using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Lucile.Dynamic
{
    public class StaticAssemblyBuilderFactory : AssemblyBuilderFactory
    {
        private readonly string _assemblyName;
        private readonly object _builderLocker = new object();
        private Guid _assemblyGuid;
        private AssemblyBuilder _builder;

        public StaticAssemblyBuilderFactory(string assemblyName)
        {
            _assemblyName = assemblyName;
        }

        public override System.Reflection.Emit.AssemblyBuilder GetAssemblyBuilder()
        {
            if (_builder == null)
            {
                lock (_builderLocker)
                {
                    if (_builder == null)
                    {
                        this._assemblyGuid = Guid.NewGuid();
                        var an = new AssemblyName(_assemblyName);
#if DEBUGDYNAMIC
                        if (this.CanPersist) {
                            _builder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave);
                        } else {
                            _builder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
                        }
#else
                        _builder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
#endif
                    }
                }
            }

            return _builder;
        }

        public override string GetUniqueTypeName(Type baseType)
        {
            return $"Lucile.Dynamic.TransactionProxy.{baseType.Name}";
        }
    }
}