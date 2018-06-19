using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Lucile.Dynamic.Convention;

namespace Lucile.Dynamic.Methods
{
    public class SetTransactionProxyTypedTargetsMethod : DynamicMethod
    {
        private readonly Type _proxyType;
        private MethodInfo _interfaceMethod;

        internal SetTransactionProxyTypedTargetsMethod(Type proxyType)
            : base(Guid.NewGuid().ToString(), typeof(void), false, typeof(IEnumerable<>).MakeGenericType(proxyType))
        {
            _proxyType = proxyType;
        }

        public override bool IsExplicitImplementation(System.Reflection.MethodInfo methodInfo)
        {
            if (_interfaceMethod == null)
            {
                _interfaceMethod = typeof(ITransactionProxy<>).MakeGenericType(_proxyType).GetMethod("SetTargets");
            }

            return methodInfo == _interfaceMethod;
        }

        protected override void Implement(DynamicTypeBuilder config, System.Reflection.Emit.TypeBuilder typeBuilder, System.Reflection.Emit.ILGenerator il)
        {
            var convention = config.Conventions.OfType<TransactionProxyConvention>().First();

            var listCtor = convention.TargetsField.MemberType.GetConstructor(new[] { typeof(IEnumerable<>).MakeGenericType(convention.ItemType) });

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Newobj, listCtor);
            il.Emit(OpCodes.Stfld, convention.TargetsField.Field);
            il.Emit(OpCodes.Ldarg_0);
            il.EmitCall(OpCodes.Callvirt, typeof(ICommitable).GetMethod("Rollback"), null);
            il.Emit(OpCodes.Ret);
        }
    }
}