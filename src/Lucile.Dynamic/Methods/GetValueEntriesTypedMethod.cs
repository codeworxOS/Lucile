using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Lucile.Dynamic.Convention;

namespace Lucile.Dynamic.Methods
{
    public class GetValueEntriesTypedMethod : DynamicMethod
    {
        private MethodInfo _interfaceMethod;

        internal GetValueEntriesTypedMethod(Type proxyType)
            : base("Lucile.Dynamic.ITransactionProxy.GetValueEntriesTyped", typeof(IEnumerable<>).MakeGenericType(typeof(IValueEntry<,>).MakeGenericType(typeof(object), proxyType)), false, typeof(string))
        {
            _interfaceMethod = typeof(ITransactionProxy<>).MakeGenericType(proxyType).GetMethod("GetValueEntries");
        }

        public override bool IsExplicitImplementation(System.Reflection.MethodInfo methodInfo)
        {
            return methodInfo == _interfaceMethod;
        }

        protected override void Implement(DynamicTypeBuilder config, System.Reflection.Emit.TypeBuilder typeBuilder, System.Reflection.Emit.ILGenerator il)
        {
            var convention = config.Conventions.OfType<TransactionProxyConvention>().First();
            var stringEqual = typeof(string).GetMethod("op_Equality", new Type[] { typeof(string), typeof(string) });
            var getValueEntriesMethod = typeof(TransactionProxyHelper).GetMethod("GetValueEntries");

            var propertyLabels = new Dictionary<TransactionProxyConvention.TransactionProxyProperty, Label>();
            var returnLabel = il.DefineLabel();
            var notFoundLabel = il.DefineLabel();

            foreach (var item in convention.TransactionProxyProperties)
            {
                var label = il.DefineLabel();
                propertyLabels.Add(item, label);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldstr, item.Property.Name);
                il.Emit(OpCodes.Call, stringEqual);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ceq);
                il.Emit(OpCodes.Brfalse, label);
                il.Emit(OpCodes.Nop);
            }

            il.Emit(OpCodes.Br, notFoundLabel);

            foreach (var item in propertyLabels)
            {
                var method = getValueEntriesMethod.MakeGenericMethod(item.Key.Property.PropertyType, convention.ItemType);
                il.MarkLabel(item.Value);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, item.Key.ValuesProperty.BackingField);
                il.EmitCall(OpCodes.Call, method, null);
                il.Emit(OpCodes.Br, returnLabel);
            }

            var exceptionType = typeof(ArgumentOutOfRangeException);

            il.MarkLabel(notFoundLabel);

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Newobj, exceptionType.GetConstructor(new Type[] { typeof(string) }));
            il.ThrowException(exceptionType);

            il.MarkLabel(returnLabel);
            il.Emit(OpCodes.Ret);
        }
    }
}