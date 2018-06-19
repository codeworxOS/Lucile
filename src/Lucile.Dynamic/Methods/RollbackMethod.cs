using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Lucile.Dynamic.Convention;

namespace Lucile.Dynamic.Methods
{
    public class RollbackMethod : DynamicMethod
    {
        private readonly MethodInfo _interfaceMethod;

        internal RollbackMethod()
            : base("Lucile.Dynamic.ICommitable.Rollback", typeof(void), false)
        {
            _interfaceMethod = typeof(ICommitable).GetMethod("Rollback");
        }

        public override bool IsExplicitImplementation(System.Reflection.MethodInfo methodInfo)
        {
            return methodInfo == _interfaceMethod;
        }

        protected override void Implement(DynamicTypeBuilder config, System.Reflection.Emit.TypeBuilder typeBuilder, System.Reflection.Emit.ILGenerator il)
        {
            var convention = config.Conventions.OfType<TransactionProxyConvention>().First();
            var addValueMethod = typeof(TransactionProxyHelper).GetMethod("AddValue", BindingFlags.Static | BindingFlags.Public);
            var getValueMethod = typeof(TransactionProxyHelper).GetMethod("GetValue", BindingFlags.Static | BindingFlags.Public);
            var setCollectionValueMethod = typeof(TransactionProxyHelper).GetMethod("SetCollectionValue", BindingFlags.Static | BindingFlags.Public);

            var listType = typeof(IEnumerable<>).MakeGenericType(convention.ItemType);
            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(convention.ItemType);
            var enumeratorVariable = il.DeclareLocal(enumeratorType);
            var currentVariable = il.DeclareLocal(convention.ItemType);

            foreach (var item in convention.TransactionProxyProperties)
            {
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
            var leaveLabel = il.DefineLabel();

            var tryBlock = il.BeginExceptionBlock();

            il.MarkLabel(loopLabel);
            il.Emit(OpCodes.Ldloc, enumeratorVariable);
            il.EmitCall(OpCodes.Callvirt, typeof(IEnumerator).GetMethod("MoveNext"), null);
            il.Emit(OpCodes.Brfalse, leaveLabel);

            il.Emit(OpCodes.Ldloc, enumeratorVariable);
            il.EmitCall(OpCodes.Callvirt, enumeratorType.GetProperty("Current").GetGetMethod(), null);
            il.Emit(OpCodes.Stloc, currentVariable);

            foreach (var item in convention.TransactionProxyProperties)
            {
                il.Emit(OpCodes.Ldloc, currentVariable);
                il.EmitCall(OpCodes.Callvirt, item.Property.GetGetMethod(true), null);
                il.Emit(OpCodes.Ldloc, currentVariable);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, item.ValuesProperty.BackingField);
                il.EmitCall(OpCodes.Call, addValueMethod.MakeGenericMethod(item.Property.PropertyType, convention.ItemType), null);
            }

            il.Emit(OpCodes.Br, loopLabel);

            il.MarkLabel(leaveLabel);
            il.Emit(OpCodes.Leave, nextLabel);

            var endFinally = il.DefineLabel();

            il.BeginFinallyBlock();
            il.Emit(OpCodes.Ldloc, enumeratorVariable);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Brtrue_S, endFinally);
            il.Emit(OpCodes.Ldloc, enumeratorVariable);
            il.EmitCall(OpCodes.Callvirt, typeof(IDisposable).GetMethod("Dispose"), null);
            il.MarkLabel(endFinally);
            il.Emit(OpCodes.Endfinally);
            il.EndExceptionBlock();

            il.MarkLabel(nextLabel);

            foreach (var item in convention.TransactionProxyProperties)
            {
                Type itemType;
                if (IsCollectionType(item.Property.PropertyType, out itemType))
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, item.ValuesProperty.BackingField);
                    il.Emit(OpCodes.Ldarg_0);
                    il.EmitCall(OpCodes.Callvirt, item.Property.GetGetMethod(true), null);
                    il.EmitCall(OpCodes.Call, setCollectionValueMethod.MakeGenericMethod(itemType, item.Property.PropertyType, convention.ItemType), null);
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, item.ValuesProperty.BackingField);
                    il.EmitCall(OpCodes.Call, getValueMethod.MakeGenericMethod(item.Property.PropertyType, convention.ItemType), null);
                    il.EmitCall(OpCodes.Callvirt, item.Property.GetSetMethod(true), null);
                }

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, item.ValuesProperty.BackingField);
                il.EmitCall(OpCodes.Callvirt, item.ValuesProperty.BackingField.FieldType.GetProperty("Count").GetGetMethod(), null);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Ceq);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ceq);
                var skip = il.DefineLabel();
                il.Emit(OpCodes.Brfalse, skip);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4_1);
                il.EmitCall(OpCodes.Callvirt, item.HasMultipleValuesProperty.PropertySetMethod, null);

                il.MarkLabel(skip);
            }

            il.Emit(OpCodes.Ret);
        }

        private bool IsCollectionType(Type collectionType, out Type itemType)
        {
            if (collectionType.IsArray && collectionType.GetElementType() == typeof(byte))
            {
                itemType = typeof(byte);
                return false;
            }

            var interfaceCollection = collectionType.GetInterfaces().Union(new[] { collectionType })
                                   .FirstOrDefault(p => p.IsGenericType && p.GetGenericTypeDefinition() == typeof(ICollection<>));

            if (interfaceCollection != null)
            {
                itemType = interfaceCollection.GetGenericArguments().First();
                return true;
            }

            itemType = null;
            return false;
        }
    }
}