using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Lucile.Dynamic.Convention;

namespace Lucile.Dynamic.Methods
{
    public class GetTransactionProxyTargetsMethod : DynamicMethod
    {
        private MethodInfo _interfaceMethod;

        internal GetTransactionProxyTargetsMethod()
            : base(Guid.NewGuid().ToString(), typeof(IEnumerable<object>), false)
        {
            _interfaceMethod = typeof(ITransactionProxy).GetProperty("Targets").GetGetMethod();
        }

        public override bool IsExplicitImplementation(System.Reflection.MethodInfo methodInfo)
        {
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