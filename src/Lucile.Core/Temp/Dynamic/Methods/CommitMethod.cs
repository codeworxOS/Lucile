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
    public class CommitMethod : DynamicMethod
    {
        private MethodInfo interfaceMethod;
        internal CommitMethod()
            : base("Codeworx.Dynamic.ICommitable.Commit", typeof(void), false)
        {
            interfaceMethod = typeof(ICommitable).GetMethod("Commit");
        }

        public override bool IsExplicitImplementation(System.Reflection.MethodInfo methodInfo)
        {
            return methodInfo == interfaceMethod;
        }

        protected override void Implement(DynamicTypeBuilder config, System.Reflection.Emit.TypeBuilder typeBuilder, System.Reflection.Emit.ILGenerator il)
        {
            var convention = config.Conventions.OfType<TransactionProxyConvention>().First();

            var listType = typeof(IEnumerable<>).MakeGenericType(config.BaseType);
            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(config.BaseType);
            var enumeratorVariable = il.DeclareLocal(enumeratorType);
            var currentVariable = il.DeclareLocal(config.BaseType);

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

            il.Emit(OpCodes.Ret);
        }
    }
}
