﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Lucile.Dynamic.Convention;

namespace Lucile.Dynamic.Methods
{
    public class CommitMethod : DynamicMethod
    {
        private MethodInfo _interfaceMethod;

        internal CommitMethod()
            : base("Lucile.Dynamic.ICommitable.Commit", typeof(void), false)
        {
            _interfaceMethod = typeof(ICommitable).GetMethod("Commit");
        }

        public override bool IsExplicitImplementation(System.Reflection.MethodInfo methodInfo)
        {
            return methodInfo == _interfaceMethod;
        }

        protected override void Implement(DynamicTypeBuilder config, System.Reflection.Emit.TypeBuilder typeBuilder, System.Reflection.Emit.ILGenerator il)
        {
            var convention = config.Conventions.OfType<TransactionProxyConvention>().First();

            var listType = typeof(IEnumerable<>).MakeGenericType(convention.ItemType);
            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(convention.ItemType);
            var enumeratorVariable = il.DeclareLocal(enumeratorType);
            var currentVariable = il.DeclareLocal(convention.ItemType);

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
                var done = il.DefineLabel();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, item.HasMultipleValuesProperty.BackingField);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Ceq);
                il.Emit(OpCodes.Brtrue_S, done);

                il.Emit(OpCodes.Ldloc, currentVariable);
                il.Emit(OpCodes.Ldarg_0);
                il.EmitCall(OpCodes.Callvirt, item.Property.GetGetMethod(true), null);
                il.EmitCall(OpCodes.Callvirt, item.Property.GetSetMethod(true), null);

                il.MarkLabel(done);
            }

            il.Emit(OpCodes.Br, loopLabel);

            il.MarkLabel(leaveLabel);
            il.Emit(OpCodes.Leave_S, nextLabel);

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

            il.Emit(OpCodes.Ret);
        }
    }
}