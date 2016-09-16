using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Codeworx.Dynamic.Convention;

namespace Codeworx.Dynamic.Methods
{
    public class RollbackMethod : DynamicMethod
    {
        private MethodInfo interfaceMethod;
        internal RollbackMethod()
            : base("Codeworx.Dynamic.ICommitable.Rollback", typeof(void), false)
        {
            interfaceMethod = typeof(ICommitable).GetMethod("Rollback");
        }

        public override bool IsExplicitImplementation(System.Reflection.MethodInfo methodInfo)
        {
            return methodInfo == interfaceMethod;
        }

        protected override void Implement(DynamicTypeBuilder config, System.Reflection.Emit.TypeBuilder typeBuilder, System.Reflection.Emit.ILGenerator il)
        {
            var convention = config.Conventions.OfType<TransactionProxyConvention>().First();
            var addValueMethod = typeof(TransactionProxyHelper).GetMethod("AddValue", BindingFlags.Static | BindingFlags.Public);
            var getValueMethod = typeof(TransactionProxyHelper).GetMethod("GetValue", BindingFlags.Static | BindingFlags.Public);
            var setCollectionValueMethod = typeof(TransactionProxyHelper).GetMethod("SetCollectionValue", BindingFlags.Static | BindingFlags.Public);


            var listType = typeof(IEnumerable<>).MakeGenericType(config.BaseType);
            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(config.BaseType);
            var enumeratorVariable = il.DeclareLocal(enumeratorType);
            var currentVariable = il.DeclareLocal(config.BaseType);

            foreach (var item in convention.TransactionProxyProperties) {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Newobj, item.ValuesProperty.MemberType.GetConstructor(new Type[] { }));
                il.Emit(OpCodes.Stfld, item.ValuesProperty.BackingField);
            }


            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, convention.TargetsField.Field);
            il.EmitCall(OpCodes.Callvirt, listType.GetMethod("GetEnumerator"), null);
            il.Emit(OpCodes.Stloc, enumeratorVariable);

            var loopLabel = il.DefineLabel();
            var nextLabel = il.DefineLabel();

            var tryBlock = il.BeginExceptionBlock();

            il.MarkLabel(loopLabel);
            il.Emit(OpCodes.Ldloc, enumeratorVariable);
            il.EmitCall(OpCodes.Callvirt, typeof(IEnumerator).GetMethod("MoveNext"), null);
            il.Emit(OpCodes.Brfalse, nextLabel);

            il.Emit(OpCodes.Ldloc, enumeratorVariable);
            il.EmitCall(OpCodes.Callvirt, enumeratorType.GetProperty("Current").GetGetMethod(), null);
            il.Emit(OpCodes.Stloc, currentVariable);

            foreach (var item in convention.TransactionProxyProperties) {
                il.Emit(OpCodes.Ldloc, currentVariable);
                il.EmitCall(OpCodes.Callvirt, item.Property.GetGetMethod(true), null);
                il.Emit(OpCodes.Ldloc, currentVariable);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, item.ValuesProperty.BackingField);
                il.EmitCall(OpCodes.Call, addValueMethod.MakeGenericMethod(item.Property.PropertyType, config.BaseType), null);
            }

            il.Emit(OpCodes.Br, loopLabel);

            var endFinally = il.DefineLabel();

            il.BeginFinallyBlock();
            il.Emit(OpCodes.Ldloc, enumeratorVariable);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Brtrue_S, endFinally);
            il.Emit(OpCodes.Ldloc, enumeratorVariable);
            il.EmitCall(OpCodes.Callvirt, typeof(IDisposable).GetMethod("Dispose"), null);
            il.MarkLabel(endFinally);
            il.EndExceptionBlock();

            il.MarkLabel(nextLabel);

            foreach (var item in convention.TransactionProxyProperties) {

                Type itemType;
                if (IsCollectionType(item.Property.PropertyType, out itemType)) {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, item.ValuesProperty.BackingField);
                    il.Emit(OpCodes.Ldarg_0);
                    il.EmitCall(OpCodes.Callvirt, item.Property.GetGetMethod(true), null);
                    il.EmitCall(OpCodes.Call, setCollectionValueMethod.MakeGenericMethod(itemType, item.Property.PropertyType, config.BaseType), null);
                } else {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, item.ValuesProperty.BackingField);
                    il.EmitCall(OpCodes.Call, getValueMethod.MakeGenericMethod(item.Property.PropertyType, config.BaseType), null);
                    il.EmitCall(OpCodes.Callvirt, item.Property.GetSetMethod(true), null);
                }

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, item.ValuesProperty.BackingField);
                il.EmitCall(OpCodes.Callvirt, item.ValuesProperty.BackingField.FieldType.GetProperty("Count").GetGetMethod(), null);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Ceq);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ceq);
                il.EmitCall(OpCodes.Callvirt, item.HasMultipleValuesProperty.PropertySetMethod,null);
            }

            il.Emit(OpCodes.Ret);
        }

        private bool IsCollectionType(Type collectionType, out Type itemType)
        {
            var iCollection = collectionType.GetInterfaces().Union(new[] { collectionType })
                                   .FirstOrDefault(p => p.IsGenericType && p.GetGenericTypeDefinition() == typeof(ICollection<>));

            if (iCollection != null) {
                itemType = iCollection.GetGenericArguments().First();
                return true;
            }
            itemType = null;
            return false;
        }
    }
}
