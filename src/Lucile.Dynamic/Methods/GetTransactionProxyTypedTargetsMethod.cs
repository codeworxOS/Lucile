using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Lucile.Dynamic.Convention;

namespace Lucile.Dynamic.Methods
{
    public class GetTransactionProxyTypedTargetsMethod : DynamicMethod
    {
        private MethodInfo _interfaceMethod;

        internal GetTransactionProxyTypedTargetsMethod(Type proxyType)
            : base(Guid.NewGuid().ToString(), typeof(IEnumerable<>).MakeGenericType(proxyType), false)
        {
        }

        public override bool IsExplicitImplementation(System.Reflection.MethodInfo methodInfo)
        {
            if (_interfaceMethod == null)
            {
                _interfaceMethod = typeof(ITransactionProxy<>).MakeGenericType(this.DynamicTypeBuilder.BaseType).GetProperty("Targets").GetGetMethod();
            }

            return methodInfo == _interfaceMethod;
        }

        protected override void Implement(DynamicTypeBuilder config, System.Reflection.Emit.TypeBuilder typeBuilder, System.Reflection.Emit.ILGenerator il)
        {
            var convention = config.Conventions.OfType<TransactionProxyConvention>().First();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, convention.TargetsField.Field);
            il.Emit(OpCodes.Ret);
        }
    }
}