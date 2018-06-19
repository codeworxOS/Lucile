using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Lucile.Dynamic.Convention;

namespace Lucile.Dynamic.Methods
{
    public class HasMultipleValuesMethod : DynamicMethod
    {
        private MethodInfo _interfaceMethod;

        internal HasMultipleValuesMethod()
            : base("Lucile.Dynamic.ITransactionProxy.HasMultipleValues", typeof(bool), false, typeof(string))
        {
            _interfaceMethod = typeof(ITransactionProxy).GetMethod("HasMultipleValues");
        }

        public override bool IsExplicitImplementation(System.Reflection.MethodInfo methodInfo)
        {
            return methodInfo == _interfaceMethod;
        }

        protected override void Implement(DynamicTypeBuilder config, System.Reflection.Emit.TypeBuilder typeBuilder, System.Reflection.Emit.ILGenerator il)
        {
            var convention = config.Conventions.OfType<TransactionProxyConvention>().First();
            var stringEqual = typeof(string).GetMethod("op_Equality", new Type[] { typeof(string), typeof(string) });

            var propertyLabels = new Dictionary<DynamicProperty, Label>();
            var returnLabel = il.DefineLabel();
            var notFoundLabel = il.DefineLabel();

            foreach (var item in convention.TransactionProxyProperties)
            {
                var label = il.DefineLabel();
                propertyLabels.Add(item.HasMultipleValuesProperty, label);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldstr, item.Property.Name);
                il.Emit(OpCodes.Call, stringEqual);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ceq);
                il.Emit(OpCodes.Brfalse, label);
            }

            il.Emit(OpCodes.Br, notFoundLabel);

            foreach (var item in propertyLabels)
            {
                il.MarkLabel(item.Value);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Callvirt, item.Key.PropertyGetMethod);
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