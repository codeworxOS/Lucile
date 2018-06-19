using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Lucile.Dynamic.Convention;

namespace Lucile.Dynamic.Methods
{
    public class GetValueEntriesMethod : DynamicMethod
    {
        private readonly MethodInfo _interfaceMethod;

        internal GetValueEntriesMethod()
            : base("Lucile.Dynamic.ITransactionProxy.GetValueEntries", typeof(IEnumerable<IValueEntry<object, object>>), false, typeof(string))
        {
            _interfaceMethod = typeof(ITransactionProxy).GetMethod("GetValueEntries");
        }

        public override bool IsExplicitImplementation(System.Reflection.MethodInfo methodInfo)
        {
            return methodInfo == _interfaceMethod;
        }

        protected override void Implement(DynamicTypeBuilder config, System.Reflection.Emit.TypeBuilder typeBuilder, System.Reflection.Emit.ILGenerator il)
        {
            var convention = config.Conventions.OfType<TransactionProxyConvention>().First();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.EmitCall(OpCodes.Callvirt, convention.GetValueEntiresTypedMethod.Method, null);
            il.Emit(OpCodes.Ret);
        }
    }
}