using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Lucile.Dynamic
{
    public class DynamicVoid : DynamicMethod
    {
        public DynamicVoid(string memberName, params Type[] arguments)
            : base(memberName, null, false, arguments)
        {
        }

        public override void CreateDeclarations(TypeBuilder typeBuilder)
        {
            if (!typeof(DynamicObjectBase).IsAssignableFrom(typeBuilder.BaseType))
            {
                throw new InvalidOperationException("To implement dynamic void the type has to inherit from Lucile.Dynamic.DynamicObjectBase");
            }

            base.CreateDeclarations(typeBuilder);

            ////MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            ////Method = typeBuilder.DefineMethod(this.MemberName, methodAttributes, null, this.ArgumentTypes.ToArray());
        }

        protected override void Implement(DynamicTypeBuilder config, TypeBuilder typeBuilder, ILGenerator il)
        {
            var prop = typeof(DynamicObjectBase).GetProperty("DelegateRegister", BindingFlags.NonPublic | BindingFlags.Instance);

            var getEnumerator = typeof(IEnumerable<Delegate>).GetMethod("GetEnumerator");

            var moveNext = typeof(IEnumerator).GetMethod("MoveNext");
            var getCurrent = typeof(IEnumerator<Delegate>).GetProperty("Current").GetGetMethod();

            var getoraddMethod = typeof(ConcurrentDictionary<string, List<Delegate>>).GetMethod("GetOrAdd", new[] { typeof(string), typeof(List<Delegate>) });

            var subscribed = il.DeclareLocal(typeof(List<Delegate>));
            var enumerator = il.DeclareLocal(typeof(IEnumerator<Delegate>));

            il.Emit(OpCodes.Ldarg_0);
            il.EmitCall(OpCodes.Callvirt, prop.GetGetMethod(true), null);
            il.Emit(OpCodes.Ldstr, this.MemberName);
            il.Emit(OpCodes.Newobj, typeof(List<Delegate>).GetConstructor(new Type[] { }));
            il.EmitCall(OpCodes.Callvirt, getoraddMethod, null);
            il.Emit(OpCodes.Stloc_S, subscribed);
            il.Emit(OpCodes.Ldloc_S, subscribed);
            il.EmitCall(OpCodes.Callvirt, getEnumerator, null);
            il.Emit(OpCodes.Stloc_S, enumerator);

            var exitLabel = il.DefineLabel();
            var loopStart = il.DefineLabel();

            il.MarkLabel(loopStart);
            il.Emit(OpCodes.Ldloc_S, enumerator);
            il.EmitCall(OpCodes.Callvirt, moveNext, null);
            il.Emit(OpCodes.Brfalse, exitLabel);

            var arg = this.ArgumentTypes.FirstOrDefault();

            var actionType = typeof(Action);

            if (arg != null)
            {
                actionType = typeof(Action<>).MakeGenericType(arg);
            }

            il.Emit(OpCodes.Ldloc_S, enumerator);
            il.EmitCall(OpCodes.Callvirt, getCurrent, null);
            il.Emit(OpCodes.Castclass, actionType);
            if (arg != null)
            {
                il.Emit(OpCodes.Ldarg_1);
            }

            il.EmitCall(OpCodes.Callvirt, actionType.GetMethod("Invoke"), null);

            il.Emit(OpCodes.Br_S, loopStart);

            il.MarkLabel(exitLabel);
            il.Emit(OpCodes.Ret);
        }
    }
}