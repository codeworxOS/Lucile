using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Codeworx.Dynamic
{
    public class DynamicVoid : DynamicMethod
    {
        public DynamicVoid(string memberName, params Type[] arguments)
            : base(memberName, null, false, arguments)
        {
        }

        public override void CreateDeclarations(TypeBuilder typeBuilder)
        {
            if (!typeof(DynamicObjectBase).IsAssignableFrom(typeBuilder.BaseType)) {
                throw new InvalidOperationException("To implement dynamic void the type has to inherit from Codeworx.Dynamic.DynamicObjectBase");
            }

            base.CreateDeclarations(typeBuilder);

            //MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            //Method = typeBuilder.DefineMethod(this.MemberName, methodAttributes, null, this.ArgumentTypes.ToArray());
        }

        protected override void Implement(DynamicTypeBuilder config, TypeBuilder typeBuilder, ILGenerator methodBody)
        {
            var prop = typeof(DynamicObjectBase).GetProperty("DelegateRegister", BindingFlags.NonPublic | BindingFlags.Instance);

            var getEnumerator = typeof(IEnumerable<Delegate>).GetMethod("GetEnumerator");

            var moveNext = typeof(IEnumerator).GetMethod("MoveNext");
            var getCurrent = typeof(IEnumerator<Delegate>).GetProperty("Current").GetGetMethod();

            var getoraddMethod = typeof(ConcurrentDictionary<string, List<Delegate>>).GetMethod("GetOrAdd", new[] { typeof(string), typeof(List<Delegate>) });

            var subscribed = methodBody.DeclareLocal(typeof(List<Delegate>));
            var enumerator = methodBody.DeclareLocal(typeof(IEnumerator<Delegate>));

            methodBody.Emit(OpCodes.Ldarg_0);
            methodBody.EmitCall(OpCodes.Callvirt, prop.GetGetMethod(true), null);
            methodBody.Emit(OpCodes.Ldstr, this.MemberName);
            methodBody.Emit(OpCodes.Newobj, typeof(List<Delegate>).GetConstructor(new Type[] { }));
            methodBody.EmitCall(OpCodes.Callvirt, getoraddMethod, null);
            methodBody.Emit(OpCodes.Stloc_S, subscribed);
            methodBody.Emit(OpCodes.Ldloc_S, subscribed);
            methodBody.EmitCall(OpCodes.Callvirt, getEnumerator, null);
            methodBody.Emit(OpCodes.Stloc_S, enumerator);

            var exitLabel = methodBody.DefineLabel();
            var loopStart = methodBody.DefineLabel();

            methodBody.MarkLabel(loopStart);
            methodBody.Emit(OpCodes.Ldloc_S, enumerator);
            methodBody.EmitCall(OpCodes.Callvirt, moveNext, null);
            methodBody.Emit(OpCodes.Brfalse, exitLabel);

            var arg = this.ArgumentTypes.FirstOrDefault();

            var actionType = typeof(Action);

            if (arg != null) {
                actionType = typeof(Action<>).MakeGenericType(arg);
            }

            methodBody.Emit(OpCodes.Ldloc_S, enumerator);
            methodBody.EmitCall(OpCodes.Callvirt, getCurrent, null);
            methodBody.Emit(OpCodes.Castclass, actionType);
            if (arg != null) {
                methodBody.Emit(OpCodes.Ldarg_1);
            }
            methodBody.EmitCall(OpCodes.Callvirt, actionType.GetMethod("Invoke"),null);

            methodBody.Emit(OpCodes.Br_S, loopStart);

            methodBody.MarkLabel(exitLabel);
            methodBody.Emit(OpCodes.Ret);

        }
    }
}
