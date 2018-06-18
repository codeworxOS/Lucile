using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Lucile.Dynamic.Convention;

namespace Lucile.Dynamic.Methods
{
    public class SetTransactionProxyTargetsMethod : DynamicMethod
    {
        private MethodInfo _interfaceMethod;

        internal SetTransactionProxyTargetsMethod()
            : base(Guid.NewGuid().ToString(), typeof(void), false, typeof(IEnumerable<object>))
        {
            _interfaceMethod = typeof(ITransactionProxy).GetMethod("SetTargets");
        }

        public override bool IsExplicitImplementation(System.Reflection.MethodInfo methodInfo)
        {
            return methodInfo == _interfaceMethod;
        }

        protected override void Implement(DynamicTypeBuilder config, System.Reflection.Emit.TypeBuilder typeBuilder, System.Reflection.Emit.ILGenerator il)
        {
            var convention = config.Conventions.OfType<TransactionProxyConvention>().First();

            var method = typeof(ITransactionProxy<>).MakeGenericType(config.BaseType).GetMethod("SetTargets");

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Castclass, typeof(IEnumerable<>).MakeGenericType(config.BaseType));
            il.EmitCall(OpCodes.Callvirt, method, null);
            il.Emit(OpCodes.Ret);
        }
    }
}